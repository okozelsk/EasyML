namespace EasyMLCore
{
    /// <summary>
    /// Common interface of components providing output computation.
    /// </summary>
    public interface IComputable
    {
        /// <summary>
        /// Gets number of output features.
        /// </summary>
        int NumOfOutputFeatures { get; }

        /// <summary>
        /// Computes an output.
        /// </summary>
        /// <param name="input">Input vector.</param>
        /// <returns>Computed output vector.</returns>
        double[] Compute(double[] input);

    }//IComputable

}//Namespace
