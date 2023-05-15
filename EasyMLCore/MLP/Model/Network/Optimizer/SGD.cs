using System;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the SGD optimizer with momentum option (Stochastic Gradient Descent).
    /// </summary>
    [Serializable]
    public class SGD : SerializableObject, IOptimizer
    {
        //Attributes
        private readonly SGDConfig _cfg;
        private readonly double[] _m;
        private int _step;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public SGD(int numOfWeights, SGDConfig cfg)
        {
            _cfg = (SGDConfig)cfg.DeepClone();
            _m = new double[numOfWeights];
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
            _step = 0;
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
            ++_step;
            //Learning rate step
            double lr = learningPermeability * _cfg.IniLR;
            //Update
            //Too cheap to parallelize.
            for (int weightFlatIdx = 0; weightFlatIdx < flatWeights.Length; weightFlatIdx++)
            {
                double grad;
                if (_cfg.Momentum > 0d)
                {
                    if(_step == 1)
                    {
                        _m[weightFlatIdx] = flatGrads[weightFlatIdx];
                    }
                    else
                    {
                        _m[weightFlatIdx] *= _cfg.Momentum;
                        _m[weightFlatIdx] += (1d - _cfg.Dampening) * flatGrads[weightFlatIdx];
                    }
                    if(_cfg.Nesterov)
                    {
                        grad = flatGrads[weightFlatIdx] + _cfg.Momentum * _m[weightFlatIdx];
                    }
                    else
                    {
                        grad = _m[weightFlatIdx];
                    }
                }
                else
                {
                    grad = flatGrads[weightFlatIdx];
                }
                flatWeights[weightFlatIdx] -= lr * grad;
            }
            return;
        }

    }//SGD

}//Namespace
