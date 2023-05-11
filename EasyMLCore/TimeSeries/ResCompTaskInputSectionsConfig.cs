using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of the ResCompTask input sections.
    /// </summary>
    [Serializable]
    public class ResCompTaskInputSectionsConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ResCompTaskInputSectionsConfig";

        //Attribute properties
        /// <summary>
        /// The collection of ResCompTask input section configurations.
        /// </summary>
        public List<ResCompTaskInputSectionConfig> InputSectionCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="resOutSections">A collection of Reservoir.OutSection identifiers.</param>
        public ResCompTaskInputSectionsConfig(IEnumerable<Reservoir.OutSection> resOutSections)
        {
            InputSectionCfgCollection = new List<ResCompTaskInputSectionConfig>();
            foreach (Reservoir.OutSection name in resOutSections)
            {
                InputSectionCfgCollection.Add(new ResCompTaskInputSectionConfig(name));
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputSectionCfgs">The collection of ResCompTask input section configurations.</param>
        public ResCompTaskInputSectionsConfig(IEnumerable<ResCompTaskInputSectionConfig> inputSectionCfgs)
        {
            InputSectionCfgCollection = new List<ResCompTaskInputSectionConfig>();
            foreach (ResCompTaskInputSectionConfig sectionCfg in inputSectionCfgs)
            {
                InputSectionCfgCollection.Add((ResCompTaskInputSectionConfig)sectionCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputSectionCfgs">Input section configuration (params).</param>
        public ResCompTaskInputSectionsConfig(params ResCompTaskInputSectionConfig[] inputSectionCfgs)
            : this(inputSectionCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputSectionID">Input section identifier (params).</param>
        public ResCompTaskInputSectionsConfig(params Reservoir.OutSection[] inputSectionID)
            : this(inputSectionID.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ResCompTaskInputSectionsConfig(ResCompTaskInputSectionsConfig source)
            : this(source.InputSectionCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ResCompTaskInputSectionsConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            InputSectionCfgCollection = new List<ResCompTaskInputSectionConfig>();
            foreach (XElement sectionElem in validatedElem.Elements("section"))
            {
                InputSectionCfgCollection.Add(new ResCompTaskInputSectionConfig(sectionElem));
            }
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Gets a list of ResCompTask input Reservoir.OutSection sections.
        /// </summary>
        public List<Reservoir.OutSection> GetResOutSections()
        {
            return new List<Reservoir.OutSection>(from sectionCfg in InputSectionCfgCollection select sectionCfg.Name);
        }

        /// <inheritdoc/>
        protected override void Check()
        {
            if (InputSectionCfgCollection.Count < 1)
            {
                throw new ArgumentException($"At least one input section has to be defined.", nameof(InputSectionCfgCollection));
            }
            //Names uniqueness check
            //Uniqueness of the field name
            if(!(from section in InputSectionCfgCollection select section.Name.ToString()).ContainsOnlyUniques())
            {
                throw new ArgumentException($"Input sections are not unique.", nameof(InputSectionCfgCollection));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new ResCompTaskInputSectionsConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (ResCompTaskInputSectionConfig sectionCfg in InputSectionCfgCollection)
            {
                rootElem.Add(sectionCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("inputSections", suppressDefaults);
        }

    }//ResCompTaskInputSectionsConfig

}//Namespace
