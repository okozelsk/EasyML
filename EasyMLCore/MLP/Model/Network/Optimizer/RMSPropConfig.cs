using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the RMSProp optimizer with Centered option.
    /// </summary>
    [Serializable]
    public class RMSPropConfig : ConfigBase, IOptimizerConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RMSPropConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the initial learning rate. Default is 1e-3.
        /// </summary>
        public const double DefaultIniLR = 0.001d;
        /// <summary>
        /// Default value of the parameter specifying whether to apply centered version (the gradient is normalized by an estimation of its variance). Default is false.
        /// </summary>
        public const bool DefaultCentered = false;
        /// <summary>
        /// Default value of the parameter specifying the smoothing constant. Default is 0.99.
        /// </summary>
        public const double DefaultAlpha = 0.99d;
        /// <summary>
        /// Default value of the parameter specifying the momentum factor. Default is 0.
        /// </summary>
        public const double DefaultMomentum = 0d;

        //Attributes
        /// <summary>
        /// Specifies the initial learning rate.
        /// </summary>
        public double IniLR { get; }

        /// <summary>
        /// Specifies whether to apply centered version (the gradient is normalized by an estimation of its variance).
        /// </summary>
        public bool Centered { get; }

        /// <summary>
        /// Specifies the smoothing constant.
        /// </summary>
        public double Alpha { get; }

        /// <summary>
        /// Specifies the momentum factor.
        /// </summary>
        public double Momentum { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="iniLR">Specifies the initial learning rate. Default is 1e-3.</param>
        /// <param name="centered">Specifies whether to apply centered version (the gradient is normalized by an estimation of its variance). Default is false.</param>
        /// <param name="alpha">Specifies the smoothing constant. Default is 0.99.</param>
        /// <param name="momentum">Specifies the momentum factor. Default is 0.</param>
        public RMSPropConfig(double iniLR = DefaultIniLR,
                          bool centered = DefaultCentered,
                          double alpha = DefaultAlpha,
                          double momentum = DefaultMomentum
                          )
        {
            IniLR = iniLR;
            Centered = centered;
            Alpha = alpha;
            Momentum = momentum;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RMSPropConfig(RMSPropConfig source)
            : this(source.IniLR, source.Centered, source.Alpha, source.Momentum)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RMSPropConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            IniLR = double.Parse(validatedElem.Attribute("iniLR").Value, CultureInfo.InvariantCulture);
            Centered = bool.Parse(validatedElem.Attribute("centered").Value);
            Alpha = double.Parse(validatedElem.Attribute("alpha").Value, CultureInfo.InvariantCulture);
            Momentum = double.Parse(validatedElem.Attribute("momentum").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer OptimizerID { get { return Optimizer.RMSProp; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultIniLR { get { return (IniLR == DefaultIniLR); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultCentered { get { return (Centered == DefaultCentered); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultAlpha { get { return (Alpha == DefaultAlpha); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMomentum { get { return (Momentum == DefaultMomentum); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultIniLR &&
                       IsDefaultCentered &&
                       IsDefaultAlpha &&
                       IsDefaultMomentum;
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
            if (Alpha <= 0d || Alpha >= 1d)
            {
                throw new ArgumentException($"Alpha must be GT 0 and LT 1.", nameof(Alpha));
            }
            if (Momentum < 0d || Momentum >= 1d)
            {
                throw new ArgumentException($"Momentum must be GE 0 and LT 1.", nameof(Momentum));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new RMSPropConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultIniLR)
            {
                rootElem.Add(new XAttribute("iniLR", IniLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultCentered)
            {
                rootElem.Add(new XAttribute("centered", Centered.GetXmlCode()));
            }
            if (!suppressDefaults || !IsDefaultAlpha)
            {
                rootElem.Add(new XAttribute("alpha", Alpha.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMomentum)
            {
                rootElem.Add(new XAttribute("momentum", Momentum.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("rmsprop", suppressDefaults);
        }

    }//RMSPropConfig

}//Namespace
