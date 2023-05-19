using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MLP.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the model which baggs N inner half-stacking models.
    /// Model output is weighted average of inner half-stacking models outputs (bagging).
    /// </summary>
    [Serializable]
    public class BHSModel : ModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "BHSM";

        //Attributes
        private readonly List<HSModel> _members;
        private double[][] _weights;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="modelConfig">Model configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        private BHSModel(BHSModelConfig modelConfig,
                         string name,
                         OutputTaskType taskType,
                         IEnumerable<string> outputFeatureNames
                         )
            : base(modelConfig, name, taskType, outputFeatureNames)
        {
            _members = new List<HSModel>();
            _weights = null;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public BHSModel(BHSModel source)
            : base(source)
        {
            _members = new List<HSModel>(source._members.Count);
            foreach (HSModel model in source._members)
            {
                _members.Add((HSModel)model.DeepClone());
            }
            _weights = (double[][])source._weights.Clone();
            return;
        }

        //Methods
        /// <summary>
        /// Adds a new member HS model.
        /// </summary>
        /// <param name="newMember">A new member HS model to be added.</param>
        private void AddMember(HSModel newMember)
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
                throw new InvalidOperationException("At least one member must be added before the finalization.");
            }
            //Set weights
            _weights = GetWeights(_members.ToList<ModelBase>());
            //Set metrics
            FinalizeModel(new ModelConfidenceMetrics(TaskType, (from member in _members select member.ConfidenceMetrics)));
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
        public override ModelDiagnosticData DiagnosticTest(SampleDataset testingData, ModelTestProgressChangedHandler progressInfoSubscriber = null)
        {
            ModelErrStat errStat = Test(testingData, out _, progressInfoSubscriber);
            ModelDiagnosticData diagData = new ModelDiagnosticData(Name, errStat);
            foreach (ModelBase model in _members)
            {
                ModelDiagnosticData memberDiagData = model.DiagnosticTest(testingData, progressInfoSubscriber);
                diagData.AddSubModelDiagData(memberDiagData);
            }
            diagData.SetFinalized();
            return diagData;
        }

        /// <inheritdoc/>
        public override ModelBase DeepClone()
        {
            return new BHSModel(this);
        }

        //Static methods
        /// <summary>
        /// Builds a BHSModel.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public static BHSModel Build(IModelConfig cfg,
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
            if (cfg.GetType() != typeof(BHSModelConfig))
            {
                throw new ArgumentException($"Wrong type of configuration. Expected {typeof(BHSModelConfig)} but received {cfg.GetType()}.", nameof(cfg));
            }
            SampleDataset localDataset = trainingData.ShallowClone();
            //Model
            BHSModelConfig modelConfig = cfg as BHSModelConfig;
            BHSModel model = new BHSModel(modelConfig,
                                          (name + BHSModel.ContextPathID),
                                          taskType,
                                          outputFeatureNames
                                          );
            //Build members
            for(int repetition = 1; repetition <= modelConfig.Repetitions; repetition++)
            {
                string repetitionNumStr = "R" + (repetition).ToLeftPaddedString(modelConfig.Repetitions, '0');
                //Reshuffle local data
                localDataset.Shuffle(new Random(GetRandomSeed()));
                //Split data to two folds
                List<SampleDataset> foldCollection = localDataset.Folderize(0.5d, taskType);
                //Build H1 HSModel
                HSModel h1Model = HSModel.Build(modelConfig.HSModelCfg,
                                                $"{model.Name}.{repetitionNumStr}-H1-",
                                                taskType,
                                                outputFeatureNames,
                                                foldCollection[0],
                                                foldCollection[1],
                                                progressInfoSubscriber
                                                );
                model.AddMember(h1Model);
                //Build H2 HSModel
                HSModel h2Model = HSModel.Build(modelConfig.HSModelCfg,
                                                $"{model.Name}.{repetitionNumStr}-H2-",
                                                taskType,
                                                outputFeatureNames,
                                                foldCollection[1],
                                                foldCollection[0],
                                                progressInfoSubscriber
                                                );
                model.AddMember(h2Model);
            }
            //Set model operationable
            model.SetOperationable();
            //Return built model
            return model;
        }



    }//BHSModel

}//Namespace
