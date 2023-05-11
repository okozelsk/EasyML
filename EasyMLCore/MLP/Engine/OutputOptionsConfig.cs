using System;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the MLP network's output options.
    /// </summary>
    [Serializable]
    public class OutputOptionsConfig : ConfigBase
    {
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "OutputOptionsConfig";

        //Attributes
        /// <summary>
        /// Weights L1 (lasso) regularization configuration.
        /// </summary>
        public RegL1Config RegL1Cfg { get; }

        /// <summary>
        /// Weights L2 (ridge) regularization configuration.
        /// </summary>
        public RegL2Config RegL2Cfg { get; }

        /// <summary>
        /// Weights norm constraint configuration.
        /// </summary>
        public NormConsConfig NormConsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="regL1Cfg">Weights L1 (lasso) regularization configuration (can be null).</param>
        /// <param name="regL2Cfg">Weights L2 (ridge) regularization configuration (can be null).</param>
        /// <param name="normConsCfg">Weights norm constraint configuration (can be null).</param>
        public OutputOptionsConfig(RegL1Config regL1Cfg = null,
                            RegL2Config regL2Cfg = null,
                            NormConsConfig normConsCfg = null
                            )
        {
            RegL1Cfg = regL1Cfg == null ? new RegL1Config() : (RegL1Config)regL1Cfg.DeepClone();
            RegL2Cfg = regL2Cfg == null ? new RegL2Config() : (RegL2Config)regL2Cfg.DeepClone();
            NormConsCfg = normConsCfg == null ? new NormConsConfig() : (NormConsConfig)normConsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public OutputOptionsConfig(OutputOptionsConfig source)
            : this(source.RegL1Cfg, source.RegL2Cfg, source.NormConsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public OutputOptionsConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement regL1Elem = validatedElem.Elements("regL1").FirstOrDefault();
            RegL1Cfg = regL1Elem == null ? new RegL1Config() : new RegL1Config(regL1Elem);
            XElement regL2Elem = validatedElem.Elements("regL2").FirstOrDefault();
            RegL2Cfg = regL2Elem == null ? new RegL2Config() : new RegL2Config(regL2Elem);
            XElement normConsElem = validatedElem.Elements("normConst").FirstOrDefault();
            NormConsCfg = normConsElem == null ? new NormConsConfig() : new NormConsConfig(normConsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRegL1Cfg { get { return (RegL1Cfg.ContainsOnlyDefaults); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRegL2Cfg { get { return (RegL2Cfg.ContainsOnlyDefaults); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultNormConsCfg { get { return (NormConsCfg.ContainsOnlyDefaults); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultRegL1Cfg && IsDefaultRegL2Cfg && IsDefaultNormConsCfg; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new OutputOptionsConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultRegL1Cfg)
            {
                rootElem.Add(RegL1Cfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultRegL2Cfg)
            {
                rootElem.Add(RegL2Cfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultNormConsCfg)
            {
                rootElem.Add(NormConsCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("outputOptions", suppressDefaults);
        }

    }//OutputOptionsConfig

}//Namespace
