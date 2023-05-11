using EasyMLCore.Data;
using System;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements a builder of any defined model.
    /// </summary>
    public class ModelBuilder
    {

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        public event ModelBuildProgressChangedHandler BuildProgressChanged;

        //Attributes
        private readonly IModelConfig _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">Configuration of the model to be built.</param>
        public ModelBuilder(IModelConfig cfg)
        {
            _cfg = cfg;
            return;
        }

        //Methods
        private void OnBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            //Re-raise event only
            BuildProgressChanged?.Invoke(progressInfo);
            return;
        }

        /// <summary>
        /// Builds specified model.
        /// </summary>
        /// <param name="name">Output task name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public ModelBase Build(string name,
                               OutputTaskType taskType,
                               List<string> outputFeatureNames,
                               SampleDataset trainingData,
                               ModelBuildProgressChangedHandler progressInfoSubscriber = null
                               )
        {
            string modelName = name.Length > 0 ? $"({name})" : string.Empty;
            if (progressInfoSubscriber != null)
            {
                BuildProgressChanged += progressInfoSubscriber;
            }
            Type modelCfgType = _cfg.GetType();
            if (modelCfgType == typeof(NetworkModelConfig))
            {
                NetworkModelBuilder buider =
                    new NetworkModelBuilder(_cfg as NetworkModelConfig);
                return buider.Build($"{NetworkModel.ContextPathID}{modelName}", taskType, outputFeatureNames, trainingData, null, OnBuildProgressChanged, false);
            }
            else if (modelCfgType == typeof(CrossValModelConfig))
            {
                CrossValModelBuilder buider =
                    new CrossValModelBuilder(_cfg as CrossValModelConfig);
                return buider.Build($"{CrossValModel.ContextPathID}{modelName}", taskType, outputFeatureNames, trainingData, OnBuildProgressChanged);
            }
            else if (modelCfgType == typeof(StackingModelConfig))
            {
                StackingModelBuilder buider =
                    new StackingModelBuilder(_cfg as StackingModelConfig);
                return buider.Build($"{StackingModel.ContextPathID}{modelName}", taskType, outputFeatureNames, trainingData, OnBuildProgressChanged);
            }
            else if (modelCfgType == typeof(CompositeModelConfig))
            {
                CompositeModelBuilder buider =
                    new CompositeModelBuilder(_cfg as CompositeModelConfig);
                return buider.Build($"{CompositeModel.ContextPathID}{modelName}", taskType, outputFeatureNames, trainingData, OnBuildProgressChanged);
            }
            else
            {
                throw new ArgumentException($"Configuration contains an unsupported meta-learner model {modelCfgType}.");
            }
        }

    }//ModelBuilder

}//Namespace
