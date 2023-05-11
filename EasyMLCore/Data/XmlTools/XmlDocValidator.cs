using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements the xml document loader and validator.
    /// </summary>
    [Serializable]
    public class XmlDocValidator : SerializableObject
    {
        //Constants
        //Attributes
        private readonly XmlSchemaSet _schemaSet;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public XmlDocValidator()
        {
            _schemaSet = new XmlSchemaSet();
            return;
        }

        //Methods
        /// <summary>
        /// Adds specified xml schema into the schema set.
        /// </summary>
        /// <param name="xmlSchema">A xml schema to be added.</param>
        public void AddSchema(XmlSchema xmlSchema)
        {
            //Add schema into the schema set
            _schemaSet.Add(xmlSchema);
            return;
        }

        /// <summary>
        /// Loads the xml schema from a stream and adds it into the schema set.
        /// </summary>
        /// <param name="schemaStream">A stream to load from.</param>
        public void AddSchema(Stream schemaStream)
        {
            //Load the schema
            XmlSchema schema = XmlSchema.Read(schemaStream, new ValidationEventHandler(XmlValidationCallback));
            //Add the schema into the schema set
            AddSchema(schema);
            return;
        }

        /// <summary>
        /// Loads a xml document from file.
        /// </summary>
        /// <remarks>
        /// Xml document is validated against the internal SchemaSet.
        /// </remarks>
        /// <param name="filename">Xml file name.</param>
        public XDocument LoadXDocFromFile(string filename)
        {
            var binDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            XDocument xDoc = XDocument.Load(Path.Combine(binDir, filename));
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback), true);
            return xDoc;
        }


        /// <summary>
        /// Loads a xml document from string.
        /// </summary>
        /// <remarks>
        /// Xml document is validated against the internal SchemaSet.
        /// </remarks>
        /// <param name="xmlContent">A xml content.</param>
        public XDocument LoadXDocFromString(string xmlContent)
        {

            XDocument xDoc = XDocument.Parse(xmlContent);
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback), true);
            return xDoc;
        }

        /// <summary>
        /// Callback function called during the xml validation.
        /// </summary>
        private void XmlValidationCallback(object sender, ValidationEventArgs args)
        {
            throw new InvalidOperationException($"Validation error: {args.Message}");
        }

    }//XmlDocValidator

}//Namespace

