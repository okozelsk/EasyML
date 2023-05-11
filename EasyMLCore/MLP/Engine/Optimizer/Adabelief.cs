using EasyMLCore.Extensions;
using System;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the Adabelief optimizer (Adapting Stepsizes by the Belief in Observed Gradients).
    /// </summary>
    /// <remarks>
    /// https://arxiv.org/pdf/2010.07468.pdf
    /// </remarks>
    [Serializable]
    public class Adabelief : SerializableObject, IOptimizer
    {
        //Attributes
        private readonly AdabeliefConfig _cfg;
        private readonly double[] _m;
        private readonly double[] _s;
        private double _poweredBeta1;
        private double _poweredBeta2;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public Adabelief(int numOfWeights, AdabeliefConfig cfg)
        {
            _cfg = (AdabeliefConfig)cfg.DeepClone();
            _m = new double[numOfWeights];
            _s = new double[numOfWeights];
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
            Array.Fill(_s, 0d);
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
                _s[weightFlatIdx] = _cfg.Beta2 * _s[weightFlatIdx] + (1d - _cfg.Beta2) * (flatGrads[weightFlatIdx] - _m[weightFlatIdx]).Power(2) + Common.Epsilon;
                //Update weight
                double denominator = Math.Sqrt(_s[weightFlatIdx]) + Common.Epsilon;
                flatWeights[weightFlatIdx] += -lr * (denominator == 0d ? 0d : _m[weightFlatIdx] / denominator);
            }
            //Power betas
            _poweredBeta1 *= _cfg.Beta1;
            _poweredBeta2 *= _cfg.Beta2;
            return;
        }

    }//Adabelief

}//Namespace
