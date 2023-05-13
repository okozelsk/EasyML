﻿using EasyMLCore.Data;
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
        private ResCompTask(ResCompTaskConfig cfg)
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

        /// <inheritdoc/>
        public OutputTaskType TaskType { get { return _cfg.TaskType; } }

        /// <inheritdoc/>
        public int NumOfOutputFeatures { get { return OutputFeatureNames.Count; } }

        //Methods
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
        /// <param name="cfg">Configuration of the RC's task.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Confidence metrics of built model.</returns>
        public static ResCompTask Build(ResCompTaskConfig cfg,
                                        SampleDataset trainingData,
                                        ModelBuildProgressChangedHandler progressInfoSubscriber = null
                                        )
        {
            if (!trainingData.IsConsistent || trainingData.Count < 1 || !trainingData.IsConsistentInputLength())
            {
                throw new ArgumentException($"Invalid or insufficient data.", nameof(trainingData));
            }
            //Build
            ResCompTask resCompTask = new ResCompTask(cfg);
            Type modelCfgType = cfg.ModelCfg.GetType();
            string modelNamePrefix = $"({resCompTask.Name}){ContextPathID}-";
            ModelBase model = null;
            if (modelCfgType == typeof(NetworkModelConfig))
            {
                model = NetworkModel.Build(cfg.ModelCfg,
                                           modelNamePrefix,
                                           resCompTask.TaskType,
                                           resCompTask.OutputFeatureNames,
                                           trainingData,
                                           null,
                                           progressInfoSubscriber
                                           );
            }
            else if (modelCfgType == typeof(CrossValModelConfig))
            {
                model = CrossValModel.Build(cfg.ModelCfg,
                                            modelNamePrefix,
                                            resCompTask.TaskType,
                                            resCompTask.OutputFeatureNames,
                                            trainingData,
                                            progressInfoSubscriber
                                            );
            }
            else if (modelCfgType == typeof(StackingModelConfig))
            {
                model = StackingModel.Build(cfg.ModelCfg,
                                            modelNamePrefix,
                                            resCompTask.TaskType,
                                            resCompTask.OutputFeatureNames,
                                            trainingData,
                                            progressInfoSubscriber
                                            );
            }
            else if (modelCfgType == typeof(CompositeModelConfig))
            {
                model = CompositeModel.Build(cfg.ModelCfg,
                                            modelNamePrefix,
                                            resCompTask.TaskType,
                                            resCompTask.OutputFeatureNames,
                                            trainingData,
                                            progressInfoSubscriber
                                            );
            }
            else
            {
                throw new ApplicationException($"Unsupported model configuration {modelCfgType}.");
            }
            resCompTask._model = model;
            return resCompTask;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Both input and computed values are in the same ranges as were previously
        /// submited into the Build method.
        /// </remarks>
        public double[] Compute(double[] input)
        {
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
