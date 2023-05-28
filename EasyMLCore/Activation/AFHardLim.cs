using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the HardLim (aka Binary step) activation function.
    /// </summary>
    [Serializable]
    public class AFHardLim : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFHardLim()
            : base(ActivationFnID.HardLim, Interval.IntZP1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return sum >= 0 ? 1d : 0d;
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return 0d;
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //Xavier-Glorot initialization
            return Math.Sqrt(2d / (numOfInputNodes + numOfLayerNeurons));
        }

    }//AFHardLim

}//Namespace
