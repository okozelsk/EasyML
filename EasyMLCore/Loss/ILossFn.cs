namespace EasyMLCore.Loss
{
    /// <summary>
    /// Common interface of the loss functions.
    /// </summary>
    public interface ILossFn
    {
        /// <summary>
        /// Computes the loss.
        /// </summary>
        /// <param name="ideal">Ideal value.</param>
        /// <param name="computed">Computed value.</param>
        double Compute(double ideal, double computed);

        /// <summary>
        /// Computes the gradient on output Z node.
        /// </summary>
        /// <param name="derivative">Output activation derivative.</param>
        /// <param name="ideal">Ideal value.</param>
        /// <param name="computed">Computed value.</param>
        double ComputeZGradient(double derivative, double ideal, double computed);

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        ILossFn DeepClone();

    }//ILossFn

}//Namespace
