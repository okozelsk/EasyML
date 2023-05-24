using EasyMLCore.Activation;
using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.Loss;
using EasyMLCore.MathTools;
using EasyMLCore.TimeSeries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyMLCore.MLP
{
    //Delegates
    /// <summary>
    /// Delegate of the RVFLInitProgressChanged event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the init process.</param>
    public delegate void RVFLInitProgressChangedHandler(RVFLInitProgressInfo progressInfo);

    /// <summary>
    /// Implements the RVFL preprocessor.
    /// </summary>
    [Serializable]
    public class RVFLPreprocessor : SerializableObject, IComputable
    {
        //Constants
        /// <summary>
        /// Short identifier for context path.
        /// </summary>
        public const string ContextPathID = "RVFLPreprocessor";

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the init process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        private event RVFLInitProgressChangedHandler InitProgressChanged;


        //Attribute properties
        /// <summary>
        /// Number of input features.
        /// </summary>
        public int NumOfInputFeatures { get; }

        /// <inheritdoc/>
        public int NumOfOutputFeatures { get; }

        /// <summary>
        /// Number of network neurons.
        /// </summary>
        public int NumOfNeurons { get; private set; }

        /// <summary>
        /// Number of predictors.
        /// </summary>
        public int NumOfPredictors { get; }

        /// <summary>
        /// The collection of layers.
        /// </summary>
        public List<Layer> LayerCollection { get; }

        /// <summary>
        /// Weights statistics.
        /// </summary>
        public BasicStat WeightsStat { get; }

        /// <summary>
        /// Biases statistics.
        /// </summary>
        public BasicStat BiasesStat { get; }

        /// <summary>
        /// Indicates initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        //Attributes
        /// <summary>
        /// Scale factor of the first layer's weigths.
        /// </summary>
        private double _scaleFactor;
        /// <summary>
        /// All weights in a flat structure.
        /// </summary>
        private readonly double[] _flatWeights;
        /// <summary>
        /// Input filters.
        /// </summary>
        private readonly FeatureFilterBase[] _inputFilters;

        //Constructors
        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RVFLPreprocessor(RVFLPreprocessor source)
        {
            NumOfInputFeatures = source.NumOfInputFeatures;
            NumOfOutputFeatures = source.NumOfOutputFeatures;
            NumOfNeurons = source.NumOfNeurons;
            NumOfPredictors = source.NumOfPredictors;
            LayerCollection = new List<Layer>(source.LayerCollection.Count);
            foreach (Layer layer in source.LayerCollection)
            {
                LayerCollection.Add(layer.DeepClone());
            }
            _scaleFactor = source._scaleFactor;
            _flatWeights = (double[])source._flatWeights.Clone();
            _inputFilters = new FeatureFilterBase[source._inputFilters.Length];
            for(int i = 0; i < _inputFilters.Length; i++)
            {
                _inputFilters[i] = source._inputFilters[i].DeepClone();
            }
            WeightsStat = new BasicStat(source.WeightsStat);
            BiasesStat = new BasicStat(source.BiasesStat);
            Initialized = source.Initialized;
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="numOfInputFeatures">Number of input features.</param>
        /// <param name="modelCfg">Configuration of the RVFL model.</param>
        public RVFLPreprocessor(int numOfInputFeatures,
                                RVFLModelConfig modelCfg
                                )
        {
            NumOfInputFeatures = numOfInputFeatures;
            NumOfNeurons = 0;
            NumOfPredictors = 0;
            //Layers
            LayerCollection = new List<Layer>(modelCfg.LayersCfg.LayerCfgCollection.Count);
            int weightsFlatStartIdx = 0;
            int neuronsFlatStartIdx = 0;
            foreach (RVFLHiddenLayerConfig layerCfg in modelCfg.LayersCfg.LayerCfgCollection)
            {
                int numOfLayerInputNodes;
                if(LayerCollection.Count == 0)
                {
                    numOfLayerInputNodes = NumOfInputFeatures;
                }
                else
                {
                    numOfLayerInputNodes = LayerCollection[LayerCollection.Count - 1].NumOfLayerNeurons;
                }
                Layer layer = new Layer(LayerCollection.Count, numOfLayerInputNodes, neuronsFlatStartIdx, weightsFlatStartIdx, layerCfg);
                LayerCollection.Add(layer);
                weightsFlatStartIdx += layer.NumOfLayerWeights;
                neuronsFlatStartIdx += layer.NumOfLayerNeurons;
                NumOfNeurons += layer.NumOfLayerNeurons;
                NumOfPredictors += layer.NumOfPredictors;
            }
            NumOfOutputFeatures = NumOfPredictors + (modelCfg.RouteInput ? NumOfInputFeatures : 0);
            _scaleFactor = modelCfg.ScaleFactor;
            _flatWeights = new double[weightsFlatStartIdx];
            _inputFilters = new FeatureFilterBase[NumOfInputFeatures];
            for(int i = 0; i < _inputFilters.Length; i++)
            {
                _inputFilters[i] = new RealFeatureFilter(FeatureFilterBase.FeatureUse.Input);
            }
            WeightsStat = new BasicStat();
            BiasesStat = new BasicStat();
            Initialized = false;
            return;
        }

        //Properties
        /// <summary>
        /// The number of network's internal weights.
        /// </summary>
        public int NumOfWeights { get { return _flatWeights.Length; } }

        //Methods
        /// <summary>
        /// Randomizes internal weights.
        /// </summary>
        /// <param name="rand">The random generator to be used.</param>
        private void RandomizeWeights(Random rand)
        {
            WeightsStat.Reset();
            BiasesStat.Reset();
            foreach (Layer layer in LayerCollection)
            {
                layer.RandomizeWights(_flatWeights, rand, _scaleFactor);
                WeightsStat.Merge(layer.WeightsStat);
                BiasesStat.Merge(layer.BiasesStat);
            }
            return;
        }

        private double[] Normalize(double[] input)
        {
            double[] stdInput = new double[input.Length];
            for(int i = 0; i < input.Length; i++)
            {
                stdInput[i] = _inputFilters[i].ApplyFilter(input[i]);
            }
            return stdInput;
        }

        private double[] ComputeInternal(double[] input)
        {
            double[] stdInput = Normalize(input);
            double[] output = new double[NumOfOutputFeatures];
            int outputIdx = 0;
            if (NumOfOutputFeatures > NumOfPredictors)
            {
                input.CopyTo(output, outputIdx);
                outputIdx += NumOfInputFeatures;
            }
            double[] layerInput = stdInput;
            foreach (Layer layer in LayerCollection)
            {
                layerInput = layer.Compute(layerInput, _flatWeights, out double[] predictors);
                if (predictors.Length > 0)
                {
                    predictors.CopyTo(output, outputIdx);
                    outputIdx += predictors.Length;
                }
            }
            return output;
        }

        /// <inheritdoc/>
        public double[] Compute(double[] input)
        {
            if(input == null)
            {
                throw new ArgumentNullException("input");
            }
            if(input.Length != NumOfInputFeatures)
            {
                throw new ArgumentException($"Invalid number of input features {input.Length}. Epected {NumOfInputFeatures}.", nameof(input));
            }
            return ComputeInternal(input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trainingData">Training data.</param>
        /// <param name="rand">Random object to be used.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Preprocessed data.</returns>
        public SampleDataset Init(SampleDataset trainingData,
                                  Random rand,
                                  RVFLInitProgressChangedHandler progressInfoSubscriber = null
                                  )
        {
            if (progressInfoSubscriber != null)
            {
                InitProgressChanged += progressInfoSubscriber;
            }
            try
            {
                //Initialization
                Initialized = false;
                //New weights
                RandomizeWeights(rand);
                //Input filters
                for (int i = 0; i < _inputFilters.Length; i++)
                {
                    _inputFilters[i].Reset();
                }
                Parallel.For(0, _inputFilters.Length, featureIdx =>
                {
                    for (int sampleIdx = 0; sampleIdx < trainingData.SampleCollection.Count; sampleIdx++)
                    {
                        _inputFilters[featureIdx].Update(trainingData.SampleCollection[sampleIdx].InputVector[featureIdx]);
                    }
                });
                //Input data
                double[][] stdInputs = new double[trainingData.Count][];
                for (int i = 0; i < trainingData.Count; i++)
                {
                    stdInputs[i] = new double[trainingData.FirstInputVectorLength];
                }
                Parallel.For(0, _inputFilters.Length, featureIdx =>
                {
                    for (int sampleIdx = 0; sampleIdx < trainingData.SampleCollection.Count; sampleIdx++)
                    {
                        stdInputs[sampleIdx][featureIdx] = _inputFilters[featureIdx].ApplyFilter(trainingData.SampleCollection[sampleIdx].InputVector[featureIdx]);
                    }
                });
                //Output
                SampleDataset outputData = new SampleDataset(trainingData.Count);
                int numOfProcessedInputs = 0;
                foreach (Sample sample in trainingData.SampleCollection)
                {
                    double[] computed = ComputeInternal(sample.InputVector);
                    outputData.AddSample(sample.ID, computed, sample.OutputVector);
                    //Progress
                    RVFLInitProgressInfo progressInfo =
                        new RVFLInitProgressInfo(++numOfProcessedInputs,
                                                 trainingData.Count
                                                 );
                    //Raise notification event
                    InitProgressChanged?.Invoke(progressInfo);
                }
                Initialized = true;
                return outputData;
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
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public RVFLPreprocessor DeepClone()
        {
            return new RVFLPreprocessor(this);
        }

        //Inner classes
        /// <summary>
        /// Implements RVFL's layer.
        /// </summary>
        [Serializable]
        public class Layer
        {
            //Attribute properties
            /// <summary>
            /// Layer index.
            /// </summary>
            public int LayerIdx { get; }
            /// <summary>
            /// Layer's pools.
            /// </summary>
            public List<Pool> Pools { get; }
            /// <summary>
            /// The number of layer input nodes.
            /// </summary>
            public int NumOfInputNodes { get; private set; }
            /// <summary>
            /// The number of layer neurons.
            /// </summary>
            public int NumOfLayerNeurons { get; }
            /// <summary>
            /// The number of layer weights.
            /// </summary>
            public int NumOfLayerWeights { get; }
            /// <summary>
            /// The number of layer predictors.
            /// </summary>
            public int NumOfPredictors { get; }
            /// <summary>
            /// Weights statistics.
            /// </summary>
            public BasicStat WeightsStat { get; }
            /// <summary>
            /// Biases statistics.
            /// </summary>
            public BasicStat BiasesStat { get; }

            //Constructor
            /// <summary>
            /// Copy constructor.
            /// </summary>
            /// <param name="source">Source instance.</param>
            internal Layer(Layer source)
            {
                LayerIdx = source.LayerIdx;
                Pools = new List<Pool>(source.Pools.Count);
                foreach(Pool pool in source.Pools)
                {
                    Pools.Add(pool.DeepClone());
                }
                NumOfInputNodes = source.NumOfInputNodes;
                NumOfLayerNeurons = source.NumOfLayerNeurons;
                NumOfLayerWeights = source.NumOfLayerWeights;
                NumOfPredictors = source.NumOfPredictors;
                WeightsStat = new BasicStat(source.WeightsStat);
                BiasesStat = new BasicStat(source.BiasesStat);
                return;
            }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="layerIdx">Layer index.</param>
            /// <param name="numOfInputNodes">Number of input nodes.</param>
            /// <param name="neuronsFlatStartIdx">The starting index of this layer neurons in a flat structure.</param>
            /// <param name="weightsFlatStartIdx">The starting index of this layer weights in a flat structure.</param>
            /// <param name="layerCfg">Hidden layer configuration.</param>
            internal Layer(int layerIdx,
                           int numOfInputNodes,
                           int neuronsFlatStartIdx,
                           int weightsFlatStartIdx,
                           RVFLHiddenLayerConfig layerCfg
                           )
            {
                LayerIdx = layerIdx;
                NumOfInputNodes = numOfInputNodes;
                NumOfLayerNeurons = 0;
                NumOfLayerWeights = 0;
                NumOfPredictors = 0;
                Pools = new List<Pool>(layerCfg.PoolCfgCollection.Count);
                foreach (RVFLHiddenPoolConfig poolCfg in layerCfg.PoolCfgCollection)
                {
                    Pool pool = new Pool(LayerIdx, numOfInputNodes, neuronsFlatStartIdx, weightsFlatStartIdx, poolCfg);
                    Pools.Add(pool);
                    NumOfLayerNeurons += pool.NumOfNeurons;
                    NumOfLayerWeights += pool.NumOfAllWeights;
                    neuronsFlatStartIdx += pool.NumOfNeurons;
                    weightsFlatStartIdx += pool.NumOfAllWeights;
                    NumOfPredictors += pool.NumOfPredictors;
                }
                WeightsStat = new BasicStat();
                BiasesStat = new BasicStat();
                return;
            }

            //Methods
            /// <summary>
            /// Creates the deep copy instance of this layer.
            /// </summary>
            internal Layer DeepClone()
            {
                return new Layer(this);
            }

            /// <summary>
            /// Randomly initializes layer weights.
            /// </summary>
            /// <param name="flatWeights">RVFL's weights in a flat structure.</param>
            /// <param name="rand">Random generator to be used.</param>
            /// <param name="scaleFactor">Scale factor of the first layer's weigths.</param>
            internal void RandomizeWights(double[] flatWeights, Random rand, double scaleFactor)
            {
                WeightsStat.Reset();
                BiasesStat.Reset();
                foreach (Pool pool in Pools)
                {
                    pool.RandomizeWights(flatWeights, rand, scaleFactor);
                    WeightsStat.Merge(pool.WeightsStat);
                    BiasesStat.Merge(pool.BiasesStat);
                }
                return;
            }

            /// <summary>
            /// Computes layer.
            /// </summary>
            /// <param name="inputs">The inputs for this layer.</param>
            /// <param name="flatWeights">All RVFL's weights in a flat structure.</param>
            /// <param name="predictors">Predictors of this layer.</param>
            /// <returns>Layer's activations.</returns>
            internal double[] Compute(double[] inputs, double[] flatWeights, out double[] predictors)
            {
                List<double[]> poolOutputs = new List<double[]>(Pools.Count);
                predictors = new double[NumOfPredictors];
                int predictorsIdx = 0;
                foreach(Pool pool in Pools)
                {
                    poolOutputs.Add(pool.Compute(inputs, flatWeights, out double[] poolPredictors));
                    poolPredictors.CopyTo(predictors, predictorsIdx);
                    predictorsIdx += pool.NumOfPredictors;
                }
                return poolOutputs.Flattenize();
            }

            //Inner classes
            /// <summary>
            /// Implements RVFL layer's pool.
            /// </summary>
            [Serializable]
            public class Pool
            {
                //Attribute properties
                /// <summary>
                /// Home layer index.
                /// </summary>
                public int LayerIdx { get; }
                /// <summary>
                /// The activation function of the layer.
                /// </summary>
                public ActivationBase Activation { get; }
                /// <summary>
                /// The number of input nodes.
                /// </summary>
                public int NumOfInputNodes { get; private set; }
                /// <summary>
                /// The number of neurons.
                /// </summary>
                public int NumOfNeurons { get; }
                /// <summary>
                /// The number of pool predictors.
                /// </summary>
                public int NumOfPredictors { get; }
                /// <summary>
                /// The starting index of this pool weights in a flat structure.
                /// </summary>
                public int WeightsStartFlatIdx { get; private set; }
                /// <summary>
                /// The starting index of this pool biases in a flat structure.
                /// </summary>
                public int BiasesStartFlatIdx { get; private set; }
                /// <summary>
                /// The starting index of this pool neurons in a flat structure.
                /// </summary>
                public int NeuronsStartFlatIdx { get; private set; }
                /// <summary>
                /// Total number of weights including biases.
                /// </summary>
                public int NumOfAllWeights { get; private set; }
                /// <summary>
                /// Weights statistics.
                /// </summary>
                public BasicStat WeightsStat { get; }
                /// <summary>
                /// Biases statistics.
                /// </summary>
                public BasicStat BiasesStat { get; }

                //Constructor
                /// <summary>
                /// Copy constructor.
                /// </summary>
                /// <param name="source">Source instance.</param>
                internal Pool(Pool source)
                {
                    LayerIdx = source.LayerIdx;
                    Activation = source.Activation.Clone();
                    NumOfInputNodes = source.NumOfInputNodes;
                    NumOfNeurons = source.NumOfNeurons;
                    NumOfPredictors = source.NumOfPredictors;
                    WeightsStartFlatIdx = source.WeightsStartFlatIdx;
                    BiasesStartFlatIdx = source.BiasesStartFlatIdx;
                    NeuronsStartFlatIdx = source.NeuronsStartFlatIdx;
                    NumOfAllWeights = source.NumOfAllWeights;
                    WeightsStat = new BasicStat(source.WeightsStat);
                    BiasesStat = new BasicStat(source.BiasesStat);
                    return;
                }

                /// <summary>
                /// Creates an initialized instance.
                /// </summary>
                /// <param name="layerIdx">Home layer index.</param>
                /// <param name="numOfInputNodes">Number of input nodes.</param>
                /// <param name="neuronsFlatStartIdx">The starting index of this pool neurons in a flat structure.</param>
                /// <param name="weightsFlatStartIdx">The starting index of this pool weights in a flat structure.</param>
                /// <param name="poolCfg">Hidden layer's pool configuration.</param>
                internal Pool(int layerIdx,
                              int numOfInputNodes,
                              int neuronsFlatStartIdx,
                              int weightsFlatStartIdx,
                              RVFLHiddenPoolConfig poolCfg
                              )
                {
                    LayerIdx = layerIdx;
                    NumOfNeurons = poolCfg.NumOfNeurons;
                    NumOfPredictors = poolCfg.UseOutput ? NumOfNeurons : 0;
                    Activation = ActivationFactory.CreateActivationFn(poolCfg.ActivationID);
                    NumOfInputNodes = numOfInputNodes;
                    NeuronsStartFlatIdx = neuronsFlatStartIdx;
                    WeightsStartFlatIdx = weightsFlatStartIdx;
                    BiasesStartFlatIdx = weightsFlatStartIdx + NumOfNeurons * NumOfInputNodes;
                    NumOfAllWeights = NumOfNeurons * NumOfInputNodes + NumOfNeurons;
                    WeightsStat = new BasicStat();
                    BiasesStat = new BasicStat();
                    return;
                }

                //Methods
                /// <summary>
                /// Creates the deep copy instance.
                /// </summary>
                internal Pool DeepClone()
                {
                    return new Pool(this);
                }

                /// <summary>
                /// Randomly initializes pool weights.
                /// </summary>
                /// <param name="flatWeights">RVFL's weights in a flat structure.</param>
                /// <param name="rand">Random generator to be used.</param>
                /// <param name="scaleFactor">Scale factor of the first layer's weigths.</param>
                internal void RandomizeWights(double[] flatWeights, Random rand, double scaleFactor)
                {
                    if(LayerIdx == 0)
                    {
                        RandomizeWightsUni(flatWeights, rand, scaleFactor);
                    }
                    else
                    {
                        RandomizeWightsStd(flatWeights, rand);
                    }

                }

                internal void RandomizeWightsUni(double[] flatWeights, Random rand, double scaleFactor)
                {
                    double[] wBuff = new double[NumOfInputNodes * NumOfNeurons];
                    rand.FillUniform(wBuff, -scaleFactor, scaleFactor, false);
                    wBuff.CopyTo(flatWeights, WeightsStartFlatIdx);
                    WeightsStat.Reset();
                    WeightsStat.AddSampleValues(wBuff);
                    double[] bBuff = new double[NumOfNeurons];
                    rand.FillUniform(bBuff, -scaleFactor, scaleFactor, false);
                    bBuff.CopyTo(flatWeights, BiasesStartFlatIdx);
                    BiasesStat.Reset();
                    BiasesStat.AddSampleValues(bBuff);
                    return;
                }

                internal void RandomizeWightsStd(double[] flatWeights, Random rand)
                {
                    double[] wBuff = new double[NumOfInputNodes * NumOfNeurons];
                    double reqStdDev = Activation.GetNormalInitWeightsStdDev(NumOfInputNodes, NumOfNeurons);
                    rand.FillGaussianDouble(wBuff, 0d, reqStdDev);
                    wBuff.CopyTo(flatWeights, WeightsStartFlatIdx);
                    WeightsStat.Reset();
                    WeightsStat.AddSampleValues(wBuff);
                    double[] bBuff = new double[NumOfNeurons];
                    rand.FillUniform(bBuff, -1d, 1d, false);
                    bBuff.CopyTo(flatWeights, BiasesStartFlatIdx);
                    BiasesStat.Reset();
                    BiasesStat.AddSampleValues(bBuff);
                    return;
                }

                /// <summary>
                /// Computes pool.
                /// </summary>
                /// <param name="inputs">The inputs.</param>
                /// <param name="flatWeights">All RVFL's weights in a flat structure.</param>
                /// <returns>Pool's activations.</returns>
                internal double[] Compute(double[] inputs, double[] flatWeights, out double[] predictors)
                {
                    double[] sums = new double[NumOfNeurons];
                    double[] activations = new double[NumOfNeurons];
                    for (int neuronIdx = 0; neuronIdx < NumOfNeurons; neuronIdx++)
                    {
                        int weightFlatIdx = WeightsStartFlatIdx + neuronIdx * NumOfInputNodes;
                        //Compute summed weighted inputs
                        sums[neuronIdx] = flatWeights[BiasesStartFlatIdx + neuronIdx];
                        for (int inputIdx = 0; inputIdx < NumOfInputNodes; inputIdx++)
                        {
                            sums[neuronIdx] += flatWeights[weightFlatIdx + inputIdx] * inputs[inputIdx];
                        }
                        //Compute activation
                        activations[neuronIdx] = Activation.Compute(sums[neuronIdx]);
                    }
                    if(NumOfPredictors > 0)
                    {
                        predictors = (double[])activations.Clone();
                    }
                    else
                    {
                        predictors = new double[0];
                    }
                    return activations;
                }

            }//Pool

        }//Layer

    }//RVFLPreprocessor

}//Namespace
