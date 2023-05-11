using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Rectified Linear Unit activation function.
    /// </summary>
    [Serializable]
    public class AFReLU : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFReLU()
            : base(ActivationFnID.ReLU, Interval.IntZPI)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return Math.Max(0d, sum);
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return (sum > 0) ? 1d : 0d;
        }

    }//AFReLU

}//Namespace
