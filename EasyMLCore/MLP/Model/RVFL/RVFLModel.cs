﻿using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MLP.Model;
using EasyMLCore.TimeSeries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements a RVFL (Random vector functional link network) model.
    /// </summary>
    [Serializable]
    public class RVFLModel : ModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "RVFL";

        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        internal event ModelBuildProgressChangedHandler BuildProgressChanged;

        //Attribute properties
        /// <summary>
        /// Preprocessor.
        /// </summary>
        public RVFLPreprocessor Preprocessor { get; private set; }

        /// <summary>
        /// End MLP model.
        /// </summary>
        public ModelBase EndModel { get; private set; }

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="modelConfig">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        private RVFLModel(RVFLModelConfig modelConfig,
                               string name,
                               OutputTaskType taskType,
                               IEnumerable<string> outputFeatureNames
                               )
            : base(modelConfig, name, taskType, outputFeatureNames)
        {
            Preprocessor = null;
            EndModel = null;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RVFLModel(RVFLModel source)
            : base(source)
        {
            Preprocessor = source.Preprocessor.DeepClone();
            EndModel = source.EndModel.DeepClone();
            return;
        }

        //Methods
        private void OnRVFLInitProgressChanged(RVFLInitProgressInfo progressInfo)
        {
            ModelBuildProgressInfo trainProgressInfo =
                new ModelBuildProgressInfo(Name, progressInfo.ProcessedInputsTracker, null);
            BuildProgressChanged?.Invoke(trainProgressInfo);
            return;
        }

        private void OnModelBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            BuildProgressChanged?.Invoke(progressInfo);
            return;
        }

        /// <summary>
        /// Sets the model operationable.
        /// </summary>
        private void SetOperationable(ModelBase endModel)
        {
            EndModel = endModel;
            //Finalize model
            FinalizeModel(EndModel.ConfidenceMetrics);
            return;
        }

        /// <inheritdoc/>
        public override double[] Compute(double[] input)
        {
            return EndModel.Compute(Preprocessor.Compute(input));
        }

        /// <inheritdoc/>
        public override string GetInfoText(bool detail = false, int margin = 0)
        {
            margin = Math.Max(margin, 0);
            StringBuilder sb = new StringBuilder($"{Name} [{GetType()}]{Environment.NewLine}");
            sb.Append($"    Task type              : {TaskType.ToString()}{Environment.NewLine}");
            sb.Append($"    Output features info   : {OutputFeatureNames.Count.ToString(CultureInfo.InvariantCulture)}");
            int fIdx = 0;
            foreach (string outputFeatureName in OutputFeatureNames)
            {
                sb.Append($" [{outputFeatureName}, {ConfidenceMetrics.FeatureConfidences[fIdx++].ToString("F3", CultureInfo.InvariantCulture)}]");
            }
            sb.Append(Environment.NewLine);
            sb.Append($"    Preprocessor outputs   : {Preprocessor.NumOfOutputFeatures.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    End model{Environment.NewLine}");
            sb.Append(EndModel.GetInfoText(detail, 8));
            string infoText = sb.ToString();
            if (margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }


        /// <inheritdoc/>
        public override ModelDiagnosticData DiagnosticTest(SampleDataset testingData, ModelTestProgressChangedHandler progressInfoSubscriber = null)
        {
            ModelErrStat errStat = Test(testingData, out _, progressInfoSubscriber);
            ModelDiagnosticData diagData = new ModelDiagnosticData(Name, errStat);
            diagData.SetFinalized();
            return diagData;
        }

        /// <inheritdoc/>
        public override ModelBase DeepClone()
        {
            return new RVFLModel(this);
        }

        //Static methods
        /// <summary>
        /// Builds a RVFLModel.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public static RVFLModel Build(IModelConfig cfg,
                                      string name,
                                      OutputTaskType taskType,
                                      List<string> outputFeatureNames,
                                      SampleDataset trainingData,
                                      ModelBuildProgressChangedHandler progressInfoSubscriber = null
                                      )
        {
            //Checks
            if (cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }
            if (cfg.GetType() != typeof(RVFLModelConfig))
            {
                throw new ArgumentException($"Wrong type of configuration. Expected {typeof(RVFLModelConfig)} but received {cfg.GetType()}.", nameof(cfg));
            }
            //Composite model
            RVFLModelConfig modelConfig = (RVFLModelConfig)cfg;
            RVFLModel model = new RVFLModel(modelConfig,
                                            (name + RVFLModel.ContextPathID),
                                            taskType,
                                            outputFeatureNames
                                            );
            if(progressInfoSubscriber != null)
            {
                model.BuildProgressChanged += progressInfoSubscriber;
            }
            try
            {
                //Preprocessor
                model.Preprocessor = new RVFLPreprocessor(trainingData.FirstInputVectorLength,
                                                          modelConfig);
                SampleDataset rvflTrainingData =
                    model.Preprocessor.Init(trainingData,
                                            new Random(GetRandomSeed()),
                                            model.OnRVFLInitProgressChanged
                                            );
                //Build end model
                ModelBase endModel = null;
                string endModelName = $"{model.Name}.EndModel-";
                Type endModelCfgType = modelConfig.EndModelCfg.GetType();
                if (endModelCfgType == typeof(NetworkModelConfig))
                {
                    endModel =
                        NetworkModel.Build(modelConfig.EndModelCfg,
                                           endModelName,
                                           taskType,
                                           outputFeatureNames,
                                           rvflTrainingData,
                                           null,
                                           model.OnModelBuildProgressChanged
                                           );
                }
                else if (endModelCfgType == typeof(CrossValModelConfig))
                {
                    endModel =
                        CrossValModel.Build(modelConfig.EndModelCfg,
                                            endModelName,
                                            taskType,
                                            outputFeatureNames,
                                            rvflTrainingData,
                                            model.OnModelBuildProgressChanged
                                            );
                }
                else if (endModelCfgType == typeof(StackingModelConfig))
                {
                    endModel =
                        StackingModel.Build(modelConfig.EndModelCfg,
                                            endModelName,
                                            taskType,
                                            outputFeatureNames,
                                            rvflTrainingData,
                                            model.OnModelBuildProgressChanged
                                            );
                }
                else if (endModelCfgType == typeof(BHSModelConfig))
                {
                    endModel =
                        BHSModel.Build(modelConfig.EndModelCfg,
                                       endModelName,
                                       taskType,
                                       outputFeatureNames,
                                       rvflTrainingData,
                                       model.OnModelBuildProgressChanged
                                       );
                }
                else if (endModelCfgType == typeof(CompositeModelConfig))
                {
                    endModel =
                        CompositeModel.Build(modelConfig.EndModelCfg,
                                             endModelName,
                                             taskType,
                                             outputFeatureNames,
                                             rvflTrainingData,
                                             model.OnModelBuildProgressChanged
                                             );
                }
                //Set model operationable
                model.SetOperationable(endModel);
                //Return built model
                return model;
            }
            finally
            {
                if(progressInfoSubscriber != null)
                {
                    model.BuildProgressChanged -= progressInfoSubscriber;
                }
            }
        }

    }//RVFLModel

}//Namespace
