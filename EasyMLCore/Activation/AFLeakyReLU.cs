using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Leaky Rectified Linear Unit activation function.
    /// </summary>
    [Serializable]
    public class AFLeakyReLU : ActivationBase
    {
        //Constants
        private const double NegSlope = 0.01d;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFLeakyReLU()
            : base(ActivationFnID.LeakyReLU, Interval.IntNIPI)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return (sum < 0) ? (NegSlope * sum) : sum;
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return (sum < 0) ? NegSlope : 1d;
        }

    }//AFLeakyReLU

}//Namespace
