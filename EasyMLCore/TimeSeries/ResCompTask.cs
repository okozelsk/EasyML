using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MLP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Implements a reservoir computer's single task.
    /// </summary>
    [Serializable]
    public class ResCompTask : SerializableObject, IComputableTaskSpecific
    {
        //Constants
        public const string ContextPathID = "RCTask";

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        public event ModelBuildProgressChangedHandler BuildProgressChanged;

        /// <summary>
        /// This informative event occurs each time the progress of the 
        /// testing process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        public event ModelTestProgressChangedHandler ModelTestProgressChanged;

        /// <inheritdoc/>
        public List<string> OutputFeatureNames { get; }

        //Attributes
        private readonly ResCompTaskConfig _cfg;
        private ModelBase _model;

        //Constructors
        public ResCompTask(ResCompTaskConfig cfg)
        {
            OutputFeatureNames = cfg.OutputFeaturesCfg.GetFeatureNames();
            _cfg = (ResCompTaskConfig)cfg.DeepClone();
            _model = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ResCompTask(ResCompTask source)
        {
            OutputFeatureNames = new List<string>(source.OutputFeatureNames);
            _cfg = (ResCompTaskConfig)source._cfg.DeepClone();
            _model = source._model?.DeepClone();
            return;
        }

        //Properties
        /// <inheritdoc cref="ResCompTaskConfig.Name"/>
        public string Name { get { return _cfg.Name; } }

        /// <summary>
        /// Indicates readiness.
        /// </summary>
        public bool Ready { get { return _model != null; } }

        /// <inheritdoc/>
        public OutputTaskType TaskType { get { return _cfg.TaskType; } }

        /// <inheritdoc/>
        public int NumOfOutputFeatures { get { return OutputFeatureNames.Count; } }

        //Methods
        private void OnModelBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            //Update context
            progressInfo.ExtendContextPath($"{ContextPathID}({Name})");
            //Raise event
            BuildProgressChanged?.Invoke(progressInfo);
            return;
        }

        private void OnModelTestProgressChanged(ModelTestProgressInfo progressInfo)
        {
            //Update context
            progressInfo.ExtendContextPath($"{ContextPathID}({Name})");
            //Raise event
            ModelTestProgressChanged?.Invoke(progressInfo);
            return;
        }

        /// <summary>
        /// Builds inner model and gets the ResCompTask ready.
        /// </summary>
        /// <remarks>
        /// Data of samples can be in any range. Method always does data standardization.
        /// </remarks>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Confidence metrics of built model.</returns>
        public ModelConfidenceMetrics Build(SampleDataset trainingData,
                                            ModelBuildProgressChangedHandler progressInfoSubscriber = null
                                            )
        {
            if (Ready)
            {
                throw new InvalidOperationException($"ResCompTask {Name} has been already built.");
            }
            if (!trainingData.IsConsistent || trainingData.Count < 1 || !trainingData.IsConsistentInputLength())
            {
                throw new ArgumentException($"Invalid or insufficient data.", nameof(trainingData));
            }
            if (progressInfoSubscriber != null)
            {
                BuildProgressChanged += progressInfoSubscriber;
            }
            //Build model
            ModelBuilder builder = new ModelBuilder(_cfg.ModelCfg);
            _model = builder.Build(string.Empty, TaskType, OutputFeatureNames, trainingData, OnModelBuildProgressChanged);
            return _model.ConfidenceMetrics;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Both input and computed values are in the same ranges as were previously
        /// submited into the Build method.
        /// </remarks>
        public double[] Compute(double[] input)
        {
            if (!Ready)
            {
                throw new InvalidOperationException($"ResCompTask {Name} has not been built.");
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            return _model.Compute(input);
        }

        /// <summary>
        /// Computes an output.
        /// </summary>
        /// <param name="input">Input vector.</param>
        /// <param name="detailedOutput">The appropriate instance of task specific detailed output.</param>
        /// <returns>Computed output vector.</returns>
        public double[] Compute(double[] input, out TaskOutputDetailBase detailedOutput)
        {
            detailedOutput = GetOutputDetail(Compute(input));
            return detailedOutput.RawData;
        }


        /// <summary>
        /// Performs RC's single task test.
        /// </summary>
        /// <remarks>
        /// Data of samples can be in any range. Data standardization is always performed internally.
        /// </remarks>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="resultDataset">Result dataset containing original samples together with computed data.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Resulting error statistics.</returns>
        public ModelErrStat Test(SampleDataset testingData,
                                 out ResultDataset resultDataset,
                                 ModelTestProgressChangedHandler progressInfoSubscriber = null
                                 )
        {
            if (!Ready)
            {
                throw new InvalidOperationException($"ResCompTask {Name} has not been built yet.");
            }
            if (progressInfoSubscriber != null)
            {
                ModelTestProgressChanged += progressInfoSubscriber;
            }
            return _model.Test(testingData, out resultDataset, OnModelTestProgressChanged);
        }

        /// <inheritdoc/>
        public TaskOutputDetailBase GetOutputDetail(double[] outputData)
        {
            return TaskType switch
            {
                OutputTaskType.Regression => new RegressionOutputDetail(OutputFeatureNames, outputData),
                OutputTaskType.Binary => new BinaryOutputDetail(OutputFeatureNames, outputData),
                OutputTaskType.Categorical => new CategoricalOutputDetail(OutputFeatureNames, outputData),
                _ => null,
            };
        }

        /// <summary>
        /// Gets an informative text about this reservoir computer's output task instance.
        /// </summary>
        /// <param name="detail">Specifies whether to include details about inner model.</param>
        /// <param name="margin">Specifies left margin.</param>
        /// <returns></returns>
        public string GetInfoText(bool detail = false, int margin = 0)
        {
            StringBuilder sb = new StringBuilder($"Task ({Name}){Environment.NewLine}");
            sb.Append($"    Ready          : {Ready.GetXmlCode()}{Environment.NewLine}");
            sb.Append($"    Task type      : {TaskType.ToString()}{Environment.NewLine}");
            sb.Append($"    Output features: {NumOfOutputFeatures.ToString(CultureInfo.InvariantCulture)}");
            foreach (string outputFeatureName in OutputFeatureNames)
            {
                sb.Append($" [{outputFeatureName}]");
            }
            sb.Append(Environment.NewLine);
            sb.Append(_model.GetInfoText(detail, 4));
            string infoText = sb.ToString();
            if( margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }


        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        public ResCompTask DeepClone()
        {
            return new ResCompTask(this);
        }

    }//ResCompTask

}//Namespace
