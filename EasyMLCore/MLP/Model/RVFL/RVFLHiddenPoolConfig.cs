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
        /// <summary>
        /// Default value of the parameter specifying the scale factor of the weights. Default value is 1.
        /// </summary>
        public const double DefaultScaleFactorW = 1d;
        /// <summary>
        /// Default value of the parameter specifying the scale factor of the biases. Default value is 1.
        /// </summary>
        public const double DefaultScaleFactorB = 1d;
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
        /// Specifies the scale factor of the weights.
        /// </summary>
        public double ScaleFactorW { get; }

        /// <summary>
        /// Specifies the scale factor of the biases.
        /// </summary>
        public double ScaleFactorB { get; }

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
        /// <param name="scaleFactorW">Specifies the scale factor of the weights. Default value is 1.</param>
        /// <param name="scaleFactorB">Specifies the scale factor of the biases. Default value is 1.</param>
        /// <param name="useOutput">Specifies whether to use output from this pool as an input for end-model. Default value is true.</param>
        public RVFLHiddenPoolConfig(int numOfNeurons,
                                    ActivationFnID activationID,
                                    double scaleFactorW = DefaultScaleFactorW,
                                    double scaleFactorB = DefaultScaleFactorB,
                                    bool useOutput = DefaultUseOutput
                                    )
        {
            NumOfNeurons = numOfNeurons;
            ActivationID = activationID;
            ScaleFactorW = scaleFactorW;
            ScaleFactorB = scaleFactorB;
            UseOutput = useOutput;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RVFLHiddenPoolConfig(RVFLHiddenPoolConfig source)
            : this(source.NumOfNeurons, source.ActivationID,
                   source.ScaleFactorW, source.ScaleFactorB,
                   source.UseOutput)
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
            ScaleFactorW = double.Parse(validatedElem.Attribute("scaleFactorW").Value, CultureInfo.InvariantCulture);
            ScaleFactorB = double.Parse(validatedElem.Attribute("scaleFactorB").Value, CultureInfo.InvariantCulture);
            UseOutput = bool.Parse(validatedElem.Attribute("useOutput").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultScaleFactorW { get { return (ScaleFactorW == DefaultScaleFactorW); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultScaleFactorB { get { return (ScaleFactorB == DefaultScaleFactorB); } }
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
            if (ScaleFactorW <= 0d)
            {
                throw new ArgumentException("Weights scale factor must be GT 0.", nameof(ScaleFactorW));
            }
            if (ScaleFactorB < 0d)
            {
                throw new ArgumentException("Biases scale factor must be GE 0.", nameof(ScaleFactorB));
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
            if (!suppressDefaults || !IsDefaultScaleFactorW)
            {
                rootElem.Add(new XAttribute("scaleFactorW", ScaleFactorW.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultScaleFactorB)
            {
                rootElem.Add(new XAttribute("scaleFactorB", ScaleFactorB.ToString(CultureInfo.InvariantCulture)));
            }
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
