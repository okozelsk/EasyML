using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the StackingModel.
    /// </summary>
    [Serializable]
    public class StackingModelConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "StackingModelConfig";
        /// <summary>
        /// The maximum fold data ratio.
        /// </summary>
        public const double MaxFoldDataRatio = 0.5d;
        //Default values
        /// <summary>
        /// Default value of the parameter specifying the ratio of samples constituting hold-out data fold. Default value is 1/10.
        /// </summary>
        public const double DefaultFoldDataRatio = 0.1d;
        /// <summary>
        /// Default value of the parameter specifying whether to provide original input to meta-learner. Default value is false.
        /// </summary>
        public const bool DefaultRouteInput = false;

        //Attribute properties
        /// <summary>
        /// Configuration of a stack of NetworkModel(s) configurations.
        /// </summary>
        public NetworkStackConfig StackCfg { get; }

        /// <summary>
        /// Configuration of a meta-learner model combining the stack members outputs.
        /// </summary>
        public IModelConfig MetaLearnerCfg { get; }

        /// <summary>
        /// Specifies the ratio of samples constituting hold-out data fold.
        /// </summary>
        public double FoldDataRatio { get; }

        /// <summary>
        /// Specifies whether to provide original input to meta-learner.
        /// </summary>
        public bool RouteInput { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="stackCfg">Configuration of a stack of NetworkModel(s) configurations.</param>
        /// <param name="metaLearnerCfg">Configuration of a meta-learner model combining the stack members outputs.</param>
        /// <param name="foldDataRatio">Specifies the ratio of samples constituting hold-out data fold. Default value is 1/10.</param>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner. Default value is false.</param>
        public StackingModelConfig(NetworkStackConfig stackCfg,
                                   IModelConfig metaLearnerCfg,
                                   double foldDataRatio = DefaultFoldDataRatio,
                                   bool routeInput = DefaultRouteInput
                                   )
        {
            StackCfg = (NetworkStackConfig)stackCfg.DeepClone();
            MetaLearnerCfg = (IModelConfig)metaLearnerCfg.DeepClone();
            FoldDataRatio = foldDataRatio;
            RouteInput = routeInput;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public StackingModelConfig(StackingModelConfig source)
            : this(source.StackCfg, source.MetaLearnerCfg, source.FoldDataRatio, source.RouteInput)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public StackingModelConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            FoldDataRatio = double.Parse(validatedElem.Attribute("foldDataRatio").Value, CultureInfo.InvariantCulture);
            RouteInput = bool.Parse(validatedElem.Attribute("routeInput").Value);
            //Stack
            XElement stackElem = validatedElem.Element("stack");
            StackCfg = new NetworkStackConfig(stackElem);
            //Meta-Learner
            XElement metaLearnerElem = stackElem.ElementsAfterSelf().First();
            MetaLearnerCfg = metaLearnerElem.Name.LocalName switch
            {
                "networkModel" => MetaLearnerCfg = new NetworkModelConfig(metaLearnerElem),
                "crossValModel" => MetaLearnerCfg = new CrossValModelConfig(metaLearnerElem),
                "stackingModel" => MetaLearnerCfg = new StackingModelConfig(metaLearnerElem),
                "bhsModel" => MetaLearnerCfg = new BHSModelConfig(metaLearnerElem),
                "rvflModel" => MetaLearnerCfg = new RVFLModelConfig(metaLearnerElem),
                "compositeModel" => MetaLearnerCfg = new CompositeModelConfig(metaLearnerElem),
                _ => throw new ArgumentException($"Unknown descendant element name {metaLearnerElem.Name.LocalName}.", nameof(elem)),
            };
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFoldDataRatio { get { return (FoldDataRatio == DefaultFoldDataRatio); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRouteInput { get { return (RouteInput == DefaultRouteInput); } }

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
            return new StackingModelConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             StackCfg.GetXml(suppressDefaults),
                                             MetaLearnerCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultFoldDataRatio)
            {
                rootElem.Add(new XAttribute("foldDataRatio", FoldDataRatio.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultRouteInput)
            {
                rootElem.Add(new XAttribute("routeInput", RouteInput.GetXmlCode()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("stackingModel", suppressDefaults);
        }

    }//StackingModelConfig

}//Namespace

