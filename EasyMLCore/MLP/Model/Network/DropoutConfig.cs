using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the dropout.
    /// </summary>
    [Serializable]
    public class DropoutConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "DropoutConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the dropout probability.  Default is 0 (no dropout).
        /// </summary>
        public const double DefaultP = 0d;
        /// <summary>
        /// Default value of the parameter specifying the dropout mode. Default is None.
        /// </summary>
        public const DropoutMode DefaultMode = DropoutMode.None;

        //Attributes
        /// <summary>
        /// Specifies the dropout probability.
        /// </summary>
        public double P { get; }

        /// <inheritdoc cref="DropoutMode"/>
        public DropoutMode Mode { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="p">Specifies the dropout probability. Default is 0 (no dropout).</param>
        /// <param name="mode">Specifies the dropout mode. Default is None (no dropout).</param>
        public DropoutConfig(double p = DefaultP,
                             DropoutMode mode = DefaultMode
                             )
        {
            P = p;
            Mode = mode;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public DropoutConfig(DropoutConfig source)
            : this(source.P, source.Mode)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public DropoutConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            P = double.Parse(validatedElem.Attribute("p").Value, CultureInfo.InvariantCulture);
            Mode = (DropoutMode)Enum.Parse(typeof(DropoutMode), validatedElem.Attribute("mode").Value, true);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultP { get { return (P == DefaultP); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMode { get { return (Mode == DefaultMode); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultP && IsDefaultMode; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (P < 0d || P >= 1d)
            {
                throw new ArgumentException($"P must be GE to 0 and LT 1.", nameof(P));
            }
            if (P == 0d && Mode != DropoutMode.None)
            {
                throw new ArgumentException($"A nonzero P must be specified when mode in not None.", nameof(P));
            }
            if (P != 0d && Mode == DropoutMode.None)
            {
                throw new ArgumentException($"P must be 0 when mode in None.", nameof(P));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new DropoutConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultP)
            {
                rootElem.Add(new XAttribute("p", P.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMode)
            {
                rootElem.Add(new XAttribute("mode", Mode.ToString()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("dropout", suppressDefaults);
        }

    }//DropoutConfig

}//Namespace
