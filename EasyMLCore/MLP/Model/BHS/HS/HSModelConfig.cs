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
    public class HSModelConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "HSModelConfig";
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
        /// Specifies whether to provide original input to meta-learner.
        /// </summary>
        public bool RouteInput { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="stackCfg">Configuration of a stack of NetworkModel(s) configurations.</param>
        /// <param name="metaLearnerCfg">Configuration of a meta-learner model combining the stack members outputs.</param>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner. Default value is false.</param>
        public HSModelConfig(NetworkStackConfig stackCfg,
                                   IModelConfig metaLearnerCfg,
                                   bool routeInput = DefaultRouteInput
                                   )
        {
            StackCfg = (NetworkStackConfig)stackCfg.DeepClone();
            MetaLearnerCfg = (IModelConfig)metaLearnerCfg.DeepClone();
            RouteInput = routeInput;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public HSModelConfig(HSModelConfig source)
            : this(source.StackCfg, source.MetaLearnerCfg, source.RouteInput)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public HSModelConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
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
        public bool IsDefaultRouteInput { get { return (RouteInput == DefaultRouteInput); } }

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
            return new HSModelConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             StackCfg.GetXml(suppressDefaults),
                                             MetaLearnerCfg.GetXml(suppressDefaults)
                                             );
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
            return GetXml("hsModel", suppressDefaults);
        }

    }//HSModelConfig

}//Namespace

