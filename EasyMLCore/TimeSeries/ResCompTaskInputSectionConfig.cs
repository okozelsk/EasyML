using System;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of the ResCompTask input section.
    /// </summary>
    [Serializable]
    public class ResCompTaskInputSectionConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ResCompTaskInputSectionConfig";

        //Attribute properties
        /// <summary>
        /// Section name.
        /// </summary>
        public Reservoir.OutSection Name { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Section name.</param>
        public ResCompTaskInputSectionConfig(Reservoir.OutSection name)
        {
            Name = name;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ResCompTaskInputSectionConfig(ResCompTaskInputSectionConfig source)
            : this(source.Name)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ResCompTaskInputSectionConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = (Reservoir.OutSection)Enum.Parse(typeof(Reservoir.OutSection), validatedElem.Attribute("name").Value);
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
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new ResCompTaskInputSectionConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name.ToString())
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("section", suppressDefaults);
        }

    }//ResCompTaskInputSectionConfig

}//Namespace
