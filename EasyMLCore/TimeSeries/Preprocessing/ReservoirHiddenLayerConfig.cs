using EasyMLCore.Activation;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of the reservoir's hidden layer.
    /// </summary>
    [Serializable]
    public class ReservoirHiddenLayerConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ReservoirHiddenLayerConfig";

        //Defult values
        /// <summary>
        /// Default value of the parameter specifying what proportion of all hidden neurons to connect to each hidden neuron.
        /// </summary>
        public const double DefaultDensity = 0.1d;

        /// <summary>
        /// Default value of the parameter specifying the maximum delay of data transfer through a hidden synapse.
        /// </summary>
        public const int DefaultMaxDelay = 0;

        /// <summary>
        /// Default value of the parameter specifying the activation function of hidden neurons.
        /// </summary>
        public const ActivationFnID DefaultActivationID = ActivationFnID.TanH;

        /// <summary>
        /// Default value of the parameter specifying the retainment of hidden neuron.
        /// </summary>
        public const double DefaultRetainment = 0d;

        /// <summary>
        /// Default value of spike event threshold. When the new activation is higher than the previous by this threshold, a spike event is emitted.
        /// </summary>
        public const double DefaultSpikeEventThreshold = 0.00125d;

        /// <summary>
        /// Default value of the parameter specifying the spectral radius of hidden weights matrix.
        /// </summary>
        public const double DefaultSpectralRadius = 0.999;

        
        //Attributes
        /// <summary>
        /// Number of hidden neurons.
        /// </summary>
        public int NumOfNeurons { get; }

        /// <summary>
        /// If it is a positive fraction less than 1 then specifies what proportion
        /// of all hidden neurons to connect to each hidden neuron.
        /// If it is an integer value greater or equal to 1 then specifies exact number
        /// of hidden neurons to connect to each hidden neuron.
        /// </summary>
        public double Density { get; }

        /// <summary>
        /// Specifies the maximum delay of data transfer through an hidden synapse (the degree of delay is evenly distributed across the synapses).
        /// </summary>
        public int MaxDelay { get; }

        /// <summary>
        /// Specifies the activation function of hidden neurons.
        /// </summary>
        public ActivationFnID ActivationID { get; }

        /// <summary>
        /// Specifies the retainment of hidden neuron.
        /// </summary>
        public double Retainment { get; }

        /// <summary>
        /// When the new activation is higher than the previous by this threshold, a spike event is emitted.
        /// </summary>
        public double SpikeEventThreshold { get; }

        /// <summary>
        /// Spectral radius of hidden weights matrix.
        /// </summary>
        public double SpectralRadius { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfNeurons">Number of hidden neurons.</param>
        /// <param name="density">If it is a positive fraction less than 1 then specifies what proportion of all hidden neurons to connect to each hidden neuron. If it is an integer value greater or equal to 1 then specifies exact number of hidden neurons to connect to each hidden neuron. Default is 1/10.</param>
        /// <param name="maxDelay">Specifies the maximum delay of data transfer through a hidden synapse (the degree of delay is evenly distributed across the synapses). Default is 0 (no delay).</param>
        /// <param name="retainment">Specifies the retainment of hidden neuron. Default is 0.</param>
        /// <param name="spectralRadius">Spectral radius of hidden weights matrix. Default is 0.999.</param>
        /// <param name="spikeEventThreshold">When the new activation is higher than the previous by this threshold, a spike event is emitted. Default is 0.00125.</param>
        /// <param name="activationID">Specifies the activation function of hidden neurons. Default is TanH.</param>
        public ReservoirHiddenLayerConfig(int numOfNeurons,
                                          double density = DefaultDensity,
                                          int maxDelay = DefaultMaxDelay,
                                          double retainment = DefaultRetainment,
                                          double spectralRadius = DefaultSpectralRadius,
                                          double spikeEventThreshold = DefaultSpikeEventThreshold,
                                          ActivationFnID activationID = DefaultActivationID
                                          )
        {

            NumOfNeurons = numOfNeurons;
            Density = density;
            MaxDelay = maxDelay;
            ActivationID = activationID;
            Retainment = retainment;
            SpikeEventThreshold = spikeEventThreshold;
            SpectralRadius = spectralRadius;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReservoirHiddenLayerConfig(ReservoirHiddenLayerConfig source)
            : this(source.NumOfNeurons, source.Density, source.MaxDelay,
                   source.Retainment, source.SpectralRadius,
                   source.SpikeEventThreshold, source.ActivationID)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ReservoirHiddenLayerConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfNeurons = int.Parse(validatedElem.Attribute("neurons").Value);
            Density = double.Parse(validatedElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            MaxDelay = int.Parse(validatedElem.Attribute("maxDelay").Value);
            ActivationID = ActivationFactory.ParseAFnID(validatedElem.Attribute("activation").Value);
            Retainment = double.Parse(validatedElem.Attribute("retainment").Value, CultureInfo.InvariantCulture);
            SpikeEventThreshold = double.Parse(validatedElem.Attribute("spikeEventThreshold").Value, CultureInfo.InvariantCulture);
            SpectralRadius = double.Parse(validatedElem.Attribute("spectralRadius").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultDensity { get { return Density == DefaultDensity; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMaxDelay { get { return MaxDelay == DefaultMaxDelay; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultActivationID { get { return ActivationID == DefaultActivationID; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRetainment { get { return Retainment == DefaultRetainment; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSpikeEventThreshold { get { return SpikeEventThreshold == DefaultSpikeEventThreshold; } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSpectralRadius { get { return SpectralRadius == DefaultSpectralRadius; } }
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (NumOfNeurons < 10)
            {
                throw new ArgumentException($"Invalid number of hidden neurons {NumOfNeurons.ToString(CultureInfo.InvariantCulture)}. Number of hidden neurons must be GE 10.", nameof(NumOfNeurons));
            }
            if (Density <= 0d)
            {
                throw new ArgumentException($"Invalid density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GT 0.", nameof(Density));
            }
            if (Density >= 1d && Math.Floor(Density) != Density)
            {
                throw new ArgumentException($"Invalid density {Density.ToString(CultureInfo.InvariantCulture)}. When GE 1 then it must be an integer value (not a fraction).", nameof(Density));
            }
            if (Density >= 1d && Density > NumOfNeurons)
            {
                throw new ArgumentException($"Invalid density {Density.ToString(CultureInfo.InvariantCulture)}. When GE 1 then it must be an integer value LE to number of neurons.", nameof(Density));
            }
            if (MaxDelay < 0)
            {
                throw new ArgumentException($"Invalid max delay {MaxDelay.ToString(CultureInfo.InvariantCulture)}. Max delay must be GE 0.", nameof(MaxDelay));
            }
            if (!ActivationFactory.IsSuitableForReservoirHiddenLayer(ActivationID))
            {
                throw new ArgumentException($"{ActivationID} activation function cannot be used in a reservoir's hidden layer.", nameof(ActivationID));
            }
            if (Retainment < 0d || Retainment >= 1d)
            {
                throw new ArgumentException($"Invalid retainment {Retainment.ToString(CultureInfo.InvariantCulture)}. Min retainment must be GE 0 and LT 1.", nameof(Retainment));
            }
            if (SpikeEventThreshold <= 0d || SpikeEventThreshold > 1d)
            {
                throw new ArgumentException($"Invalid spike event threshold {SpikeEventThreshold.ToString(CultureInfo.InvariantCulture)}. Spike event threshold must be GT 0 and LE 1.", nameof(SpikeEventThreshold));
            }
            if (SpectralRadius <= 0d)
            {
                throw new ArgumentException($"Invalid spectral radius {SpectralRadius.ToString(CultureInfo.InvariantCulture)}. Spectral radius must be GT 0.", nameof(SpectralRadius));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new ReservoirHiddenLayerConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("neurons", NumOfNeurons.ToString(CultureInfo.InvariantCulture))
                                             );

            if (!suppressDefaults || !IsDefaultDensity)
            {
                rootElem.Add(new XAttribute("density", Density.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMaxDelay)
            {
                rootElem.Add(new XAttribute("maxDelay", MaxDelay.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultActivationID)
            {
                rootElem.Add(new XAttribute("activation", ActivationID.ToString()));
            }
            if (!suppressDefaults || !IsDefaultRetainment)
            {
                rootElem.Add(new XAttribute("retainment", Retainment.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultSpikeEventThreshold)
            {
                rootElem.Add(new XAttribute("spikeEventThreshold", SpikeEventThreshold.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultSpectralRadius)
            {
                rootElem.Add(new XAttribute("spectralRadius", SpectralRadius.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("hiddenLayer", suppressDefaults);
        }

    }//ReservoirHiddenConfig

}//Namespace
