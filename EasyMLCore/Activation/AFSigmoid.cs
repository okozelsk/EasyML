using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Sigmoid activation function.
    /// </summary>
    [Serializable]
    public class AFSigmoid : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFSigmoid()
            : base(ActivationFnID.Sigmoid, Interval.IntZP1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return 1d / (1d + Math.Exp(-sum));
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return activation * (1d - activation);
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //Xavier-Glorot initialization
            return Math.Sqrt(2d / (numOfInputNodes + numOfLayerNeurons));
        }

    }//AFSigmoid

}//Namespace
