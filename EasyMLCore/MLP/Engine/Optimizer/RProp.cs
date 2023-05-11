using System;
using System.Runtime.CompilerServices;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the RProp optimizer (Resilient Back Propagation).
    /// </summary>
    /// <remarks>
    /// Designed for BGD. Note that dropout together with RProp does not work.
    /// </remarks>
    [Serializable]
    public class RProp : SerializableObject, IOptimizer
    {
        //Constants
        /// <summary>
        /// An absolute value that is still considered as zero.
        /// </summary>
        public const double ZeroTolerance = 1E-16d;

        //Attributes
        private readonly RPropConfig _cfg;
        private readonly double[] _prevGrads;
        private readonly double[] _wLRs;
        private readonly double[] _wChanges;
        private double _prevCost;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public RProp(int numOfWeights, RPropConfig cfg)
        {
            _cfg = (RPropConfig)cfg.DeepClone();
            _prevGrads = new double[numOfWeights];
            _wLRs = new double[numOfWeights];
            _wChanges = new double[numOfWeights];
            Reset();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer UpdaterID { get { return _cfg.OptimizerID; } }

        //Methods
        /// <summary>
        /// Determines the value sign. A value less than the zero tolerance is considered as zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double Sign(double value)
        {
            if (Math.Abs(value) <= ZeroTolerance)
            {
                return 0d;
            }
            else if (value > 0d)
            {
                return 1d;
            }
            else
            {
                return -1d;
            }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            Array.Fill(_prevGrads, 0d);
            Array.Fill(_wLRs, _cfg.IniLR);
            Array.Fill(_wChanges, 0d);
            _prevCost = double.MaxValue;
            return;
        }

        /// <inheritdoc/>
        public void NewEpoch(int epochNum, int maxEpoch)
        {
            //Does nothing
            return;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Implements iRPROP+ version of the weights update rule.
        /// </remarks>
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
                double gradMulSign = Sign(flatGrads[weightFlatIdx] * _prevGrads[weightFlatIdx]);
                if (gradMulSign > 0d)
                {
                    //No sign change, increase LR
                    _wLRs[weightFlatIdx] = Math.Min(_wLRs[weightFlatIdx] * _cfg.PosEta, _cfg.MaxLR);
                    _wChanges[weightFlatIdx] = -learningPermeability * (Sign(flatGrads[weightFlatIdx]) * _wLRs[weightFlatIdx]);
                    flatWeights[weightFlatIdx] += _wChanges[weightFlatIdx];
                    _prevGrads[weightFlatIdx] = flatGrads[weightFlatIdx];
                }
                else if (gradMulSign < 0d)
                {
                    //Changed sign, decrease LR
                    _wLRs[weightFlatIdx] = Math.Max(_wLRs[weightFlatIdx] * _cfg.NegEta, _cfg.MinLR);
                    //Ensure no change to LR in the next iteration
                    _prevGrads[weightFlatIdx] = 0d;
                    //Set back the previous weight when the cost has increased
                    if (cost > _prevCost)
                    {
                        _wChanges[weightFlatIdx] *= -1d;
                        flatWeights[weightFlatIdx] += _wChanges[weightFlatIdx];
                    }
                }
                else
                {
                    _wChanges[weightFlatIdx] = -learningPermeability * (Sign(flatGrads[weightFlatIdx]) * _wLRs[weightFlatIdx]);
                    flatWeights[weightFlatIdx] += _wChanges[weightFlatIdx];
                    _prevGrads[weightFlatIdx] = flatGrads[weightFlatIdx];
                }
            }
            _prevCost = cost;
            return;
        }

    }//RProp

}//Namespace
