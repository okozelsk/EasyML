using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the Learning Throttle Valve.
    /// </summary>
    /// <remarks>
    /// Throttling is based on scaled Elliot curve. The goal is to successively decay
    /// full permeability (1) as the number of epochs grows up to min permeability, which to be
    /// achieved at the specified epoch/maxEpoch ratio defined by the model.
    /// ThrottlingSlope parameter drives the decay aggresivity. As bigger the value of ThrottlingSlope
    /// parameter is, as steeper the decay curve is.
    /// ThrottlingSlope close to 0 works as a classical linear decay.
    /// ThrottlingSlope close to 2 works very similarly to a classical exponential decay.
    /// </remarks>
    [Serializable]
    public class LearningThrottleValveConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "LearningThrottleValveConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the minimum throttle permeability. Default is 1 (means full permeability, thus no throttling).
        /// </summary>
        public const double DefaultMinPermeability = 1d;
        /// <summary>
        /// Default value of the parameter specifying the throttling slope (permeability decay steepness). Default is 0 (linear mapping).
        /// </summary>
        public const double DefaultThrottlingSlope = 0d;
        /// <summary>
        /// Default value of the parameter specifying epoch/maxEpoch ratio when to achieve minPermeability and then to hold constant minPermeability. Default is 1 (means to achieve minPermeability at the last epoch).
        /// </summary>
        public const double DefaultLastThrottlingEpochRatio = 1d;

        //Attributes
        /// <summary>
        /// Specifies the the minimum throttle permeability.
        /// </summary>
        public double MinPermeability { get; }

        /// <summary>
        /// Specifies the throttling slope (permeability decay steepness).
        /// </summary>
        public double ThrottlingSlope { get; }

        /// <summary>
        /// Specifies epoch/maxEpoch ratio when to achieve minPermeability and then to hold constant minPermeability.
        /// </summary>
        public double LastThrottlingEpochRatio { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="minPermeability">Specifies the minimum throttle permeability. Default is 1 (means full permeability, thus no throttling).</param>
        /// <param name="throttlingSlope">Specifies the throttling slope (permeability decay steepness). Default is 0 (linear mapping).</param>
        /// <param name="lastThrottlingEpochRatio">Specifies epoch/maxEpoch ratio when to achieve minPermeability and then to hold constant minPermeability.</param>
        public LearningThrottleValveConfig(double minPermeability = DefaultMinPermeability,
                                           double throttlingSlope = DefaultThrottlingSlope,
                                           double lastThrottlingEpochRatio = DefaultLastThrottlingEpochRatio
                                           )
        {
            MinPermeability = minPermeability;
            ThrottlingSlope = throttlingSlope;
            LastThrottlingEpochRatio = lastThrottlingEpochRatio;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public LearningThrottleValveConfig(LearningThrottleValveConfig source)
            : this(source.MinPermeability, source.ThrottlingSlope, source.LastThrottlingEpochRatio)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public LearningThrottleValveConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            MinPermeability = double.Parse(validatedElem.Attribute("minPermeability").Value, CultureInfo.InvariantCulture);
            ThrottlingSlope = double.Parse(validatedElem.Attribute("throttlingSlope").Value, CultureInfo.InvariantCulture);
            LastThrottlingEpochRatio = double.Parse(validatedElem.Attribute("LastThrottlingEpochRatio").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMinPermeability { get { return (MinPermeability == DefaultMinPermeability); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultThrottlingSlope { get { return (ThrottlingSlope == DefaultThrottlingSlope); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultLastThrottlingEpochRatio { get { return (LastThrottlingEpochRatio == DefaultLastThrottlingEpochRatio); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultMinPermeability && IsDefaultThrottlingSlope && IsDefaultLastThrottlingEpochRatio;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (MinPermeability <= 0d || MinPermeability > 1d)
            {
                throw new ArgumentException($"MinPermeability must be GT 0 and LE 1.", nameof(MinPermeability));
            }
            if (ThrottlingSlope < 0d)
            {
                throw new ArgumentException($"ThrottlingSlope must be GE 0.", nameof(ThrottlingSlope));
            }
            if (LastThrottlingEpochRatio <= 0d || LastThrottlingEpochRatio > 1d)
            {
                throw new ArgumentException($"LastThrottlingEpochRatio must be GT 0 and LE 1.", nameof(LastThrottlingEpochRatio));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new LearningThrottleValveConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultMinPermeability)
            {
                rootElem.Add(new XAttribute("minPermeability", MinPermeability.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultThrottlingSlope)
            {
                rootElem.Add(new XAttribute("throttlingSlope", ThrottlingSlope.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultLastThrottlingEpochRatio)
            {
                rootElem.Add(new XAttribute("lastThrottlingEpochRatio", LastThrottlingEpochRatio.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("learningThrottleValve", suppressDefaults);
        }

    }//LearningThrottleValveConfig

}//Namespace
