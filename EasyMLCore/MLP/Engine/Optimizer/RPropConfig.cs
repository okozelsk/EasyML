using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the RProp optimizer (Resilient Back Propagation).
    /// </summary>
    /// <remarks>
    /// Default parameters are optimal for BGD without the Dropout.
    /// When Dropout is used, RProp parameters have to be tuned and good starting values are: iniLR: 0.01d, minLR: 0.001d, maxLR: 5d, posEta: 1.01d, negEta: 0.1d.
    /// </remarks>
    [Serializable]
    public class RPropConfig : ConfigBase, IOptimizerConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RPropConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the initial learning rate. Default is 0.0025.
        /// </summary>
        public const double DefaultIniLR = 0.0025d;
        /// <summary>
        /// Default value of the parameter specifying the min learning rate. Default is 1e-6.
        /// </summary>
        public const double DefaultMinLR = 1e-6d;
        /// <summary>
        /// Default value of the parameter specifying the min learning rate. Default is 0.0075.
        /// </summary>
        public const double DefaultMaxLR = 0.0075d;
        /// <summary>
        /// Default value of the parameter specifying the learning rate increase coefficient. Default is 1.2.
        /// </summary>
        public const double DefaultPosEta = 1.2d;
        /// <summary>
        /// Default value of the parameter specifying the learning rate decrease coefficient. Default is 0.5.
        /// </summary>
        public const double DefaultNegEta = 0.5d;

        //Attributes
        /// <summary>
        /// Specifies the initial learning rate.
        /// </summary>
        public double IniLR { get; }

        /// <summary>
        /// Specifies the min learning rate.
        /// </summary>
        public double MinLR { get; }

        /// <summary>
        /// Specifies the max learning rate.
        /// </summary>
        public double MaxLR { get; }

        /// <summary>
        /// Specifies the learning rate increase coefficient.
        /// </summary>
        public double PosEta { get; }

        /// <summary>
        /// Specifies the learning rate decrease coefficient.
        /// </summary>
        public double NegEta { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="iniLR">Specifies the initial learning rate. Default is 0.0025.</param>
        /// <param name="minLR">Specifies the min learning rate. Default is 1e-6.</param>
        /// <param name="maxLR">Specifies the max learning rate. Default is 0.0075.</param>
        /// <param name="posEta">Specifies the learning rate increase coefficient. Default is 1.2.</param>
        /// <param name="negEta">Specifies the learning rate decrease coefficient. Default is 0.5.</param>
        public RPropConfig(double iniLR = DefaultIniLR,
                           double minLR = DefaultMinLR,
                           double maxLR = DefaultMaxLR,
                           double posEta = DefaultPosEta,
                           double negEta = DefaultNegEta
                           )
        {
            IniLR = iniLR;
            MinLR = minLR;
            MaxLR = maxLR;
            PosEta = posEta;
            NegEta = negEta;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RPropConfig(RPropConfig source)
            : this(source.IniLR, source.MinLR, source.MaxLR, source.PosEta, source.NegEta)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RPropConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            IniLR = double.Parse(validatedElem.Attribute("iniLR").Value, CultureInfo.InvariantCulture);
            MinLR = double.Parse(validatedElem.Attribute("minLR").Value, CultureInfo.InvariantCulture);
            MaxLR = double.Parse(validatedElem.Attribute("maxLR").Value, CultureInfo.InvariantCulture);
            PosEta = double.Parse(validatedElem.Attribute("posEta").Value, CultureInfo.InvariantCulture);
            NegEta = double.Parse(validatedElem.Attribute("negEta").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer OptimizerID { get { return Optimizer.RProp; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultIniLR { get { return (IniLR == DefaultIniLR); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMinLR { get { return (MinLR == DefaultMinLR); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMaxLR { get { return (MaxLR == DefaultMaxLR); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultPosEta { get { return (PosEta == DefaultPosEta); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultNegEta { get { return (NegEta == DefaultNegEta); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultIniLR &&
                       IsDefaultMinLR &&
                       IsDefaultMaxLR &&
                       IsDefaultPosEta &&
                       IsDefaultNegEta;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (IniLR <= 0d)
            {
                throw new ArgumentException($"Initial learning rate must be GT 0.", nameof(IniLR));
            }
            if (MinLR <= 0d || MinLR > IniLR)
            {
                throw new ArgumentException($"Min learning rate must be GT 0 and LE to IniLR.", nameof(MinLR));
            }
            if (MaxLR < MinLR)
            {
                throw new ArgumentException($"Max learning rate must be GE to MinLR.", nameof(MaxLR));
            }
            if (PosEta <= 1d)
            {
                throw new ArgumentException($"Learning rate increase coefficient must be GT 1.", nameof(PosEta));
            }
            if (NegEta <= 0d || NegEta >= 1d)
            {
                throw new ArgumentException($"Learning rate decrease coefficient must be GT 0 and LT 1.", nameof(NegEta));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new RPropConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultIniLR)
            {
                rootElem.Add(new XAttribute("iniLR", IniLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMinLR)
            {
                rootElem.Add(new XAttribute("minLR", MinLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMaxLR)
            {
                rootElem.Add(new XAttribute("maxLR", MaxLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultPosEta)
            {
                rootElem.Add(new XAttribute("posEta", PosEta.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultNegEta)
            {
                rootElem.Add(new XAttribute("negEta", NegEta.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("rprop", suppressDefaults);
        }

    }//RPropConfig

}//Namespace
