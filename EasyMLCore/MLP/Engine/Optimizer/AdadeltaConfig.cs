using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the Adadelta optimizer (an extension of Adagrad that attempts to solve its radically diminishing learning rates).
    /// </summary>
    [Serializable]
    public class AdadeltaConfig : ConfigBase, IOptimizerConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "AdadeltaConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the gamma decay rate. Default is 0.95.
        /// </summary>
        public const double DefaultGamma = 0.95d;

        /// <summary>
        /// Specifies the gamma decay rate.
        /// </summary>
        public double Gamma { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="gamma">Specifies the gamma decay rate. Default is 0.95.</param>
        public AdadeltaConfig(double gamma = DefaultGamma)
        {
            Gamma = gamma;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AdadeltaConfig(AdadeltaConfig source)
            : this(source.Gamma)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AdadeltaConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Gamma = double.Parse(validatedElem.Attribute("gamma").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public Optimizer OptimizerID { get { return Optimizer.Adadelta; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultGamma { get { return (Gamma == DefaultGamma); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultGamma;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Gamma <= 0d || Gamma >= 1d)
            {
                throw new ArgumentException($"Gamma must be GT 0 and LT 1.", nameof(Gamma));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new AdadeltaConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultGamma)
            {
                rootElem.Add(new XAttribute("gamma", Gamma.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("adadelta", suppressDefaults);
        }

    }//AdadeltaConfig

}//Namespace
