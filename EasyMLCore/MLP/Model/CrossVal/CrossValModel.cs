using EasyMLCore.Data;
using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the x-fold cross validated networks model.
    /// Model output is weighted average of inner networks models outputs (bagging).
    /// </summary>
    [Serializable]
    public class CrossValModel : MLPModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "CVM";

        //Attributes
        private readonly List<NetworkModel> _members;
        private double[][] _weights;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="modelConfig">Model configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        private CrossValModel(CrossValModelConfig modelConfig,
                              string name,
                              OutputTaskType taskType,
                              IEnumerable<string> outputFeatureNames
                              )
            : base(modelConfig, name, taskType, outputFeatureNames)
        {
            _members = new List<NetworkModel>();
            _weights = null;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CrossValModel(CrossValModel source)
            : base(source)
        {
            _members = new List<NetworkModel>(source._members.Count);
            foreach (NetworkModel networkModel in source._members)
            {
                _members.Add((NetworkModel)networkModel.DeepClone());
            }
            _weights = (double[][])source._weights.Clone();
            return;
        }

        //Methods
        /// <summary>
        /// Adds a new member Network model.
        /// </summary>
        /// <param name="newMember">A new member Network model to be added.</param>
        private void AddMember(NetworkModel newMember)
        {
            //Checks
            if (newMember.NumOfOutputFeatures != NumOfOutputFeatures)
            {
                throw new ArgumentException("Different number of new member outputs.", nameof(newMember));
            }
            if (newMember.TaskType != TaskType)
            {
                throw new ArgumentException("Different output task of new member.", nameof(newMember));
            }
            //Add new member to inner ensemble
            _members.Add(newMember);
            return;
        }

        /// <summary>
        /// Sets the model operationable.
        /// </summary>
        private void SetOperationable()
        {
            //Checks
            if (_members.Count < 1)
            {
                throw new InvalidOperationException("At least one member Network must be added before the finalization.");
            }
            //Set weights
            _weights = GetWeights(_members.ToList<MLPModelBase>());
            //Set metrics
            FinalizeModel(new MLPModelConfidenceMetrics(TaskType, (from member in _members select member.ConfidenceMetrics)));
            return;
        }

        /// <summary>
        /// Computes outputs of all members.
        /// </summary>
        /// <param name="inputVector">Input vector.</param>
        private List<double[]> ComputeMembers(double[] inputVector)
        {
            List<double[]> outputVectors = new List<double[]>(_members.Count);
            for (int memberIdx = 0; memberIdx < _members.Count; memberIdx++)
            {
                outputVectors.Add(_members[memberIdx].Compute(inputVector));
            }
            return outputVectors;
        }

        /// <inheritdoc/>
        public override double[] Compute(double[] input)
        {
            return ComputeAggregation(ComputeMembers(input), _weights);
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
            sb.Append($"    Number of member models: {_members.Count.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if (detail)
            {
                sb.Append($"    Inner models one by one >>>{Environment.NewLine}");
                for (int i = 0; i < _members.Count; i++)
                {
                    sb.Append(_members[i].GetInfoText(detail, 8));
                }
            }
            string infoText = sb.ToString();
            if (margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }

        /// <inheritdoc/>
        public override MLPModelDiagnosticData DiagnosticTest(SampleDataset testingData, ProgressChangedHandler progressInfoSubscriber = null)
        {
            MLPModelErrStat errStat = Test(testingData, out _, progressInfoSubscriber);
            MLPModelDiagnosticData diagData = new MLPModelDiagnosticData(Name, errStat);
            foreach (MLPModelBase model in _members)
            {
                MLPModelDiagnosticData memberDiagData = model.DiagnosticTest(testingData, progressInfoSubscriber);
                diagData.AddSubModelDiagData(memberDiagData);
            }
            diagData.SetFinalized();
            return diagData;
        }

        /// <inheritdoc/>
        public override MLPModelBase DeepClone()
        {
            return new CrossValModel(this);
        }

        //Static methods
        /// <summary>
        /// Builds a CrossValModel.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public static CrossValModel Build(IModelConfig cfg,
                                          string name,
                                          OutputTaskType taskType,
                                          List<string> outputFeatureNames,
                                          SampleDataset trainingData,
                                          ProgressChangedHandler progressInfoSubscriber = null
                                          )
        {
            //Checks
            if (cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }
            if (cfg.GetType() != typeof(CrossValModelConfig))
            {
                throw new ArgumentException($"Wrong type of configuration. Expected {typeof(CrossValModelConfig)} but received {cfg.GetType()}.", nameof(cfg));
            }
            SampleDataset localDataset = trainingData.ShallowClone();
            //Model
            CrossValModelConfig modelConfig = cfg as CrossValModelConfig;
            CrossValModel model = new CrossValModel(modelConfig,
                                                    (name + CrossValModel.ContextPathID),
                                                    taskType,
                                                    outputFeatureNames
                                                    );
            //Reshuffle local data
            localDataset.Shuffle(new Random(GetRandomSeed()));
            //Split data to folds
            List<SampleDataset> foldCollection = localDataset.Folderize(modelConfig.FoldDataRatio, taskType);
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
                //Build network
                NetworkModel network =
                    NetworkModel.Build(modelConfig.NetworkModelCfg,
                                       $"{model.Name}.{validationFoldNumStr}-",
                                       taskType,
                                       outputFeatureNames,
                                       nodeTrainingData,
                                       foldCollection[validationFoldIdx],
                                       progressInfoSubscriber
                                       );
                //Add network into the model
                model.AddMember(network);
            }//validationFoldIdx
            //Set model operationable
            model.SetOperationable();
            //Return built model
            return model;
        }



    }//CrossValModel

}//Namespace
