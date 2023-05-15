using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the Adam optimizer with Amsgrad option (Adaptive Moment Estimation).
    /// </summary>
    [Serializable]
    public class AdamConfig : ConfigBase, IOptimizerConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "AdamConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the initial learning rate. Default is 1e-3.
        /// </summary>
        public const double DefaultIniLR = 0.001d;
        /// <summary>
        /// Default value of the parameter specifying whether to apply amsgrad modification. Default is false.
        /// </summary>
        public const bool DefaultAmsgrad = false;
        /// <summary>
        /// Default value of the parameter specifying the first moment decay rate. Default is 0.9.
        /// </summary>
        public const double DefaultBeta1 = 0.9d;
        /// <summary>
        /// Default value of the parameter specifying the second moment decay rate. Default is 0.999.
        /// </summary>
        public const double DefaultBeta2 = 0.999d;

        //Attributes
        /// <summary>
        /// Specifies the initial learning rate.
        /// </summary>
        public double IniLR { get; }

        /// <summary>
        /// Specifies whether to apply amsgrad modification.
        /// </summary>
        public bool Amsgrad { get; }

        /// <summary>
        /// Specifies the first moment decay rate.
        /// </summary>
        public double Beta1 { get; }

        /// <summary>
        /// Specifies the second moment decay rate.
        /// </summary>
        public double Beta2 { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="iniLR">Specifies the initial learning rate. Default is 1e-3.</param>
        /// <param name="amsgrad">Specifies whether to apply amsgrad modification. Default is false.</param>
        /// <param name="beta1">Specifies the first moment decay rate. Default is 0.9.</param>
        /// <param name="beta2">Specifies the second moment decay rate. Default is 0.999.</param>
        public AdamConfig(double iniLR = DefaultIniLR,
                          bool amsgrad = DefaultAmsgrad,
                          double beta1 = DefaultBeta1,
                          double beta2 = DefaultBeta2
                          )
        {
            IniLR = iniLR;
            Amsgrad = amsgrad;
            Beta1 = beta1;
            Beta2 = beta2;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AdamConfig(AdamConfig source)
            : this(source.IniLR, source.Amsgrad, source.Beta1, source.Beta2)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AdamConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            IniLR = double.Parse(validatedElem.Attribute("iniLR").Value, CultureInfo.InvariantCulture);
            Amsgrad = bool.Parse(validatedElem.Attribute("amsgrad").Value);
            Beta1 = double.Parse(validatedElem.Attribute("beta1").Value, CultureInfo.InvariantCulture);
            Beta2 = double.Parse(validatedElem.Attribute("beta2").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer OptimizerID { get { return Optimizer.Adam; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultIniLR { get { return (IniLR == DefaultIniLR); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultAmsgrad { get { return (Amsgrad == DefaultAmsgrad); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBeta1 { get { return (Beta1 == DefaultBeta1); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBeta2 { get { return (Beta2 == DefaultBeta2); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultIniLR &&
                       IsDefaultAmsgrad &&
                       IsDefaultBeta1 &&
                       IsDefaultBeta2;
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
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new AdamConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultIniLR)
            {
                rootElem.Add(new XAttribute("iniLR", IniLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultAmsgrad)
            {
                rootElem.Add(new XAttribute("amsgrad", Amsgrad.GetXmlCode()));
            }
            if (!suppressDefaults || !IsDefaultBeta1)
            {
                rootElem.Add(new XAttribute("beta1", Beta1.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultBeta2)
            {
                rootElem.Add(new XAttribute("beta2", Beta2.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("adam", suppressDefaults);
        }

    }//AdamConfig

}//Namespace
