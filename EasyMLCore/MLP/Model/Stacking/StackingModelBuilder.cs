using EasyMLCore.Data;
using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the StackingModel builder.
    /// </summary>
    public class StackingModelBuilder
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
        private readonly StackingModelConfig _cfg;
        private string _name;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        public StackingModelBuilder(StackingModelConfig cfg)
        {
            _cfg = cfg;
            return;
        }

        //Methods
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

        private void OnBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            //Update context
            progressInfo.ExtendContextPath(_name);
            //Raise event
            BuildProgressChanged?.Invoke(progressInfo);
            return;
        }

        /// <summary>
        /// Builds a StackingModel.
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public StackingModel Build(string name,
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
            _name = name.Length > 0 ? name : StackingModel.ContextPathID;
            //Model instance
            StackingModel model = new StackingModel(_name, taskType, outputFeatureNames, _cfg.RouteInput);
            //Copy the data locally
            SampleDataset localDataset = trainingData.ShallowClone();
            //Shuffle local data
            localDataset.Shuffle(new Random(RandomSeed));
            //Folderize local data
            List<SampleDataset> foldCollection = localDataset.Folderize(_cfg.FoldDataRatio, taskType);
            //Weak networks
            //Array of weak networks
            NetworkModel[][] weakNetworks = new NetworkModel[_cfg.StackCfg.NetworkModelCfgCollection.Count][];
            for(int i = 0; i < _cfg.StackCfg.NetworkModelCfgCollection.Count; i++)
            {
                weakNetworks[i] = new NetworkModel[foldCollection.Count];
            }
            //Weak networks validation outputs storage
            double[][][][] stackNetsHoldOutOutputs = new double[foldCollection.Count][][][];
            for(int i =  0; i < foldCollection.Count; i++)
            {
                stackNetsHoldOutOutputs[i] = new double[_cfg.StackCfg.NetworkModelCfgCollection.Count][][];
            }
            //Weak networks cumulated training err statistics
            ModelErrStat[] weakNetErrStats = new ModelErrStat[_cfg.StackCfg.NetworkModelCfgCollection.Count];
            for(int i = 0; i < _cfg.StackCfg.NetworkModelCfgCollection.Count; i++)
            {
                weakNetErrStats[i] = new ModelErrStat(taskType, outputFeatureNames);
            }
            NetworkModel[] strongNetworks = new NetworkModel[_cfg.StackCfg.NetworkModelCfgCollection.Count];
            //Build stack's weak networks and prepare input data for meta model
            for (int holdOutFoldIdx = 0; holdOutFoldIdx < foldCollection.Count; holdOutFoldIdx++)
            {
                string holdOutFoldNumStr = "F" + (holdOutFoldIdx + 1).ToLeftPaddedString(foldCollection.Count, '0');
                //Prepare training data
                SampleDataset weakNetTrainingData = new SampleDataset();
                for (int foldIdx = 0; foldIdx < foldCollection.Count; foldIdx++)
                {
                    if (foldIdx != holdOutFoldIdx)
                    {
                        weakNetTrainingData.Add(foldCollection[foldIdx]);
                    }
                }
                //Build weak networks on training data and prepare hold-out data fold as an input for meta model
                for(int stackNetIdx = 0;  stackNetIdx < _cfg.StackCfg.NetworkModelCfgCollection.Count; stackNetIdx++)
                {
                    string netNumStr = (stackNetIdx + 1).ToLeftPaddedString(_cfg.StackCfg.NetworkModelCfgCollection.Count, '0');
                    //Network builder
                    NetworkModelBuilder weakNetBuilder = new NetworkModelBuilder(_cfg.StackCfg.NetworkModelCfgCollection[stackNetIdx]);
                    //Build weak network
                    NetworkModel weakNetwork =
                        weakNetBuilder.Build($"{holdOutFoldNumStr}-Weak{netNumStr}-{NetworkModel.ContextPathID}",
                                                     taskType,
                                                     outputFeatureNames,
                                                     weakNetTrainingData,
                                                     foldCollection[holdOutFoldIdx],
                                                     OnBuildProgressChanged,
                                                     false //Do not engage hold-out fold
                                                     );
                    weakNetworks[stackNetIdx][holdOutFoldIdx] = weakNetwork;
                    weakNetErrStats[stackNetIdx].Merge(weakNetwork.TrainingErrorStat);
                    stackNetsHoldOutOutputs[holdOutFoldIdx][stackNetIdx] =
                        weakNetwork.ComputeSampleDataset(foldCollection[holdOutFoldIdx], out _);
                }//stackNetIdx
            }//holdOutFoldIdx
            //Build stack's strong networks on whole data and add them into the model's stack
            for (int stackNetIdx = 0; stackNetIdx < _cfg.StackCfg.NetworkModelCfgCollection.Count; stackNetIdx++)
            {
                string netNumStr = (stackNetIdx + 1).ToLeftPaddedString(_cfg.StackCfg.NetworkModelCfgCollection.Count, '0');
                //Network builder
                NetworkModelBuilder strongNetBuilder = new NetworkModelBuilder(_cfg.StackCfg.NetworkModelCfgCollection[stackNetIdx]);
                //Build strong network
                NetworkModel strongNetwork =
                    strongNetBuilder.Build($"Strong{netNumStr}-{NetworkModel.ContextPathID}",
                                                    taskType,
                                                    outputFeatureNames,
                                                    localDataset,
                                                    null,
                                                    OnBuildProgressChanged,
                                                    false
                                                    );
                strongNetworks[stackNetIdx] = strongNetwork;
                //Add strong network into the model's stack
                model.AddStackMember(strongNetwork);
            }//stackNetIdx
            //Prepare data for meta-learner model
            Sample[] metaLearnerTrainingDataArray = new Sample[localDataset.Count];
            Parallel.For(0, foldCollection.Count, foldIdx =>
            {
                for (int sampleIdx = 0; sampleIdx < foldCollection[foldIdx].Count; sampleIdx++)
                {
                    double[][] stackNetsOutputs = new double[_cfg.StackCfg.NetworkModelCfgCollection.Count][];
                    for (int netIdx = 0; netIdx < _cfg.StackCfg.NetworkModelCfgCollection.Count; netIdx++)
                    {
                        stackNetsOutputs[netIdx] = stackNetsHoldOutOutputs[foldIdx][netIdx][sampleIdx];
                        //Average weak outputs and strong outputs
                        double[] strongNetOutput = strongNetworks[netIdx].Compute(foldCollection[foldIdx].SampleCollection[sampleIdx].InputVector);
                        for (int i = 0; i < stackNetsOutputs[netIdx].Length; i++)
                        {
                            //(1:1) seems to be the best choice
                            stackNetsOutputs[netIdx][i] = (stackNetsOutputs[netIdx][i] + strongNetOutput[i]) / 2d;
                        }
                    }//netIdx
                    metaLearnerTrainingDataArray[foldCollection[foldIdx].SampleCollection[sampleIdx].ID] =
                        new Sample(foldCollection[foldIdx].SampleCollection[sampleIdx].ID,
                                   _cfg.RouteInput ? (double[])foldCollection[foldIdx].SampleCollection[sampleIdx].InputVector.Concat(stackNetsOutputs.Flattenize()) : stackNetsOutputs.Flattenize(),
                                   foldCollection[foldIdx].SampleCollection[sampleIdx].OutputVector
                                   );
                }//sampleIdx
            });//foldIdx
            SampleDataset metaLearnerTrainingData = new SampleDataset(metaLearnerTrainingDataArray);
            //Build a meta-learner
            Type metaLearnerCfgType = _cfg.MetaLearnerCfg.GetType();
            string metaLearnerModelStr = string.Empty;
            ModelBase metaLearnerModel = null;
            if (metaLearnerCfgType == typeof(NetworkModelConfig))
            {
                metaLearnerModelStr = $"Meta-Learner-{NetworkModel.ContextPathID}";
                NetworkModelBuilder buider = new NetworkModelBuilder(_cfg.MetaLearnerCfg as NetworkModelConfig);
                metaLearnerModel = buider.Build(metaLearnerModelStr,
                                                taskType,
                                                outputFeatureNames,
                                                metaLearnerTrainingData,
                                                null,
                                                OnBuildProgressChanged,
                                                false
                                                );
            }
            else if (metaLearnerCfgType == typeof(CrossValModelConfig))
            {
                metaLearnerModelStr = $"Meta-Learner-{CrossValModel.ContextPathID}";
                CrossValModelBuilder buider = new CrossValModelBuilder(_cfg.MetaLearnerCfg as CrossValModelConfig);
                metaLearnerModel = buider.Build(metaLearnerModelStr,
                                                    taskType,
                                                    outputFeatureNames,
                                                    metaLearnerTrainingData,
                                                    OnBuildProgressChanged
                                                    );
            }
            else if (metaLearnerCfgType == typeof(StackingModelConfig))
            {
                metaLearnerModelStr = $"Meta-Learner-{StackingModel.ContextPathID}";
                StackingModelBuilder buider = new StackingModelBuilder(_cfg.MetaLearnerCfg as StackingModelConfig);
                metaLearnerModel = buider.Build(metaLearnerModelStr,
                                                taskType,
                                                outputFeatureNames,
                                                metaLearnerTrainingData,
                                                OnBuildProgressChanged
                                                );
            }
            else if (metaLearnerCfgType == typeof(CompositeModelConfig))
            {
                metaLearnerModelStr = $"Meta-Learner-{CompositeModel.ContextPathID}";
                CompositeModelBuilder buider = new CompositeModelBuilder(_cfg.MetaLearnerCfg as CompositeModelConfig);
                metaLearnerModel = buider.Build(metaLearnerModelStr,
                                                taskType,
                                                outputFeatureNames,
                                                metaLearnerTrainingData,
                                                OnBuildProgressChanged
                                                );
            }
            //Set model operationable
            model.SetOperationable(metaLearnerModel);
            //Return built model
            return model;
        }

    }//StackingModelBuilder

}//Namespace
