using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the base class of all activation classes.
    /// </summary>
    [Serializable]
    public abstract class ActivationBase : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Activation ID.
        /// </summary>
        public ActivationFnID ID { get; }

        /// <summary>
        /// Activation output range.
        /// </summary>
        public Interval OutputRange { get; }

        //Protected constructor
        protected ActivationBase(ActivationFnID identifier, Interval outputRange)
        {
            ID = identifier;
            OutputRange = outputRange;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates whether activation and derivative computation requieres sums on whole layer (true) or single sum only (false).
        /// </summary>
        public virtual bool RequiresWholeLayerComputation { get { return false; } }

        //Methods
        /// <summary>
        /// Computes an activation.
        /// </summary>
        /// <param name="sum">Summed activation input.</param>
        /// <returns>Computed activation.</returns>
        public abstract double Compute(double sum);

        /// <summary>
        /// Computes layer activations.
        /// </summary>
        /// <param name="sums">Summed activation inputs.</param>
        /// <param name="sIdx">Start index within the sums.</param>
        /// <param name="activations">Activations.</param>
        /// <param name="aIdx">Start index within the activations.</param>
        /// <param name="count">Number of layer neurons.</param>
        public virtual void Compute(double[] sums, int sIdx, double[] activations, int aIdx, int count)
        {
            for (int i = 0; i < count; i++, sIdx++, aIdx++)
            {
                activations[aIdx] = Compute(sums[sIdx]);
            }
            return;
        }

        /// <summary>
        /// Computes the derivative.
        /// </summary>
        /// <param name="sum">Summed activation input.</param>
        /// <param name="activation">Computed activation.</param>
        /// <returns>Computed derivative.</returns>
        public abstract double ComputeDerivative(double sum, double activation);

        /// <summary>
        /// Computes layer activations and derivatives.
        /// </summary>
        /// <param name="sums">Summed inputs of the activations.</param>
        /// <param name="sIdx">Start index within the sums.</param>
        /// <param name="activations">Activations.</param>
        /// <param name="aIdx">Start index within the activations.</param>
        /// <param name="derivatives">Derivatives</param>
        /// <param name="dIdx">Start index within the derivatives.</param>
        /// <param name="count">Number of layer neurons.</param>
        public virtual void Compute(double[] sums,
                                    int sIdx,
                                    double[] activations,
                                    int aIdx,
                                    double[] derivatives,
                                    int dIdx,
                                    int count
                                    )
        {
            for (int i = 0; i < count; i++, sIdx++, aIdx++, dIdx++)
            {
                activations[aIdx] = Compute(sums[sIdx]);
                derivatives[dIdx] = ComputeDerivative(sums[sIdx], activations[aIdx]);
            }
            return;
        }

        /// <summary>
        /// Adjusts layer activations under specified dropout mode.
        /// </summary>
        /// <param name="mode">Dropout mode.</param>
        /// <param name="dropoutP">The dropout probability.</param>
        /// <param name="rand">Random generator to be used.</param>
        /// <param name="switches">Node boolean switches.</param>
        /// <param name="sIdx">Specifies the start index within the switches.</param>
        /// <param name="activations">Activations.</param>
        /// <param name="aIdx">Specifies the start index within the activations.</param>
        /// <param name="derivatives">Derivatives (can be null).</param>
        /// <param name="dIdx">Specifies the start index within the derivatives.</param>
        /// <param name="count">Number of layer neurons.</param>
        public virtual void Dropout(DropoutMode mode,
                                    double dropoutP,
                                    Random rand,
                                    bool[] switches,
                                    int sIdx,
                                    double[] activations,
                                    int aIdx,
                                    double[] derivatives,
                                    int dIdx,
                                    int count
                                    )
        {
            if (mode == DropoutMode.Bernoulli)
            {
                //Bernoulli gate
                double enabledGateCoeff = 1d / (1d - dropoutP);
                for (int i = 0; i < count; i++, sIdx++, aIdx++, dIdx++)
                {
                    switches[sIdx] = rand.NextDouble() >= dropoutP;
                    if (!switches[sIdx])
                    {
                        activations[aIdx] = 0d;
                        if (derivatives != null)
                        {
                            derivatives[dIdx] = 0d;
                        }
                    }
                    else
                    {
                        activations[aIdx] *= enabledGateCoeff;
                    }
                }
            }
            else
            {
                //Gaussian gate
                for (int i = 0; i < count; i++, sIdx++, aIdx++)
                {
                    switches[sIdx] = true;
                    activations[aIdx] *= rand.NextGaussianDouble(1d, Math.Sqrt(dropoutP / (1d - dropoutP)));
                }
            }
        }


        /// <summary>
        /// Gets the standard deviation for layer weights initialization from the normal distribution.
        /// </summary>
        /// <param name="numOfInputNodes">Number of layer input nodes.</param>
        /// <param name="numOfLayerNeurons">Number of layer neurons.</param>
        public virtual double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //He-initialization. This covers most of ReLU like activation functions
            return Math.Sqrt(2d / numOfInputNodes);
        }

        /// <summary>
        /// Creates a clone.
        /// </summary>
        /// <returns>Clone of this activation function.</returns>
        public ActivationBase Clone()
        {
            return ActivationFactory.CreateActivationFn(ID);
        }

    }//ActivationBase

}//Namespace
