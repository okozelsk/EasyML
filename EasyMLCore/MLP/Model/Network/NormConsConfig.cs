using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Weights norm constraint configuration.
    /// </summary>
    [Serializable]
    public class NormConsConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NormConsConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the L2 min-norm constraint. Default is 0 (no constraint).
        /// </summary>
        public const double DefaultMin = 0d;
        /// <summary>
        /// Default value of the parameter specifying the L2 max-norm constraint. Default is 0 (no constraint).
        /// </summary>
        public const double DefaultMax = 0d;
        /// <summary>
        /// Default value of the parameter specifying whether to include biases during norm constraint application. Default value is false.
        /// </summary>
        public const bool DefaultBiases = false;

        //Attributes
        /// <summary>
        /// Specifies the L2 min-norm constraint.
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Specifies the L2 max-norm constraint.
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Specifies whether to include biases during norm constraint application.
        /// </summary>
        public bool Biases { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="min">Specifies the L2 min-norm constraint. Default is 0 (no constraint).</param>
        /// <param name="max">Specifies the L2 max-norm constraint. Default is 0 (no constraint).</param>
        /// <param name="biases">Specifies whether to include biases during norm constraint application. Default is false.</param>
        public NormConsConfig(double min = DefaultMin,
                              double max = DefaultMax,
                              bool biases = DefaultBiases
                              )
        {
            Min = min;
            Max = max;
            Biases = biases;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NormConsConfig(NormConsConfig source)
            : this(source.Min, source.Max, source.Biases)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public NormConsConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Min = double.Parse(validatedElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(validatedElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            Biases = bool.Parse(validatedElem.Attribute("biases").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMin { get { return (Min == DefaultMin); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMax { get { return (Max == DefaultMax); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBiases { get { return (Biases == DefaultBiases); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultMin && IsDefaultMax && IsDefaultBiases; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Min < 0d)
            {
                throw new ArgumentException($"Min must be GE to 0.", nameof(Min));
            }
            if (Max < 0d)
            {
                throw new ArgumentException($"Max must be GE to 0.", nameof(Max));
            }
            if (Min > Max)
            {
                throw new ArgumentException($"Min must be GE to Max.", nameof(Min));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new NormConsConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultMin)
            {
                rootElem.Add(new XAttribute("min", Min.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMax)
            {
                rootElem.Add(new XAttribute("max", Max.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("normCons", suppressDefaults);
        }

    }//NormConsConfig

}//Namespace
