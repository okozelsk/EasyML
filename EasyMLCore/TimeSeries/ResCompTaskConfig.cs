using EasyMLCore.MLP;
using System;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of the ResCompTask.
    /// </summary>
    [Serializable]
    public class ResCompTaskConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ResCompTaskConfig";

        //Attribute properties
        /// <summary>
        /// ResCompTask name.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc cref="OutputTaskType"/>
        public OutputTaskType TaskType { get; }

        /// <summary>
        /// Input sections configuration.
        /// </summary>
        public ResCompTaskInputSectionsConfig InputSectionsCfg { get; }

        /// <summary>
        /// Output features configuration.
        /// </summary>
        public FeaturesConfig OutputFeaturesCfg { get; }

        /// <summary>
        /// Model configuration.
        /// </summary>
        public IModelConfig ModelCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">ResCompTask name.</param>
        /// <param name="taskType">Computation output task type.</param>
        /// <param name="inputSectionsCfg">Input sections configuration.</param>
        /// <param name="outputFeaturesCfg">Output features configuration.</param>
        /// <param name="modelCfg">Model configuration.</param>
        public ResCompTaskConfig(string name,
                                 OutputTaskType taskType,
                                 ResCompTaskInputSectionsConfig inputSectionsCfg,
                                 FeaturesConfig outputFeaturesCfg,
                                 IModelConfig modelCfg
                                 )
        {
            Name = name;
            TaskType = taskType;
            InputSectionsCfg = (ResCompTaskInputSectionsConfig)inputSectionsCfg.DeepClone();
            OutputFeaturesCfg = (FeaturesConfig)outputFeaturesCfg.DeepClone();
            ModelCfg = (IModelConfig)modelCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ResCompTaskConfig(ResCompTaskConfig source)
            : this(source.Name, source.TaskType, source.InputSectionsCfg,
                   source.OutputFeaturesCfg, source.ModelCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ResCompTaskConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = validatedElem.Attribute("name").Value;
            TaskType = (OutputTaskType)Enum.Parse(typeof(OutputTaskType), validatedElem.Attribute("taskType").Value, true);
            InputSectionsCfg = new ResCompTaskInputSectionsConfig(validatedElem.Elements("inputSections").First());
            OutputFeaturesCfg = new FeaturesConfig(validatedElem.Elements("outputFeatures").First());
            //Model configuration
            XElement modelElem = validatedElem.Elements().Last();
            ModelCfg = modelElem.Name.LocalName switch
            {
                "networkModel" => new NetworkModelConfig(modelElem),
                "crossValModel" => new CrossValModelConfig(modelElem),
                "stackingModel" => new StackingModelConfig(modelElem),
                "bhsModel" => new BHSModelConfig(modelElem),
                "rvflModel" => new RVFLModelConfig(modelElem),
                "compositeModel" => new CompositeModelConfig(modelElem),
                _ => throw new ArgumentException($"Unknown model element name {modelElem.Name.LocalName}.", nameof(elem)),
            };
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
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Task name cannot be empty.", nameof(Name));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new ResCompTaskConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             new XAttribute("taskType", TaskType.ToString()),
                                             InputSectionsCfg.GetXml(suppressDefaults),
                                             OutputFeaturesCfg.GetXml("outputFeatures", suppressDefaults),
                                             ModelCfg.GetXml(suppressDefaults)
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("task", suppressDefaults);
        }

    }//ResCompTaskConfig  

}//Namespace

