using EasyMLCore.Data;
using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the CompositeModel builder.
    /// </summary>
    public class CompositeModelBuilder
    {

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        public event ModelBuildProgressChangedHandler BuildProgressChanged;

        //Attributes
        private readonly CompositeModelConfig _cfg;
        private string _name;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        public CompositeModelBuilder(CompositeModelConfig cfg)
        {
            _cfg = cfg;
            return;
        }

        //Methods
        private void OnBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            //Update context
            progressInfo.ExtendContextPath(_name);
            //Raise event
            BuildProgressChanged?.Invoke(progressInfo);
            return;
        }

        /// <summary>
        /// Builds a CompositeModel.
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public CompositeModel Build(string name,
                                    OutputTaskType taskType,
                                    List<string> outputFeatureNames,
                                    SampleDataset trainingData,
                                    ModelBuildProgressChangedHandler progressInfoSubscriber = null
                                    )
        {
            if (progressInfoSubscriber != null)
            {
                BuildProgressChanged += progressInfoSubscriber;
            }
            _name = name.Length > 0 ? name : CompositeModel.ContextPathID;
            //Composite model
            CompositeModel model = new CompositeModel(name,
                                                      taskType,
                                                      outputFeatureNames
                                                      );
            //Sub models build
            for (int subModelIdx = 0; subModelIdx < _cfg.SubModelCfgCollection.Count; subModelIdx++)
            {
                string subModelNum = "M" + (subModelIdx + 1).ToLeftPaddedString(_cfg.SubModelCfgCollection.Count, '0');
                Type subModelCfgType = _cfg.SubModelCfgCollection[subModelIdx].GetType();
                if (subModelCfgType == typeof(NetworkModelConfig))
                {
                    NetworkModelBuilder buider =
                        new NetworkModelBuilder(_cfg.SubModelCfgCollection[subModelIdx] as NetworkModelConfig);
                    NetworkModel subModel =
                        buider.Build($"{subModelNum}-{NetworkModel.ContextPathID}",
                                                taskType,
                                                outputFeatureNames,
                                                trainingData,
                                                null,
                                                OnBuildProgressChanged,
                                                false
                                                );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(CrossValModelConfig))
                {
                    CrossValModelBuilder buider =
                        new CrossValModelBuilder(_cfg.SubModelCfgCollection[subModelIdx] as CrossValModelConfig);
                    CrossValModel subModel =
                        buider.Build($"{subModelNum}-{CrossValModel.ContextPathID}",
                                                taskType,
                                                outputFeatureNames,
                                                trainingData,
                                                OnBuildProgressChanged
                                                );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(StackingModelConfig))
                {
                    StackingModelBuilder buider =
                        new StackingModelBuilder(_cfg.SubModelCfgCollection[subModelIdx] as StackingModelConfig);
                    StackingModel subModel =
                        buider.Build($"{subModelNum}-{StackingModel.ContextPathID}",
                                                taskType,
                                                outputFeatureNames,
                                                trainingData,
                                                OnBuildProgressChanged
                                                );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(CompositeModelConfig))
                {
                    CompositeModelBuilder buider =
                        new CompositeModelBuilder(_cfg.SubModelCfgCollection[subModelIdx] as CompositeModelConfig);
                    CompositeModel subModel =
                        buider.Build($"{subModelNum}-{CompositeModel.ContextPathID}",
                                                taskType,
                                                outputFeatureNames,
                                                trainingData,
                                                OnBuildProgressChanged
                                                );
                    model.AddMember(subModel);
                }
            }//subModelIdx
            //Set model operationable
            model.SetOperationable();
            //Return built model
            return model;
        }

    }//CompositeModelBuilder

}//Namespace
