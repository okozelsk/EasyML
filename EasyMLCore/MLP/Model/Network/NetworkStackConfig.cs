using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the stack of NetworkModel configurations.
    /// </summary>
    [Serializable]
    public class NetworkStackConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NetworkStackConfig";

        //Attribute properties
        /// <summary>
        /// The collection of Network model configurations.
        /// </summary>
        public List<NetworkModelConfig> NetworkModelCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="networkModelCfgs">The collection of Network model configurations.</param>
        public NetworkStackConfig(IEnumerable<NetworkModelConfig> networkModelCfgs)
        {
            NetworkModelCfgCollection = new List<NetworkModelConfig>();
            foreach (NetworkModelConfig modelCfg in networkModelCfgs)
            {
                NetworkModelCfgCollection.Add((NetworkModelConfig)modelCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="networkModelCfgs">Network model configuration (params).</param>
        public NetworkStackConfig(params NetworkModelConfig[] networkModelCfgs)
            : this(networkModelCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NetworkStackConfig(NetworkStackConfig source)
            : this(source.NetworkModelCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public NetworkStackConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            NetworkModelCfgCollection = new List<NetworkModelConfig>();
            //Parsing
            foreach (XElement modelElem in validatedElem.Elements("networkModel"))
            {
                NetworkModelCfgCollection.Add(new NetworkModelConfig(modelElem));
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
            if (NetworkModelCfgCollection.Count < 1)
            {
                throw new ArgumentException($"At least one Network model configuration has to be defined.", nameof(NetworkModelCfgCollection));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new NetworkStackConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (NetworkModelConfig modelCfg in NetworkModelCfgCollection)
            {
                rootElem.Add(modelCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("stack", suppressDefaults);
        }

    }//NetworkStackConfig

}//Namespace
