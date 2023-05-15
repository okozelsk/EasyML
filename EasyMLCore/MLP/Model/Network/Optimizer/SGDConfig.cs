using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the SGD optimizer with momentum option (Stochastic Gradient Descent).
    /// </summary>
    [Serializable]
    public class SGDConfig : ConfigBase, IOptimizerConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SGDConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the initial learning rate. Default is 1e-4.
        /// </summary>
        public const double DefaultIniLR = 0.0001d;
        /// <summary>
        /// Default value of the parameter specifying the momentum. Default is 0.9.
        /// </summary>
        public const double DefaultMomentum = 0.9d;
        /// <summary>
        /// Default value of the parameter specifying the dampening for momentum. Default is 0.
        /// </summary>
        public const double DefaultDampening = 0d;
        /// <summary>
        /// Default value of the parameter specifying whether to apply Nesterov momentum. Default is false.
        /// </summary>
        public const bool DefaultNesterov = false;

        //Attributes
        /// <summary>
        /// Specifies the initial learning rate.
        /// </summary>
        public double IniLR { get; }

        /// <summary>
        /// Specifies the momentum.
        /// </summary>
        public double Momentum { get; }

        /// <summary>
        /// Specifies the dampening for momentum.
        /// </summary>
        public double Dampening { get; }

        /// <summary>
        /// Specifies whether to apply Nesterov momentum.
        /// </summary>
        public bool Nesterov { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="iniLR">Specifies the initial learning rate. Default is 1e-4.</param>
        /// <param name="momentum">Specifies the momentum. Default is 0.9.</param>
        /// <param name="dampening">Specifies the dampening for momentum. Default is 0.</param>
        /// <param name="nesterov">Specifies whether to apply Nesterov momentum. Default is false.</param>
        public SGDConfig(double iniLR = DefaultIniLR,
                         double momentum = DefaultMomentum,
                         double dampening = DefaultDampening,
                         bool nesterov = DefaultNesterov
                         )
        {
            IniLR = iniLR;
            Momentum = momentum;
            Dampening = dampening;
            Nesterov = nesterov;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SGDConfig(SGDConfig source)
            : this(source.IniLR, source.Momentum, source.Dampening, source.Nesterov)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public SGDConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            IniLR = double.Parse(validatedElem.Attribute("iniLR").Value, CultureInfo.InvariantCulture);
            Momentum = double.Parse(validatedElem.Attribute("momentum").Value, CultureInfo.InvariantCulture);
            Dampening = double.Parse(validatedElem.Attribute("dampening").Value, CultureInfo.InvariantCulture);
            Nesterov = bool.Parse(validatedElem.Attribute("nesterov").Value);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer OptimizerID { get { return Optimizer.SGD; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultIniLR { get { return (IniLR == DefaultIniLR); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMomentum { get { return (Momentum == DefaultMomentum); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultDampening { get { return (Dampening == DefaultDampening); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultNesterov { get { return (Nesterov == DefaultNesterov); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultIniLR &&
                       IsDefaultMomentum &&
                       IsDefaultDampening &&
                       IsDefaultNesterov;
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
            if (Momentum < 0d || Momentum >= 1d)
            {
                throw new ArgumentException($"Momentum must be GE 0 and LT 1.", nameof(Momentum));
            }
            if (Dampening < 0d)
            {
                throw new ArgumentException($"Dampening must be GE 0.", nameof(Dampening));
            }
            if (Nesterov  && (Dampening > 0d || Momentum == 0d))
            {
                throw new ArgumentException($"Nesterov momentum requires Momentum GT 0 and Dampening EQ 0.", nameof(Nesterov));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new SGDConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultIniLR)
            {
                rootElem.Add(new XAttribute("iniLR", IniLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMomentum)
            {
                rootElem.Add(new XAttribute("momentum", Momentum.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultDampening)
            {
                rootElem.Add(new XAttribute("dampening", Dampening.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultNesterov)
            {
                rootElem.Add(new XAttribute("nesterov", Nesterov.GetXmlCode()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("sgd", suppressDefaults);
        }

    }//SGDConfig

}//Namespace
