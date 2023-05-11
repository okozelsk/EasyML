using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Hyperbolic Tangent activation function.
    /// </summary>
    [Serializable]
    public class AFTanH : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFTanH()
            : base(ActivationFnID.TanH, Interval.IntN1P1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return 2d / (1d + Math.Exp(-2d * sum)) - 1d;
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return 1d - activation * activation;
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //Xavier-Glorot initialization
            return Math.Sqrt(2d / (numOfInputNodes + numOfLayerNeurons));
        }


    }//AFMish

}//Namespace
