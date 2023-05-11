using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Weights L2 (ridge) regularization configuration.
    /// </summary>
    [Serializable]
    public class RegL2Config : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RegL2Config";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the L2 penalty strength (ridge). Default is 0 (no L2 regularization).
        /// </summary>
        public const double DefaultStrength = 0d;
        /// <summary>
        /// Default value of the parameter specifying whether to apply L2 regularization to biases. Default value is false.
        /// </summary>
        public const bool DefaultBiases = false;

        //Attributes
        /// <summary>
        /// Specifies the L2 penalty strength (ridge).
        /// </summary>
        public double Strength { get; }

        /// <summary>
        /// Specifies whether to apply L2 regularization to biases.
        /// </summary>
        public bool Biases { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="strength">Specifies the L2 penalty strength (ridge). Default is 0 (no L2 regularization).</param>
        /// <param name="biases">Specifies whether to apply L2 regularization to biases. Default is false.</param>
        public RegL2Config(double strength = DefaultStrength,
                           bool biases = DefaultBiases
                           )
        {
            Strength = strength;
            Biases = biases;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RegL2Config(RegL2Config source)
            : this(source.Strength, source.Biases)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RegL2Config(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Strength = double.Parse(validatedElem.Attribute("strength").Value, CultureInfo.InvariantCulture);
            Biases = bool.Parse(validatedElem.Attribute("biases").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultStrength { get { return (Strength == DefaultStrength); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBiases { get { return (Biases == DefaultBiases); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultStrength && IsDefaultBiases; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Strength < 0d)
            {
                throw new ArgumentException($"Strength must be GE to 0.", nameof(Strength));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new RegL2Config(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultStrength)
            {
                rootElem.Add(new XAttribute("strength", Strength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultBiases)
            {
                rootElem.Add(new XAttribute("biases", Biases.GetXmlCode()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("regL2", suppressDefaults);
        }

    }//RegL2Config

}//Namespace
