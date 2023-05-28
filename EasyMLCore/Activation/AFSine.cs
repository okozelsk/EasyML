using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Sine activation function.
    /// </summary>
    [Serializable]
    public class AFSine : ActivationBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFSine()
            : base(ActivationFnID.Sine, Interval.IntN1P1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return Math.Sin(sum);
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return Math.Cos(sum);
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            //Xavier-Glorot initialization
            return Math.Sqrt(2d / (numOfInputNodes + numOfLayerNeurons));
        }

    }//AFSine

}//Namespace
