using EasyMLCore.Extensions;
using System;

namespace EasyMLCore.Loss
{
    /// <summary>
    /// Implements the Softmax Categorical Cross Entropy Loss function.
    /// </summary>
    /// <remarks>
    /// Always used in conjunction with the Softmax activation function.
    /// </remarks>
    [Serializable]
    public class SoftmaxCrossEntropyLoss : SerializableObject, ILossFn
    {
        //Constants
        private const double LowerBound = Common.Epsilon;
        private const double UpperBound = 1d - Common.Epsilon;

        /// <inheritdoc/>
        public double Compute(double ideal, double computed)
        {
            if (ideal >= 0.5d)
            {
                return -Math.Log(computed.Bound(LowerBound, UpperBound));
            }
            else
            {
                return 0d;
            }
        }

        /// <inheritdoc/>
        public double ComputeZGradient(double derivative, double ideal, double computed)
        {
            return computed - ideal;
        }

        /// <inheritdoc/>
        public ILossFn DeepClone()
        {
            return new SoftmaxCrossEntropyLoss();
        }

    }//SoftmaxCrossEntropyLoss
}//Namespace
