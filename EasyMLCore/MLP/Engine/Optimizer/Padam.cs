using System;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the Padam optimizer (Partially Adaptive Moment Estimation).
    /// </summary>
    /// <remarks>
    /// https://arxiv.org/pdf/1806.06763.pdf
    /// </remarks>
    [Serializable]
    public class Padam : SerializableObject, IOptimizer
    {
        //Attributes
        private readonly PadamConfig _cfg;
        private readonly double[] _m;
        private readonly double[] _v;
        private readonly double[] _maxV;
        private double _poweredBeta1;
        private double _poweredBeta2;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public Padam(int numOfWeights, PadamConfig cfg)
        {
            _cfg = (PadamConfig)cfg.DeepClone();
            _m = new double[numOfWeights];
            _v = new double[numOfWeights];
            _maxV = new double[numOfWeights];
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
            Array.Fill(_v, 0d);
            Array.Fill(_maxV, 0d);
            _poweredBeta1 = _cfg.Beta1;
            _poweredBeta2 = _cfg.Beta2;
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
            double corrBias2 = 1d - _poweredBeta2;
            //Learning rate step
            double lr = learningPermeability * _cfg.IniLR * Math.Sqrt(corrBias2) / corrBias1;
            //Update
            //Too cheap to parallelize.
            for (int weightFlatIdx = 0; weightFlatIdx < flatWeights.Length; weightFlatIdx++)
            {
                //Update running exp averages
                _m[weightFlatIdx] = _cfg.Beta1 * _m[weightFlatIdx] + (1d - _cfg.Beta1) * flatGrads[weightFlatIdx];
                _v[weightFlatIdx] = _cfg.Beta2 * _v[weightFlatIdx] + (1d - _cfg.Beta2) * flatGrads[weightFlatIdx] * flatGrads[weightFlatIdx];
                _maxV[weightFlatIdx] = Math.Max(_maxV[weightFlatIdx], _v[weightFlatIdx]);
                //Update weight
                double denominator = Math.Pow(_maxV[weightFlatIdx] + Common.Epsilon, _cfg.P);
                flatWeights[weightFlatIdx] += -lr * (denominator == 0d ? 0d : _m[weightFlatIdx] / denominator);
            }
            //Power betas
            _poweredBeta1 *= _cfg.Beta1;
            _poweredBeta2 *= _cfg.Beta2;
            return;
        }

    }//Padam

}//Namespace
