using System;

namespace EasyMLCore.MiscTools
{
    /// <summary>
    /// Maps values from the first parameter to the values of the second parameter.
    /// </summary>
    /// <remarks>
    /// Mapping can be pure linear when specified slope is 0.
    /// When slope is greater than 0, mapping is nonlinear through Elliot curve. 
    /// </remarks>
    [Serializable]
    public class ParamValMapper : SerializableObject
    {
        //Constants
        private const double MinNonlinearSlope = 1e-15;
        private const double MaxNonlinearSlope = 1e15;

        //Attributes
        private readonly double _param1From;
        private readonly double _param1To;
        private readonly double _param1Span;
        private readonly double _param2From;
        private readonly double _param2To;
        private readonly double _param2Span;
        private readonly double _param2Default;
        private readonly double _slope;
        private readonly double _param2TransMax;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="param1From">Param1 value from.</param>
        /// <param name="param1To">Param1 value to.</param>
        /// <param name="param2From">Param2 value corresponding to param1 value from.</param>
        /// <param name="param2To">Param2 value corresponding to param1 value to.</param>
        /// <param name="param2Default">Param2 default value (used when param1 is out of boundaries).</param>
        /// <param name="slope">Nonlinearity slope.</param>
        public ParamValMapper(double param1From,
                              double param1To,
                              double param2From,
                              double param2To,
                              double param2Default,
                              double slope
            )
        {
            _param1From = param1From;
            _param1To = param1To;
            _param1Span = _param1To - _param1From;
            _param2From = param2From;
            _param2To = param2To;
            _param2Span = _param2To - _param2From;
            _param2Default = param2Default;
            _slope = Math.Abs(slope);
            if (_slope < MinNonlinearSlope || _slope > MaxNonlinearSlope)
            {
                _param2TransMax = 1d;
            }
            else
            {
                _param2TransMax = TransFn(1d);
            }
            return;
        }

        /// <summary>
        /// Transform function.
        /// </summary>
        /// <param name="nrmParam1">Normalized param1.</param>
        /// <returns>Normalized param2.</returns>
        private double TransFn(double nrmParam1)
        {
            if (_slope > 0d)
            {
                double sParam1 = _slope * nrmParam1;
                return sParam1 / (1d + sParam1);
            }
            else
            {
                return nrmParam1;
            }
        }

        /// <summary>
        /// Maps param1 value to param2 value.
        /// </summary>
        /// <param name="param1">param1 value.</param>
        /// <returns>param2 value.</returns>
        public double Map(double param1)
        {
            double nrmParam1 = (param1 - _param1From) / _param1Span;
            if (nrmParam1 < 0d || nrmParam1 > 1d)
            {
                return _param2Default;
            }
            double nrmParam2 = TransFn(nrmParam1) / _param2TransMax;
            double param2 = nrmParam2 * _param2Span + _param2From;
            return param2;
        }

    }//ParamValMapper

}//Namespace
