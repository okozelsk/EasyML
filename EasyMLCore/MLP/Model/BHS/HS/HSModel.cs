using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MLP.Model;
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
    /// Model output is an output of trained meta-learner. Meta-Learner can be any kind of model except HSModel.
    /// This model serves as a part of BHS model and is not intended to be used for the final predictions.
    /// </summary>
    [Serializable]
    public class HSModel : ModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "HSM";

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
        private HSModel(HSModelConfig modelConfig,
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
        public HSModel(HSModel source)
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
        public override ModelDiagnosticData DiagnosticTest(SampleDataset testingData, ModelTestProgressChangedHandler progressInfoSubscriber = null)
        {
            ModelErrStat errStat = Test(testingData, out _, progressInfoSubscriber);
            ModelDiagnosticData diagData = new ModelDiagnosticData(Name, errStat);
            foreach (ModelBase model in _stack)
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
            return new HSModel(this);
        }

        //Static methods
        /// <summary>
        /// Builds a HSModel.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="validationData">Validation samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built model.</returns>
        public static HSModel Build(IModelConfig cfg,
                                    string name,
                                    OutputTaskType taskType,
                                    List<string> outputFeatureNames,
                                    SampleDataset trainingData,
                                    SampleDataset validationData,
                                    ModelBuildProgressChangedHandler progressInfoSubscriber = null
                                    )
        {
            //Checks
            if (cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }
            if (cfg.GetType() != typeof(HSModelConfig))
            {
                throw new ArgumentException($"Wrong type of configuration. Expected {typeof(HSModelConfig)} but received {cfg.GetType()}.", nameof(cfg));
            }
            //Model instance
            HSModelConfig modelConfig = (HSModelConfig)cfg;
            HSModel model = new HSModel(modelConfig, (name + HSModel.ContextPathID), taskType, outputFeatureNames, modelConfig.RouteInput);
            //Copy the data locally
            SampleDataset localTrainDataset = new SampleDataset(trainingData.Count);
            for (int sampleID = 0; sampleID < trainingData.Count; sampleID++)
            {
                localTrainDataset.AddSample(sampleID, trainingData.SampleCollection[sampleID].InputVector, trainingData.SampleCollection[sampleID].OutputVector);
            }
            SampleDataset localValDataset = new SampleDataset(validationData.Count);
            for (int sampleID = 0; sampleID < validationData.Count; sampleID++)
            {
                localValDataset.AddSample(sampleID, validationData.SampleCollection[sampleID].InputVector, validationData.SampleCollection[sampleID].OutputVector);
            }
            //Shuffle local data
            localTrainDataset.Shuffle(new Random(GetRandomSeed()));
            localValDataset.Shuffle(new Random(GetRandomSeed()));
            //Build stack's networks and prepare input data for meta model
            double[][][] weakNetsOutputs = new double[modelConfig.StackCfg.NetworkModelCfgCollection.Count][][];
            for (int stackNetIdx = 0; stackNetIdx < modelConfig.StackCfg.NetworkModelCfgCollection.Count; stackNetIdx++)
            {
                string netNumStr = (stackNetIdx + 1).ToLeftPaddedString(modelConfig.StackCfg.NetworkModelCfgCollection.Count, '0');
                //Build network
                NetworkModel stackNetwork =
                    NetworkModel.Build(modelConfig.StackCfg.NetworkModelCfgCollection[stackNetIdx],
                                        $"{model.Name}.StackNet{netNumStr}-",
                                        taskType,
                                        outputFeatureNames,
                                        localTrainDataset,
                                        localValDataset,
                                        progressInfoSubscriber
                                        );
                weakNetsOutputs[stackNetIdx] =
                    stackNetwork.ComputeSampleDataset(localValDataset, out _);
                model.AddStackMember(stackNetwork);
            }//stackNetIdx
            //Prepare data for meta-learner model
            SampleDataset metaLearnerTrainingData = new SampleDataset(localValDataset.Count);
            for (int sampleIdx = 0; sampleIdx < localValDataset.Count; sampleIdx++)
            {
                double[][] stackNetsOutputs = new double[modelConfig.StackCfg.NetworkModelCfgCollection.Count][];
                for (int netIdx = 0; netIdx < modelConfig.StackCfg.NetworkModelCfgCollection.Count; netIdx++)
                {
                    stackNetsOutputs[netIdx] = weakNetsOutputs[netIdx][sampleIdx];
                }//netIdx
                Sample sample = localValDataset.SampleCollection[sampleIdx];
                metaLearnerTrainingData.AddSample(sample.ID,
                                                  modelConfig.RouteInput ? (double[])sample.InputVector.Concat(stackNetsOutputs.Flattenize()) : stackNetsOutputs.Flattenize(),
                                                  sample.OutputVector
                                                  );
            }//sampleIdx
            SampleDataset metaLearnerValidationData = new SampleDataset(localTrainDataset.Count);
            for (int sampleIdx = 0; sampleIdx < localTrainDataset.Count; sampleIdx++)
            {
                double[][] stackNetsOutputs = new double[modelConfig.StackCfg.NetworkModelCfgCollection.Count][];
                for (int netIdx = 0; netIdx < modelConfig.StackCfg.NetworkModelCfgCollection.Count; netIdx++)
                {
                    stackNetsOutputs[netIdx] = model._stack[netIdx].Compute(localTrainDataset.SampleCollection[sampleIdx].InputVector);
                }//netIdx
                Sample sample = localTrainDataset.SampleCollection[sampleIdx];
                metaLearnerValidationData.AddSample(sample.ID,
                                                    modelConfig.RouteInput ? (double[])sample.InputVector.Concat(stackNetsOutputs.Flattenize()) : stackNetsOutputs.Flattenize(),
                                                    sample.OutputVector
                                                    );
            }//sampleIdx
            //Build the meta-learner
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
                                                      metaLearnerValidationData,
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
            else if (metaLearnerCfgType == typeof(BHSModelConfig))
            {
                metaLearnerModel = BHSModel.Build(modelConfig.MetaLearnerCfg,
                                                  metaLearnerModelStr,
                                                  taskType,
                                                  outputFeatureNames,
                                                  metaLearnerTrainingData,
                                                  progressInfoSubscriber
                                                  );
            }
            else if (metaLearnerCfgType == typeof(RVFLModelConfig))
            {
                metaLearnerModel = RVFLModel.Build(modelConfig.MetaLearnerCfg,
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


    }//HSModel

}//Namespace
