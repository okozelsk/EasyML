using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Softplus activation function.
    /// </summary>
    [Serializable]
    public class AFSoftplus : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFSoftplus()
            : base(ActivationFnID.Softplus, Interval.IntZPI)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return Math.Log(1d + Math.Exp(sum));
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return 1d / (1d + Math.Exp(-sum));
        }

    }//AFSoftplus

}//Namespace
