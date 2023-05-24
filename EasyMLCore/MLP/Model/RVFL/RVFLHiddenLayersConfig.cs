using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the RVFL hidden layers.
    /// </summary>
    [Serializable]
    public class RVFLHiddenLayersConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RVFLHiddenLayersConfig";

        //Attribute properties
        /// <summary>
        /// The collection of layer configurations.
        /// </summary>
        public List<RVFLHiddenLayerConfig> LayerCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="layerCfgs">The collection of layer configurations.</param>
        public RVFLHiddenLayersConfig(IEnumerable<RVFLHiddenLayerConfig> layerCfgs)
        {
            LayerCfgCollection = new List<RVFLHiddenLayerConfig>();
            foreach (RVFLHiddenLayerConfig layerCfg in layerCfgs)
            {
                LayerCfgCollection.Add((RVFLHiddenLayerConfig)layerCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="layerCfgs">Layer configuration (params).</param>
        public RVFLHiddenLayersConfig(params RVFLHiddenLayerConfig[] layerCfgs)
            : this(layerCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RVFLHiddenLayersConfig(RVFLHiddenLayersConfig source)
            : this(source.LayerCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RVFLHiddenLayersConfig(XElement elem)
            : this()
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            foreach (XElement layerElem in validatedElem.Elements("layer"))
            {
                LayerCfgCollection.Add(new RVFLHiddenLayerConfig(layerElem));
            }
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (LayerCfgCollection.Count == 0)
            {
                throw new ArgumentException("At least one hidden layer has to be specified.", nameof(LayerCfgCollection));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new RVFLHiddenLayersConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (RVFLHiddenLayerConfig layerCfg in LayerCfgCollection)
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

    }//RVFLHiddenLayersConfig

}//Namespace
