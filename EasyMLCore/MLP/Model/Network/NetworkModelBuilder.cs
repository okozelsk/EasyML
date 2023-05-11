using EasyMLCore.Data;
using System;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    //Delegates
    /// <summary>
    /// Delegate of the model's build progress changed event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the model's build process.</param>
    public delegate void ModelBuildProgressChangedHandler(ModelBuildProgressInfo progressInfo);

    /// <summary>
    /// Implements the NetworkModel builder.
    /// </summary>
    public class NetworkModelBuilder
    {
        private bool EnabledFineTuning = true;
        
        /// <summary>
        /// Default training total RMSE treshold.
        /// </summary>
        public const double DefaultRMSETreshold = 1E-6d;

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        public event ModelBuildProgressChangedHandler BuildProgressChanged;

        //Static variables
        /// <summary>
        /// A number used to initialize pseudo random numbers.
        /// </summary>
        private static int RandomSeed = Common.DefaultRandomSeed;

        //Attribute properties
        //Additional stop criterion
        /// <summary>
        /// Stop criterion on training RMSE.
        /// </summary>
        public double RMSETreshold { get; set; }


        //Attributes
        private readonly NetworkModelConfig _cfg;
        private readonly Random _rand;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">Model configuration.</param>
        public NetworkModelBuilder(NetworkModelConfig cfg)
        {
            _cfg = cfg;
            _rand = new Random(RandomSeed);
            RMSETreshold = DefaultRMSETreshold;
            return;
        }

        //Static methods
        /// <summary>
        /// Changes a number used to initialize pseudo random numbers.
        /// </summary>
        /// <param name="seed">New seed value.</param>
        public static void SetRandomSeed(int seed)
        {
            RandomSeed = seed;
            return;
        }

        /// <summary>
        /// Gets a number to be used to initialize pseudo random numbers.
        /// </summary>
        /// <param name="seed">New seed value.</param>
        public static int GetRandomSeed()
        {
            return RandomSeed;
        }

        //Methods
        /// <summary>
        /// Builds a NetworkModel.
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task type.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="validationData">Validation samples (can be null).</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <param name="engageValidationData">Specifies whether the best network selection will be impacted by network performance on validation data (if data is available).</param>
        /// <returns>Built model.</returns>
        public NetworkModel Build(string name,
                                  OutputTaskType taskType,
                                  IEnumerable<string> outputFeatureNames,
                                  SampleDataset trainingData,
                                  SampleDataset validationData,
                                  ModelBuildProgressChangedHandler progressInfoSubscriber = null,
                                  bool engageValidationData = true
                                  )
        {
            if (engageValidationData && validationData == null)
            {
                throw new ArgumentException("Can't engage validation data. Validation data is missing.", nameof(engageValidationData));
            }
            if (progressInfoSubscriber != null)
            {
                BuildProgressChanged += progressInfoSubscriber;
            }
            NetworkModel bestNet = null;
            int bestNetAttempt = 0;
            int bestNetAttemptEpoch = 0;
            NetworkModel lastImprovementNet = null;
            int lastImprovementEpoch = 0;
            bool inFineTunePhase = false;
            //Name
            if (name.Length == 0)
            {
                name = NetworkModel.ContextPathID;
            }
            //Create network engine and trainer
            //Network engine
            MLPEngine engine = new MLPEngine(taskType, trainingData.InputVectorLength, outputFeatureNames, _cfg);
            //Trainer
            Trainer trainer = new Trainer(_cfg, engine, trainingData, _rand);
            //Iterate training cycles
            while (trainer.Epoch())
            {
                //Create current network instance and compute error statistics after training iteration
                NetworkModel currNet = new NetworkModel(name,
                                                        outputFeatureNames,
                                                        engine,
                                                        trainer.InputFilters,
                                                        trainer.OutputFilters,
                                                        trainer.EpochErrStat,
                                                        validationData
                                                        );
                //Initialization of the best network
                if (bestNet == null)
                {
                    bestNet = (NetworkModel)currNet.DeepClone();
                    bestNetAttempt = trainer.Attempt;
                }
                //Reset attempt scope variables when new training attempt starts
                if (trainer.AttemptEpoch == 1)
                {
                    lastImprovementEpoch = 0;
                    lastImprovementNet = null;
                    inFineTunePhase = false;
                }
                //Update the last improvement point
                if (lastImprovementNet == null || lastImprovementNet.IsBetter(currNet, !engageValidationData))
                {
                    lastImprovementNet = currNet;
                    lastImprovementEpoch = trainer.AttemptEpoch;
                }
                //Stop all attempts?
                bool stopAllAttempts = false;
                //Is current network better than the best network so far?
                if (bestNet.IsBetter(currNet, !engageValidationData))
                {
                    //Adopt current network as the best one
                    bestNet = (NetworkModel)currNet.DeepClone();
                    bestNetAttempt = trainer.Attempt;
                    bestNetAttemptEpoch = trainer.AttemptEpoch;
                    //Entering the fine tune phase?
                    if(engageValidationData)
                    {
                        if(EnabledFineTuning)
                        {
                            inFineTunePhase = (taskType != OutputTaskType.Regression && bestNet.ConfidenceMetrics.BinaryAccuracy == 1d);
                        }
                        else
                        {
                            stopAllAttempts |= (taskType != OutputTaskType.Regression && bestNet.ConfidenceMetrics.BinaryAccuracy == 1d);
                        }
                    }
                }
                else
                {
                    stopAllAttempts |= engageValidationData && inFineTunePhase;
                }
                stopAllAttempts |= inFineTunePhase && trainer.AttemptEpoch == trainer.MaxAttemptEpochs;
                if (!stopAllAttempts && !engageValidationData)
                {
                    //Stop all attempts when accuracy on training data reaches 100%
                    stopAllAttempts = (taskType != OutputTaskType.Regression && ((MultipleDecisionErrStat)currNet.TrainingErrorStat.StatData).BinaryAccuracy == 1d) ||
                                      (taskType == OutputTaskType.Regression && ((MultiplePrecisionErrStat)currNet.TrainingErrorStat.StatData).TotalPrecisionStat.RootMeanSquare < RMSETreshold);
                }
                //Stop current attempt?
                bool stopCurrAttempt = stopAllAttempts;
                if(!stopCurrAttempt)
                {
                    //Stop current training attempt when improvement patiency is over the limit
                    stopCurrAttempt |= (trainer.AttemptEpoch - lastImprovementEpoch >= trainer.MaxAttemptEpochs * _cfg.StopAttemptPatiency);
                    stopCurrAttempt |= ((MultiplePrecisionErrStat)currNet.TrainingErrorStat.StatData).TotalPrecisionStat.RootMeanSquare < RMSETreshold;
                }
                //Progress info
                ModelBuildProgressInfo progressInfo =
                    new ModelBuildProgressInfo(trainer.Attempt,
                                               trainer.MaxAttempts,
                                               trainer.AttemptEpoch,
                                               trainer.MaxAttemptEpochs,
                                               currNet,
                                               bestNet,
                                               bestNetAttempt,
                                               bestNetAttemptEpoch,
                                               stopCurrAttempt
                                               );
                //Raise notification event
                BuildProgressChanged?.Invoke(progressInfo);
                //Stop?
                if (stopAllAttempts)
                {
                    break;
                }
                else if (stopCurrAttempt)
                {
                    //Push trainer to next attempt
                    if (!trainer.NextAttempt())
                    {
                        //No next attempt available
                        break;
                    }
                }
            }//while (trainer iteration)
            return bestNet;
        }

    }//NetworkModelBuilder

}//Namespace
