using EasyMLCore.Data;
using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the CrossValModel builder.
    /// </summary>
    public class CrossValModelBuilder
    {
        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        public event ModelBuildProgressChangedHandler BuildProgressChanged;

        //Static variables
        /// <summary>
        /// A number used to initialize pseudo random numbers.
        /// </summary>
        private static int RandomSeed = Common.DefaultRandomSeed;

        //Attributes
        private readonly CrossValModelConfig _cfg;
        private string _name;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        public CrossValModelBuilder(CrossValModelConfig cfg)
        {
            _cfg = cfg;
            return;
        }

        //Static methods
        /// <summary>
        /// Changes a number used to initialize pseudo random numbers.
        /// </summary>
        /// <param name="seed">New seed value.</param>
        public static void SetRandomSeed(int seed)
        {
            RandomSeed = seed;
            return;
        }

        /// <summary>
        /// Gets a number to be used to initialize pseudo random numbers.
        /// </summary>
        /// <param name="seed">New seed value.</param>
        public static int GetRandomSeed()
        {
            return RandomSeed;
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
        /// Builds a CrossValModel.
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public CrossValModel Build(string name,
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
            _name = name.Length > 0 ? name : CrossValModel.ContextPathID;
            SampleDataset localDataset = trainingData.ShallowClone();
            //Model
            CrossValModel model = new CrossValModel(_name,
                                                    taskType,
                                                    outputFeatureNames
                                                    );
            //Reshuffle local data
            localDataset.Shuffle(new Random(RandomSeed));
            //Split data to folds
            List<SampleDataset> foldCollection = localDataset.Folderize(_cfg.FoldDataRatio, taskType);
            //Member's training
            //Train a network for each validation fold.
            for (int validationFoldIdx = 0; validationFoldIdx < foldCollection.Count; validationFoldIdx++)
            {
                //Prepare training data dataset
                SampleDataset nodeTrainingData = new SampleDataset();
                for (int foldIdx = 0; foldIdx < foldCollection.Count; foldIdx++)
                {
                    if (foldIdx != validationFoldIdx)
                    {
                        nodeTrainingData.Add(foldCollection[foldIdx]);
                    }
                }
                string validationFoldNumStr = "F" + (validationFoldIdx + 1).ToLeftPaddedString(foldCollection.Count, '0');
                NetworkModelBuilder netBuilder = new NetworkModelBuilder(_cfg.NetworkModelCfg);
                //Build network
                NetworkModel network =
                    netBuilder.Build($"{validationFoldNumStr}-{NetworkModel.ContextPathID}",
                                             taskType,
                                             outputFeatureNames,
                                             nodeTrainingData,
                                             foldCollection[validationFoldIdx],
                                             OnBuildProgressChanged
                                             );
                //Add network into the model
                model.AddMember(network);
            }//validationFoldIdx
            //Set model operationable
            model.SetOperationable();
            //Return built model
            return model;
        }


    }//CrossValModelBuilder

}//Namespace
