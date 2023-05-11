using System;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the Adadelta optimizer (an extension of Adagrad that attempts to solve its radically diminishing learning rates).
    /// </summary>
    [Serializable]
    public class Adadelta : SerializableObject, IOptimizer
    {
        //Attributes
        private readonly AdadeltaConfig _cfg;
        private readonly double[] _g;
        private readonly double[] _d;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public Adadelta(int numOfWeights, AdadeltaConfig cfg)
        {
            _cfg = (AdadeltaConfig)cfg.DeepClone();
            _g = new double[numOfWeights];
            _d = new double[numOfWeights];
            Reset();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer UpdaterID { get { return _cfg.OptimizerID; } }

        //Methods
        /// <inheritdoc/>
        public void Reset()
        {
            Array.Fill(_g, Math.Sqrt(Common.Epsilon));
            Array.Fill(_d, Math.Sqrt(Common.Epsilon));
            return;
        }

        /// <inheritdoc/>
        public void NewEpoch(int epochNum, int maxEpoch)
        {
            //Does nothing
            return;
        }

        /// <inheritdoc/>
        public void Update(double learningPermeability,
                           double cost,
                           bool[] flatGradSwitches,
                           double[] flatGrads,
                           double[] flatWeights
                           )
        {
            //Update
            //Too cheap to parallelize.
            for (int weightFlatIdx = 0; weightFlatIdx < flatWeights.Length; weightFlatIdx++)
            {
                _g[weightFlatIdx] = _cfg.Gamma * _g[weightFlatIdx] + (1d - _cfg.Gamma) * flatGrads[weightFlatIdx] * flatGrads[weightFlatIdx];
                double gRMS = Math.Sqrt(_g[weightFlatIdx] + Common.Epsilon);
                double dRMS = Math.Sqrt(_d[weightFlatIdx] + Common.Epsilon);
                double deltaW = (dRMS * flatGrads[weightFlatIdx]) / gRMS;
                _d[weightFlatIdx] = _cfg.Gamma * _d[weightFlatIdx] + (1d - _cfg.Gamma) * deltaW * deltaW;
                flatWeights[weightFlatIdx] += -learningPermeability * deltaW;
            }
            return;
        }

    }//Adadelta

}//Namespace
