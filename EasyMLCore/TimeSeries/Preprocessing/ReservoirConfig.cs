using System;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of a reservoir.
    /// </summary>
    [Serializable]
    public class ReservoirConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ReservoirConfig";

        //Attribute properties
        /// <inheritdoc cref="ReservoirInputConfig"/>
        public ReservoirInputConfig InputCfg { get; }

        /// <inheritdoc cref="ReservoirHiddenLayerConfig"/>
        public ReservoirHiddenLayerConfig HiddenLayerCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputCfg">Configuration of the reservoir's input.</param>
        /// <param name="hiddenLayerCfg">Configuration of the reservoir's hidden layer.</param>
        public ReservoirConfig(ReservoirInputConfig inputCfg,
                               ReservoirHiddenLayerConfig hiddenLayerCfg
                               )
        {
            InputCfg = (ReservoirInputConfig)inputCfg.DeepClone();
            HiddenLayerCfg = (ReservoirHiddenLayerConfig)hiddenLayerCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReservoirConfig(ReservoirConfig source)
            : this(source.InputCfg, source.HiddenLayerCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ReservoirConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement inputElem = validatedElem.Elements("input").FirstOrDefault();
            InputCfg = new ReservoirInputConfig(inputElem);
            XElement hiddenLayerElem = validatedElem.Elements("hiddenLayer").FirstOrDefault();
            HiddenLayerCfg = new ReservoirHiddenLayerConfig(hiddenLayerElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultInputCfg { get { return InputCfg.ContainsOnlyDefaults; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultHiddenLayerCfg { get { return HiddenLayerCfg.ContainsOnlyDefaults; } }

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
            return new ReservoirConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             InputCfg.GetXml(suppressDefaults),
                                             HiddenLayerCfg.GetXml(suppressDefaults)
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("reservoir", suppressDefaults);
        }

    }//ReservoirConfig

}//Namespace

