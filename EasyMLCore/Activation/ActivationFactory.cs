using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Mediates the operations with activation functions.
    /// </summary>
    public static class ActivationFactory
    {

        /// <summary>
        /// Creates an activation function instance corresponding to its identifier.
        /// </summary>
        /// <param name="activationFnID">Identifier of an activation function.</param>
        /// <returns>Created activation function instance.</returns>
        public static ActivationBase CreateActivationFn(ActivationFnID activationFnID)
        {
            return activationFnID switch
            {
                ActivationFnID.BentIdentity => new AFBentIdentity(),
                ActivationFnID.ElliotSig => new AFElliotSig(),
                ActivationFnID.ELU => new AFELU(),
                ActivationFnID.GELU => new AFGELU(),
                ActivationFnID.LeakyReLU => new AFLeakyReLU(),
                ActivationFnID.Linear => new AFLinear(),
                ActivationFnID.ReLU => new AFReLU(),
                ActivationFnID.SELU => new AFSELU(),
                ActivationFnID.Sigmoid => new AFSigmoid(),
                ActivationFnID.Softplus => new AFSoftplus(),
                ActivationFnID.TanH => new AFTanH(),
                ActivationFnID.Softmax => new AFSoftmax(),
                _ => throw new ArgumentException($"Unknown activation function {activationFnID}.", nameof(activationFnID)),
            };
        }//CreateActivationFn

        /// <summary>
        /// Parses activation function identifier by its code.
        /// </summary>
        /// <param name="code">The code of an activation function.</param>
        /// <returns>The activation function identifier.</returns>
        public static ActivationFnID ParseAFnID(string code)
        {
            return (ActivationFnID)Enum.Parse(typeof(ActivationFnID), code, true);
        }

        /// <summary>
        /// Tells whether an activation function is suitable to be used on MLP network's hidden layer.
        /// </summary>
        public static bool IsSuitableForMLPHiddenLayer(ActivationFnID activationID)
        {
            return !(activationID == ActivationFnID.Linear || activationID == ActivationFnID.Softmax);
        }

        /// <summary>
        /// Tells whether an activation function is suitable to be used on reservoir's hidden layer.
        /// </summary>
        public static bool IsSuitableForReservoirHiddenLayer(ActivationFnID activationID)
        {
            ActivationBase aFn = CreateActivationFn(activationID);
            return (!aFn.RequiresWholeLayerComputation && aFn.OutputRange.Min == -1d && aFn.OutputRange.Max == 1d);
        }

    }//ActivationFactory

}//Namespace
