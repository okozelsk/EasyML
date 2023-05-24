using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Radial Basis (aka Gaussian) activation function.
    /// </summary>
    [Serializable]
    public class AFRadBas : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFRadBas()
            : base(ActivationFnID.RadBas, Interval.IntZP1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return Math.Exp(-(sum*sum));
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return -2d * activation * sum;
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //Xavier-Glorot initialization
            return Math.Sqrt(2d / (numOfInputNodes + numOfLayerNeurons));
        }

    }//AFRadBas

}//Namespace
