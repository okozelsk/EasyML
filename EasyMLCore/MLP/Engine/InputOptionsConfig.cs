using System;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the MLP network's input options.
    /// </summary>
    [Serializable]
    public class InputOptionsConfig : ConfigBase
    {
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "InputOptionsConfig";

        //Attributes
        /// <summary>
        /// Dropout configuration.
        /// </summary>
        public DropoutConfig DropoutCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="dropoutCfg">Dropout configuration (can be null).</param>
        public InputOptionsConfig(DropoutConfig dropoutCfg = null)
        {
            DropoutCfg = dropoutCfg == null ? new DropoutConfig() : (DropoutConfig)dropoutCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public InputOptionsConfig(InputOptionsConfig source)
            : this(source.DropoutCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public InputOptionsConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement dropoutElem = validatedElem.Elements("dropout").FirstOrDefault();
            DropoutCfg = dropoutElem == null ? new DropoutConfig() : new DropoutConfig(dropoutElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultDropoutCfg { get { return (DropoutCfg.ContainsOnlyDefaults); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultDropoutCfg; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new InputOptionsConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultDropoutCfg)
            {
                rootElem.Add(DropoutCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("inputOptions", suppressDefaults);
        }

    }//InputOptionsConfig

}//Namespace
