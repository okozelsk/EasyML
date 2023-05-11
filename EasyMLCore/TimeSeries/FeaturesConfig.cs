using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of the features.
    /// </summary>
    [Serializable]
    public class FeaturesConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "FeaturesConfig";

        //Attribute properties
        /// <summary>
        /// The collection of feature configurations.
        /// </summary>
        public List<FeatureConfig> FeatureCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureNames">Feature names.</param>
        public FeaturesConfig(IEnumerable<string> featureNames)
        {
            FeatureCfgCollection = new List<FeatureConfig>();
            foreach (string name in featureNames)
            {
                FeatureCfgCollection.Add(new FeatureConfig(name));
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureCfgs">The collection of feature configurations.</param>
        public FeaturesConfig(IEnumerable<FeatureConfig> featureCfgs)
        {
            FeatureCfgCollection = new List<FeatureConfig>();
            foreach (FeatureConfig featureCfg in featureCfgs)
            {
                FeatureCfgCollection.Add((FeatureConfig)featureCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureCfgs">Feature configuration (params).</param>
        public FeaturesConfig(params FeatureConfig[] featureCfgs)
            : this(featureCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public FeaturesConfig(FeaturesConfig source)
            : this(source.FeatureCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public FeaturesConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            FeatureCfgCollection = new List<FeatureConfig>();
            foreach (XElement featureElem in validatedElem.Elements("feature"))
            {
                FeatureCfgCollection.Add(new FeatureConfig(featureElem));
            }
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Gets a list of defined feature names.
        /// </summary>
        public List<string> GetFeatureNames()
        {
            return new List<string>(from featureCfg in FeatureCfgCollection select featureCfg.Name);
        }

        /// <inheritdoc/>
        protected override void Check()
        {
            if (FeatureCfgCollection.Count < 1)
            {
                throw new ArgumentException($"At least one feature has to be defined.", nameof(FeatureCfgCollection));
            }
            //Names uniqueness check
            if(!(from feature in FeatureCfgCollection select feature.Name).ContainsOnlyUniques())
            {
                throw new ArgumentException($"Feature names are not unique.", nameof(FeatureCfgCollection));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new FeaturesConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (FeatureConfig featureCfg in FeatureCfgCollection)
            {
                rootElem.Add(featureCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("features", suppressDefaults);
        }

    }//FeaturesConfig

}//Namespace
