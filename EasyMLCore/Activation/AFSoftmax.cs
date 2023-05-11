using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Softmax activation function.
    /// </summary>
    [Serializable]
    public class AFSoftmax : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFSoftmax()
            : base(ActivationFnID.Softmax, Interval.IntZP1)
        {
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool RequiresWholeLayerComputation { get { return true; } }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            throw new InvalidOperationException("Can't compute SoftMax activation. SoftMax activation requires the input vector. Use Compute(double[], double[]) method version.");
        }

        /// <inheritdoc/>
        public override void Compute(double[] sums, int sIdx, double[] activations, int aIdx, int count)
        {
            double maxX = double.MinValue;
            for (int i = 0, flatIdx = sIdx; i < count; i++, flatIdx++)
            {
                if (sums[flatIdx] > maxX)
                {
                    maxX = sums[flatIdx];
                }
            }
            double sum = 0d;
            for (int i = 0; i < count; i++)
            {
                activations[aIdx + i] = Math.Exp(sums[sIdx + i] - maxX);
                sum += activations[aIdx + i];
            }
            for (int i = 0; i < count; i++)
            {
                activations[aIdx + i] /= sum;
            }
            return;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Always retuns 1d.
        /// Gradient is computed by the loss function.
        /// </remarks>
        public override double ComputeDerivative(double sum, double activation)
        {
            return 1d;
        }

        /// <inheritdoc/>
        public override void Compute(double[] sums,
                                     int sIdx,
                                     double[] activations,
                                     int aIdx,
                                     double[] derivatives,
                                     int dIdx,
                                     int count
                                     )
        {
            double maxX = double.MinValue;
            for (int i = 0, flatIdx = sIdx; i < count; i++, flatIdx++)
            {
                if (sums[flatIdx] > maxX)
                {
                    maxX = sums[flatIdx];
                }
            }
            double sum = 0d;
            for (int i = 0; i < count; i++)
            {
                activations[aIdx + i] = Math.Exp(sums[sIdx + i] - maxX);
                sum += activations[aIdx + i];
            }
            for (int i = 0; i < count; i++)
            {
                activations[aIdx + i] /= sum;
                derivatives[dIdx + i] = activations[aIdx + i] * (1d - activations[aIdx + i]);
            }
            return;
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //Xavier-Glorot initialization
            return Math.Sqrt(2d / (numOfInputNodes + numOfLayerNeurons));
        }

    }//AFSoftmax

}//Namespace
