using EasyMLCore.Extensions;
using System;

namespace EasyMLCore.Loss
{
    /// <summary>
    /// Implements the Squared Error Loss function.
    /// </summary>
    [Serializable]
    public class SquaredErrorLoss : SerializableObject, ILossFn
    {
        /// <inheritdoc/>
        public double Compute(double ideal, double computed)
        {
            return ((ideal - computed).Power(2)) / 2d;
        }

        /// <inheritdoc/>
        public double ComputeZGradient(double derivative, double ideal, double computed)
        {
            return derivative * (computed - ideal);
        }

        /// <inheritdoc/>
        public ILossFn DeepClone()
        {
            return new SquaredErrorLoss();
        }

    }//SquaredErrorLoss
}//Namespace
