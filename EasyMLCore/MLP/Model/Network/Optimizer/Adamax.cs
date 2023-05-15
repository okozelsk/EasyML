using System;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the Adamax optimizer (variant of Adam based on the infinity norm).
    /// </summary>
    [Serializable]
    public class Adamax : SerializableObject, IOptimizer
    {
        //Attributes
        private readonly AdamaxConfig _cfg;
        private readonly double[] _m;
        private readonly double[] _u;
        private double _poweredBeta1;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public Adamax(int numOfWeights, AdamaxConfig cfg)
        {
            _cfg = (AdamaxConfig)cfg.DeepClone();
            _m = new double[numOfWeights];
            _u = new double[numOfWeights];
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
            Array.Fill(_m, 0d);
            Array.Fill(_u, 0d);
            _poweredBeta1 = _cfg.Beta1;
            return;
        }

        /// <inheritdoc/>
        public void NewEpoch(int epochNum, int maxEpoch)
        {
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
            //Bias correction
            double corrBias1 = 1d - _poweredBeta1;
            //Learning rate step
            double lr = learningPermeability * _cfg.IniLR / corrBias1;
            //Update
            //Too cheap to parallelize.
            for (int weightFlatIdx = 0; weightFlatIdx < flatWeights.Length; weightFlatIdx++)
            {
                //Update running exp averages
                _m[weightFlatIdx] = _cfg.Beta1 * _m[weightFlatIdx] + (1d - _cfg.Beta1) * flatGrads[weightFlatIdx];
                _u[weightFlatIdx] = Math.Max(_cfg.Beta2 * _u[weightFlatIdx], Math.Abs(flatGrads[weightFlatIdx]));
                //Update weight
                flatWeights[weightFlatIdx] += -lr * (_u[weightFlatIdx] == 0d ? 0d : (_m[weightFlatIdx] / corrBias1) / _u[weightFlatIdx]);
            }
            _poweredBeta1 *= _cfg.Beta1;
            return;
        }

    }//Adamax

}//Namespace
