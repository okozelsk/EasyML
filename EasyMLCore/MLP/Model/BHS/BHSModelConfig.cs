using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the BHSModel.
    /// </summary>
    [Serializable]
    public class BHSModelConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "BHSModelConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying how many times to repeat halfing. Default value is 1.
        /// </summary>
        public const int DefaultRepetitions = 1;

        //Attribute properties
        /// <summary>
        /// Configuration of a HS model.
        /// </summary>
        public HSModelConfig HSModelCfg { get; }

        /// <summary>
        /// Specifies how many times to repeat halfing.
        /// </summary>
        public int Repetitions { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="hsModelCfg">Configuration of a HS model.</param>
        /// <param name="Repetitions">Specifies how many times to repeat halfing. Default value is 1.</param>
        public BHSModelConfig(HSModelConfig hsModelCfg,
                              int repetitions = DefaultRepetitions
                              )
        {
            HSModelCfg = (HSModelConfig)hsModelCfg.DeepClone();
            Repetitions = repetitions;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public BHSModelConfig(BHSModelConfig source)
            : this(source.HSModelCfg, source.Repetitions)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public BHSModelConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Repetitions = int.Parse(validatedElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
            //Stack
            XElement hsModelElem = validatedElem.Element("hsModel");
            HSModelCfg = new HSModelConfig(hsModelElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRepetitions { get { return (Repetitions == DefaultRepetitions); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Repetitions <= 0)
            {
                throw new ArgumentException($"Invalid Repetitions {Repetitions.ToString(CultureInfo.InvariantCulture)}. Repetitions must be GT 0.", nameof(Repetitions));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new BHSModelConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             HSModelCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultRepetitions)
            {
                rootElem.Add(new XAttribute("repetitions", Repetitions.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("bhsModel", suppressDefaults);
        }

    }//BHSModelConfig

}//Namespace

