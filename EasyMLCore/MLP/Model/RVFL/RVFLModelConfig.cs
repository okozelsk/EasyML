using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the RVFLModel.
    /// </summary>
    [Serializable]
    public class RVFLModelConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RVFLModelConfig";
        /// <summary>
        /// Default value of the parameter specifying the scale factor of the first layer's weights. Default value is 1.
        /// </summary>
        public const double DefaultScaleFactor = 1d;
        /// <summary>
        /// Default value of the parameter specifying whether to provide original input to end-model. Default value is false.
        /// </summary>
        public const bool DefaultRouteInput = false;

        //Attribute properties
        /// <summary>
        /// Configuration of hidden layers.
        /// </summary>
        public RVFLHiddenLayersConfig LayersCfg { get; }

        /// <summary>
        /// Configuration of an end-model.
        /// </summary>
        public IModelConfig EndModelCfg { get; }

        /// <summary>
        /// Specifies the scale factor of the first layer's weights.
        /// </summary>
        public double ScaleFactor { get; }

        /// <summary>
        /// Specifies whether to provide original input to end-model.
        /// </summary>
        public bool RouteInput { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="layersCfg">Configuration of hidden layers.</param>
        /// <param name="endModelCfg">Configuration of an end-model.</param>
        /// <param name="scaleFactor">Specifies the scale factor of the first layer's weights. Default value is 1.</param>
        /// <param name="routeInput">Specifies whether to provide original input to end-model. Default value is false.</param>
        public RVFLModelConfig(RVFLHiddenLayersConfig layersCfg,
                               IModelConfig endModelCfg,
                               double scaleFactor = DefaultScaleFactor,
                               bool routeInput = DefaultRouteInput
                               )
        {
            LayersCfg = (RVFLHiddenLayersConfig)layersCfg.DeepClone();
            EndModelCfg = (IModelConfig)endModelCfg.DeepClone();
            ScaleFactor = scaleFactor;
            RouteInput = routeInput;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RVFLModelConfig(RVFLModelConfig source)
            : this(source.LayersCfg, source.EndModelCfg, source.ScaleFactor,
                   source.RouteInput)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RVFLModelConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            ScaleFactor = double.Parse(validatedElem.Attribute("scaleFactor").Value, CultureInfo.InvariantCulture);
            RouteInput = bool.Parse(validatedElem.Attribute("routeInput").Value);
            //Hidden layers
            XElement layersElem = validatedElem.Element("hiddenLayers");
            LayersCfg = new RVFLHiddenLayersConfig(layersElem);
            //end-model
            XElement endModelElem = layersElem.ElementsAfterSelf().First();
            EndModelCfg = endModelElem.Name.LocalName switch
            {
                "networkModel" => EndModelCfg = new NetworkModelConfig(endModelElem),
                "crossValModel" => EndModelCfg = new CrossValModelConfig(endModelElem),
                "stackingModel" => EndModelCfg = new StackingModelConfig(endModelElem),
                "bhsModel" => EndModelCfg = new BHSModelConfig(endModelElem),
                "compositeModel" => EndModelCfg = new CompositeModelConfig(endModelElem),
                _ => throw new ArgumentException($"Unknown descendant element name {endModelElem.Name.LocalName}.", nameof(elem)),
            };
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRouteInput { get { return (RouteInput == DefaultRouteInput); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultScaleFactor { get { return (ScaleFactor == DefaultScaleFactor); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if(ScaleFactor <= 0d)
            {
                throw new ArgumentException("Scale factor must be GT 0.", nameof(ScaleFactor));
            }
            //Ensure input to end-model
            if(!RouteInput)
            {
                bool endModelInput = false;
                foreach(RVFLHiddenLayerConfig layerCfg in LayersCfg.LayerCfgCollection)
                {
                    foreach(RVFLHiddenPoolConfig poolCfg in layerCfg.PoolCfgCollection)
                    {
                        if(poolCfg.UseOutput)
                        {
                            endModelInput = true;
                            break;
                        }
                    }
                    if(endModelInput)
                    {
                        break;
                    }
                }
                if(!endModelInput)
                {
                    throw new ArgumentException("Input routing must be true when no output is defined on hidden layers.", nameof(RouteInput));
                }
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new RVFLModelConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             LayersCfg.GetXml(suppressDefaults),
                                             EndModelCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultScaleFactor)
            {
                rootElem.Add(new XAttribute("scaleFactor", ScaleFactor.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("rvflModel", suppressDefaults);
        }

    }//RVFLModelConfig

}//Namespace

