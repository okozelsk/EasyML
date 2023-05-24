using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the CompositeModel.
    /// </summary>
    [Serializable]
    public class CompositeModelConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "CompositeModelConfig";

        //Attribute properties
        /// <summary>
        /// The collection of submodel configurations.
        /// </summary>
        public List<IModelConfig> SubModelCfgCollection { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="subModelCfgs">The collection of submodel configurations.</param>
        public CompositeModelConfig(IEnumerable<IModelConfig> subModelCfgs)
        {
            SubModelCfgCollection = new List<IModelConfig>();
            foreach (IModelConfig modelCfg in subModelCfgs)
            {
                SubModelCfgCollection.Add((IModelConfig)modelCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="subModelCfgs">Submodel configuration (params).</param>
        public CompositeModelConfig(params IModelConfig[] subModelCfgs)
            : this(subModelCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CompositeModelConfig(CompositeModelConfig source)
            : this(source.SubModelCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public CompositeModelConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            SubModelCfgCollection = new List<IModelConfig>();
            //Parsing
            //Models
            foreach (XElement modelElem in validatedElem.Elements())
            {
                switch (modelElem.Name.LocalName)
                {
                    case "networkModel":
                        SubModelCfgCollection.Add(new NetworkModelConfig(modelElem));
                        break;
                    case "crossValModel":
                        SubModelCfgCollection.Add(new CrossValModelConfig(modelElem));
                        break;
                    case "stackingModel":
                        SubModelCfgCollection.Add(new StackingModelConfig(modelElem));
                        break;
                    case "bhsModel":
                        SubModelCfgCollection.Add(new BHSModelConfig(modelElem));
                        break;
                    case "rvflModel":
                        SubModelCfgCollection.Add(new RVFLModelConfig(modelElem));
                        break;
                    case "compositeModel":
                        SubModelCfgCollection.Add(new CompositeModelConfig(modelElem));
                        break;
                    default:
                        throw new ArgumentException($"Unknown descendant element name {modelElem.Name.LocalName}.", nameof(elem));
                }
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
            if (SubModelCfgCollection.Count < 1)
            {
                throw new ArgumentException($"At least one model configuration has to be defined.", nameof(SubModelCfgCollection));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new CompositeModelConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (IModelConfig subModelCfg in SubModelCfgCollection)
            {
                rootElem.Add(subModelCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("compositeModel", suppressDefaults);
        }

    }//CompositeModelConfig

}//Namespace
