using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the RVFL hidden layer.
    /// </summary>
    [Serializable]
    public class RVFLHiddenLayerConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RVFLHiddenLayerConfig";

        //Attribute properties
        /// <summary>
        /// The collection of pool configurations.
        /// </summary>
        public List<RVFLHiddenPoolConfig> PoolCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="poolCfgs">The collection of pool configurations.</param>
        public RVFLHiddenLayerConfig(IEnumerable<RVFLHiddenPoolConfig> poolCfgs)
        {
            PoolCfgCollection = new List<RVFLHiddenPoolConfig>();
            foreach (RVFLHiddenPoolConfig poolCfg in poolCfgs)
            {
                PoolCfgCollection.Add((RVFLHiddenPoolConfig)poolCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="poolCfgs">Pool configuration (params).</param>
        public RVFLHiddenLayerConfig(params RVFLHiddenPoolConfig[] poolCfgs)
            : this(poolCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RVFLHiddenLayerConfig(RVFLHiddenLayerConfig source)
            : this(source.PoolCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RVFLHiddenLayerConfig(XElement elem)
            : this()
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            foreach (XElement poolElem in validatedElem.Elements("pool"))
            {
                PoolCfgCollection.Add(new RVFLHiddenPoolConfig(poolElem));
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
            if(PoolCfgCollection.Count == 0)
            {
                throw new ArgumentException("At least one pool has to be specified.", nameof(PoolCfgCollection));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new RVFLHiddenLayerConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (RVFLHiddenPoolConfig poolCfg in PoolCfgCollection)
            {
                rootElem.Add(poolCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("layer", suppressDefaults);
        }

    }//RVFLHiddenLayerConfig

}//Namespace
