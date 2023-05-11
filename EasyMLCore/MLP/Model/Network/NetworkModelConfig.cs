using EasyMLCore.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Configuration of the NetworkModel.
    /// </summary>
    [Serializable]
    public class NetworkModelConfig : ConfigBase, IModelConfig
    {
        //Constants
        /// <summary>
        /// Name of an associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NetworkModelConfig";
        /// <summary>
        /// Numeric code of automatic determination of the batch size.
        /// </summary>
        public const int AutoBatchSizeNumCode = 0;
        /// <summary>
        /// String code of automatic determination of the batch size.
        /// </summary>
        public const string AutoBatchSizeStrCode = "Auto";
        /// <summary>
        /// Numeric code of the full batch size (BGD).
        /// </summary>
        public const int FullBatchSizeNumCode = -1;
        /// <summary>
        /// String code of the full batch size (BGD).
        /// </summary>
        public const string FullBatchSizeStrCode = "Full";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying batch size. Default value is Auto (parameter to be determined automatically at runtime).
        /// </summary>
        public const int DefaultBatchSize = AutoBatchSizeNumCode;
        /// <summary>
        /// Default value of the parameter specifying max norm of the weight gradients. Default value is 0 (means no clipping).
        /// </summary>
        public const double DefaultGradClipNorm = 0d;
        /// <summary>
        /// Default value of the parameter specifying max absolute value of the weight gradient. Default value is 0 (means no clipping).
        /// </summary>
        public const double DefaultGradClipVal = 0d;
        /// <summary>
        /// Default value of the parameter specifying whether to apply class-balanced loss. Default value is true.
        /// </summary>
        public const bool DefaultClassBalancedLoss = true;
        /// <summary>
        /// Default value of the parameter specifying the ratio of continuous non-improvement epochs to stop current training attempt. Default value is 1/4.
        /// </summary>
        public const double DefaultStopAttemptPatiency = 0.25d;

        //Attribute properties
        /// <summary>
        /// Configuration of an associated optimizer.
        /// </summary>
        public IOptimizerConfig OptimizerCfg { get; }
        
        /// <inheritdoc cref="InputOptionsConfig"/>
        public InputOptionsConfig InputOptionsCfg { get; }

        /// <inheritdoc cref="HiddenLayersConfig"/>
        public HiddenLayersConfig HiddenLayersCfg { get; }

        /// <inheritdoc cref="OutputOptionsConfig"/>
        public OutputOptionsConfig OutputOptionsCfg { get; }

        /// <inheritdoc cref="LearningThrottleValveConfig"/>
        public LearningThrottleValveConfig LearningThrottleValveCfg { get; }

        /// <summary>
        /// Maximum number of training attempts.
        /// </summary>
        public int Attempts { get; }

        /// <summary>
        /// Maximum number of epochs within a training attempt.
        /// </summary>
        public int Epochs { get; }

        /// <summary>
        /// Specifies number of samples within one batch.
        /// </summary>
        public int BatchSize { get; }

        /// <summary>
        /// Specifies max norm of the weight gradients.
        /// </summary>
        public double GradClipNorm { get; }

        /// <summary>
        /// Specifies max absolute value of the weight gradient.
        /// </summary>
        public double GradClipVal { get; }

        /// <summary>
        /// Specifying whether to apply class-balanced loss.
        /// </summary>
        public bool ClassBalancedLoss { get; }

        /// <summary>
        /// Specifies the ratio of continuous non-improvement epochs to stop current training attempt.
        /// </summary>
        public double StopAttemptPatiency { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="attempts">Maximum number of training attempts.</param>
        /// <param name="epochs">Maximum number of epochs within the training attempt.</param>
        /// <param name="optimizerCfg">Configuration of an associated optimizer.</param>
        /// <param name="hiddenLayersCfg">Configuration of the MLP network's hidden layers (can be null).</param>
        /// <param name="inputOptionsCfg">Configuration of the MLP network's input options (can be null).</param>
        /// <param name="outputOptionsCfg">Configuration of the MLP network's output options (can be null).</param>
        /// <param name="learningThrottleValveCfg">Configuration of the Learning Throttle Valve (can be null).</param>
        /// <param name="batchSize">Specifies number of samples within one batch. Default value is Auto (to be determined automatically at runtime).</param>
        /// <param name="gradClipNorm">Specifies max norm of the weight gradients. Default value is 0 (means no clipping).</param>
        /// <param name="gradClipVal">Specifies max absolute value of the weight gradient. Default value is 0 (means no clipping).</param>
        /// <param name="classBalancedLoss">Specifies whether to apply class-balanced loss. Default value is true.</param>
        /// <param name="stopAttemptPatiency">Specifies the ratio of continuous non-improvement epochs to stop current training attempt. Default value is 1/4.</param>
        public NetworkModelConfig(int attempts,
                                  int epochs,
                                  IOptimizerConfig optimizerCfg,
                                  HiddenLayersConfig hiddenLayersCfg = null,
                                  InputOptionsConfig inputOptionsCfg = null,
                                  OutputOptionsConfig outputOptionsCfg = null,
                                  LearningThrottleValveConfig learningThrottleValveCfg = null,
                                  int batchSize = DefaultBatchSize,
                                  double gradClipNorm = DefaultGradClipNorm,
                                  double gradClipVal = DefaultGradClipVal,
                                  bool classBalancedLoss = DefaultClassBalancedLoss,
                                  double stopAttemptPatiency = DefaultStopAttemptPatiency
                                  )
        {
            Attempts = attempts;
            Epochs = epochs;
            OptimizerCfg = (IOptimizerConfig)optimizerCfg.DeepClone();
            HiddenLayersCfg = hiddenLayersCfg == null ? new HiddenLayersConfig() : (HiddenLayersConfig)hiddenLayersCfg.DeepClone();
            InputOptionsCfg = inputOptionsCfg == null ? new InputOptionsConfig() : (InputOptionsConfig)inputOptionsCfg.DeepClone();
            OutputOptionsCfg = outputOptionsCfg == null ? new OutputOptionsConfig() : (OutputOptionsConfig)outputOptionsCfg.DeepClone();
            LearningThrottleValveCfg = learningThrottleValveCfg == null ? new LearningThrottleValveConfig() : (LearningThrottleValveConfig)learningThrottleValveCfg.DeepClone();
            BatchSize = batchSize;
            GradClipNorm = gradClipNorm;
            GradClipVal = gradClipVal;
            ClassBalancedLoss = classBalancedLoss;
            StopAttemptPatiency = stopAttemptPatiency;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NetworkModelConfig(NetworkModelConfig source)
            : this(source.Attempts, source.Epochs, source.OptimizerCfg,
                   source.HiddenLayersCfg, source.InputOptionsCfg, source.OutputOptionsCfg,
                   source.LearningThrottleValveCfg, source.BatchSize,
                   source.GradClipNorm, source.GradClipVal, source.ClassBalancedLoss,
                   source.StopAttemptPatiency)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public NetworkModelConfig(XElement elem)
        {
            //Validation
            XElement validatedElem = Validate(elem, XsdTypeName);
            //Parsing
            Attempts = int.Parse(validatedElem.Attribute("attempts").Value, CultureInfo.InvariantCulture);
            Epochs = int.Parse(validatedElem.Attribute("epochs").Value, CultureInfo.InvariantCulture);
            XElement optimizerElem = validatedElem.Elements().First();
            OptimizerCfg = optimizerElem.Name.LocalName switch
            {
                "rprop" => new RPropConfig(optimizerElem),
                "sgd" => new SGDConfig(optimizerElem),
                "adam" => new AdamConfig(optimizerElem),
                "adabelief" => new AdabeliefConfig(optimizerElem),
                "padam" => new PadamConfig(optimizerElem),
                "adamax" => new AdamaxConfig(optimizerElem),
                "adadelta" => new AdadeltaConfig(optimizerElem),
                _ => throw new ArgumentException($"Unsupported optimizer {optimizerElem.Name.LocalName}.", nameof(elem)),
            };
            XElement hiddenLayersElem = validatedElem.Elements("hiddenLayers").FirstOrDefault();
            HiddenLayersCfg = hiddenLayersElem == null ? new HiddenLayersConfig() : new HiddenLayersConfig(hiddenLayersElem);
            XElement inputElem = validatedElem.Elements("inputOptions").FirstOrDefault();
            InputOptionsCfg = inputElem == null ? new InputOptionsConfig() : new InputOptionsConfig(inputElem);
            XElement outputElem = validatedElem.Elements("outputOptions").FirstOrDefault();
            OutputOptionsCfg = outputElem == null ? new OutputOptionsConfig() : new OutputOptionsConfig(outputElem);
            XElement throttlingElem = validatedElem.Elements("learningThrottleValve").FirstOrDefault();
            LearningThrottleValveCfg = throttlingElem == null ? new LearningThrottleValveConfig() : new LearningThrottleValveConfig(throttlingElem);
            string batchSizeVal = validatedElem.Attribute("batchSize").Value;
            if(batchSizeVal == AutoBatchSizeStrCode)
            {
                BatchSize = AutoBatchSizeNumCode;
            }
            else if(batchSizeVal == FullBatchSizeStrCode)
            {
                BatchSize = FullBatchSizeNumCode;
            }
            else
            {
                BatchSize = int.Parse(batchSizeVal, CultureInfo.InvariantCulture);
            }
            GradClipNorm = double.Parse(validatedElem.Attribute("gradClipNorm").Value, CultureInfo.InvariantCulture);
            GradClipVal = double.Parse(validatedElem.Attribute("gradClipVal").Value, CultureInfo.InvariantCulture);
            ClassBalancedLoss = bool.Parse(validatedElem.Attribute("classBalancedLoss").Value);
            StopAttemptPatiency = double.Parse(validatedElem.Attribute("stopAttemptPatiency").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultHiddenLayersCfg { get { return HiddenLayersCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultInputOptionsCfg { get { return InputOptionsCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultOutputOptionsCfg { get { return OutputOptionsCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultLearningThrottleValveCfg { get { return LearningThrottleValveCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBatchSize { get { return (BatchSize == DefaultBatchSize); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultGradClipNorm { get { return (GradClipNorm == DefaultGradClipNorm); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultGradClipVal { get { return (GradClipVal == DefaultGradClipVal); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultClassBalancedLoss { get { return (ClassBalancedLoss == DefaultClassBalancedLoss); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultStopAttemptPatiency { get { return (StopAttemptPatiency == DefaultStopAttemptPatiency); } }


        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Attempts < 1)
            {
                throw new ArgumentException($"Number of training attempts must be GT 0.", nameof(Attempts));
            }
            if (Epochs < 1)
            {
                throw new ArgumentException($"Number of attempt epochs must be GT 0.", nameof(Epochs));
            }
            if(BatchSize <= 0 && BatchSize != AutoBatchSizeNumCode && BatchSize != FullBatchSizeNumCode)
            {
                throw new ArgumentException($"Batch size must be GT 0 or defined code value.", nameof(BatchSize));
            }
            if (GradClipNorm < 0d)
            {
                throw new ArgumentException($"Gradient clip-norm must be GE 0.", nameof(GradClipNorm));
            }
            if (GradClipVal < 0d)
            {
                throw new ArgumentException($"Gradient clip-val must be GE 0.", nameof(GradClipVal));
            }
            if(GradClipNorm > 0d && GradClipVal > 0d)
            {
                throw new ArgumentException($"Gradient clip-val and clip-norm can not go together.");
            }
            if (StopAttemptPatiency < 0d || StopAttemptPatiency >= 1d)
            {
                throw new ArgumentException($"The ratio of continuous non-improvement epochs to stop training attempt must be GE to 0 and LT 1.", nameof(StopAttemptPatiency));
            }
            //RProp optimizer specific checks
            if (OptimizerCfg.OptimizerID == Optimizer.RProp)
            {
                if (BatchSize != FullBatchSizeNumCode && BatchSize != AutoBatchSizeNumCode)
                {
                    throw new ArgumentException($"In case of RProp optimizer are allowed only Full or Auto values.", nameof(BatchSize));
                }
                bool dropout = (InputOptionsCfg.DropoutCfg.Mode != DropoutMode.None);
                if (!dropout)
                {
                    foreach (HiddenLayerConfig hlc in HiddenLayersCfg.LayerCfgCollection)
                    {
                        if (hlc.DropoutCfg.Mode != DropoutMode.None)
                        {
                            dropout = true;
                            break;
                        }
                    }
                }
                if (dropout)
                {
                    throw new ArgumentException($"In case of RProp optimizer Dropout is not allowed.", nameof(InputOptionsCfg.DropoutCfg));
                }
            }

            return;
        }

        /// <inheritdoc/>
        public override ConfigBase DeepClone()
        {
            return new NetworkModelConfig(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("attempts", Attempts.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("epochs", Epochs.ToString(CultureInfo.InvariantCulture)),
                                             new XElement(OptimizerCfg.GetXml(suppressDefaults))
                                             );
            if (!suppressDefaults || !IsDefaultHiddenLayersCfg)
            {
                rootElem.Add(HiddenLayersCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultInputOptionsCfg)
            {
                rootElem.Add(InputOptionsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultOutputOptionsCfg)
            {
                rootElem.Add(OutputOptionsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultLearningThrottleValveCfg)
            {
                rootElem.Add(LearningThrottleValveCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultBatchSize)
            {
                string batchSizeStr;
                if(BatchSize == AutoBatchSizeNumCode)
                {
                    batchSizeStr = AutoBatchSizeStrCode;
                }
                else if(BatchSize == FullBatchSizeNumCode)
                {
                    batchSizeStr = FullBatchSizeStrCode;
                }
                else
                {
                    batchSizeStr = BatchSize.ToString(CultureInfo.InvariantCulture);
                }
                rootElem.Add(new XAttribute("batchSize", batchSizeStr));
            }
            if (!suppressDefaults || !IsDefaultGradClipNorm)
            {
                rootElem.Add(new XAttribute("gradClipNorm", GradClipNorm.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultGradClipVal)
            {
                rootElem.Add(new XAttribute("gradClipVal", GradClipVal.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultClassBalancedLoss)
            {
                rootElem.Add(new XAttribute("classBalancedLoss", ClassBalancedLoss.GetXmlCode()));
            }
            if (!suppressDefaults || !IsDefaultStopAttemptPatiency)
            {
                rootElem.Add(new XAttribute("stopAttemptPatiency", StopAttemptPatiency.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("networkModel", suppressDefaults);
        }

    }//NetworkModelConfig

}//Namespace

