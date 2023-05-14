using EasyMLCore.Data;
using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements a model of the meta-learner, which is trained on outputs of NetworkModel(s)
    /// defined in a stack.
    /// Model output is an output of trained meta-learner. Meta-Learner can be any kind of model.
    /// </summary>
    [Serializable]
    public class StackingModel : ModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "Stacking";

        //Attributes
        private readonly List<NetworkModel> _stack;
        private ModelBase _metaLearner;
        private readonly bool _routeInput;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="modelConfig">Model configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner.</param>
        private StackingModel(StackingModelConfig modelConfig,
                              string name,
                              OutputTaskType taskType,
                              IEnumerable<string> outputFeatureNames,
                              bool routeInput
                              )
            : base(modelConfig, name, taskType, outputFeatureNames)
        {
            _stack = new List<NetworkModel>();
            _metaLearner = null;
            _routeInput = routeInput;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public StackingModel(StackingModel source)
            : base(source)
        {
            _stack = new List<NetworkModel>(source._stack.Count);
            foreach (NetworkModel sourceStackMember in source._stack)
            {
                _stack.Add((NetworkModel)sourceStackMember.DeepClone());
            }
            _metaLearner = source._metaLearner?.DeepClone();
            return;
        }

        //Methods
        /// <summary>
        /// Adds network into the stack.
        /// </summary>
        /// <param name="stackMember">A network model to be added into the stack.</param>
        private void AddStackMember(NetworkModel stackMember)
        {
            //Check
            if (stackMember.TaskType != TaskType || stackMember.NumOfOutputFeatures != NumOfOutputFeatures)
            {
                throw new ArgumentException("Inconsistent member network in terms of output task type or number of output features.", nameof(stackMember));
            }
            //Add member
            _stack.Add(stackMember);
            return;
        }

        /// <summary>
        /// Sets the model operationable.
        /// </summary>
        /// <param name="metaLearner">A meta-learner model combining outputs od the stack members.</param>
        private void SetOperationable(ModelBase metaLearner)
        {
            //Checks
            if (metaLearner.TaskType != TaskType || metaLearner.NumOfOutputFeatures != NumOfOutputFeatures)
            {
                throw new ArgumentException("Inconsistent meta-learner in terms of output task type or number of output features.", nameof(metaLearner));
            }
            if (_stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }
            _metaLearner = metaLearner;
            //Set metrics
            FinalizeModel(_metaLearner.ConfidenceMetrics);
            return;
        }

        /// <summary>
        /// Computes outputs of stack members.
        /// </summary>
        /// <param name="inputVector">Input vector.</param>
        public List<double[]> ComputeMembers(double[] inputVector)
        {
            List<double[]> outputVectors = new List<double[]>(_stack.Count);
            foreach (NetworkModel net in _stack)
            {
                outputVectors.Add(net.Compute(inputVector));
            }
            return outputVectors;
        }

        /// <inheritdoc/>
        public override double[] Compute(double[] input)
        {
            double[] stackOutputs = ComputeMembers(input).Flattenize();
            return _metaLearner.Compute(_routeInput ? (double[])input.Concat(stackOutputs) : stackOutputs);
        }

        /// <inheritdoc/>
        public override string GetInfoText(bool detail = false, int margin = 0)
        {
            margin = Math.Max(margin, 0);
            StringBuilder sb = new StringBuilder($"{Name} [{GetType()}]{Environment.NewLine}");
            sb.Append($"    Task type                  : {TaskType.ToString()}{Environment.NewLine}");
            sb.Append($"    Output features info       : {OutputFeatureNames.Count.ToString(CultureInfo.InvariantCulture)}");
            int fIdx = 0;
            foreach (string outputFeatureName in OutputFeatureNames)
            {
                sb.Append($" [{outputFeatureName}, {ConfidenceMetrics.FeatureConfidences[fIdx++].ToString("F3", CultureInfo.InvariantCulture)}]");
            }
            sb.Append(Environment.NewLine);
            sb.Append($"    Number of stacked models   : {_stack.Count.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if (detail)
            {
                sb.Append($"    Stacked models one by one >>>{Environment.NewLine}");
                for (int i = 0; i < _stack.Count; i++)
                {
                    sb.Append(_stack[i].GetInfoText(detail, 8));
                }
            }
            sb.Append($"    Route input to meta learner: {_routeInput.GetXmlCode()}{Environment.NewLine}");
            sb.Append(_metaLearner.GetInfoText(detail, 4));
            string infoText = sb.ToString();
            if (margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }

        /// <inheritdoc/>
        public override ModelBase DeepClone()
        {
            return new StackingModel(this);
        }

        //Static methods
        /// <summary>
        /// Builds a StackingModel.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public static StackingModel Build(IModelConfig cfg,
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
            if (cfg.GetType() != typeof(StackingModelConfig))
            {
                throw new ArgumentException($"Wrong type of configuration. Expected {typeof(StackingModelConfig)} but received {cfg.GetType()}.", nameof(cfg));
            }
            //Model instance
            StackingModelConfig modelConfig = (StackingModelConfig)cfg;
            StackingModel model = new StackingModel(modelConfig, (name + StackingModel.ContextPathID), taskType, outputFeatureNames, modelConfig.RouteInput);
            //Copy the data locally
            SampleDataset localDataset = trainingData.ShallowClone();
            //Shuffle local data
            localDataset.Shuffle(new Random(GetRandomSeed()));
            //Folderize local data
            List<SampleDataset> foldCollection = localDataset.Folderize(modelConfig.FoldDataRatio, taskType);
            //Weak networks
            //Array of weak networks
            NetworkModel[][] weakNetworks = new NetworkModel[modelConfig.StackCfg.NetworkModelCfgCollection.Count][];
            for (int i = 0; i < modelConfig.StackCfg.NetworkModelCfgCollection.Count; i++)
            {
                weakNetworks[i] = new NetworkModel[foldCollection.Count];
            }
            //Weak networks validation outputs storage
            double[][][][] stackNetsHoldOutOutputs = new double[foldCollection.Count][][][];
            for (int i = 0; i < foldCollection.Count; i++)
            {
                stackNetsHoldOutOutputs[i] = new double[modelConfig.StackCfg.NetworkModelCfgCollection.Count][][];
            }
            //Weak networks cumulated training err statistics
            ModelErrStat[] weakNetErrStats = new ModelErrStat[modelConfig.StackCfg.NetworkModelCfgCollection.Count];
            for (int i = 0; i < modelConfig.StackCfg.NetworkModelCfgCollection.Count; i++)
            {
                weakNetErrStats[i] = new ModelErrStat(taskType, outputFeatureNames);
            }
            NetworkModel[] strongNetworks = new NetworkModel[modelConfig.StackCfg.NetworkModelCfgCollection.Count];
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
                for (int stackNetIdx = 0; stackNetIdx < modelConfig.StackCfg.NetworkModelCfgCollection.Count; stackNetIdx++)
                {
                    string netNumStr = (stackNetIdx + 1).ToLeftPaddedString(modelConfig.StackCfg.NetworkModelCfgCollection.Count, '0');
                    //Build weak network
                    NetworkModel weakNetwork =
                        NetworkModel.Build(modelConfig.StackCfg.NetworkModelCfgCollection[stackNetIdx],
                                           $"{model.Name}.{holdOutFoldNumStr}-Weak{netNumStr}-",
                                           taskType,
                                           outputFeatureNames,
                                           weakNetTrainingData,
                                           null,
                                           progressInfoSubscriber
                                           );
                    weakNetworks[stackNetIdx][holdOutFoldIdx] = weakNetwork;
                    weakNetErrStats[stackNetIdx].Merge(weakNetwork.TrainingErrorStat);
                    stackNetsHoldOutOutputs[holdOutFoldIdx][stackNetIdx] =
                        weakNetwork.ComputeSampleDataset(foldCollection[holdOutFoldIdx], out _);
                }//stackNetIdx
            }//holdOutFoldIdx
            //Build stack's strong networks on whole data and add them into the model's stack
            for (int stackNetIdx = 0; stackNetIdx < modelConfig.StackCfg.NetworkModelCfgCollection.Count; stackNetIdx++)
            {
                string netNumStr = (stackNetIdx + 1).ToLeftPaddedString(modelConfig.StackCfg.NetworkModelCfgCollection.Count, '0');
                //Build strong network
                NetworkModel strongNetwork =
                    NetworkModel.Build(modelConfig.StackCfg.NetworkModelCfgCollection[stackNetIdx],
                                       $"{model.Name}.Strong{netNumStr}-",
                                       taskType,
                                       outputFeatureNames,
                                       localDataset,
                                       null,
                                       progressInfoSubscriber
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
                    double[][] stackNetsOutputs = new double[modelConfig.StackCfg.NetworkModelCfgCollection.Count][];
                    for (int netIdx = 0; netIdx < modelConfig.StackCfg.NetworkModelCfgCollection.Count; netIdx++)
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
                                   modelConfig.RouteInput ? (double[])foldCollection[foldIdx].SampleCollection[sampleIdx].InputVector.Concat(stackNetsOutputs.Flattenize()) : stackNetsOutputs.Flattenize(),
                                   foldCollection[foldIdx].SampleCollection[sampleIdx].OutputVector
                                   );
                }//sampleIdx
            });//foldIdx
            SampleDataset metaLearnerTrainingData = new SampleDataset(metaLearnerTrainingDataArray);
            //Build a meta-learner
            Type metaLearnerCfgType = modelConfig.MetaLearnerCfg.GetType();
            string metaLearnerModelStr = $"{model.Name}.Meta-Learner-";
            ModelBase metaLearnerModel = null;
            if (metaLearnerCfgType == typeof(NetworkModelConfig))
            {
                metaLearnerModel = NetworkModel.Build(modelConfig.MetaLearnerCfg,
                                                      metaLearnerModelStr,
                                                      taskType,
                                                      outputFeatureNames,
                                                      metaLearnerTrainingData,
                                                      null,
                                                      progressInfoSubscriber
                                                      );
            }
            else if (metaLearnerCfgType == typeof(CrossValModelConfig))
            {
                metaLearnerModel = CrossValModel.Build(modelConfig.MetaLearnerCfg,
                                                       metaLearnerModelStr,
                                                       taskType,
                                                       outputFeatureNames,
                                                       metaLearnerTrainingData,
                                                       progressInfoSubscriber
                                                       );
            }
            else if (metaLearnerCfgType == typeof(StackingModelConfig))
            {
                metaLearnerModel = StackingModel.Build(modelConfig.MetaLearnerCfg,
                                                       metaLearnerModelStr,
                                                       taskType,
                                                       outputFeatureNames,
                                                       metaLearnerTrainingData,
                                                       progressInfoSubscriber
                                                       );
            }
            else if (metaLearnerCfgType == typeof(CompositeModelConfig))
            {
                metaLearnerModel = CompositeModel.Build(modelConfig.MetaLearnerCfg,
                                                        metaLearnerModelStr,
                                                        taskType,
                                                        outputFeatureNames,
                                                        metaLearnerTrainingData,
                                                        progressInfoSubscriber
                                                        );
            }
            //Set model operationable
            model.SetOperationable(metaLearnerModel);
            //Return built model
            return model;
        }



    }//StackingModel

}//Namespace
