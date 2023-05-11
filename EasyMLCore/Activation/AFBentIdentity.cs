using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Bent Identity activation function.
    /// </summary>
    [Serializable]
    public class AFBentIdentity : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFBentIdentity()
            : base(ActivationFnID.BentIdentity, Interval.IntZPI)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return (Math.Sqrt(sum.Power(2) + 1d) - 1d) / 2d + sum;
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return sum / (2d * Math.Sqrt(sum.Power(2) + 1d)) + 1d;
        }

    }//BentIdentity

}//Namespace
