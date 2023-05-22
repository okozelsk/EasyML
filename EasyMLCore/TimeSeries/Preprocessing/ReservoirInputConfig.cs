using EasyMLCore.Data;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Configuration of the reservoir's input.
    /// </summary>
    [Serializable]
    public class ReservoirInputConfig : ConfigBase
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ReservoirInputConfig";

        //Defult values
        /// <summary>
        /// Default value of the parameter specifying what proportion of all hidden neurons to connect to each input variable.
        /// </summary>
        public const double DefaultDensity = 0.25d;

        /// <summary>
        /// Default value of the parameter specifying the maximum delay of data transfer through an input synapse.
        /// </summary>
        public const int DefaultMaxDelay = 0;

        /// <summary>
        /// Default value of the parameter specifying the maximum strength of an input synapse.
        /// </summary>
        public const double DefaultMaxStrength = 2d;


        //Attributes
        /// <inheritdoc cref="Reservoir.InputFeeding"/>
        public Reservoir.InputFeeding Feeding { get; }

        /// <summary>
        /// Number of input variables.
        /// </summary>
        public int NumOfVariables { get; }

        /// <inheritdoc cref="TimeSeriesPattern.FlatVarSchema"/>
        public TimeSeriesPattern.FlatVarSchema VarSchema { get; }

        /// <summary>
        /// If it is a positive fraction less than 1 then specifies what proportion
        /// of all hidden neurons to connect to each input variable.
        /// If it is an integer greater or equal to 1 then specifies exact number
        /// of hidden neurons to connect to each input variable.
        /// </summary>
        public double Density { get; }

        /// <summary>
        /// Specifies the maximum delay of data transfer through an input synapse (the degree of delay is evenly distributed across the synapses).
        /// </summary>
        public int MaxDelay { get; }

        /// <summary>
        /// Specifies the maximum strength of input per hidden neuron (the minimum strength is then half of the maximum strength).
        /// </summary>
        public double MaxStrength { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="feeding">Specifies regime of input data feeding.</param>
        /// <param name="numOfVariables">Number of input variables.</param>
        /// <param name="varSchema">Schema of input variables organization in the time series flat array.</param>
        /// <param name="density">If it is a positive fraction less than 1 then specifies what proportion of all hidden neurons to connect to each input variable. If it is an integer greater or equal to 1 then specifies exact number of hidden neurons to connect to each input variable. Default is 1/4.</param>
        /// <param name="maxDelay">Specifies the maximum delay of data transfer through an input synapse (the degree of delay is evenly distributed across the synapses). Default is 0 (no delay).</param>
        /// <param name="maxStrength">Specifies the maximum strength of input per hidden neuron (the minimum strength is then half of the maximum strength). Default is 2.</param>
        public ReservoirInputConfig(Reservoir.InputFeeding feeding,
                                    int numOfVariables,
                                    TimeSeriesPattern.FlatVarSchema varSchema,
                                    double density = DefaultDensity,
                                    int maxDelay = DefaultMaxDelay,
                                    double maxStrength = DefaultMaxStrength
                                    )
        {
            Feeding = feeding;
            NumOfVariables = numOfVariables;
            VarSchema = varSchema;
            Density = density;
            MaxDelay = maxDelay;
            MaxStrength = maxStrength;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReservoirInputConfig(ReservoirInputConfig source)
            : this(source.Feeding, source.NumOfVariables, source.VarSchema,
                   source.Density, source.MaxDelay, source.MaxStrength)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ReservoirInputConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Feeding = (Reservoir.InputFeeding)Enum.Parse(typeof(Reservoir.InputFeeding), validatedElem.Attribute("feeding").Value);
            NumOfVariables = int.Parse(validatedElem.Attribute("variables").Value);
            VarSchema = (TimeSeriesPattern.FlatVarSchema)Enum.Parse(typeof(TimeSeriesPattern.FlatVarSchema), validatedElem.Attribute("varSchema").Value);
            Density = double.Parse(validatedElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            MaxDelay = int.Parse(validatedElem.Attribute("maxDelay").Value);
            MaxStrength = double.Parse(validatedElem.Attribute("maxStrength").Value, CultureInfo.InvariantCulture);
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
        public bool IsDefaultMaxStrength { get { return MaxStrength == DefaultMaxStrength; } }
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (NumOfVariables <= 0)
            {
                throw new ArgumentException($"Invalid number of input variables {NumOfVariables.ToString(CultureInfo.InvariantCulture)}. It must be GT 0.", nameof(NumOfVariables));
            }
            if (Density <= 0d)
            {
                throw new ArgumentException($"Invalid density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GT 0.", nameof(Density));
            }
            if (Density >= 1d && Math.Floor(Density) != Density)
            {
                throw new ArgumentException($"Invalid density {Density.ToString(CultureInfo.InvariantCulture)}. When GE 1 then it must be an integer value.", nameof(Density));
            }
            if (MaxDelay < 0)
            {
                throw new ArgumentException($"Invalid max delay {MaxDelay.ToString(CultureInfo.InvariantCulture)}. Max delay must be GE 0.", nameof(MaxDelay));
            }
            if (MaxStrength <= 0d)
            {
                throw new ArgumentException($"Invalid max strength {MaxStrength.ToString(CultureInfo.InvariantCulture)}. Max strength must be GT 0.", nameof(MaxStrength));
            }
            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new ReservoirInputConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("feeding", Feeding.ToString()),
                                             new XAttribute("variables", NumOfVariables.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("varSchema", VarSchema.ToString())
                                             );

            if (!suppressDefaults || !IsDefaultDensity)
            {
                rootElem.Add(new XAttribute("density", Density.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMaxDelay)
            {
                rootElem.Add(new XAttribute("maxDelay", MaxDelay.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMaxStrength)
            {
                rootElem.Add(new XAttribute("maxStrength", MaxStrength.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("input", suppressDefaults);
        }

    }//ReservoirInputConfig

}//Namespace
