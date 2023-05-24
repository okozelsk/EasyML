using EasyMLCore.Activation;
using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the RVFL hidden layer's pool.
    /// </summary>
    [Serializable]
    public class RVFLHiddenPoolConfig : ConfigBase
    {
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RVFLHiddenPoolConfig";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying whether to use output from this pool as an input for end-model. Default value is true.
        /// </summary>
        public const bool DefaultUseOutput = true;

        //Attributes
        /// <summary>
        /// Number of pool neurons.
        /// </summary>
        public int NumOfNeurons { get; }

        /// <summary>
        /// Pool's activation function identifier.
        /// </summary>
        public ActivationFnID ActivationID { get; }

        /// <summary>
        /// Specifies whether to use output from this pool as an input for end-model.
        /// </summary>
        public bool UseOutput { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfNeurons">Number of pool neurons.</param>
        /// <param name="activationID">Pool's activation function identifier.</param>
        /// <param name="useOutput">Specifies whether to use output from this pool as an input for end-model. Default value is true.</param>
        public RVFLHiddenPoolConfig(int numOfNeurons,
                                    ActivationFnID activationID,
                                    bool useOutput = DefaultUseOutput
                                    )
        {
            NumOfNeurons = numOfNeurons;
            ActivationID = activationID;
            UseOutput = useOutput;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RVFLHiddenPoolConfig(RVFLHiddenPoolConfig source)
            : this(source.NumOfNeurons, source.ActivationID, source.UseOutput)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RVFLHiddenPoolConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfNeurons = int.Parse(validatedElem.Attribute("neurons").Value);
            ActivationID = ActivationFactory.ParseAFnID(validatedElem.Attribute("activation").Value);
            UseOutput = bool.Parse(validatedElem.Attribute("useOutput").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultUseOutput { get { return UseOutput == DefaultUseOutput; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (NumOfNeurons < 1)
            {
                throw new ArgumentException($"Invalid number of pool neurons {NumOfNeurons.ToString(CultureInfo.InvariantCulture)}. Number of neurons must be GT 0.", nameof(NumOfNeurons));
            }
            if (!ActivationFactory.IsSuitableForMLPHiddenLayer(ActivationID))
            {
                throw new ArgumentException($"{ActivationID} activation function cannot be used in a RVFL pool.", nameof(ActivationID));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new RVFLHiddenPoolConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("neurons", NumOfNeurons.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("activation", ActivationID.ToString())
                                             );
            if (!suppressDefaults || !IsDefaultUseOutput)
            {
                rootElem.Add(new XAttribute("useOutput", UseOutput.GetXmlCode()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pool", suppressDefaults);
        }

    }//RVFLHiddenPoolConfig

}//Namespace
