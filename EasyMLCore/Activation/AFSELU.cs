using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Activation
{
    /// <summary>
    /// Implements the Scaled Exponential Linear Unit activation function.
    /// </summary>
    [Serializable]
    public class AFSELU : ActivationBase
    {
        //Constants
        private const double Alpha = 1.6732632423543772848170429916717d;
        private const double Scale = 1.0507009873554804934193349852946d;
        private const double AlphaPrime = -Scale * Alpha;
        //Static members
        private static readonly Interval _range = new Interval(AlphaPrime, double.MaxValue.Bound(), false, true);

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFSELU()
            : base(ActivationFnID.SELU, _range)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double sum)
        {
            return Scale * (sum > 0d ? sum : Alpha * (Math.Exp(sum) - 1d));
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double sum, double activation)
        {
            return Scale * (sum > 0d ? 1d : Alpha * Math.Exp(sum));
        }

        /// <inheritdoc/>
        public override double GetNormalInitWeightsStdDev(int numOfInputNodes, int numOfLayerNeurons)
        {
            return Math.Sqrt(1d / numOfInputNodes);
        }

        /// <inheritdoc/>
        public override void Dropout(DropoutMode mode,
                                     double dropoutP,
                                     Random rand,
                                     bool[] switches,
                                     int sIdx,
                                     double[] activations,
                                     int aIdx,
                                     double[] derivatives,
                                     int dIdx,
                                     int count
                                     )
        {
            if (mode == DropoutMode.Bernoulli)
            {
                //Bernouli gate - use Alpha Dropout
                double keepP = 1d - dropoutP;
                double a = Math.Sqrt(1d / (keepP * (dropoutP * AlphaPrime * AlphaPrime + 1d)));
                double b = -a * (dropoutP * AlphaPrime);
                for (int i = 0; i < count; i++, sIdx++, aIdx++, dIdx++)
                {
                    switches[sIdx] = rand.NextDouble() >= dropoutP;
                    if (!switches[sIdx])
                    {
                        activations[aIdx] = AlphaPrime;
                        if (derivatives != null)
                        {
                            derivatives[dIdx] = 0d;
                        }
                    }
                    activations[aIdx] = activations[aIdx] * a + b;
                }
            }
            else
            {
                //Gaussian gate - use base version
                base.Dropout(mode,
                             dropoutP,
                             rand,
                             switches,
                             sIdx,
                             activations,
                             aIdx,
                             derivatives,
                             dIdx,
                             count
                             );
            }
        }

    }//AFSELU

}//Namespace
