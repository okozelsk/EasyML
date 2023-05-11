using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Gaussian Error Linear Unit activation function.
    /// </summary>
    [Serializable]
    public class AFGELU : ActivationBase
    {
        //Static members
        private static readonly double PF = Math.Sqrt(2d / Math.PI);
        private static readonly Interval _range = new Interval(-0.170041d, double.MaxValue.Bound(), false, true);

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFGELU()
            : base(ActivationFnID.GELU, _range)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return 0.5d * sum * (1d + Math.Tanh(PF * (sum + 0.044715d * (sum * sum * sum))));
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return 0.5 * Math.Tanh(0.0356774d * sum * sum * sum + 0.797885d * sum) + (1d / Math.Cosh(0.0535161d * sum * sum * sum + 0.398942d * sum)).Power(2) * (0.0356774d * sum * sum * sum + 0.797885d * sum) + 0.5d;
        }


    }//AFGELU

}//Namespace
