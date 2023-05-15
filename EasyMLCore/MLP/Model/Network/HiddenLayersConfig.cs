using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the MLP network's hidden layers.
    /// </summary>
    [Serializable]
    public class HiddenLayersConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "HiddenLayersConfig";

        //Attribute properties
        /// <summary>
        /// The collection of layer configurations.
        /// </summary>
        public List<HiddenLayerConfig> LayerCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="layerCfgs">The collection of layer configurations.</param>
        public HiddenLayersConfig(IEnumerable<HiddenLayerConfig> layerCfgs)
        {
            LayerCfgCollection = new List<HiddenLayerConfig>();
            foreach (HiddenLayerConfig layerCfg in layerCfgs)
            {
                LayerCfgCollection.Add((HiddenLayerConfig)layerCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="layerCfgs">Layer configuration (params).</param>
        public HiddenLayersConfig(params HiddenLayerConfig[] layerCfgs)
            : this(layerCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfNeurons">Number of layer neurons.</param>
        /// <param name="activationID">Layer's activation function identifier.</param>
        /// <param name="numOfLayers">Number of hidden layers.</param>
        /// <param name="dropoutCfg">Dropout configuration (can be null).</param>
        /// <param name="regL1Cfg">Weights L1 (lasso) regularization configuration (can be null).</param>
        /// <param name="regL2Cfg">Weights L2 (ridge) regularization configuration (can be null).</param>
        /// <param name="normConsCfg">Weights norm constraint configuration (can be null).</param>
        public HiddenLayersConfig(int numOfNeurons,
                                  ActivationFnID activationID,
                                  int numOfLayers,
                                  DropoutConfig dropoutCfg = null,
                                  RegL1Config regL1Cfg = null,
                                  RegL2Config regL2Cfg = null,
                                  NormConsConfig normConsCfg = null
                                  )
        {
            LayerCfgCollection = new List<HiddenLayerConfig>(numOfLayers);
            for (int i = 0; i < numOfLayers; i++)
            {
                LayerCfgCollection.Add(new HiddenLayerConfig(numOfNeurons,
                                                             activationID,
                                                             dropoutCfg,
                                                             regL1Cfg,
                                                             regL2Cfg,
                                                             normConsCfg
                                                             )
                                       );
            }
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public HiddenLayersConfig(HiddenLayersConfig source)
            : this(source.LayerCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public HiddenLayersConfig(XElement elem)
            : this()
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            foreach (XElement layerElem in validatedElem.Elements("layer"))
            {
                LayerCfgCollection.Add(new HiddenLayerConfig(layerElem));
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultLayerCfgCollection { get { return (LayerCfgCollection.Count == 0); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultLayerCfgCollection; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new HiddenLayersConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (HiddenLayerConfig layerCfg in LayerCfgCollection)
            {
                rootElem.Add(layerCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("hiddenLayers", suppressDefaults);
        }

    }//HiddenLayersConfig

}//Namespace
