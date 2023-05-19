using EasyMLCore.Activation;
using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MiscTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the trainer of a MLP network engine.
    /// </summary>
    [Serializable]
    public class Trainer : SerializableObject
    {
        //Constants
        private const double MaxAcceptableWeightMagnitude = 1e10d;
        private const int NumOfTrainingSamplesIncrementingMiniBatchSize = 100;
        private const int OptimalMiniBatchMinSize = 32;
        private const int OptimalMiniBatchMaxSize = 128;

        //Attribute properties
        /// <summary>
        /// Input feature filters.
        /// </summary>
        public FeatureFilterBase[] InputFilters { get; }

        /// <summary>
        /// Output feature filters.
        /// </summary>
        public FeatureFilterBase[] OutputFilters { get; }

        /// <summary>
        /// Dataset of standardized training samples.
        /// </summary>
        public SampleDataset StdTrainingDataset { get; }

        /// <summary>
        /// Maximum number of training attempts.
        /// </summary>
        public int MaxAttempts { get; private set; }

        /// <summary>
        /// Current training attempt number.
        /// </summary>
        public int Attempt { get; private set; }

        /// <summary>
        /// Maximum number of training epochs within a training attempt.
        /// </summary>
        public int MaxAttemptEpochs { get; private set; }

        /// <summary>
        /// Current epoch number within the current training attempt.
        /// </summary>
        public int AttemptEpoch { get; private set; }

        /// <summary>
        /// Last epoch error statistics.
        /// </summary>
        public ModelErrStat EpochErrStat { get; private set; }


        //Attributes
        //Model config
        private readonly NetworkModelConfig _modelCfg;
        //Network engine
        private readonly MLPEngine _engine;
        //Original samples
        private readonly SampleDataset _orgTrainingDataset;
        //Shuffled standardized samples
        private readonly SampleDataset _shuffledDataset;
        //Main random generator
        private readonly Random _rand;
        //Network weights
        private readonly double[] _flatWeights;
        //Imbalances
        private readonly double[][] _outputImbalanceCoeffs;
        //Learning Throttle Valve
        private readonly ParamValMapper _learningThrottleValve;
        //Dropouts
        private readonly AFLinear _linearAF;
        private readonly DropoutMode[] _dropoutModes;
        private readonly double[] _dropoutPs;
        private readonly double[] _keepPs;
        private bool _dropoutActive;
        //L1 regularization
        private readonly double[] _regL1WLambdas;
        private readonly double[] _regL1BLambdas;
        //L2 regularization
        private readonly double[] _regL2WLambdas;
        private readonly double[] _regL2BLambdas;
        //Norm constraints
        private readonly double[] _minNorms;
        private readonly double[] _maxNorms;
        private readonly bool[] _normBiases;
        //Optimizer
        private readonly IOptimizer _optimizer;
        private int _batchSize;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="modelCfg">Configuration of the model of single MLP network.</param>
        /// <param name="engine">MLP network engine to be trained.</param>
        /// <param name="trainingDataset">Training samples.</param>
        /// <param name="rand">Random generator to be used.</param>
        public Trainer(NetworkModelConfig modelCfg,
                       MLPEngine engine,
                       SampleDataset trainingDataset,
                       Random rand
                       )
        {
            _modelCfg = modelCfg;
            MaxAttempts = _modelCfg.Attempts;
            MaxAttemptEpochs = _modelCfg.Epochs;
            _engine = engine;
            _rand = rand;
            //Data standardization
            _orgTrainingDataset = trainingDataset;
            StdTrainingDataset =
                _orgTrainingDataset.CreateStandardized(_engine.TaskType,
                                                      out FeatureFilterBase[] inputFilters,
                                                      out FeatureFilterBase[] outputFilters
                                                      );
            InputFilters = inputFilters;
            OutputFilters = outputFilters;
            //Shuffled dataset with its own IDs
            _shuffledDataset = new SampleDataset(StdTrainingDataset.Count);
            int sampleID = 0;
            foreach(Sample sample in StdTrainingDataset.SampleCollection)
            {
                _shuffledDataset.AddSample(sampleID, sample.InputVector, sample.OutputVector);
                ++sampleID;
            }
            //Network weights
            _flatWeights = new double[_engine.NumOfWeights];
            //Imbalances
            if (_engine.TaskType != OutputTaskType.Regression)
            {
                _outputImbalanceCoeffs = new double[2][];
                _outputImbalanceCoeffs[0] = new double[_engine.NumOfOutputFeatures];
                _outputImbalanceCoeffs[1] = new double[_engine.NumOfOutputFeatures];
                InitImbalances();
            }
            else
            {
                _outputImbalanceCoeffs = null;
            }

            //Learning Throttle Valve
            _learningThrottleValve = null;
            if(_modelCfg.LearningThrottleValveCfg.MinPermeability < 1d)
            {
                _learningThrottleValve =
                    new ParamValMapper(1,
                                       _modelCfg.Epochs * _modelCfg.LearningThrottleValveCfg.LastThrottlingEpochRatio,
                                       1d,
                                       _modelCfg.LearningThrottleValveCfg.MinPermeability,
                                       _modelCfg.LearningThrottleValveCfg.MinPermeability,
                                       _modelCfg.LearningThrottleValveCfg.ThrottlingSlope
                                       );
            }
            //Dropouts
            _linearAF = new AFLinear();
            _dropoutModes = new DropoutMode[1 + _modelCfg.HiddenLayersCfg.LayerCfgCollection.Count];
            _dropoutPs = new double[1 + _modelCfg.HiddenLayersCfg.LayerCfgCollection.Count];
            _keepPs = new double[1 + _modelCfg.HiddenLayersCfg.LayerCfgCollection.Count];
            InitDropout();
            //L1  and L2 regularizations
            _regL1WLambdas = new double[_engine.LayerCollection.Count];
            _regL1BLambdas = new double[_engine.LayerCollection.Count];
            _regL2WLambdas = new double[_engine.LayerCollection.Count];
            _regL2BLambdas = new double[_engine.LayerCollection.Count];
            InitL12Regularizations();
            //Norm constraints
            _minNorms = new double[_engine.LayerCollection.Count];
            _maxNorms = new double[_engine.LayerCollection.Count];
            _normBiases = new bool[_engine.LayerCollection.Count];
            InitNormConstraints();
            //Optimizer
            _optimizer = CreateOptimizer(_modelCfg.OptimizerCfg.OptimizerID);
            InitBatchSize();
            //Start new training attempt
            Attempt = 0;
            NextAttempt();
            return;
        }

        //Properties
        /// <summary>
        /// The MLP network engine under training.
        /// </summary>
        public MLPEngine CurrentEngine { get { return _engine; } }

        //Methods
        private void InitImbalances()
        {
            double numOfClasses = _engine.TaskType == OutputTaskType.Binary ? 2d : _engine.NumOfOutputFeatures;
            double beta = (StdTrainingDataset.Count - 1d) / StdTrainingDataset.Count;

            if (_engine.TaskType == OutputTaskType.Categorical)
            {
                //Categorical
                double sum = 0d;
                for (int i = 0; i < _engine.NumOfOutputFeatures; i++)
                {
                    double effectiveNum = 1d - Math.Pow(beta, OutputFilters[i].SamplesStat.NumOfNonzeroSamples);
                    double weight = (1d - beta) / effectiveNum;
                    sum += weight;
                    _outputImbalanceCoeffs[0][i] = weight;
                    _outputImbalanceCoeffs[1][i] = weight;
                }
                //Normalize
                for (int i = 0; i < _engine.NumOfOutputFeatures; i++)
                {
                    _outputImbalanceCoeffs[0][i] = (_outputImbalanceCoeffs[0][i] / sum) * numOfClasses;
                    _outputImbalanceCoeffs[1][i] = (_outputImbalanceCoeffs[1][i] / sum) * numOfClasses;
                }
            }
            else
            {
                //Binary
                for (int i = 0; i < _engine.NumOfOutputFeatures; i++)
                {
                    double effectiveNum0 = 1d - Math.Pow(beta, OutputFilters[i].SamplesStat.NumOfSamples - OutputFilters[i].SamplesStat.NumOfNonzeroSamples);
                    double effectiveNum1 = 1d - Math.Pow(beta, OutputFilters[i].SamplesStat.NumOfNonzeroSamples);
                    _outputImbalanceCoeffs[0][i] = (1d - beta) / effectiveNum0;
                    _outputImbalanceCoeffs[1][i] = (1d - beta) / effectiveNum1;
                    //Normalize
                    double sum = _outputImbalanceCoeffs[0][i] + _outputImbalanceCoeffs[1][i];
                    _outputImbalanceCoeffs[0][i] = (_outputImbalanceCoeffs[0][i] / sum) * numOfClasses;
                    _outputImbalanceCoeffs[1][i] = (_outputImbalanceCoeffs[1][i] / sum) * numOfClasses;
                }
            }
            return;
        }

        /// <summary>
        /// Initializes dropout on layers.
        /// </summary>
        private void InitDropout()
        {
            _dropoutActive = _modelCfg.InputOptionsCfg.DropoutCfg.Mode != DropoutMode.None;
            //Input dropout
            _dropoutModes[0] = _modelCfg.InputOptionsCfg.DropoutCfg.Mode;
            _dropoutPs[0] = _modelCfg.InputOptionsCfg.DropoutCfg.P;
            _keepPs[0] = 1d - _dropoutPs[0];
            //Hidden layers dropout
            for (int i = 0; i < _modelCfg.HiddenLayersCfg.LayerCfgCollection.Count; i++)
            {
                _dropoutActive = _dropoutActive || _modelCfg.HiddenLayersCfg.LayerCfgCollection[i].DropoutCfg.Mode != DropoutMode.None;
                _dropoutModes[1 + i] = _modelCfg.HiddenLayersCfg.LayerCfgCollection[i].DropoutCfg.Mode;
                _dropoutPs[1 + i] = _modelCfg.HiddenLayersCfg.LayerCfgCollection[i].DropoutCfg.P;
                _keepPs[1 + i] = 1d - _dropoutPs[1 + i];
            }
            return;
        }

        /// <summary>
        /// Initializes L1 and L2 lambdas on layers.
        /// </summary>
        private void InitL12Regularizations()
        {
            //Lambdas and proportional scales by weight engagement probability
            for (int i = 0; i < _engine.LayerCollection.Count; i++)
            {
                RegL1Config regL1Cfg = _engine.LayerCollection[i].OutputLayer ? _modelCfg.OutputOptionsCfg.RegL1Cfg : _modelCfg.HiddenLayersCfg.LayerCfgCollection[i].RegL1Cfg;
                RegL2Config regL2Cfg = _engine.LayerCollection[i].OutputLayer ? _modelCfg.OutputOptionsCfg.RegL2Cfg : _modelCfg.HiddenLayersCfg.LayerCfgCollection[i].RegL2Cfg;
                _regL1WLambdas[i] = regL1Cfg.Strength;
                _regL1BLambdas[i] = regL1Cfg.Biases ? regL1Cfg.Strength : 0d;
                _regL2WLambdas[i] = regL2Cfg.Strength;
                _regL2BLambdas[i] = regL2Cfg.Biases ? regL2Cfg.Strength : 0d;
                //Scale lambdas by total number of training samples
                _regL1WLambdas[i] /= StdTrainingDataset.Count;
                _regL1BLambdas[i] /= StdTrainingDataset.Count;
                _regL2WLambdas[i] /= StdTrainingDataset.Count;
                _regL2BLambdas[i] /= StdTrainingDataset.Count;
            }
            return;
        }

        /// <summary>
        /// Initializes weights norm-constraints on network layers.
        /// </summary>
        private void InitNormConstraints()
        {
            for (int i = 0; i < _engine.LayerCollection.Count; i++)
            {
                NormConsConfig normConsCfg = _engine.LayerCollection[i].OutputLayer ? _modelCfg.OutputOptionsCfg.NormConsCfg : _modelCfg.HiddenLayersCfg.LayerCfgCollection[i].NormConsCfg;
                _minNorms[i] = normConsCfg.Min;
                _maxNorms[i] = normConsCfg.Max;
                _normBiases[i] = normConsCfg.Biases;
            }
            return;
        }

        /// <summary>
        /// Creates an instance of the appropriate optimizer.
        /// </summary>
        private IOptimizer CreateOptimizer(Optimizer optimizerID)
        {
            return optimizerID switch
            {
                Optimizer.RProp => new RProp(_engine.NumOfWeights, (RPropConfig)_modelCfg.OptimizerCfg),
                Optimizer.SGD => new SGD(_engine.NumOfWeights, (SGDConfig)_modelCfg.OptimizerCfg),
                Optimizer.Adam => new Adam(_engine.NumOfWeights, (AdamConfig)_modelCfg.OptimizerCfg),
                Optimizer.Adabelief => new Adabelief(_engine.NumOfWeights, (AdabeliefConfig)_modelCfg.OptimizerCfg),
                Optimizer.Padam => new Padam(_engine.NumOfWeights, (PadamConfig)_modelCfg.OptimizerCfg),
                Optimizer.Adamax => new Adamax(_engine.NumOfWeights, (AdamaxConfig)_modelCfg.OptimizerCfg),
                Optimizer.Adagrad => new Adagrad(_engine.NumOfWeights, (AdagradConfig)_modelCfg.OptimizerCfg),
                Optimizer.Adadelta => new Adadelta(_engine.NumOfWeights, (AdadeltaConfig)_modelCfg.OptimizerCfg),
                Optimizer.RMSProp => new RMSProp(_engine.NumOfWeights, (RMSPropConfig)_modelCfg.OptimizerCfg),
                _ => throw new ArgumentException($"Unsupported optimizer {optimizerID}.", nameof(optimizerID)),
            };
        }

        private void InitBatchSize()
        {
            if (_modelCfg.BatchSize == NetworkModelConfig.FullBatchSizeNumCode ||
                StdTrainingDataset.Count == 1 ||
                _modelCfg.OptimizerCfg.OptimizerID == Optimizer.RProp)
            {
                //Full batch size (BGD)
                _batchSize = StdTrainingDataset.Count;
            }
            else
            {
                if(_modelCfg.BatchSize == NetworkModelConfig.AutoBatchSizeNumCode)
                {
                    if (_optimizer.UpdaterID == Optimizer.SGD)
                    {
                        //Default for SGD
                        _batchSize = 1;
                    }
                    else
                    {
                        //Default optimal batch size computation
                        //Base size
                        _batchSize = (int)Math.Round(StdTrainingDataset.Count /
                                     (double)NumOfTrainingSamplesIncrementingMiniBatchSize,
                                     MidpointRounding.AwayFromZero);
                        //Optimal min size to be kept 
                        _batchSize = Math.Max(OptimalMiniBatchMinSize, _batchSize);
                        //Optimal max size not to be exceeded
                        _batchSize = Math.Min(OptimalMiniBatchMaxSize, _batchSize);
                    }
                }
                else
                {
                    _batchSize = _modelCfg.BatchSize;

                }
            }
            //Real max size not to be exceeded 
            _batchSize = Math.Min(_batchSize, StdTrainingDataset.Count);
            return;
        }

        /// <summary>
        /// Starts the next training attempt.
        /// </summary>
        public bool NextAttempt()
        {
            if (Attempt < MaxAttempts)
            {
                //Reset last epoch error statistics
                EpochErrStat = null;
                //Next attempt is allowed
                ++Attempt;
                //Reset
                AttemptEpoch = 0;
                _engine.RandomizeWeights(_rand);
                _engine.GetWeightsCopy().CopyTo(_flatWeights, 0);
                _optimizer.Reset();
                return true;
            }
            else
            {
                //Max attempt reached -> do nothing and return false
                return false;
            }
        }

        /// <summary>
        /// Finalizes training epoch.
        /// </summary>
        private void FinalizeEpoch()
        {
            object monitor = new object();
            EpochErrStat = new ModelErrStat(_engine.TaskType, _engine.OutputFeatureNames);
            _engine.SetWeights(_flatWeights);
            Parallel.ForEach(Partitioner.Create(0, StdTrainingDataset.Count), range =>
            {
                ModelErrStat rangeStat = new ModelErrStat(_engine.TaskType, _engine.OutputFeatureNames);
                //Reusable buffers
                double[] sums = new double[_engine.NumOfNeurons];
                double[] activations = new double[_engine.NumOfInputFeatures + _engine.NumOfNeurons];
                double[] computed = new double[_engine.NumOfOutputFeatures];
                //Worker loop over range of samples
                for (int sampleIdx = range.Item1; sampleIdx < range.Item2; sampleIdx++)
                {
                    //Input
                    StdTrainingDataset.SampleCollection[sampleIdx].InputVector.CopyTo(activations, 0);
                    //Compute
                    int aOutIdx = _engine.Compute(activations, sums);
                    //Output
                    //Naturalize
                    for (int i = 0; i < computed.Length; i++)
                    {
                        computed[i] = OutputFilters[i].ApplyReverse(activations[aOutIdx + i]);
                    }
                    //Update stat
                    rangeStat.Update(computed, _orgTrainingDataset.SampleCollection[sampleIdx].OutputVector);
                }
                lock(monitor)
                {
                    EpochErrStat.Merge(rangeStat);
                }
            });
            return;
        }

        /// <summary>
        /// Performs next training epoch.
        /// </summary>
        public bool Epoch()
        {
            if (AttemptEpoch == MaxAttemptEpochs)
            {
                //Max epoch reached, try new attempt
                if (!NextAttempt())
                {
                    //Next attempt is not available
                    return false;
                }
            }
            //Next epoch
            ++AttemptEpoch;
            _optimizer.NewEpoch(AttemptEpoch, _modelCfg.Epochs);
            if (_batchSize != StdTrainingDataset.Count)
            {
                //Shuffle samples
                _shuffledDataset.Shuffle(_rand);
            }
            //Loop batches
            int batchFirstSampleIdx = 0;
            while (batchFirstSampleIdx < _shuffledDataset.Count)
            {
                int batchSamplesCount = _batchSize;
                if(batchFirstSampleIdx + batchSamplesCount > _shuffledDataset.Count)
                {
                    batchSamplesCount = _shuffledDataset.Count - batchFirstSampleIdx;
                }
                PerformBatch(new Tuple<int, int>(batchFirstSampleIdx, batchSamplesCount));
                batchFirstSampleIdx += batchSamplesCount;
            }
            //Finalize epoch
            FinalizeEpoch();
            return true;
        }

        /// <summary>
        /// Performs network training iteration on specified batch.
        /// </summary>
        /// <param name="batch">Identifies samples to be performed (index, count).</param>
        private void PerformBatch(Tuple<int, int> batch)
        {
            int batchFirstSampleIdx = batch.Item1;
            int batchSamplesCount = batch.Item2;
            //Locking
            object monitor = new object();
            //Network output layer shortcut
            MLPEngine.Layer outputLayer = _engine.LayerCollection[_engine.LayerCollection.Count - 1];
            //Variables for collection of workers' outputs
            LinkedList<double[]> workersLinkedWeightFlatGads = new LinkedList<double[]>();
            double cost = 0d;
            //Prepare partitions for process gradient workers
            List<Tuple<int, int, int>> partitions = Common.GetFixedPartitions(batchSamplesCount);
            //If dropout then prepare randoms for process gradient workers
            Random[] workersRandom = null;
            if (_dropoutActive)
            {
                workersRandom = new Random[partitions.Count];
                for (int i = 0; i < workersRandom.Length; i++)
                {
                    workersRandom[i] = new Random(_rand.Next());
                }
            }
            //Process parallel gradient workers
            Parallel.ForEach(partitions, partition =>
            {
                //Gradient worker outputs
                //Defaultly initialized to zeroes
                double[] workerWeightFlatGrads = new double[_engine.NumOfWeights];
                double workerLossSum = 0d;
                //----------------------------------------------------------------------------------------------------
                //Gradient worker local variables
                double[] sums = new double[_engine.NumOfNeurons];
                double[] activations = new double[_engine.NumOfInputFeatures + _engine.NumOfNeurons];
                double[] derivatives = new double[_engine.NumOfNeurons];
                double[] nodeGrads = new double[_engine.NumOfNeurons];
                bool[] nodeSwitches = new bool[_engine.NumOfInputFeatures + _engine.NumOfNeurons];
                Array.Fill(nodeSwitches, true);
                Random workerRand = workersRandom?[partition.Item3];
                //Loop the worker over the planned range of samples
                for (int sampleIdx = partition.Item1; sampleIdx < partition.Item2; sampleIdx++)
                {
                    Sample sample = _shuffledDataset.SampleCollection[batchFirstSampleIdx + sampleIdx];
                    //Forward pass
                    //Input
                    sample.InputVector.CopyTo(activations, 0);
                    if (_dropoutModes[0] != DropoutMode.None)
                    {
                        _linearAF.Dropout(_dropoutModes[0],
                                          _dropoutPs[0],
                                          workerRand,
                                          nodeSwitches,
                                          0,
                                          activations,
                                          0,
                                          null,
                                          0,
                                          _engine.NumOfInputFeatures
                                          );
                    }
                    //Layers
                    int inputStartFlatIdx = 0;
                    for (int layerIdx = 0; layerIdx < _engine.LayerCollection.Count; layerIdx++)
                    {
                        MLPEngine.Layer layer = _engine.LayerCollection[layerIdx];
                        //Compute sums
                        for (int neuronIdx = 0, sumsFlatIdx = layer.NeuronsStartFlatIdx, weightFlatIdx = layer.WeightsStartFlatIdx, biasFlatIdx = layer.BiasesStartFlatIdx; neuronIdx < layer.NumOfLayerNeurons; neuronIdx++, sumsFlatIdx++, biasFlatIdx++)
                        {
                            sums[sumsFlatIdx] = _flatWeights[biasFlatIdx];
                            for (int inputIdx = 0; inputIdx < layer.NumOfInputNodes; inputIdx++, weightFlatIdx++)
                            {
                                sums[sumsFlatIdx] += _flatWeights[weightFlatIdx] * activations[inputStartFlatIdx + inputIdx];
                            }
                        }
                        //Compute activations and derivatives
                        layer.Activation.Compute(sums,
                                                 layer.NeuronsStartFlatIdx,
                                                 activations,
                                                 _engine.NumOfInputFeatures + layer.NeuronsStartFlatIdx,
                                                 derivatives,
                                                 layer.NeuronsStartFlatIdx,
                                                 layer.NumOfLayerNeurons
                                                 );
                        //Hidden dropout
                        if (!layer.OutputLayer && _dropoutModes[1 + layerIdx] != DropoutMode.None)
                        {
                            layer.Activation.Dropout(_dropoutModes[1 + layerIdx],
                                                     _dropoutPs[1 + layerIdx],
                                                     workerRand,
                                                     nodeSwitches,
                                                     _engine.NumOfInputFeatures + layer.NeuronsStartFlatIdx,
                                                     activations,
                                                     _engine.NumOfInputFeatures + layer.NeuronsStartFlatIdx,
                                                     derivatives,
                                                     layer.NeuronsStartFlatIdx,
                                                     layer.NumOfLayerNeurons
                                                     );

                        }
                        inputStartFlatIdx += layer.NumOfInputNodes;
                    }
                    //----------------------------------------------------------------------------------------------------
                    //Backward pass
                    //Compute output layer local gradients
                    for (int neuronIdx = 0, outputLayerNeuronFlatIdx = outputLayer.NeuronsStartFlatIdx; neuronIdx < outputLayer.NumOfLayerNeurons; neuronIdx++, outputLayerNeuronFlatIdx++)
                    {
                        double ideal = sample.OutputVector[neuronIdx];
                        double computed = activations[_engine.NumOfInputFeatures + outputLayerNeuronFlatIdx];
                        //Update sum of loss
                        double loss = _engine.LossFn.Compute(ideal, computed);
                        workerLossSum += loss;
                        //Local Z gradient computation
                        nodeGrads[outputLayerNeuronFlatIdx] = _engine.LossFn.ComputeZGradient(derivatives[outputLayerNeuronFlatIdx], ideal, computed);
                        //Affect imbalance
                        if (_engine.TaskType != OutputTaskType.Regression && _modelCfg.ClassBalancedLoss)
                        {
                            int imbalanceCoeffIdx = ideal >= 0.5d ? 1 : 0;
                            nodeGrads[outputLayerNeuronFlatIdx] *= _outputImbalanceCoeffs[imbalanceCoeffIdx][neuronIdx];
                        }
                    }//neuronIdx
                    //----------------------------------------------------------------------------------------------------
                    //Compute hidden neurons local gradients
                    for (int layerIdx = _engine.LayerCollection.Count - 2; layerIdx >= 0; layerIdx--)
                    {
                        MLPEngine.Layer currLayer = _engine.LayerCollection[layerIdx];
                        MLPEngine.Layer nextLayer = _engine.LayerCollection[layerIdx + 1];
                        for (int currLayerNeuronIdx = 0, currLayerNeuronFlatIdx = currLayer.NeuronsStartFlatIdx; currLayerNeuronIdx < currLayer.NumOfLayerNeurons; currLayerNeuronIdx++, currLayerNeuronFlatIdx++)
                        {
                            if (nodeSwitches[_engine.NumOfInputFeatures + currLayerNeuronFlatIdx])
                            {
                                double sum = 0d;
                                for (int nextLayerNeuronIdx = 0; nextLayerNeuronIdx < nextLayer.NumOfLayerNeurons; nextLayerNeuronIdx++)
                                {
                                    int weightFlatIdx = nextLayer.WeightsStartFlatIdx + nextLayerNeuronIdx * nextLayer.NumOfInputNodes + currLayerNeuronIdx;
                                    sum += nodeGrads[nextLayer.NeuronsStartFlatIdx + nextLayerNeuronIdx] * _flatWeights[weightFlatIdx];
                                }//nextLayerNeuronIdx
                                //Compute local gradient
                                nodeGrads[currLayerNeuronFlatIdx] = derivatives[currLayerNeuronFlatIdx] * sum;
                                if (_dropoutModes[1 + layerIdx] == DropoutMode.Bernoulli)
                                {
                                    nodeGrads[currLayerNeuronFlatIdx] /= _keepPs[1 + layerIdx];
                                }
                            }
                            else
                            {
                                nodeGrads[currLayerNeuronFlatIdx] = 0d;
                            }
                        }//currLayerNeuronIdx
                    }//layerIdx
                    //----------------------------------------------------------------------------------------------------
                    //Compute weight gradients
                    inputStartFlatIdx = 0;
                    for (int layerIdx = 0; layerIdx < _engine.LayerCollection.Count; layerIdx++)
                    {
                        MLPEngine.Layer layer = _engine.LayerCollection[layerIdx];
                        for (int neuronIdx = 0, neuronFlatIdx = layer.NeuronsStartFlatIdx, biasFlatIdx = layer.BiasesStartFlatIdx, weightFlatIdx = layer.WeightsStartFlatIdx; neuronIdx < layer.NumOfLayerNeurons; neuronIdx++, neuronFlatIdx++, biasFlatIdx++)
                        {
                            //Weights gradients accumulation
                            if (nodeSwitches[_engine.NumOfInputFeatures + neuronFlatIdx])
                            {
                                //Layer's inputs
                                for (int inputIdx = 0; inputIdx < layer.NumOfInputNodes; inputIdx++, weightFlatIdx++)
                                {
                                    if (nodeSwitches[inputStartFlatIdx + inputIdx])
                                    {
                                        workerWeightFlatGrads[weightFlatIdx] += activations[inputStartFlatIdx + inputIdx] * nodeGrads[neuronFlatIdx];
                                        if (_regL1WLambdas[layerIdx] > 0d)
                                        {
                                            workerWeightFlatGrads[weightFlatIdx] += _regL1WLambdas[layerIdx] * Math.Sign(_flatWeights[weightFlatIdx]);
                                        }
                                        if (_regL2WLambdas[layerIdx] > 0d)
                                        {
                                            workerWeightFlatGrads[weightFlatIdx] += _regL2WLambdas[layerIdx] * _flatWeights[weightFlatIdx];
                                        }
                                    }
                                }
                                //Bias
                                workerWeightFlatGrads[biasFlatIdx] += nodeGrads[neuronFlatIdx];
                                if (_regL1BLambdas[layerIdx] > 0d)
                                {
                                    workerWeightFlatGrads[biasFlatIdx] += _regL1BLambdas[layerIdx] * Math.Sign(_flatWeights[biasFlatIdx]);
                                }
                                if (_regL2BLambdas[layerIdx] > 0d)
                                {
                                    workerWeightFlatGrads[biasFlatIdx] += _regL2BLambdas[layerIdx] * _flatWeights[biasFlatIdx];
                                }
                            }
                            else
                            {
                                weightFlatIdx += layer.NumOfInputNodes;
                            }
                        }//neuronIdx
                        inputStartFlatIdx += layer.NumOfInputNodes;
                    }//layerIdx
                }//Worker main loop
                //Store results
                lock(monitor)
                {
                    workersLinkedWeightFlatGads.AddLast(workerWeightFlatGrads);
                    cost += workerLossSum;
                }
            });//Worker finish
            ///////////////////////////////////////////////////////////////////////////////////////
            //Adjust total cost (averaged loss)
            cost /= (_engine.TaskType == OutputTaskType.Categorical ? StdTrainingDataset.Count : (StdTrainingDataset.Count * _engine.NumOfOutputFeatures));
            ///////////////////////////////////////////////////////////////////////////////////////
            //Aggregate and scale gradients from workers,
            //identify gradients disabled by dropout
            //and clip gradients norm or value
            bool[] weightFlatGradSwitches = new bool[_flatWeights.Length];
            double[] weightFlatGrads = new double[_flatWeights.Length];
            double weightGradsSumOfSquares = 0d;
            double[][] workersWeightFlatGads = workersLinkedWeightFlatGads.ToArray();

            for (int i = 0; i < weightFlatGrads.Length; i++)
            {
                weightFlatGrads[i] = 0d;
                for (int idx = 0; idx < workersWeightFlatGads.Length; idx++)
                {
                    weightFlatGrads[i] += workersWeightFlatGads[idx][i];
                }
                weightFlatGradSwitches[i] = weightFlatGrads[i] != 0d;
                weightFlatGrads[i] /= batchSamplesCount;
                //Clip gradient magnitude
                if (_modelCfg.GradClipVal > 0d && Math.Abs(weightFlatGrads[i]) > _modelCfg.GradClipVal)
                {
                    weightFlatGrads[i] = Math.Sign(weightFlatGrads[i]) * _modelCfg.GradClipVal;
                }
                weightGradsSumOfSquares += weightFlatGrads[i] * weightFlatGrads[i];
            }

            //Clip gradients norm
            if (_modelCfg.GradClipNorm > 0d)
            {
                double gnorm = Math.Sqrt(weightGradsSumOfSquares);
                if (gnorm > _modelCfg.GradClipNorm)
                {
                    weightFlatGrads.Scale(_modelCfg.GradClipNorm / gnorm);
                }
            }

            ///////////////////////////////////////////////////////////////////////////////////////
            //Update weights and biases
            double learningPermeability = _learningThrottleValve == null ? 1d : _learningThrottleValve.Map(AttemptEpoch);
            _optimizer.Update(learningPermeability, cost, weightFlatGradSwitches, weightFlatGrads, _flatWeights);
            //Check numerical stability
            double wMagnitude = _flatWeights.Magnitude();
            if (double.IsNaN(wMagnitude) || wMagnitude >= MaxAcceptableWeightMagnitude)
            {
                throw new ApplicationException($"Weight magnitude in NaN or exceeds {MaxAcceptableWeightMagnitude:E3} after last update. Try to decrease learning rate to avoid numerical instability.");
            }
            ///////////////////////////////////////////////////////////////////////////////////////
            //Apply weight-norm constraints
            for (int layerIdx = 0; layerIdx < _engine.LayerCollection.Count; layerIdx++)
            {
                if (_maxNorms[layerIdx] > 0d)
                {
                    MLPEngine.Layer currLayer = _engine.LayerCollection[layerIdx];
                    Parallel.ForEach(Partitioner.Create(0, currLayer.NumOfLayerNeurons), neuronRange =>
                    {
                        for (int neuronIdx = neuronRange.Item1; neuronIdx < neuronRange.Item2; neuronIdx++)
                        {
                            int weightFlatIdx = currLayer.WeightsStartFlatIdx + neuronIdx * currLayer.NumOfInputNodes;
                            int biasFlatIdx = currLayer.BiasesStartFlatIdx + neuronIdx;
                            double norm = _normBiases[layerIdx] ? _flatWeights[biasFlatIdx].Power(2) : 0d;
                            for (int inputIdx = 0; inputIdx < currLayer.NumOfInputNodes; inputIdx++)
                            {
                                norm += _flatWeights[weightFlatIdx + inputIdx].Power(2);
                            }
                            norm = Math.Sqrt(norm);
                            if (norm < _minNorms[layerIdx])
                            {
                                for (int inputIdx = 0; inputIdx < currLayer.NumOfInputNodes; inputIdx++)
                                {
                                    _flatWeights[weightFlatIdx + inputIdx] *= _minNorms[layerIdx] / norm;
                                }
                                if (_normBiases[layerIdx])
                                {
                                    _flatWeights[biasFlatIdx] *= _minNorms[layerIdx] / norm;
                                }
                            }
                            else if (norm > _maxNorms[layerIdx])
                            {
                                for (int inputIdx = 0; inputIdx < currLayer.NumOfInputNodes; inputIdx++)
                                {
                                    _flatWeights[weightFlatIdx + inputIdx] *= _maxNorms[layerIdx] / norm;
                                }
                                if (_normBiases[layerIdx])
                                {
                                    _flatWeights[biasFlatIdx] *= _maxNorms[layerIdx] / norm;
                                }
                            }
                        }
                    });
                }
            }
            return;
        }


    }//Trainer

}//Namespace
