using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Elliot activation function (aka Softsign).
    /// </summary>
    [Serializable]
    public class AFElliotSig : ActivationBase
    {
        //Constants
        private const double Slope = 1d;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFElliotSig()
            : base(ActivationFnID.ElliotSig, Interval.IntN1P1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            sum *= Slope;
            return sum / (1d + Math.Abs(sum));
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return Slope / ((1d + Math.Abs(activation * Slope)).Power(2));
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //Xavier Initialization
            return Math.Sqrt(2d / (numOfInputNodes + numOfLayerNeurons));
        }

    }//AFElliotSig

}//Namespace
