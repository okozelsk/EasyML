using EasyMLCore.Activation;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the MLP network's hidden layer.
    /// </summary>
    [Serializable]
    public class HiddenLayerConfig : ConfigBase
    {
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "HiddenLayerConfig";

        //Attributes
        /// <summary>
        /// Number of layer neurons.
        /// </summary>
        public int NumOfNeurons { get; }

        /// <summary>
        /// Layer's activation function identifier.
        /// </summary>
        public ActivationFnID ActivationID { get; }

        /// <summary>
        /// Dropout configuration.
        /// </summary>
        public DropoutConfig DropoutCfg { get; }

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
        /// <param name="numOfNeurons">Number of layer neurons.</param>
        /// <param name="activationID">Layer's activation function identifier.</param>
        /// <param name="dropoutCfg">Dropout configuration (can be null).</param>
        /// <param name="regL1Cfg">Weights L1 (lasso) regularization configuration (can be null).</param>
        /// <param name="regL2Cfg">Weights L2 (ridge) regularization configuration (can be null).</param>
        /// <param name="normConsCfg">Weights norm constraint configuration (can be null).</param>
        public HiddenLayerConfig(int numOfNeurons,
                                 ActivationFnID activationID,
                                 DropoutConfig dropoutCfg = null,
                                 RegL1Config regL1Cfg = null,
                                 RegL2Config regL2Cfg = null,
                                 NormConsConfig normConsCfg = null
                                 )
        {
            NumOfNeurons = numOfNeurons;
            ActivationID = activationID;
            DropoutCfg = dropoutCfg == null ? new DropoutConfig() : (DropoutConfig)dropoutCfg.DeepClone();
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
        public HiddenLayerConfig(HiddenLayerConfig source)
            : this(source.NumOfNeurons, source.ActivationID, source.DropoutCfg, source.RegL1Cfg,
                   source.RegL2Cfg, source.NormConsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public HiddenLayerConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfNeurons = int.Parse(validatedElem.Attribute("neurons").Value);
            ActivationID = ActivationFactory.ParseAFnID(validatedElem.Attribute("activation").Value);
            XElement dropoutElem = validatedElem.Elements("dropout").FirstOrDefault();
            DropoutCfg = dropoutElem == null ? new DropoutConfig() : new DropoutConfig(dropoutElem);
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
        public bool IsDefaultDropoutCfg { get { return (DropoutCfg.ContainsOnlyDefaults); } }

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
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (NumOfNeurons < 1)
            {
                throw new ArgumentException($"Invalid number of layer neurons {NumOfNeurons.ToString(CultureInfo.InvariantCulture)}. Number of layer neurons must be GT 0.", nameof(NumOfNeurons));
            }
            if (!ActivationFactory.IsSuitableForMLPHiddenLayer(ActivationID))
            {
                throw new ArgumentException($"{ActivationID} activation function cannot be used in a hidden layer.", nameof(ActivationID));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new HiddenLayerConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("neurons", NumOfNeurons.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("activation", ActivationID.ToString())
                                             );
            if (!suppressDefaults || !IsDefaultDropoutCfg)
            {
                rootElem.Add(DropoutCfg.GetXml(suppressDefaults));
            }
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
            return GetXml("layer", suppressDefaults);
        }

    }//HiddenLayerConfig

}//Namespace
