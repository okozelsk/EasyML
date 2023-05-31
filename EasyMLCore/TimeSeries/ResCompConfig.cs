using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of a reservoir computer.
    /// </summary>
    [Serializable]
    public class ResCompConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ResCompConfig";

        //Attribute properties
        /// <inheritdoc cref="ReservoirConfig"/>
        public ReservoirConfig ReservoirCfg { get; }

        /// <summary>
        /// The collection of task configurations.
        /// </summary>
        public List<ResCompTaskConfig> TaskCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirCfg">Configuration of the reservoir.</param>
        /// <param name="taskCfgCollection">The collection of task configurations.</param>
        public ResCompConfig(ReservoirConfig reservoirCfg,
                             IEnumerable<ResCompTaskConfig> taskCfgCollection
                             )
        {
            ReservoirCfg = (ReservoirConfig)reservoirCfg.DeepClone();
            TaskCfgCollection = new List<ResCompTaskConfig>();
            foreach (ResCompTaskConfig taskCfg in taskCfgCollection)
            {
                TaskCfgCollection.Add(new ResCompTaskConfig(taskCfg));
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirCfg">Configuration of the reservoir.</param>
        /// <param name="taskCfgs">Task configuration (params).</param>
        public ResCompConfig(ReservoirConfig reservoirCfg,
                             params ResCompTaskConfig[] taskCfgs
                             )
            :this(reservoirCfg, (taskCfgs.AsEnumerable()))
        {
            return;
        }
        
        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ResCompConfig(ResCompConfig source)
            : this(source.ReservoirCfg, source.TaskCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ResCompConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement inputElem = validatedElem.Elements("reservoir").FirstOrDefault();
            ReservoirCfg = new ReservoirConfig(inputElem);
            foreach (XElement taskElem in validatedElem.Elements("task"))
            {
                TaskCfgCollection.Add(new ResCompTaskConfig(taskElem));
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultReservoirCfg { get { return ReservoirCfg.ContainsOnlyDefaults; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            //Names uniqueness check
            if (!(from taskCfg in TaskCfgCollection select taskCfg.Name).ContainsOnlyUniques())
            {
                throw new ArgumentException($"Task names are not unique.", nameof(TaskCfgCollection));
            }
            //Check ResInput vs varying length pattern feeding
            if(ReservoirCfg.InputCfg.Feeding == Reservoir.InputFeeding.PatternVarLength)
            {
                foreach(ResCompTaskConfig taskCfg in TaskCfgCollection)
                {
                    foreach(ResCompTaskInputSectionConfig sectionCfg in taskCfg.InputSectionsCfg.InputSectionCfgCollection)
                    {
                        if(sectionCfg.Name == Reservoir.OutSection.ResInputs)
                        {
                            throw new ArgumentException($"Incompatible input section in one or more task configuratins. When reservoir's feeding mode is varying length patterns, reservoir input can not be used as an input to task.", nameof(TaskCfgCollection));
                        }
                    }
                }
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new ResCompConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             ReservoirCfg.GetXml(suppressDefaults)
                                             );

            foreach (ResCompTaskConfig taskCfg in TaskCfgCollection)
            {
                rootElem.Add(taskCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("rescomp", suppressDefaults);
        }

    }//ResCompConfig

}//Namespace

