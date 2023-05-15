using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the Padam optimizer (Partially Adaptive Moment Estimation).
    /// </summary>
    [Serializable]
    public class PadamConfig : ConfigBase, IOptimizerConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PadamConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the initial learning rate. Default is 1e-2.
        /// </summary>
        /// <remarks>
        /// Default value had to be decreased due to numerical stability.
        /// </remarks>
        public const double DefaultIniLR = 0.01d;
        /// <summary>
        /// Default value of the parameter specifying the first moment decay rate. Default is 0.9.
        /// </summary>
        public const double DefaultBeta1 = 0.9d;
        /// <summary>
        /// Default value of the parameter specifying the second moment decay rate. Default is 0.999.
        /// </summary>
        public const double DefaultBeta2 = 0.999d;
        /// <summary>
        /// Default value of the parameter specifying the partially adaptive parameter. Default is 0.125.
        /// </summary>
        public const double DefaultP = 0.125d;

        //Attributes
        /// <summary>
        /// Specifies the initial learning rate.
        /// </summary>
        public double IniLR { get; }

        /// <summary>
        /// Specifies the first moment decay rate.
        /// </summary>
        public double Beta1 { get; }

        /// <summary>
        /// Specifies the second moment decay rate.
        /// </summary>
        public double Beta2 { get; }

        /// <summary>
        /// Specifies the partially adaptive parameter.
        /// </summary>
        public double P { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="iniLR">Specifies the initial learning rate. Default is 1e-2.</param>
        /// <param name="beta1">Specifies the first moment decay rate. Default is 0.9.</param>
        /// <param name="beta2">Specifies the second moment decay rate. Default is 0.999.</param>
        /// <param name="p">Specifies the partially adaptive parameter. Default is 0.125.</param>
        public PadamConfig(double iniLR = DefaultIniLR,
                           double beta1 = DefaultBeta1,
                           double beta2 = DefaultBeta2,
                           double p = DefaultP
                           )
        {
            IniLR = iniLR;
            Beta1 = beta1;
            Beta2 = beta2;
            P = p;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PadamConfig(PadamConfig source)
            : this(source.IniLR, source.Beta1, source.Beta2, source.P)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PadamConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            IniLR = double.Parse(validatedElem.Attribute("iniLR").Value, CultureInfo.InvariantCulture);
            Beta1 = double.Parse(validatedElem.Attribute("beta1").Value, CultureInfo.InvariantCulture);
            Beta2 = double.Parse(validatedElem.Attribute("beta2").Value, CultureInfo.InvariantCulture);
            P = double.Parse(validatedElem.Attribute("p").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer OptimizerID { get { return Optimizer.Padam; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultIniLR { get { return (IniLR == DefaultIniLR); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBeta1 { get { return (Beta1 == DefaultBeta1); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBeta2 { get { return (Beta2 == DefaultBeta2); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultP { get { return (P == DefaultP); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultIniLR &&
                       IsDefaultBeta1 &&
                       IsDefaultBeta2 &&
                       IsDefaultP;
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
            if (Beta1 <= 0d || Beta1 >= 1d)
            {
                throw new ArgumentException($"Beta1 must be GT 0 and LT 1.", nameof(Beta1));
            }
            if (Beta2 <= 0d || Beta2 >= 1d)
            {
                throw new ArgumentException($"Beta2 must be GT 0 and LT 1.", nameof(Beta2));
            }
            if (P < 0d || P > 0.5d)
            {
                throw new ArgumentException($"P must be GE to 0 and LE to 0.5.", nameof(P));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new PadamConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultIniLR)
            {
                rootElem.Add(new XAttribute("iniLR", IniLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultBeta1)
            {
                rootElem.Add(new XAttribute("beta1", Beta1.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultBeta2)
            {
                rootElem.Add(new XAttribute("beta2", Beta2.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultP)
            {
                rootElem.Add(new XAttribute("p", P.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("padam", suppressDefaults);
        }

    }//PadamConfig

}//Namespace
