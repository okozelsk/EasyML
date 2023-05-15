using System;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the RMSProp optimizer with Centered option.
    /// </summary>
    [Serializable]
    public class RMSProp : SerializableObject, IOptimizer
    {
        //Attributes
        private readonly RMSPropConfig _cfg;
        private readonly double[] _s;
        private readonly double[] _m;
        private readonly double[] _g;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public RMSProp(int numOfWeights, RMSPropConfig cfg)
        {
            _cfg = (RMSPropConfig)cfg.DeepClone();
            _s = new double[numOfWeights];
            _m = new double[numOfWeights];
            _g = new double[numOfWeights];
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
            Array.Fill(_s, 0d);
            Array.Fill(_m, 0d);
            Array.Fill(_g, 0d);
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
            //Learning rate step
            double lr = learningPermeability * _cfg.IniLR;
            //Update
            //Too cheap to parallelize.
            for (int weightFlatIdx = 0; weightFlatIdx < flatWeights.Length; weightFlatIdx++)
            {
                //Update running averages
                _s[weightFlatIdx] = _cfg.Alpha * _s[weightFlatIdx] + (1d - _cfg.Alpha) * flatGrads[weightFlatIdx] * flatGrads[weightFlatIdx];
                double avg;
                if(_cfg.Centered)
                {
                    _g[weightFlatIdx] = _cfg.Alpha * _g[weightFlatIdx] + (1d - _cfg.Alpha) * flatGrads[weightFlatIdx];
                    avg = _s[weightFlatIdx] - _g[weightFlatIdx] * _g[weightFlatIdx];
                }
                else
                {
                    avg = _s[weightFlatIdx];
                }
                avg = Math.Sqrt(avg) + Common.Epsilon;
                if(_cfg.Momentum > 0d)
                {
                    _m[weightFlatIdx] = _cfg.Momentum * _m[weightFlatIdx] + flatGrads[weightFlatIdx] / avg;
                    flatWeights[weightFlatIdx] += -lr * _m[weightFlatIdx];
                }
                else
                {
                    flatWeights[weightFlatIdx] += -lr * flatGrads[weightFlatIdx] / avg;
                }
            }
            return;
        }

    }//RMSProp

}//Namespace
