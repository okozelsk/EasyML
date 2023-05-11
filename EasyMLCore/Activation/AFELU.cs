using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Exponential Linear Unit activation function.
    /// </summary>
    [Serializable]
    public class AFELU : ActivationBase
    {
        //Constants
        private const double Alpha = 1d;
        //Static members
        private static readonly Interval _range = new Interval(-Alpha, double.MaxValue.Bound(), false, true);

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFELU()
            : base(ActivationFnID.ELU, _range)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return (sum < 0) ? (Alpha * (Math.Exp(sum) - 1)) : sum;
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return (sum < 0) ? (activation + Alpha) : 1d;
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            return Math.Sqrt(1d / numOfInputNodes);
        }

    }//AFELU

}//Namespace
