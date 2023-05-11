using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;

namespace EasyMLCore.Extensions
{
    /// <summary>
    /// Implements extensions of the Random class.
    /// </summary>
    public static class ExtensionsRandom
    {
        //Constants
        private const double PI2 = 2d * Math.PI;
        private static readonly double Log4 = Math.Log(4d);
        private static readonly double GammaAlgConst = 1d + Math.Log(4.5d);

        /// <summary>
        /// Randomly shuffles items within a collection.
        /// </summary>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="collection">A collection of items to be shuffled.</param>
        /// <param name="rand">This random generator.</param>
        public static void Shuffle<T>(this Random rand, IList<T> collection)
        {
            int n = collection.Count;
            if (n == 0)
            {
                throw new InvalidOperationException("Collection has no items.");
            }
            while (n > 1)
            {
                int k = rand.Next(n--);
                (collection[k], collection[n]) = (collection[n], collection[k]);
            }
            return;
        }

        /// <summary>
        /// Returns the random sign (values 1 or -1).
        /// </summary>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="rand">This random generator.</param>
        public static double NextSign(this Random rand)
        {
            return rand.NextDouble() >= 0.5 ? 1d : -1d;
        }

        /// <inheritdoc cref="Random.NextDouble"/>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="rand">This random generator.</param>
        public static double NextUniformDouble(this Random rand)
        {
            return rand.NextDouble();
        }

        /// <summary>
        /// Returns a random double within the specified range.
        /// Optionally it randomizes sign.
        /// </summary>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (exclusive).</param>
        /// <param name="randomSign">Specifies whether to randomize sign.</param>
        /// <param name="rand">This random generator.</param>
        public static double NextRangedUniformDouble(this Random rand, double min = -1, double max = 1, bool randomSign = false)
        {
            //Check for randomness suppression
            if (min == max)
            {
                return min;
            }
            //Arguments validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max", nameof(min));
            }
            //Computation
            return (rand.NextUniformDouble() * (max - min) + min)*(randomSign ? rand.NextSign() : 1d); ;
        }

        /// <summary>
        /// Fills a buffer with random double values within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Uniform distribution.
        /// </para>
        /// </remarks>
        /// <param name="buffer">A buffer to be filled.</param>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (exclusive).</param>
        /// <param name="randomSign">Specifies whether to randomize sign.</param>
        /// <param name="rand">This random generator.</param>
        public static void FillUniform(this Random rand, IList<double> buffer, double min, double max, bool randomSign)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                buffer[i] = rand.NextRangedUniformDouble(min, max, randomSign);
            }
            return;
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Gaussian distribution.
        /// </remarks>
        /// <param name="mean">Required mean.</param>
        /// <param name="stdDev">Required standard deviation.</param>
        /// <param name="rand">This random generator.</param>
        public static double NextGaussianDouble(this Random rand, double mean = 0, double stdDev = 1)
        {
            //Uniform (0,1> random doubles
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            //Computation
            return mean + stdDev * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(PI2 * u2);
        }

        /// <summary>
        /// Fills given buffer by random values following the Gaussian distribution.
        /// </summary>
        /// <remarks>
        /// It performs final correction of random values to achieve requested mean and standard deviation.
        /// </remarks>
        /// <param name="buffer">Buffer to be filled.</param>
        /// <param name="mean">Required mean.</param>
        /// <param name="stdDev">Required standard deviation.</param>
        /// <param name="rand">This random generator.</param>
        public static void FillGaussianDouble(this Random rand, IList<double> buffer, double mean = 0, double stdDev = 1)
        {
            BasicStat stat = new BasicStat();
            for (int i = 0; i < buffer.Count; i++)
            {
                buffer[i] = NextGaussianDouble(rand, 0d, 1d);
                stat.AddSample(buffer[i]);
            }
            //Final shift and scale
            for (int i = 0; i < buffer.Count; i++)
            {
                buffer[i] += mean - stat.ArithAvg;
                buffer[i] *= stdDev / stat.StdDev;
            }
            return;
        }

        /// <summary>
        /// Returns a random double value within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Gaussian distribution.
        /// </para>
        ///  <para>
        /// Warning: due to application of the filtering loop to get values belonging into the specified range, this function can lead to a bad performance. The performance strongly depends on specified parameters.
        /// </para>
        /// </remarks>
        /// <param name="mean">Required mean.</param>
        /// <param name="stdDev">Required standard deviation.</param>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (inclusive).</param>
        /// <param name="rand">This random generator.</param>
        public static double NextRangedGaussianDouble(this Random rand, double mean, double stdDev, double min, double max)
        {
            //Check the randomness suppression
            if (min == max)
            {
                return min;
            }
            //Validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max.", nameof(min));
            }
            //Filtering loop
            double result;
            do
            {
                result = rand.NextGaussianDouble(mean, stdDev);
            } while (result < min || result > max);
            return result;
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Exponential distribution.
        /// </remarks>
        /// <param name="mean">Required mean.</param>
        /// <param name="rand">This random generator.</param>
        public static double NextExponentialDouble(this Random rand, double mean)
        {
            //Checks
            if (mean == 0)
            {
                throw new ArgumentException("Mean parameter equals to 0.", nameof(mean));
            }
            //Lambda
            double lambda = 1d / mean;
            //Computation
            return -Math.Log(1d - rand.NextDouble()) / lambda;
        }

        /// <summary>
        /// Returns a random double value within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Exponential distribution.
        /// </para>
        ///  <para>
        /// Warning: due to application of the filtering loop to get values belonging into the specified range, this function can lead to a bad performance. The performance strongly depends on specified parameters.
        /// </para>
        /// </remarks>
        /// <param name="mean">Required mean.</param>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (inclusive).</param>
        /// <param name="rand">This random generator.</param>
        public static double NextRangedExponentialDouble(this Random rand, double mean, double min, double max)
        {
            //Check the randomness suppression
            if (min == max)
            {
                return min;
            }
            //Validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max.", nameof(min));
            }
            //Filtering loop
            double result;
            do
            {
                result = rand.NextExponentialDouble(mean);
            } while (result < min || result > max);
            return result;
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Gamma distribution.
        /// </para>
        /// <para>
        /// Implementation is converted from Python.
        /// Mean tends to alpha/beta and StdDev tends to Sqrt(alpha/(beta*beta)).
        /// Generated number is always positive.
        /// </para>
        /// </remarks>
        /// <param name="alpha">The shape parameter (must be greater than 0).</param>
        /// <param name="beta">The rate parameter (must be greater than 0).</param>
        /// <param name="rand">This random generator.</param>
        public static double NextGammaDouble(this Random rand, double alpha, double beta)
        {
            //Checks
            if (alpha <= 0)
            {
                throw new ArgumentException("Alpha parameter must be GT 0.", nameof(alpha));
            }
            if (beta <= 0)
            {
                throw new ArgumentException("Beta parameter must be GT 0.", nameof(beta));
            }
            //Computation
            if (alpha > 1d)
            {
                /* 
                 * R.C.H. Cheng, "The generation of Gamma variables with non-integral shape parameters"
                 * Applied Statistics, (1977), 26, No. 1, p71-74
                 */
                double ainv = Math.Sqrt(2d * alpha - 1d);
                double bbb = alpha - Log4;
                double ccc = alpha + ainv;
                while (true)
                {
                    double u1 = rand.NextDouble();
                    if (u1 > 1e-7d && u1 < 0.9999999d)
                    {
                        double u2 = 1d - rand.NextDouble();
                        double v = Math.Log(u1 / (1d - u1)) / ainv;
                        double x = alpha * Math.Exp(v);
                        double z = u1 * u1 * u2;
                        double r = bbb + ccc * v - x;
                        if (r + GammaAlgConst - 4.5d * z >= 0d || r >= Math.Log(z))
                        {
                            return x * beta;
                        }
                    }
                }

            }
            else if (alpha == 1d)
            {
                //Exponential distribution
                return -Math.Log(1d - rand.NextDouble()) * beta;
            }
            else
            {
                //Algorithm GS of Statistical Computing - Kennedy & Gentle
                double x, p, r;
                do
                {
                    double b = (Math.E + alpha) / Math.E;
                    p = rand.NextDouble() * b;
                    if (p <= 1d)
                    {
                        x = Math.Pow(p, (1d / alpha));
                    }
                    else
                    {
                        x = -Math.Log((b - p) / alpha);
                    }
                    r = rand.NextDouble();
                } while (!(r <= Math.Exp(-x) || (p > 1d && r <= Math.Pow(x, alpha - 1d))));
                return x * beta;
            }
        }

        /// <summary>
        /// Returns a random double value within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Gamma distribution.
        /// </para>
        ///  <para>
        /// Warning: due to application of the filtering loop to get values belonging into the specified range, this function can lead to a bad performance. The performance strongly depends on specified parameters.
        /// </para>
        /// </remarks>
        /// <param name="alpha">The shape parameter (must be greater than 0).</param>
        /// <param name="beta">The rate parameter (must be greater than 0).</param>
        /// <param name="min">The min value (inclusive, must be greater than 0).</param>
        /// <param name="max">The max value (inclusive, must be greater than 0).</param>
        /// <param name="rand">This random generator.</param>
        public static double NextRangedGammaDouble(this Random rand, double alpha, double beta, double min, double max)
        {
            //Check for randomness suppression
            if (min == max)
            {
                return min;
            }
            //Arguments validations
            if (min < 0)
            {
                throw new ArgumentException($"Min is less than 0", nameof(min));
            }
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max", nameof(min));
            }
            //Filterring loop
            double result;
            do
            {
                result = rand.NextGammaDouble(alpha, beta);
            } while (result < min || result > max);
            return result;
        }

    }//ExtensionsRandom

}//Namespace

