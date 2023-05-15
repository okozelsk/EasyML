namespace EasyMLCore.MLP
{
    /// <summary>
    /// Common interface of optimizers.
    /// </summary>
    public interface IOptimizer
    {

        /// <inheritdoc cref="Optimizer"/>
        Optimizer UpdaterID { get; }

        /// <summary>
        /// Resets updater to its initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Called when new epoch to be started.
        /// </summary>
        void NewEpoch(int epochNum, int maxEpoch);

        /// <summary>
        /// Updates network weights.
        /// </summary>
        /// <param name="learningPermeability">The global learning permeability.</param>
        /// <param name="cost">The cost (error) corresponding to current weight gradients.</param>
        /// <param name="flatGradSwitches">Weight gradients on/off boolean switches in a flat structure.</param>
        /// <param name="flatGrads">Weight gradients in a flat structure.</param>
        /// <param name="flatWeights">Weights in a flat structure.</param>
        void Update(double learningPermeability,
                    double cost,
                    bool[] flatGradSwitches,
                    double[] flatGrads,
                    double[] flatWeights
                    );
    }//IOptimizer

}//Namespace
