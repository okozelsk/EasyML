using EasyMLCore.Extensions;
using System;

namespace EasyMLCore.Loss
{
    /// <summary>
    /// Implements the Sigmoid Binary Cross Entropy Loss function.
    /// </summary>
    /// <remarks>
    /// Always used in conjunction with the Sigmoid activation function.
    /// </remarks>
    [Serializable]
    public class SigmoidCrossEntropyLoss : SerializableObject, ILossFn
    {
        //Constants
        private const double LowerBound = Common.Epsilon;
        private const double UpperBound = 1d - Common.Epsilon;

        /// <inheritdoc/>
        public double Compute(double ideal, double computed)
        {
            computed = computed.Bound(LowerBound, UpperBound);
            if (ideal >= 0.5d)
            {
                return -Math.Log(computed);
            }
            else
            {
                return -Math.Log(1d - computed);
            }
        }

        /// <inheritdoc/>
        public double ComputeZGradient(double derivative, double ideal, double computed)
        {
            return (computed - ideal);
        }

        /// <inheritdoc/>
        public ILossFn DeepClone()
        {
            return new SigmoidCrossEntropyLoss();
        }


    }//SigmoidCrossEntropyLoss
}//Namespace
