using EasyMLCore.Activation;
using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.Loss;
using EasyMLCore.MathTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the Multi Layer Perceptron network engine.
    /// </summary>
    [Serializable]
    public class MLPEngine : SerializableObject, IComputableTaskSpecific
    {
        //Constants
        /// <summary>
        /// Specifies whether to center features when applaying feature filters.
        /// </summary>
        public const bool UseCenteredFeatures = true;

        //Enumerations
        //Attribute properties
        /// <inheritdoc cref="OutputTaskType"/>
        public OutputTaskType TaskType { get; }

        /// <summary>
        /// Loss function.
        /// </summary>
        public ILossFn LossFn { get; }

        /// <summary>
        /// Number of input features.
        /// </summary>
        public int NumOfInputFeatures { get; }

        /// <inheritdoc/>
        public List<string> OutputFeatureNames { get; }

        /// <inheritdoc/>
        public int NumOfOutputFeatures { get; }

        /// <summary>
        /// Number of network neurons.
        /// </summary>
        public int NumOfNeurons { get; private set; }

        /// <summary>
        /// The collection of network layers.
        /// </summary>
        public List<Layer> LayerCollection { get; }

        /// <summary>
        /// Weights statistics on hidden layers (biases not included).
        /// </summary>
        public BasicStat HLWeightsStat { get; }

        /// <summary>
        /// Weights statistics on output layer (biases not included).
        /// </summary>
        public BasicStat OLWeightsStat { get; }


        //Attributes
        /// <summary>
        /// Network weights in a flat structure.
        /// </summary>
        private readonly double[] _flatWeights;


        //Constructors
        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public MLPEngine(MLPEngine source)
        {
            TaskType = source.TaskType;
            LossFn = source.LossFn.DeepClone();
            NumOfInputFeatures = source.NumOfInputFeatures;
            OutputFeatureNames = new List<string>(source.OutputFeatureNames);
            NumOfOutputFeatures = source.NumOfOutputFeatures;
            NumOfNeurons = source.NumOfNeurons;
            LayerCollection = new List<Layer>(source.LayerCollection.Count);
            foreach (Layer layer in source.LayerCollection)
            {
                LayerCollection.Add(layer.DeepClone());
            }
            _flatWeights = (double[])source._flatWeights.Clone();
            HLWeightsStat = source.HLWeightsStat.DeepClone();
            OLWeightsStat = source.OLWeightsStat.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance ready for training.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setup of output layer activation together with the loss function is determined automatically depending on the output task type and number of output features.
        /// </para>
        /// </remarks>
        /// <param name="taskType">Network's output task type.</param>
        /// <param name="numOfInputFeatures">Number of the network's input features.</param>
        /// <param name="outputFeatureNames">Names of the network's output features.</param>
        /// <param name="networkModelCfg">Configuration of the network model.</param>
        public MLPEngine(OutputTaskType taskType,
                         int numOfInputFeatures,
                         IEnumerable<string> outputFeatureNames,
                         NetworkModelConfig networkModelCfg
                         )
        {
            //Network's output task
            TaskType = taskType;
            //Input/Output counts
            NumOfInputFeatures = numOfInputFeatures;
            OutputFeatureNames = new List<string>(outputFeatureNames);
            NumOfOutputFeatures = OutputFeatureNames.Count;
            //Network layers
            LayerCollection = new List<Layer>();
            NumOfNeurons = 0;
            int numOfLayerInputNodes = NumOfInputFeatures;
            int weightsFlatStartIdx = 0;
            //Hidden layers
            for (int i = 0; i < networkModelCfg.HiddenLayersCfg.LayerCfgCollection.Count; i++)
            {
                Layer layer = new Layer(networkModelCfg.HiddenLayersCfg.LayerCfgCollection[i].NumOfNeurons,
                                        networkModelCfg.HiddenLayersCfg.LayerCfgCollection[i].ActivationID,
                                        numOfLayerInputNodes,
                                        NumOfNeurons,
                                        weightsFlatStartIdx,
                                        (i == 0),
                                        false
                                        );
                LayerCollection.Add(layer);
                NumOfNeurons += layer.NumOfLayerNeurons;
                weightsFlatStartIdx += layer.NumOfLayerNeurons * layer.NumOfInputNodes + layer.NumOfLayerNeurons;
                numOfLayerInputNodes = layer.NumOfLayerNeurons;
            }
            //Output layer
            //Automatically determine the output activation and corresponding loss function
            ActivationFnID outputActivationID;
            if (TaskType == OutputTaskType.Binary)
            {
                outputActivationID = ActivationFnID.Sigmoid;
                LossFn = new SigmoidCrossEntropyLoss();
            }
            else if (TaskType == OutputTaskType.Categorical)
            {
                outputActivationID = ActivationFnID.Softmax;
                LossFn = new SoftmaxCrossEntropyLoss();
            }
            else
            {
                outputActivationID = ActivationFnID.Linear;
                LossFn = new SquaredErrorLoss();
            }
            //Create output layer
            Layer outputLayer = new Layer(NumOfOutputFeatures,
                                          outputActivationID,
                                          numOfLayerInputNodes,
                                          NumOfNeurons,
                                          weightsFlatStartIdx,
                                          (LayerCollection.Count == 0),
                                          true
                                          );
            //Add output layer to network's layers
            LayerCollection.Add(outputLayer);
            NumOfNeurons += NumOfOutputFeatures;
            weightsFlatStartIdx += outputLayer.NumOfLayerNeurons * outputLayer.NumOfInputNodes + outputLayer.NumOfLayerNeurons;
            //Allocate weights flat buffer
            _flatWeights = new double[weightsFlatStartIdx];
            HLWeightsStat = new BasicStat();
            OLWeightsStat = new BasicStat();
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
        public void RandomizeWeights(Random rand)
        {
            foreach (Layer layer in LayerCollection)
            {
                layer.RandomizeWights(_flatWeights, rand);
            }
            return;
        }

        /// <summary>
        /// Actualizes weights statistics.
        /// </summary>
        private void ActualizeWeightsStat()
        {
            HLWeightsStat.Reset();
            OLWeightsStat.Reset();
            for (int layerIdx = 0; layerIdx < LayerCollection.Count; layerIdx++)
            {
                int weightFlatIdx = LayerCollection[layerIdx].WeightsStartFlatIdx;
                int biasWeightFlatIdx = LayerCollection[layerIdx].BiasesStartFlatIdx;
                for (int i = 0; i < LayerCollection[layerIdx].NumOfLayerNeurons; i++, biasWeightFlatIdx++)
                {
                    for (int j = 0; j < LayerCollection[layerIdx].NumOfInputNodes; j++, weightFlatIdx++)
                    {
                        if (layerIdx < LayerCollection.Count - 1)
                        {
                            HLWeightsStat.AddSample(_flatWeights[weightFlatIdx]);
                        }
                        else
                        {
                            OLWeightsStat.AddSample(_flatWeights[weightFlatIdx]);
                        }
                    }
                }
            }
            return;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Version for single computation.
        /// </remarks>
        public double[] Compute(double[] input)
        {
            double[] result = input;
            foreach (Layer layer in LayerCollection)
            {
                result = layer.Compute(result, _flatWeights);
            }
            return result;
        }

        /// <summary>
        /// Computes the network.
        /// </summary>
        /// <remarks>
        /// Version for multiple computations.
        /// </remarks>
        /// <param name="flatActivations">Allocated buffer with initialized network input and space for all neuron activations.</param>
        /// <param name="flatSums">Allocated buffer for all neuron summed inputs.</param>
        /// <returns>Index of the first computed output feature within the flatActivations.</returns>
        public int Compute(double[] flatActivations, double[] flatSums)
        {
            int layerInputStartFlatIdx = 0;
            for (int layerIdx = 0; layerIdx < LayerCollection.Count; layerIdx++)
            {
                Layer layer = LayerCollection[layerIdx];
                //Compute neurons
                for (int neuronIdx = 0, neuronFlatIdx = layer.NeuronsStartFlatIdx; neuronIdx < layer.NumOfLayerNeurons; neuronIdx++, neuronFlatIdx++)
                {
                    int weightFlatIdx = layer.WeightsStartFlatIdx + neuronIdx * layer.NumOfInputNodes;
                    flatSums[neuronFlatIdx] = _flatWeights[layer.BiasesStartFlatIdx + neuronIdx];
                    for (int inputIdx = 0; inputIdx < layer.NumOfInputNodes; inputIdx++)
                    {
                        flatSums[neuronFlatIdx] += _flatWeights[weightFlatIdx + inputIdx] * flatActivations[layerInputStartFlatIdx + inputIdx];
                    }
                    if (!layer.OutputLayer)
                    {
                        //Compute hidden layer neuron activation
                        flatActivations[NumOfInputFeatures + neuronFlatIdx] =
                            layer.Activation.Compute(flatSums[neuronFlatIdx]);
                    }
                }
                if (layer.OutputLayer)
                {
                    //Compute output layer neuron activations
                    layer.Activation.Compute(flatSums, layer.NeuronsStartFlatIdx, flatActivations, NumOfInputFeatures + layer.NeuronsStartFlatIdx, layer.NumOfLayerNeurons);
                }
                layerInputStartFlatIdx += layer.NumOfInputNodes;
            }
            return layerInputStartFlatIdx;
        }

        /// <summary>
        /// Gets a copy of internal weights (in a flat format).
        /// </summary>
        public double[] GetWeightsCopy()
        {
            return (double[])_flatWeights.Clone();
        }

        /// <summary>
        /// Sets the internal weights.
        /// </summary>
        /// <param name="newFlatWeights">New flat formatted weights to be adopted.</param>
        public void SetWeights(double[] newFlatWeights)
        {
            newFlatWeights.CopyTo(_flatWeights, 0);
            ActualizeWeightsStat();
            return;
        }

        /// <inheritdoc/>
        public TaskOutputDetailBase GetOutputDetail(double[] outputData)
        {
            return TaskType switch
            {
                OutputTaskType.Regression => new RegressionOutputDetail(OutputFeatureNames, outputData),
                OutputTaskType.Binary => new BinaryOutputDetail(OutputFeatureNames, outputData),
                OutputTaskType.Categorical => new CategoricalOutputDetail(OutputFeatureNames, outputData),
                _ => null,
            };
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public MLPEngine DeepClone()
        {
            return new MLPEngine(this);
        }

        //Inner classes
        /// <summary>
        /// Implements network's layer.
        /// </summary>
        [Serializable]
        public class Layer
        {
            //Attribute properties
            /// <summary>
            /// The activation function of the layer.
            /// </summary>
            public ActivationBase Activation { get; }
            /// <summary>
            /// The number of layer input nodes.
            /// </summary>
            public int NumOfInputNodes { get; private set; }
            /// <summary>
            /// The number of layer neurons.
            /// </summary>
            public int NumOfLayerNeurons { get; }
            /// <summary>
            /// The starting index of this layer weights in a flat structure.
            /// </summary>
            public int WeightsStartFlatIdx { get; private set; }
            /// <summary>
            /// The starting index of this layer biases in a flat structure.
            /// </summary>
            public int BiasesStartFlatIdx { get; private set; }
            /// <summary>
            /// The starting index of this layer neurons in a flat structure.
            /// </summary>
            public int NeuronsStartFlatIdx { get; private set; }
            /// <summary>
            /// Identifies the first layer of the network.
            /// </summary>
            public bool FirstLayer { get; }
            /// <summary>
            /// Identifies output layer.
            /// </summary>
            public bool OutputLayer { get; }

            //Constructor
            /// <summary>
            /// Copy constructor.
            /// </summary>
            /// <param name="source">Source instance.</param>
            internal Layer(Layer source)
            {
                Activation = source.Activation.Clone();
                NumOfInputNodes = source.NumOfInputNodes;
                NumOfLayerNeurons = source.NumOfLayerNeurons;
                WeightsStartFlatIdx = source.WeightsStartFlatIdx;
                BiasesStartFlatIdx = source.BiasesStartFlatIdx;
                NeuronsStartFlatIdx = source.NeuronsStartFlatIdx;
                FirstLayer = source.FirstLayer;
                OutputLayer = source.OutputLayer;
                return;
            }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="numOfNeurons">Number of layer neurons.</param>
            /// <param name="activationID">Activation function identifier.</param>
            /// <param name="numOfInputNodes">Number of input nodes.</param>
            /// <param name="neuronsFlatStartIdx">The starting index of this layer neurons in a flat structure.</param>
            /// <param name="weightsFlatStartIdx">The starting index of this layer weights in a flat structure.</param>
            /// <param name="firstLayer">Identifies the first layer of the network.</param>
            /// <param name="outputLayer">Identifies output layer.</param>
            internal Layer(int numOfNeurons,
                           ActivationFnID activationID,
                           int numOfInputNodes,
                           int neuronsFlatStartIdx,
                           int weightsFlatStartIdx,
                           bool firstLayer,
                           bool outputLayer
                           )
            {
                NumOfLayerNeurons = numOfNeurons;
                Activation = ActivationFactory.CreateActivationFn(activationID);
                NumOfInputNodes = numOfInputNodes;
                NeuronsStartFlatIdx = neuronsFlatStartIdx;
                WeightsStartFlatIdx = weightsFlatStartIdx;
                BiasesStartFlatIdx = weightsFlatStartIdx + NumOfLayerNeurons * NumOfInputNodes;
                FirstLayer = firstLayer;
                OutputLayer = outputLayer;
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
            /// <param name="flatWeights">Network's weights in a flat structure.</param>
            /// <param name="rand">Random generator to be used.</param>
            internal void RandomizeWights(double[] flatWeights, Random rand)
            {
                double[] wBuff = new double[NumOfInputNodes * NumOfLayerNeurons];
                int weightFlatIndex = WeightsStartFlatIdx;
                int biasFlatIndex = BiasesStartFlatIdx;
                double reqStdDev = Activation.GetNormalInitWeightsStdDev(NumOfInputNodes, NumOfLayerNeurons);
                rand.FillGaussianDouble(wBuff, 0d, reqStdDev);
                for (int layerNeuronIdx = 0, wBuffIdx = 0; layerNeuronIdx < NumOfLayerNeurons; layerNeuronIdx++, biasFlatIndex++)
                {
                    for (int inputNodeIdx = 0; inputNodeIdx < NumOfInputNodes; inputNodeIdx++, weightFlatIndex++, wBuffIdx++)
                    {
                        flatWeights[weightFlatIndex] = wBuff[wBuffIdx];
                    }
                    if (OutputLayer && Activation.ID == ActivationFnID.Softmax)
                    {
                        //Bias setup for the Categorical task to avoid huge initial loss
                        flatWeights[biasFlatIndex] = -Math.Log(NumOfLayerNeurons - 1d);
                    }
                    else
                    {
                        flatWeights[biasFlatIndex] = 0d;
                    }
                }
                return;
            }

            /// <summary>
            /// Computes layer.
            /// </summary>
            /// <param name="inputs">The inputs for this layer.</param>
            /// <param name="flatWeights">All network's weights in a flat structure.</param>
            /// <returns>Layer's activations.</returns>
            internal double[] Compute(double[] inputs, double[] flatWeights)
            {
                //Compute summed weighted inputs
                double[] sums = new double[NumOfLayerNeurons];
                double[] activations = new double[NumOfLayerNeurons];
                for (int neuronIdx = 0; neuronIdx < NumOfLayerNeurons; neuronIdx++)
                {
                    int weightFlatIdx = WeightsStartFlatIdx + neuronIdx * NumOfInputNodes;
                    sums[neuronIdx] = flatWeights[BiasesStartFlatIdx + neuronIdx];
                    for (int inputIdx = 0; inputIdx < NumOfInputNodes; inputIdx++)
                    {
                        sums[neuronIdx] += flatWeights[weightFlatIdx + inputIdx] * inputs[inputIdx];
                    }
                    if (!OutputLayer)
                    {
                        //Compute hidden layer neuron
                        activations[neuronIdx] = Activation.Compute(sums[neuronIdx]);
                    }
                }
                if (OutputLayer)
                {
                    //Compute output layer neurons
                    Activation.Compute(sums, 0, activations, 0, NumOfLayerNeurons);
                }
                return activations;
            }

        }//Layer

    }//MLPEngine

}//Namespace
