using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements a MLP network model encapsulating the core MLP engine.
    /// </summary>
    [Serializable]
    public class NetworkModel : MLPModelBase
    {
        //Static params
        /// <summary>
        /// Specifies whether to try additional fine tuning when achieved 100% accuracy
        /// </summary>
        private static readonly bool EnabledFineTuning = true;

        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "MLP";

        /// <summary>
        /// Default training total RMSE treshold.
        /// </summary>
        private const double RMSETreshold = 1E-6d;
        
        //Attribute properties
        /// <summary>
        /// MLP network engine.
        /// </summary>
        public MLPEngine Engine { get; }

        /// <summary>
        /// Error statistics of the network on training data.
        /// </summary>
        public MLPModelErrStat TrainingErrorStat { get; }

        /// <summary>
        /// Error statistics of the network on validation data.
        /// </summary>
        public MLPModelErrStat ValidationErrorStat { get; }

        //Attributes
        private readonly FeatureFilterBase[] _inputFilters;
        private readonly FeatureFilterBase[] _outputFilters;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="modelConfig">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="outputFeatureNames">A collection of output feature names.</param>
        /// <param name="engine">MLP network engine.</param>
        /// <param name="inputFilters">Prepared input filters.</param>
        /// <param name="outputFilters">Prepared output filters.</param>
        /// <param name="trainingErrStat">Error statistics from the training.</param>
        /// <param name="validationData">Validation dataset (can be null).</param>
        private NetworkModel(NetworkModelConfig modelConfig,
                             string name,
                             IEnumerable<string> outputFeatureNames,
                             MLPEngine engine,
                             FeatureFilterBase[] inputFilters,
                             FeatureFilterBase[] outputFilters,
                             MLPModelErrStat trainingErrStat,
                             SampleDataset validationData
                             )
            : base(modelConfig, name, engine.TaskType, outputFeatureNames)
        {
            if (OutputFeatureNames.Count != engine.NumOfOutputFeatures)
            {
                throw new ArgumentException("Number of feature names differs from number of network's output features.", nameof(outputFeatureNames));
            }
            Engine = engine.DeepClone();
            _inputFilters = new FeatureFilterBase[inputFilters.Length];
            for (int i = 0; i < _inputFilters.Length; i++)
            {
                _inputFilters[i] = inputFilters[i].DeepClone();
            }
            _outputFilters = new FeatureFilterBase[outputFilters.Length];
            for (int i = 0; i < _outputFilters.Length; i++)
            {
                _outputFilters[i] = outputFilters[i].DeepClone();
            }
            //Training error statistics
            TrainingErrorStat = trainingErrStat.DeepClone();
            //Validation data and error statistics
            if (validationData != null)
            {
                ValidationErrorStat = ComputeBatchErrStat(validationData);
            }
            else
            {
                ValidationErrorStat = null;
            }
            //Finalize model
            FinalizeModel(new MLPModelConfidenceMetrics(TrainingErrorStat, ValidationErrorStat));
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NetworkModel(NetworkModel source)
            : base(source)
        {
            Engine = source.Engine.DeepClone();
            _inputFilters = new FeatureFilterBase[source._inputFilters.Length];
            for (int i = 0; i < _inputFilters.Length; i++)
            {
                _inputFilters[i] = source._inputFilters[i].DeepClone();
            }
            _outputFilters = new FeatureFilterBase[source._outputFilters.Length];
            for (int i = 0; i < _outputFilters.Length; i++)
            {
                _outputFilters[i] = source._outputFilters[i].DeepClone();
            }
            TrainingErrorStat = source.TrainingErrorStat.DeepClone();
            ValidationErrorStat = source.ValidationErrorStat?.DeepClone();
            return;
        }

        //Static methods
        /// <summary>
        /// Compares two network models.
        /// </summary>
        /// <param name="model1">Network model 1.</param>
        /// <param name="model2">Network model 2.</param>
        /// <param name="trainingStatOnly">Specifies whether to consider only training results or also validation results (if available).</param>
        /// <returns>-1 if model1 is better, 1 if model2 is better, 0 if no difference.</returns>
        public bool IsBetter(NetworkModel otherModel, bool trainingStatOnly = false)
        {
            if (trainingStatOnly)
            {
                return TrainingErrorStat.IsBetter(otherModel.TrainingErrorStat);
            }
            else
            {
                return MLPModelConfidenceMetrics.Comparer(ConfidenceMetrics,
                                                       otherModel.ConfidenceMetrics
                                                       ) == 1;
            }
        }

        //Methods
        /// <summary>
        /// Version for single computation.
        /// </summary>
        private double[] Normalize(double[] natVector)
        {
            double[] normVector = new double[natVector.Length];
            for (int i = 0; i < natVector.Length; i ++)
            {
                normVector[i] = _inputFilters[i].ApplyFilter(natVector[i], MLPEngine.UseCenteredFeatures);
            }
            return normVector;
        }

        /// <summary>
        /// Version for multiple computations.
        /// </summary>
        private void Normalize(double[] natVector, double[] normVector)
        {
            for (int i = 0; i < natVector.Length; i++)
            {
                normVector[i] = _inputFilters[i].ApplyFilter(natVector[i], MLPEngine.UseCenteredFeatures);
            }
            return;
        }

        /// <summary>
        /// Version for single computation.
        /// </summary>
        private double[] Naturalize(double[] vector)
        {
            double[] nVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                nVector[i] = _outputFilters[i].ApplyReverse(vector[i], MLPEngine.UseCenteredFeatures);
            }
            return nVector;
        }

        /// <summary>
        /// Version for multiple computations.
        /// </summary>
        private void Naturalize(double[] activations, int aIdx, double[] natVector)
        {
            for (int i = 0; i < natVector.Length; i++, aIdx++)
            {
                natVector[i] = _outputFilters[i].ApplyReverse(activations[aIdx], MLPEngine.UseCenteredFeatures);
            }
            return;
        }

        /// <inheritdoc/>
        public override double[] Compute(double[] input)
        {
            return Naturalize(Engine.Compute(Normalize(input)));
        }

        /// <inheritdoc/>
        public override MLPModelBase DeepClone()
        {
            return new NetworkModel(this);
        }

        /// <summary>
        /// Computes given sample dataset.
        /// </summary>
        /// <param name="dataset">Sample dataset to be computed.</param>
        /// <param name="errStat">Resulting error statistics.</param>
        /// <returns>Computed vectors in the same order as in sample dataset.</returns>
        public double[][] ComputeSampleDataset(SampleDataset dataset, out MLPModelErrStat errStat)
        {
            double[][] computedVectors = new double[dataset.Count][];
            MLPModelErrStat batchErrStat = new MLPModelErrStat(TaskType, OutputFeatureNames);
            object monitor = new object();
            Parallel.ForEach(Partitioner.Create(0, dataset.Count), range =>
            {
                MLPModelErrStat rangeStat = new MLPModelErrStat(Engine.TaskType, OutputFeatureNames);
                //Reusable buffers
                double[] sums = new double[Engine.NumOfNeurons];
                double[] activations = new double[Engine.NumOfInputFeatures + Engine.NumOfNeurons];
                double[] computed = new double[Engine.NumOfOutputFeatures];
                //Worker thread loop
                for (int sampleIdx = range.Item1; sampleIdx < range.Item2; sampleIdx++)
                {
                    //Input
                    Normalize(dataset.SampleCollection[sampleIdx].InputVector, activations);
                    //Compute
                    int aOutIdx = Engine.Compute(activations, sums);
                    //Output
                    Naturalize(activations, aOutIdx, computed);
                    computedVectors[sampleIdx] = (double[])computed.Clone();
                    //Update stat
                    rangeStat.Update(computed, dataset.SampleCollection[sampleIdx].OutputVector);
                }
                lock (monitor)
                {
                    batchErrStat.Merge(rangeStat);
                }
            });
            errStat = batchErrStat;
            return computedVectors;
        }

        private MLPModelErrStat ComputeBatchErrStat(SampleDataset dataset)
        {
            MLPModelErrStat batchErrStat = new MLPModelErrStat(TaskType, OutputFeatureNames);
            object monitor = new object();
            Parallel.ForEach(Partitioner.Create(0, dataset.Count), range =>
            {
                MLPModelErrStat rangeStat = new MLPModelErrStat(Engine.TaskType, OutputFeatureNames);
                //Reusable buffers
                double[] sums = new double[Engine.NumOfNeurons];
                double[] activations = new double[Engine.NumOfInputFeatures + Engine.NumOfNeurons];
                double[] computed = new double[Engine.NumOfOutputFeatures];
                //Worker thread loop
                for (int sampleIdx = range.Item1; sampleIdx < range.Item2; sampleIdx++)
                {
                    //Input
                    Normalize(dataset.SampleCollection[sampleIdx].InputVector, activations);
                    //Compute
                    int aOutIdx = Engine.Compute(activations, sums);
                    //Output
                    Naturalize(activations, aOutIdx, computed);
                    //Update stat
                    rangeStat.Update(computed, dataset.SampleCollection[sampleIdx].OutputVector);
                }
                lock (monitor)
                {
                    batchErrStat.Merge(rangeStat);
                }
            });
            return batchErrStat;
        }

        public override string GetInfoText(bool detail = false, int margin = 0)
        {
            margin = Math.Max(margin, 0);
            StringBuilder sb = new StringBuilder($"{Name} [{GetType()}]{Environment.NewLine}");
            sb.Append($"    Task type               : {Engine.TaskType.ToString()}{Environment.NewLine}");
            sb.Append($"    Number of input features: {Engine.NumOfInputFeatures.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Output features info    : {Engine.NumOfOutputFeatures.ToString(CultureInfo.InvariantCulture)}");
            int fIdx = 0;
            foreach(string outputFeatureName in OutputFeatureNames)
            {
                sb.Append($" [{outputFeatureName}, {ConfidenceMetrics.FeatureConfidences[fIdx++].ToString("F3", CultureInfo.InvariantCulture)}]");
            }
            sb.Append(Environment.NewLine);
            sb.Append($"    Total number of neurons : {Engine.NumOfNeurons.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Number of layers        : {Engine.LayerCollection.Count.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if(detail)
            {
                sb.Append($"    Layers one by one >>>{Environment.NewLine}");
                for(int i = 0; i < Engine.LayerCollection.Count; i++)
                {
                    string layerName = (i == Engine.LayerCollection.Count - 1) ? "Output layer" : "Hidden layer";
                    sb.Append($"        {layerName}{Environment.NewLine}");
                    sb.Append($"            Number of neurons: {Engine.LayerCollection[i].NumOfLayerNeurons.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
                    sb.Append($"            Activation       : {Engine.LayerCollection[i].Activation.ID.ToString()}{Environment.NewLine}");
                    sb.Append($"            Number of weights: {(Engine.LayerCollection[i].NumOfInputNodes * Engine.LayerCollection[i].NumOfLayerNeurons + Engine.LayerCollection[i].NumOfLayerNeurons).ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");

                }
            }
            sb.Append($"    Total number of weights : {Engine.NumOfWeights.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if(detail)
            {
                if (Engine.LayerCollection.Count > 1)
                {
                    sb.Append($"        Hidden weights ({Engine.HLWeightsStat.NumOfSamples.ToString(CultureInfo.InvariantCulture)}){Environment.NewLine}");
                    sb.Append($"{Engine.HLWeightsStat.GetInfoText(12, BasicStat.StatisticalFigure.Min, BasicStat.StatisticalFigure.Max, BasicStat.StatisticalFigure.ArithAvg, BasicStat.StatisticalFigure.RootMeanSquare, BasicStat.StatisticalFigure.StdDev)}");
                }
                sb.Append($"        Output weights ({Engine.OLWeightsStat.NumOfSamples.ToString(CultureInfo.InvariantCulture)}){Environment.NewLine}");
                sb.Append($"{Engine.OLWeightsStat.GetInfoText(12, BasicStat.StatisticalFigure.Min, BasicStat.StatisticalFigure.Max, BasicStat.StatisticalFigure.ArithAvg, BasicStat.StatisticalFigure.RootMeanSquare, BasicStat.StatisticalFigure.StdDev)}");
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
            diagData.SetFinalized();
            return diagData;
        }

        //Static methods
        /// <summary>
        /// Builds a NetworkModel.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="validationData">Validation samples (can be null).</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built network model.</returns>
        public static NetworkModel Build(IModelConfig cfg,
                                         string name,
                                         OutputTaskType taskType,
                                         IEnumerable<string> outputFeatureNames,
                                         SampleDataset trainingData,
                                         SampleDataset validationData,
                                         ProgressChangedHandler progressInfoSubscriber = null
                                        )
        {
            //Checks
            if(cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }
            if(cfg.GetType() != typeof(NetworkModelConfig))
            {
                throw new ArgumentException($"Wrong type of configuration. Expected {typeof(NetworkModelConfig)} but received {cfg.GetType()}.", nameof(cfg));
            }
            ProgressChangedEventDisp eventDisp = new ProgressChangedEventDisp();
            if (progressInfoSubscriber != null)
            {
                eventDisp.ProgressChanged += progressInfoSubscriber;
            }
            //Build
            try
            {
                NetworkModelConfig modelConfig = (NetworkModelConfig)cfg;
                bool engageValidationData = validationData != null;
                Random rand = new Random(GetRandomSeed());
                NetworkModel bestNet = null;
                int bestNetAttempt = 0;
                int bestNetAttemptEpoch = 0;
                NetworkModel lastImprovementNet = null;
                int lastImprovementEpoch = 0;
                bool inFineTunePhase = false;
                //Create network engine and trainer
                //Network engine
                MLPEngine engine = new MLPEngine(taskType, trainingData.FirstInputVectorLength, outputFeatureNames, modelConfig);
                //Trainer
                Trainer trainer = new Trainer(modelConfig, engine, trainingData, rand);
                //Iterate training cycles
                while (trainer.Epoch())
                {
                    //Create current network instance and compute error statistics after training iteration
                    NetworkModel currNet = new NetworkModel(modelConfig,
                                                            (name + NetworkModel.ContextPathID),
                                                            outputFeatureNames,
                                                            engine,
                                                            trainer.InputFilters,
                                                            trainer.OutputFilters,
                                                            trainer.EpochErrStat,
                                                            validationData
                                                            );
                    //Initialization of the best network
                    if (bestNet == null)
                    {
                        bestNet = (NetworkModel)currNet.DeepClone();
                        bestNetAttempt = trainer.Attempt;
                    }
                    //Reset attempt scope variables when new training attempt starts
                    if (trainer.AttemptEpoch == 1)
                    {
                        lastImprovementEpoch = 0;
                        lastImprovementNet = null;
                        inFineTunePhase = false;
                    }
                    //Update the last improvement point
                    if (lastImprovementNet == null || lastImprovementNet.IsBetter(currNet, !engageValidationData))
                    {
                        lastImprovementNet = currNet;
                        lastImprovementEpoch = trainer.AttemptEpoch;
                    }
                    //Stop all attempts?
                    bool stopAllAttempts = false;
                    //Is current network better than the best network so far?
                    if (bestNet.IsBetter(currNet, !engageValidationData))
                    {
                        //Adopt current network as the best one
                        bestNet = (NetworkModel)currNet.DeepClone();
                        bestNetAttempt = trainer.Attempt;
                        bestNetAttemptEpoch = trainer.AttemptEpoch;
                        //Entering the fine tune phase?
                        if (engageValidationData)
                        {
                            if (EnabledFineTuning)
                            {
                                inFineTunePhase = (taskType != OutputTaskType.Regression && bestNet.ConfidenceMetrics.BinaryAccuracy == 1d);
                            }
                            else
                            {
                                stopAllAttempts |= (taskType != OutputTaskType.Regression && bestNet.ConfidenceMetrics.BinaryAccuracy == 1d);
                            }
                        }
                    }
                    else
                    {
                        stopAllAttempts |= engageValidationData && inFineTunePhase;
                    }
                    stopAllAttempts |= inFineTunePhase && trainer.AttemptEpoch == trainer.MaxAttemptEpochs;
                    if (!stopAllAttempts && !engageValidationData)
                    {
                        //Stop all attempts when accuracy on training data reaches 100%
                        stopAllAttempts = (taskType != OutputTaskType.Regression && ((MultipleDecisionErrStat)currNet.TrainingErrorStat.StatData).BinaryAccuracy == 1d) ||
                                          (taskType == OutputTaskType.Regression && ((MultiplePrecisionErrStat)currNet.TrainingErrorStat.StatData).TotalPrecisionStat.RootMeanSquare < RMSETreshold);
                    }
                    //Stop current attempt?
                    bool stopCurrAttempt = stopAllAttempts;
                    if (!stopCurrAttempt)
                    {
                        //Stop current training attempt when improvement patiency is over the limit
                        stopCurrAttempt |= (trainer.AttemptEpoch - lastImprovementEpoch >= trainer.MaxAttemptEpochs * modelConfig.StopAttemptPatiency);
                        stopCurrAttempt |= ((MultiplePrecisionErrStat)currNet.TrainingErrorStat.StatData).TotalPrecisionStat.RootMeanSquare < RMSETreshold;
                    }
                    //Progress info
                    NetworkBuildProgressInfo progressInfo =
                        new NetworkBuildProgressInfo(trainer.Attempt,
                                                     trainer.MaxAttempts,
                                                     trainer.AttemptEpoch,
                                                     trainer.MaxAttemptEpochs,
                                                     currNet,
                                                     bestNet,
                                                     bestNetAttempt,
                                                     bestNetAttemptEpoch,
                                                     stopCurrAttempt
                                                     );
                    //Raise notification event
                    eventDisp.InvokeBuildProgressChanged(new ModelBuildProgressInfo(currNet.Name, null, progressInfo));
                    //Stop?
                    if (stopAllAttempts)
                    {
                        break;
                    }
                    else if (stopCurrAttempt)
                    {
                        //Push trainer to next attempt
                        if (!trainer.NextAttempt())
                        {
                            //No next attempt available
                            break;
                        }
                    }
                }//while (trainer iteration)
                return bestNet;
            }
            finally
            {
                //Unsubscibe from event
                if (progressInfoSubscriber != null)
                {
                    eventDisp.ProgressChanged -= progressInfoSubscriber;
                }
            }
        }

        //Inner classes
        internal class ProgressChangedEventDisp
        {
            /// <summary>
            /// This informative event occurs each time the progress of the build process takes a step forward.
            /// </summary>
            [field: NonSerialized]
            internal event ProgressChangedHandler ProgressChanged;

            /// <summary>
            /// Invokes BuildProgressChanged.
            /// </summary>
            /// <param name="progressInfo">Progress info.</param>
            internal void InvokeBuildProgressChanged(ProgressInfoBase progressInfo)
            {
                ProgressChanged?.Invoke(progressInfo);
                return;
            }
        }//ProgressChangedEventDisp


    }//NetworkModel

}//Namespace
