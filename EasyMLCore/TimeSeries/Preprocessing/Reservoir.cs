using EasyMLCore.Activation;
using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using EasyMLCore.MiscTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EasyMLCore.TimeSeries
{
    //Delegates
    /// <summary>
    /// Delegate of the ReservoirInitProgressChanged event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the init process.</param>
    public delegate void ReservoirInitProgressChangedHandler(ReservoirInitProgressInfo progressInfo);

    /// <summary>
    /// Implements a reservoir.
    /// </summary>
    [Serializable]
    public class Reservoir : SerializableObject, IComputable
    {
        //Constants
        /// <summary>
        /// Short identifier for context path.
        /// </summary>
        public const string ContextPathID = "Reservoir";

        /// <summary>
        /// Specifies whether to center features when applaying feature filters.
        /// </summary>
        public const bool UseCenteredFeatures = true;

        //Static variables
        /// <summary>
        /// A number used to initialize pseudo random numbers.
        /// </summary>
        private static int RandomSeed = Common.DefaultRandomSeed;
        
        //Enumerations
        /// <summary>
        /// Supported types of input data feeding.
        /// </summary>
        public enum InputFeeding
        {
            /// <summary>
            /// Data is fed continuously at separated time points and
            /// reservoir state is never reseted.
            /// Predictors are collected after each time point computation.
            /// </summary>
            TimePoint,
            /// <summary>
            /// Time series data has constant length.
            /// Data is processed first from right to left (reversed time order).
            /// Predictors are collected and reservoir's state is reseted.
            /// Subsequently, the time series data is processed from left to right
            /// and the second set of predictors is collected.
            /// </summary>
            PatternConstLength,
            /// <summary>
            /// Time series data has varying length.
            /// Data is processed first from right to left (reversed time order).
            /// Predictors are collected and reservoir's state is reseted.
            /// Subsequently, the time series data is processed from left to right
            /// and the second set of predictors is collected.
            /// </summary>
            PatternVarLength
        }

        /// <summary>
        /// Reservoir's output sections enum.
        /// </summary>
        public enum OutSection
        {
            /// <summary>
            /// Activation predictors.
            /// </summary>
            Activations,
            /// <summary>
            /// Squared activation predictors.
            /// </summary>
            SquaredActivations,
            /// <summary>
            /// Spikes fading traces predictors.
            /// </summary>
            SpikesFadingTraces,
            /// <summary>
            /// Reservoir's input data.
            /// </summary>
            ResInputs
        };


        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the reservoir's init process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        private event ReservoirInitProgressChangedHandler InitProgressChanged;

        //Attribute properties
        /// <summary>
        /// Reservoir's configuration.
        /// </summary>
        public ReservoirConfig ResCfg { get; }
        
        /// <summary>
        /// Lengths of output sections.
        /// </summary>
        public int[] OutSectionsLengths { get; }

        /// <summary>
        /// Total number of input synapses.
        /// </summary>
        public int NumOfInputSynapses { get; }

        /// <summary>
        /// Stat of weights of input synapses.
        /// </summary>
        public BasicStat InputSynapsesWeightStat { get; }

        /// <summary>
        /// Total number of hidden synapses.
        /// </summary>
        public int NumOfHiddenSynapses { get; }

        /// <summary>
        /// Stat of weights of hidden synapses.
        /// </summary>
        public BasicStat HiddenSynapsesWeightStat { get; }

        //Attributes
        private readonly RealFeatureFilter[] _inputFilters;
        private readonly ActivationBase _inputActivationFn;
        private readonly ReservoirNeuron[] _inputNeurons;
        private readonly ActivationBase _hiddenActivationFn;
        private readonly ReservoirNeuron[] _hiddenNeurons;
        private readonly double[] _hiddenBiases;
        private readonly int _numOfOutputSections;
        private readonly int _predictorSectionFullLength;
        private readonly List<string> _outputSectionNames;
        private int _bootingCountdown;
        private bool _initialized;

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="cfg">Reservoir's configuration.</param>
        public Reservoir(ReservoirConfig cfg)
        {
            //Number of reservoir's output sections
            _numOfOutputSections = Enum.GetValues(typeof(OutSection)).Length;
            //Names of reservoir's output sections
            _outputSectionNames = new List<string>(Enum.GetNames(typeof(OutSection)));
            //Store configuration
            ResCfg = (ReservoirConfig)cfg.DeepClone();
            //Ensure always the same random sequence
            Random rand = new Random(RandomSeed);
            //Switch off init switch
            _initialized = false;
            //Input filters
            _inputFilters = new RealFeatureFilter[ResCfg.InputCfg.NumOfVariables];
            for (int i = 0; i < ResCfg.InputCfg.NumOfVariables; i++)
            {
                _inputFilters[i] = new RealFeatureFilter(FeatureFilterBase.FeatureUse.Input);
            }
            //Neurons
            //Input neurons
            _inputActivationFn = ActivationFactory.CreateActivationFn(ActivationFnID.Linear);
            _inputNeurons = new ReservoirNeuron[ResCfg.InputCfg.NumOfVariables];
            for (int i = 0; i < _inputNeurons.Length; i++)
            {
                _inputNeurons[i] = new ReservoirNeuron(i,
                                                       _inputActivationFn,
                                                       ResCfg.HiddenLayerCfg.SpikeEventThreshold,
                                                       0d,
                                                       0d
                                                       );
            }
            //Hidden neurons
            double fadingCoeff;
            if (ResCfg.InputCfg.Feeding == InputFeeding.TimePoint)
            {
                //Synchronize fading coefficient and number of necessary reservoir's booting cycles
                fadingCoeff = 1d - Math.Min(0.5d, 1d / ResCfg.HiddenLayerCfg.NumOfNeurons);
            }
            else
            {
                //For patterns, no fading is necessary, so the fading coefficient can be simply 1
                fadingCoeff = 1d;
                //fadingCoeff = 1d - Math.Min(0.5d, (1d / (ResCfg.InputCfg.FlatDataLength / ResCfg.InputCfg.Variables)) * 0.25d);
            }
            _hiddenActivationFn = ActivationFactory.CreateActivationFn(ResCfg.HiddenLayerCfg.ActivationID);
            _hiddenNeurons = new ReservoirNeuron[ResCfg.HiddenLayerCfg.NumOfNeurons];
            for (int i = 0; i < _hiddenNeurons.Length; i++)
            {
                _hiddenNeurons[i] = new ReservoirNeuron(i,
                                                        _hiddenActivationFn,
                                                        ResCfg.HiddenLayerCfg.SpikeEventThreshold,
                                                        fadingCoeff,
                                                        ResCfg.HiddenLayerCfg.Retainment
                                                        );
            }
            //Connections
            int[] hiddenNeuronIndices = new int[_hiddenNeurons.Length];
            hiddenNeuronIndices.Indices();
            //Hidden
            NumOfHiddenSynapses = 0;
            int numOfHNConns = Math.Max(1, ResCfg.HiddenLayerCfg.Density < 1d ? (int)Math.Round(_hiddenNeurons.Length * ResCfg.HiddenLayerCfg.Density, MidpointRounding.AwayFromZero)
                                                                            : (int)ResCfg.HiddenLayerCfg.Density);
            numOfHNConns = Math.Min(_hiddenNeurons.Length, numOfHNConns);
            int maxHDelay = Math.Min(numOfHNConns - 1, ResCfg.HiddenLayerCfg.MaxDelay);
            CyclingCounter hcc = maxHDelay > 0 ? new CyclingCounter(0, maxHDelay, 1) : null;
            double[] absWeightsSums = new double[_hiddenNeurons.Length];
            //Loop a random ring schema to maximize passing the signal through the reservoir
            for (int connNum = 1; connNum <= numOfHNConns; connNum++)
            {
                //Shuffle indices
                rand.Shuffle(hiddenNeuronIndices);
                hcc?.Reset();
                for (int tNIdx = 0; tNIdx < hiddenNeuronIndices.Length; tNIdx++)
                {
                    double weight = rand.NextRangedUniformDouble(-1d, 1d);
                    int delay = hcc == null ? 0 : hcc.GetNext();
                    int tNRealIdx = hiddenNeuronIndices[tNIdx];
                    int sNRealIdx = tNIdx == hiddenNeuronIndices.Length - 1 ? hiddenNeuronIndices[0] : hiddenNeuronIndices[tNIdx + 1];
                    absWeightsSums[tNRealIdx] += Math.Abs(weight);
                    //Connect
                    _hiddenNeurons[tNRealIdx].ConnectHiddenNeuron(sNRealIdx, weight, delay);
                    ++NumOfHiddenSynapses;
                }
            }
            //Set homogenous excitability
            for(int i = 0; i < _hiddenNeurons.Length; i++)
            {
                _hiddenNeurons[i].ScaleHiddenSynapsesWeight(1d / absWeightsSums[i]);
            }
            //Set spectral radius
            double eigenVal = EstimateSpectralRadius();
            foreach (ReservoirNeuron hiddenNeuron in _hiddenNeurons)
            {
                hiddenNeuron.ScaleHiddenSynapsesWeight(ResCfg.HiddenLayerCfg.SpectralRadius / eigenVal);
            }
            //Setup biases
            _hiddenBiases = new double[_hiddenNeurons.Length];
            FillBias(0.1d, _hiddenBiases);

            //Input
            NumOfInputSynapses = 0;
            int numOfINConns = Math.Max(1, ResCfg.InputCfg.Density < 1d ? (int)Math.Round(_hiddenNeurons.Length * ResCfg.InputCfg.Density, MidpointRounding.AwayFromZero)
                                                                      : (int)ResCfg.InputCfg.Density);
            numOfINConns = Math.Min(_hiddenNeurons.Length, numOfINConns);
            int maxIDelay = Math.Min(numOfINConns - 1, ResCfg.InputCfg.MaxDelay);
            CyclingCounter icc = maxIDelay > 0 ? new CyclingCounter(0, maxIDelay, 1) : null;
            for (int neuronIdx = 0; neuronIdx < _inputNeurons.Length; neuronIdx++)
            {
                rand.Shuffle(hiddenNeuronIndices);
                icc?.Reset();
                for (int i = 0; i < numOfINConns; i++)
                {
                    double weight = rand.NextRangedUniformDouble(ResCfg.InputCfg.MaxStrength / 2d, ResCfg.InputCfg.MaxStrength);
                    int delay = icc == null ? 0 : icc.GetNext();
                    _hiddenNeurons[hiddenNeuronIndices[i]].ConnectInputNeuron(neuronIdx, weight, delay);
                    ++NumOfInputSynapses;
                }
            }
            foreach(ReservoirNeuron neuron in _hiddenNeurons)
            {
                neuron.AdjustInputSynapsesWeight();
            }
            //Weights stats
            InputSynapsesWeightStat = new BasicStat();
            HiddenSynapsesWeightStat = new BasicStat();
            foreach(ReservoirNeuron neuron in _hiddenNeurons)
            {
                neuron.UpdateSynapsesWeightStat(InputSynapsesWeightStat, HiddenSynapsesWeightStat);
            }
            //Total number of output features
            _predictorSectionFullLength = _hiddenNeurons.Length * (ResCfg.InputCfg.Feeding != InputFeeding.TimePoint ? 2 : 1);
            OutSectionsLengths = new int[_numOfOutputSections];
            Array.Fill(OutSectionsLengths, _predictorSectionFullLength);
            OutSectionsLengths[(int)OutSection.ResInputs] = 0; //Now is unknown
            //Booting cycles countdown
            SetBootingCountdown();
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">Source instance.</param>
        public Reservoir(Reservoir source)
        {
            OutSectionsLengths = (int[])source.OutSectionsLengths.Clone();
            NumOfInputSynapses = source.NumOfInputSynapses;
            InputSynapsesWeightStat = new BasicStat(source.InputSynapsesWeightStat);
            NumOfHiddenSynapses = source.NumOfHiddenSynapses;
            HiddenSynapsesWeightStat = new BasicStat(source.HiddenSynapsesWeightStat);
            ResCfg = (ReservoirConfig)source.ResCfg.DeepClone();
            _inputFilters = new RealFeatureFilter[source._inputFilters.Length];
            for(int i = 0; i < source._inputFilters.Length; i++)
            {
                _inputFilters[i] = (RealFeatureFilter)source._inputFilters[i].DeepClone();
            }
            _inputActivationFn = ActivationFactory.CreateActivationFn(source._inputActivationFn.ID);
            _inputNeurons = new ReservoirNeuron[source._inputNeurons.Length];
            for (int i = 0; i < source._inputNeurons.Length; i++)
            {
                _inputNeurons[i] = source._inputNeurons[i].DeepClone();
            }
            _hiddenActivationFn = ActivationFactory.CreateActivationFn(source._hiddenActivationFn.ID);
            _hiddenNeurons = new ReservoirNeuron[source._hiddenNeurons.Length];
            for (int i = 0; i < source._hiddenNeurons.Length; i++)
            {
                _hiddenNeurons[i] = source._hiddenNeurons[i].DeepClone();
            }
            _hiddenBiases = (double[])source._hiddenBiases.Clone();
            _numOfOutputSections = source._numOfOutputSections;
            _predictorSectionFullLength = source._predictorSectionFullLength;
            _outputSectionNames = new List<string>(source._outputSectionNames);
            _bootingCountdown = source._bootingCountdown;
            _initialized = source._initialized;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates resevoir state ready for predictors collection.
        /// </summary>
        public bool Ready { get { return _initialized && _bootingCountdown == 0;} }

        /// <inheritdoc/>
        public int NumOfOutputFeatures { get { return OutSectionsLengths.Sum(); } }

        /// <summary>
        /// Number of input neurons (=input features).
        /// </summary>
        public int NumOfInputNeurons { get { return _inputNeurons.Length; } }

        /// <summary>
        /// Number of input neurons (=input features).
        /// </summary>
        public int NumOfHiddenNeurons { get { return _hiddenNeurons.Length; } }

        /// <summary>
        /// ID of hidden neurons' activation function.
        /// </summary>
        public ActivationFnID HiddenActivationFnID { get { return ResCfg.HiddenLayerCfg.ActivationID; } }

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
        /// <summary>
        /// Prepares biases for hidden neurons.
        /// </summary>
        /// <param name="magnitude">Max strength in magnitude.</param>
        /// <param name="biases">Array to be filled.</param>
        private static void FillBias(double magnitude, double[] biases)
        {
            new Random(0).FillUniform(biases, -magnitude, +magnitude, false);
            return;
        }

        /// <summary>
        /// Estimates the spectral radius of hiden neurons weight matrix.
        /// </summary>
        /// <remarks>
        /// Implements the Power Iteration Method.
        /// </remarks>
        /// <param name="maxNumOfIterations">The maximum number of the iterations.</param>
        /// <param name="stopDelta">The stopping corvengence delta of the previous iteration and current iteration.</param>
        /// <returns>The estimated spectral radius.</returns>
        private double EstimateSpectralRadius(int maxNumOfIterations = 1000,
                                              double stopDelta = 1e-6
                                              )
        {
            //Local variables
            //Iteration initialization
            int iteration = 0;
            double iterationDelta;
            double[] tmpVector = new double[_hiddenNeurons.Length];
            double eigenValue = 0;
            double[] eigenVector = new double[_hiddenNeurons.Length];
            Array.Fill(eigenVector, 1d);
            //Results
            double minDelta = double.MaxValue;
            double spectralRadius = 0;
            //Convergence loop
            do
            {
                Parallel.ForEach(Partitioner.Create(0, _hiddenNeurons.Length), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        tmpVector[i] = 0;
                        for (int j = 0; j < _hiddenNeurons[i].HiddenSynapses.Count; j++)
                        {
                            tmpVector[i] += _hiddenNeurons[i].HiddenSynapses[j].GetWeight() *
                                             eigenVector[_hiddenNeurons[i].HiddenSynapses[j].GetPresynapticNeuronIndex()];
                        }
                    }
                });
                //Find element having max magnitude (= new eigen value)
                double prevEigenValue = eigenValue;
                eigenValue = tmpVector.Magnitude();
                //Prepare new normalized eigenVector
                for (int i = 0; i < _hiddenNeurons.Length; i++)
                {
                    eigenVector[i] = tmpVector[i] / eigenValue;
                }
                //Iteration results
                ++iteration;
                iterationDelta = Math.Abs(eigenValue - prevEigenValue);
                if (minDelta > iterationDelta)
                {
                    minDelta = iterationDelta;
                    spectralRadius = eigenValue;
                }
            } while (iteration < maxNumOfIterations && iterationDelta > stopDelta);
            return spectralRadius;
        }

        /// <summary>
        /// Determines and sets the number of samples necessary to process before reservoir's output can be used.
        /// </summary>
        private void SetBootingCountdown()
        {
            _bootingCountdown = ResCfg.InputCfg.Feeding == InputFeeding.TimePoint ? _hiddenNeurons.Length : 0;
            return;
        }

        /// <summary>
        /// Resets reservoir's neurons and synapses.
        /// </summary>
        private void ResetState()
        {
            foreach(ReservoirNeuron neuron in _inputNeurons) { neuron.Reset(); }
            foreach (ReservoirNeuron neuron in _hiddenNeurons) { neuron.Reset(); }
            return;
        }

        /// <summary>
        /// Resets reservoir to its "before Init" state.
        /// </summary>
        public void Reset()
        {
            ResetState();
            foreach(RealFeatureFilter filter in _inputFilters) { filter.Reset(); }
            SetBootingCountdown();
            _initialized = false;
            return;
        }

        /// <summary>
        /// Pushes timepoint data into the reservoir and recomputes it.
        /// </summary>
        /// <param name="timepointData">Time point data.</param>
        /// <param name="hiddenNeuronsStatCollection">A collection of stats of all hidden neurons to be updated. Parameter can be null.</param>
        private void PushTimepointData(double[] timepointData, ReservoirNeuronStat[] hiddenNeuronsStatCollection)
        {
            const int SynapsesParallelLimit = 7000;
            const int NeuronsWithStatParallelLimit = 500;
            const int NeuronsOnlyParallelLimit = NeuronsWithStatParallelLimit * 2;
            //Input
            for (int i = 0; i < _inputNeurons.Length; i++)
            {
                _inputNeurons[i].CollectStimuli(null, null, timepointData[i]);
                _inputNeurons[i].Recompute();
            }
            //Hidden
            //New stimuli collecting
            if((NumOfHiddenSynapses + NumOfInputSynapses) >= SynapsesParallelLimit ||
                _hiddenNeurons.Length >= NeuronsOnlyParallelLimit)
            {
                //Parallel version
                Parallel.ForEach(Partitioner.Create(0, _hiddenNeurons.Length), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        _hiddenNeurons[i].CollectStimuli(_inputNeurons, _hiddenNeurons, _hiddenBiases[i]);
                    }
                });
            }
            else
            {
                //Single thread version
                for (int i = 0; i < _hiddenNeurons.Length; i++)
                {
                    _hiddenNeurons[i].CollectStimuli(_inputNeurons, _hiddenNeurons, _hiddenBiases[i]);
                }
            }
            //Recomputations and stats
            if (hiddenNeuronsStatCollection != null ? _hiddenNeurons.Length >= NeuronsWithStatParallelLimit : _hiddenNeurons.Length >= NeuronsOnlyParallelLimit)
            {
                //Parallel version
                Parallel.ForEach(Partitioner.Create(0, _hiddenNeurons.Length), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        _hiddenNeurons[i].Recompute();
                        hiddenNeuronsStatCollection?[i].Update(_hiddenNeurons[i]);
                    }
                });
            }
            else
            {
                //Single thread version
                foreach (ReservoirNeuron neuron in _hiddenNeurons)
                {
                    neuron.Recompute();
                    hiddenNeuronsStatCollection?[neuron.Index].Update(neuron);
                }
            }
            return;
        }


        /// <summary>
        /// Performs reservoir computation (internal version).
        /// </summary>
        /// <param name="input">Input data.</param>
        /// <param name="outSectionsData">All reservoir's outputs divided into the sections following OutSection enum. Each tuple contains section name and section data.</param>
        /// <param name="neuronStats">Neurons' stats to be updated.</param>
        /// <returns>Original input and all reservoir's outputs. Section by section (see OutSection enum) in a flat 1D array.</returns>
        private double[] ComputeInternal(double[] input, out List<Tuple<string, double[]>> outSectionsData, ReservoirNeuronStat[] neuronStats)
        {
            TimeSeriesPattern inputPattern = new TimeSeriesPattern(input, _inputFilters.Length, ResCfg.InputCfg.VarSchema);
            if (!inputPattern.Consistent)
            {
                throw new ArgumentException("Inconsistent input data.", nameof(input));
            }
            inputPattern.StandardizeData(_inputFilters, UseCenteredFeatures);
            int numOfTimepoints = inputPattern.Length;
            if (ResCfg.InputCfg.Feeding == InputFeeding.TimePoint)
            {
                //Check single timepoint
                if (numOfTimepoints != 1)
                {
                    throw new ArgumentException("Input does not contain single time point data.", nameof(input));
                }
            }
            else
            {
                //Reset state before pattern feeding
                ResetState();
            }
            outSectionsData = new List<Tuple<string, double[]>>(_numOfOutputSections);
            for (int i = 0; i < _numOfOutputSections - 1; i++)
            {
                outSectionsData.Add(new Tuple<string, double[]>(_outputSectionNames[i], new double[OutSectionsLengths[i]]));
            }
            //Add original input if allowed - always the last
            if (ResCfg.InputCfg.Feeding != InputFeeding.PatternVarLength)
            {
                outSectionsData.Add(new Tuple<string, double[]>(_outputSectionNames[_numOfOutputSections - 1], input));
            }
            int[] sectionsOutIdx = new int[_numOfOutputSections - 1];
            Array.Fill(sectionsOutIdx, 0);
            //Pushing data
            if (ResCfg.InputCfg.Feeding != InputFeeding.TimePoint)
            {
                //Reversal time order
                for (int timepointIdx = numOfTimepoints - 1; timepointIdx >= 0; timepointIdx--)
                {
                    double[] timepointData = inputPattern.GetDataAt(timepointIdx);
                    PushTimepointData(timepointData, neuronStats);
                }
                //Build first half of output
                foreach (ReservoirNeuron neuron in _hiddenNeurons)
                {
                    //Quite dangerous because the weak relation between Res OutSections and neuron predictors.
                    //Both must be synchronized in terms of order and count.
                    //But this is the cost of efficiency and inconsistency can not be caused by users.
                    for (int i = 0; i < neuron.Predictors.Length; i++)
                    {
                        if (neuron.PredictorSwitches[i])
                        {
                            outSectionsData[i].Item2[sectionsOutIdx[i]] = neuron.Predictors[i];
                            ++sectionsOutIdx[i];
                        }
                    }
                }
                //Reset state
                ResetState();
            }
            for (int timepointIdx = 0; timepointIdx < numOfTimepoints; timepointIdx++)
            {
                double[] timepointData = inputPattern.GetDataAt(timepointIdx);
                PushTimepointData(timepointData, neuronStats);
                //Booting countdown
                if (ResCfg.InputCfg.Feeding == InputFeeding.TimePoint && _bootingCountdown > 0) --_bootingCountdown;
            }
            //Update stat and prepare output
            if (Ready)
            {
                //Build output
                foreach (ReservoirNeuron neuron in _hiddenNeurons)
                {
                    //Quite dangerous because the weak relation between Res OutSections and neuron predictors.
                    //Both must be synchronized in terms of order and count.
                    //But this is the cost of efficiency and inconsistency can not be caused by users.
                    for (int i = 0; i < neuron.Predictors.Length; i++)
                    {
                        if (neuron.PredictorSwitches[i])
                        {
                            outSectionsData[i].Item2[sectionsOutIdx[i]] = neuron.Predictors[i];
                            ++sectionsOutIdx[i];
                        }
                    }
                }
                return (from section in outSectionsData select section.Item2).Flattenize();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Performs reservoir computation.
        /// </summary>
        /// <param name="input">Input data.</param>
        /// <param name="outSectionsData">All reservoir's outputs divided into the sections following OutSection enum. Each tuple contains section name and section data.</param>
        /// <returns>Original input (if allowed) and all reservoir's outputs. Section by section (see OutSection enum) in a flat 1D array.</returns>
        public double[] Compute(double[] input, out List<Tuple<string, double[]>> outSectionsData)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Reservoir is not initialized. Call Init method first.");
            }
            return ComputeInternal(input, out outSectionsData, null);
        }

        /// <inheritdoc/>
        public double[] Compute(double[] input)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Reservoir is not initialized. Call Init method first.");
            }
            return ComputeInternal(input, out _, null);
        }

        private ReservoirStat FinalizeOutput(ReservoirNeuronStat[] neuronsStats, List<List<Tuple<string, double[]>>> bulkResOutSectionsData)
        {
            const double BlockingBorder = 1e-6d;
            //Before stat finalization check the output predictors
            int totalNumOfBlockedPredictors = 0;
            int predictorOutCount = ResCfg.InputCfg.Feeding != InputFeeding.TimePoint ? 2 : 1;
            int[] numOfBlockedPredictors = new int[_numOfOutputSections - 1];
            Array.Fill(numOfBlockedPredictors, 0);
            for (int neuronIdx = 0; neuronIdx < _hiddenNeurons.Length; neuronIdx++)
            {
                for(int predictorIdx = 0; predictorIdx < _numOfOutputSections - 1; predictorIdx++)
                {
                    BasicStat predictorStat = new BasicStat();
                    for (int rowIdx = 0; rowIdx < bulkResOutSectionsData.Count; rowIdx++)
                    {
                        predictorStat.AddSample(bulkResOutSectionsData[rowIdx][predictorIdx].Item2[neuronIdx]);
                    }
                    //Block?
                    if(predictorStat.Span <= BlockingBorder ||  predictorStat.StdDev <= BlockingBorder)
                    {
                        //Block predictor
                        _hiddenNeurons[neuronIdx].PredictorSwitches[predictorIdx] = false;
                        numOfBlockedPredictors[predictorIdx] += predictorOutCount;
                        OutSectionsLengths[predictorIdx] -= predictorOutCount;
                        totalNumOfBlockedPredictors += predictorOutCount;
                    }
                }
            }
            //Rebuild output
            if(totalNumOfBlockedPredictors > 0)
            {
                for (int predictorIdx = 0; predictorIdx < _numOfOutputSections - 1; predictorIdx++)
                {
                    if(numOfBlockedPredictors[predictorIdx]  > 0)
                    {
                        for (int rowIdx = 0; rowIdx < bulkResOutSectionsData.Count; rowIdx++)
                        {
                            int firstHalfOutIdx = 0;
                            int secondHalfOutIdx = OutSectionsLengths[predictorIdx] / 2;
                            double[] filteredPredictors = new double[OutSectionsLengths[predictorIdx]];
                            for (int neuronIdx = 0; neuronIdx < _hiddenNeurons.Length; neuronIdx++)
                            {
                                if (_hiddenNeurons[neuronIdx].PredictorSwitches[predictorIdx])
                                {
                                    filteredPredictors[firstHalfOutIdx++] = bulkResOutSectionsData[rowIdx][predictorIdx].Item2[neuronIdx];
                                    if(ResCfg.InputCfg.Feeding != InputFeeding.TimePoint)
                                    {
                                        filteredPredictors[secondHalfOutIdx++] = bulkResOutSectionsData[rowIdx][predictorIdx].Item2[_hiddenNeurons.Length + neuronIdx];
                                    }
                                }
                            }//neuronIdx
                            //Set filtered predictors data
                            bulkResOutSectionsData[rowIdx][predictorIdx] = new Tuple<string, double[]>(_outputSectionNames[predictorIdx], filteredPredictors);
                        }//rowIdx
                    }
                }//predictorIdx
            }
            return new ReservoirStat(neuronsStats, numOfBlockedPredictors);
        }

        /// <summary>
        /// Computes input data and initializes reservoir.
        /// </summary>
        /// <param name="inputData">A list of input vectors.</param>
        /// <param name="bulkResOutSectionsData">Collection of collection of reservoir's outputs per section.</param>
        /// <param name="stat">Resulting reservoir's stat.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Reservoir's computed outputs in a flat arrays.</returns>
        public List<double[]> Init(List<double[]> inputData,
                                   out List<List<Tuple<string, double[]>>> bulkResOutSectionsData,
                                   out ReservoirStat stat,
                                   ReservoirInitProgressChangedHandler progressInfoSubscriber = null
                                   )
        {
            if (progressInfoSubscriber != null)
            {
                InitProgressChanged += progressInfoSubscriber;
            }
            try
            {
                if (_initialized)
                {
                    Reset();
                }
                //Setup properly ResInputs out section length
                if(ResCfg.InputCfg.Feeding != InputFeeding.PatternVarLength)
                {
                    //ResInputs has constant length -> allowed
                    OutSectionsLengths[(int)OutSection.ResInputs] = inputData[0].Length;
                }
                else
                {
                    //ResInputs has varying length -> forbidden
                    OutSectionsLengths[(int)OutSection.ResInputs] = 0;
                }
                //Convert data to input patterns
                TimeSeriesPattern[] patterns = new TimeSeriesPattern[inputData.Count];
                Parallel.ForEach(Partitioner.Create(0, inputData.Count), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        patterns[i] = new TimeSeriesPattern(inputData[i], _inputFilters.Length, ResCfg.InputCfg.VarSchema);
                    }
                });
                //Setup filters of input variables
                Parallel.ForEach(Partitioner.Create(0, _inputFilters.Length), range =>
                {
                    foreach (TimeSeriesPattern pattern in patterns)
                    {
                        for (int varIdx = range.Item1; varIdx < range.Item2; varIdx++)
                        {
                            double[] varData = pattern.VariablesDataCollection[varIdx];
                            for (int i = 0; i < varData.Length; i++)
                            {
                                _inputFilters[varIdx].Update(varData[i]);
                            }
                        }
                    }
                });
                //Preapare neurons stats
                stat = null;
                ReservoirNeuronStat[] neuronsStats = new ReservoirNeuronStat[_hiddenNeurons.Length];
                for (int i = 0; i < _hiddenNeurons.Length; i++)
                {
                    neuronsStats[i] = new ReservoirNeuronStat();
                }
                //Process flat input data [second patternization :-( ]
                bulkResOutSectionsData = new List<List<Tuple<string, double[]>>>(inputData.Count);
                List<double[]> flatOutputs = new List<double[]>(inputData.Count);
                int numOfProcessedInputs = 0;
                foreach (double[] input in inputData)
                {
                    bool theLast = (numOfProcessedInputs == inputData.Count - 1);
                    if (_bootingCountdown == 0 && !_initialized)
                    {
                        _initialized = true;
                    }
                    double[] output = ComputeInternal(input, out List<Tuple<string, double[]>> outSectionsData, _initialized ? neuronsStats : null);
                    if (output != null)
                    {
                        bulkResOutSectionsData.Add(outSectionsData);
                        flatOutputs.Add(output);
                    }
                    if (theLast)
                    {
                        //Stat and output finalization
                        stat = FinalizeOutput(neuronsStats, bulkResOutSectionsData);
                    }
                    //Progress
                    ReservoirInitProgressInfo progressInfo =
                        new ReservoirInitProgressInfo(++numOfProcessedInputs,
                                                      inputData.Count,
                                                      flatOutputs.Count,
                                                      stat
                                                      );
                    //Raise notification event
                    InitProgressChanged?.Invoke(progressInfo);
                }
                return flatOutputs;
            }
            finally
            {
                if (progressInfoSubscriber != null)
                {
                    InitProgressChanged -= progressInfoSubscriber;
                }
            }
        }

        /// <summary>
        /// Gets formatted text containing info about this Reservoir instance.
        /// </summary>
        /// <param name="detail">Specifies whether to provide max detail.</param>
        /// <param name="margin">Specifies left margin.</param>
        /// <returns>Formatted text containing info about this Reservoir instance.</returns>
        public string GetInfoText(bool detail = false, int margin = 0)
        {
            margin = Math.Max(margin, 0);
            StringBuilder sb = new StringBuilder($"Reccurent reservoir:{Environment.NewLine}");
            sb.Append($"    Ready            : {Ready.GetXmlCode()}{Environment.NewLine}");
            sb.Append($"    Input neurons    : {NumOfInputNeurons.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Hidden neurons   : {NumOfHiddenNeurons.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Hidden activation: {HiddenActivationFnID.ToString()}{Environment.NewLine}");
            sb.Append($"    Input synapses   : {NumOfInputSynapses.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if(detail)
            {
                sb.Append($"        Input synapses weights:{Environment.NewLine}");
                sb.Append($"{InputSynapsesWeightStat.GetInfoText(12, BasicStat.StatisticalFigure.Min, BasicStat.StatisticalFigure.Max, BasicStat.StatisticalFigure.ArithAvg, BasicStat.StatisticalFigure.RootMeanSquare, BasicStat.StatisticalFigure.StdDev)}");
            }
            sb.Append($"    Hidden synapses  : {NumOfHiddenSynapses.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if (detail)
            {
                sb.Append($"        Hidden synapses weights:{Environment.NewLine}");
                sb.Append($"{HiddenSynapsesWeightStat.GetInfoText(12, BasicStat.StatisticalFigure.Min, BasicStat.StatisticalFigure.Max, BasicStat.StatisticalFigure.ArithAvg, BasicStat.StatisticalFigure.RootMeanSquare, BasicStat.StatisticalFigure.StdDev)}");
            }
            sb.Append($"    Output features  : {NumOfOutputFeatures.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            string infoText = sb.ToString();
            if (margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }

        /// <summary>
        /// Creates deep clone.
        /// </summary>
        public Reservoir DeepClone()
        {
            return new Reservoir( this );
        }

    }//Reservoir
}//Namespace
