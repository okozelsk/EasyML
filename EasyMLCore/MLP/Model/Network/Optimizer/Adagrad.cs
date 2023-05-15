using System;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements the Adagrad optimizer.
    /// </summary>
    [Serializable]
    public class Adagrad : SerializableObject, IOptimizer
    {
        //Attributes
        private readonly AdagradConfig _cfg;
        private readonly double[] _s;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfWeights">Number of network's weights.</param>
        /// <param name="cfg">The configuration.</param>
        public Adagrad(int numOfWeights, AdagradConfig cfg)
        {
            _cfg = (AdagradConfig)cfg.DeepClone();
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
            Array.Fill(_s, 0d);
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
                _s[weightFlatIdx] += flatGrads[weightFlatIdx] * flatGrads[weightFlatIdx];
                double denominator = Math.Sqrt(_s[weightFlatIdx]) + Common.Epsilon;
                //Update weight
                flatWeights[weightFlatIdx] += -lr * (flatGrads[weightFlatIdx] / denominator);
            }
            return;
        }

    }//Adagrad

}//Namespace
