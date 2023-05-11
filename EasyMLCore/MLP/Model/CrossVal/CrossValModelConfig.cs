using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the CrossValModel.
    /// </summary>
    [Serializable]
    public class CrossValModelConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "CrossValModelConfig";
        /// <summary>
        /// The maximum fold data ratio.
        /// </summary>
        public const double MaxFoldDataRatio = 0.5d;
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the ratio of samples constituting validation data fold. Default value is 1/10.
        /// </summary>
        public const double DefaultFoldDataRatio = 0.1d;

        //Attribute properties
        /// <summary>
        /// Specifies the ratio of samples constituting validation data fold.
        /// </summary>
        public double FoldDataRatio { get; }

        /// <summary>
        /// Network configuration.
        /// </summary>
        public NetworkModelConfig NetworkModelCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="networkModelCfg">Network configuration.</param>
        /// <param name="foldDataRatio">Specifies the ratio of samples constituting validation data fold. Default value is 1/10.</param>
        public CrossValModelConfig(NetworkModelConfig networkModelCfg,
                                  double foldDataRatio = DefaultFoldDataRatio
                                  )
        {
            FoldDataRatio = foldDataRatio;
            NetworkModelCfg = (NetworkModelConfig)networkModelCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CrossValModelConfig(CrossValModelConfig source)
            : this(source.NetworkModelCfg, source.FoldDataRatio)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public CrossValModelConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            FoldDataRatio = double.Parse(validatedElem.Attribute("foldDataRatio").Value, CultureInfo.InvariantCulture);
            NetworkModelCfg = new NetworkModelConfig(validatedElem.Elements("networkModel").First());
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFoldDataRatio { get { return (FoldDataRatio == DefaultFoldDataRatio); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (FoldDataRatio <= 0 || FoldDataRatio > MaxFoldDataRatio)
            {
                throw new ArgumentException($"Invalid FoldDataRatio {FoldDataRatio.ToString(CultureInfo.InvariantCulture)}. TestDataRatio must be GT 0 and LE to {MaxFoldDataRatio.ToString(CultureInfo.InvariantCulture)}.", nameof(FoldDataRatio));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new CrossValModelConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             NetworkModelCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultFoldDataRatio)
            {
                rootElem.Add(new XAttribute("foldDataRatio", FoldDataRatio.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("crossValModel", suppressDefaults);
        }

    }//CrossValModelConfig

}//Namespace

