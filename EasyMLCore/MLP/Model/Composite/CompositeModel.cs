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
    /// Implements a model of aggregated child models where
    /// child model can be any model (including other composite models).
    /// Model output is weighted average of root child models outputs.
    /// </summary>
    [Serializable]
    public class CompositeModel : MLPModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "Composite";

        //Attributes
        private readonly List<MLPModelBase> _members;
        private double[][] _weights;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="modelConfig">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        private CompositeModel(CompositeModelConfig modelConfig,
                               string name,
                               OutputTaskType taskType,
                               IEnumerable<string> outputFeatureNames
                               )
            : base(modelConfig, name, taskType, outputFeatureNames)
        {
            _members = new List<MLPModelBase>();
            _weights = null;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CompositeModel(CompositeModel source)
            : base(source)
        {
            _members = new List<MLPModelBase>();
            foreach (MLPModelBase member in source._members)
            {
                _members.Add(member.DeepClone());
            }
            _weights = (double[][])source._weights.Clone();
            return;
        }

        //Methods
        /// <summary>
        /// Adds a new member.
        /// </summary>
        /// <param name="newMember">A new member to be added.</param>
        private void AddMember(MLPModelBase newMember)
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
            //Add new member
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
                throw new InvalidOperationException("At least one member must be added before the finalization.");
            }
            //Set weights
            _weights = GetWeights(_members);
            //Set metrics
            //Finalize model
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
            foreach(MLPModelBase model in _members)
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
            return new CompositeModel(this);
        }

        //Static methods
        /// <summary>
        /// Builds a CompositeModel.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public static CompositeModel Build(IModelConfig cfg,
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
            if (cfg.GetType() != typeof(CompositeModelConfig))
            {
                throw new ArgumentException($"Wrong type of configuration. Expected {typeof(CompositeModelConfig)} but received {cfg.GetType()}.", nameof(cfg));
            }
            //Composite model
            CompositeModelConfig modelConfig = (CompositeModelConfig)cfg;
            CompositeModel model = new CompositeModel(modelConfig,
                                                      (name + CompositeModel.ContextPathID),
                                                      taskType,
                                                      outputFeatureNames
                                                      );
            //Sub models build
            for (int subModelIdx = 0; subModelIdx < modelConfig.SubModelCfgCollection.Count; subModelIdx++)
            {
                string subModelNum = "M" + (subModelIdx + 1).ToLeftPaddedString(modelConfig.SubModelCfgCollection.Count, '0');
                Type subModelCfgType = modelConfig.SubModelCfgCollection[subModelIdx].GetType();
                string subModelName = $"{model.Name}.{subModelNum}-";
                if (subModelCfgType == typeof(NetworkModelConfig))
                {
                    NetworkModel subModel =
                        NetworkModel.Build(modelConfig.SubModelCfgCollection[subModelIdx],
                                           subModelName,
                                           taskType,
                                           outputFeatureNames,
                                           trainingData,
                                           null,
                                           progressInfoSubscriber
                                           );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(CrossValModelConfig))
                {
                    CrossValModel subModel =
                        CrossValModel.Build(modelConfig.SubModelCfgCollection[subModelIdx],
                                            subModelName,
                                            taskType,
                                            outputFeatureNames,
                                            trainingData,
                                            progressInfoSubscriber
                                            );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(StackingModelConfig))
                {
                    StackingModel subModel =
                        StackingModel.Build(modelConfig.SubModelCfgCollection[subModelIdx],
                                            subModelName,
                                            taskType,
                                            outputFeatureNames,
                                            trainingData,
                                            progressInfoSubscriber
                                            );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(BHSModelConfig))
                {
                    BHSModel subModel =
                        BHSModel.Build(modelConfig.SubModelCfgCollection[subModelIdx],
                                       subModelName,
                                       taskType,
                                       outputFeatureNames,
                                       trainingData,
                                       progressInfoSubscriber
                                       );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(RVFLModelConfig))
                {
                    RVFLModel subModel =
                        RVFLModel.Build(modelConfig.SubModelCfgCollection[subModelIdx],
                                        subModelName,
                                        taskType,
                                        outputFeatureNames,
                                        trainingData,
                                        out _,
                                        progressInfoSubscriber
                                        );
                    model.AddMember(subModel);
                }
                else if (subModelCfgType == typeof(CompositeModelConfig))
                {
                    CompositeModel subModel =
                        CompositeModel.Build(modelConfig.SubModelCfgCollection[subModelIdx],
                                             subModelName,
                                             taskType,
                                             outputFeatureNames,
                                             trainingData,
                                             progressInfoSubscriber
                                             );
                    model.AddMember(subModel);
                }
            }//subModelIdx
            //Set model operationable
            model.SetOperationable();
            //Return built model
            return model;
        }



    }//CompositeModel

}//Namespace
