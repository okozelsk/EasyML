using EasyMLCore.MLP;
using EasyMLCore;

namespace EasyMLDemoApp.Examples.MLP
{
    /// <summary>
    /// Shows how to create configurations of the NetworkModel, CrossValModel, StackingModel and CompositeModel.
    /// These configurations are used by other examples.
    /// </summary>
    public static class MLPModelConfigs
    {
        /*
         * Default constants and weights regularization options used in examples.
         * Changes affects almost all examples.
        */

        //Constants
        /// <summary>
        /// Common max number of training attepts of a network engine.
        /// </summary>
        private const int NetBuildMaxTrainingAttempts = 2;
        /// <summary>
        /// Common max number of epochs within one training attempt of a network engine.
        /// </summary>
        private const int NetBuildTrainingAttemptMaxEpochs = 2000;
        /// <summary>
        /// Common ratio of non-improvement epochs to stop training attempt.
        /// </summary>
        private const double NetStopAttemptPatiency = 0.5d;

        //Static members
        //Dropout configuration
        //No input dropout
        private static readonly DropoutConfig InputDropoutCfg = new DropoutConfig(0d, DropoutMode.None);
        //50% Bernouli dropout on networks hidden layers
        private static readonly DropoutConfig HiddenDropoutCfg = new DropoutConfig(0.5d, DropoutMode.Bernoulli);
        //L1 regularization configuration
        //No L1 regularization on networks hidden layers
        private static readonly RegL1Config HiddenRegL1Cfg = new RegL1Config(0d, false);
        //No L1 regularization on networks output layers
        private static readonly RegL1Config OutputRegL1Cfg = new RegL1Config(0d, false);
        //L2 regularization configuration
        //No L2 regularization on networks hidden layers
        private static readonly RegL2Config HiddenRegL2Cfg = new RegL2Config(0d, false);
        //No L2 regularization on networks output layers
        private static readonly RegL2Config OutputRegL2Cfg = new RegL2Config(0d, false);
        //Norm constraints configuration
        //No norm constraint on networks hidden layers
        private static readonly NormConsConfig HiddenNormConsCfg = new NormConsConfig(0d, 0d, false);
        //No norm constraint on networks output layers
        private static readonly NormConsConfig OutputNormConsCfg = new NormConsConfig(0d, 0d, false);

        /*
         * Network model configurations.
        */
        //Methods
        /// <summary>
        /// Creates configuration of the model of single MLP network
        /// having no hidden layers.
        /// </summary>
        /// <param name="batchSize">Specifies number of samples within one mini-batch.</param>
        public static NetworkModelConfig CreateOutputOnlyNetworkModelConfig(int batchSize = NetworkModelConfig.AutoBatchSizeNumCode)
        {
            //Model configuration
            NetworkModelConfig modelCfg =
                new NetworkModelConfig(NetBuildMaxTrainingAttempts, //Maximum number of training attempts
                                       NetBuildTrainingAttemptMaxEpochs, //Maximum number of epochs within a training attempt
                                       new AdamConfig(), //Weights updater
                                       null, //No hidden layers
                                       new InputOptionsConfig(InputDropoutCfg),
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       null, //No learning throttle valve configuration
                                       batchSize, //Batch size
                                       NetworkModelConfig.DefaultGradClipNorm,
                                       NetworkModelConfig.DefaultGradClipVal,
                                       NetworkModelConfig.DefaultClassBalancedLoss,
                                       NetStopAttemptPatiency
                                       );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the model of single MLP network having hidden layers.
        /// </summary>
        /// <param name="aFn">Activation function identifier.</param>
        /// <param name="hiddenLayerSize">Number of neurons on hidden layer.</param>
        /// <param name="numOfHiddenLayers">Number of hidden layers.</param>
        /// <param name="batchSize">Specifies number of samples within one mini-batch.</param>
        public static NetworkModelConfig CreateNetworkModelConfig(ActivationFnID aFn = ActivationFnID.ReLU,
                                                                  int hiddenLayerSize = 50,
                                                                  int numOfHiddenLayers = 2,
                                                                  int batchSize = NetworkModelConfig.AutoBatchSizeNumCode
                                                                  )
        {
            //Network hidden layers configuration
            HiddenLayersConfig hiddenLayersCfg =
                new HiddenLayersConfig(hiddenLayerSize,
                                       aFn,
                                       numOfHiddenLayers,
                                       HiddenDropoutCfg,
                                       HiddenRegL1Cfg,
                                       HiddenRegL2Cfg,
                                       HiddenNormConsCfg
                                       );
            //Network model configuration
            NetworkModelConfig modelCfg =
                new NetworkModelConfig(NetBuildMaxTrainingAttempts, //Maximum number of training attempts
                                       NetBuildTrainingAttemptMaxEpochs, //Maximum number of epochs within a training attempt
                                       new AdamConfig(), //Weights updater
                                       hiddenLayersCfg, //Hidden layers
                                       new InputOptionsConfig(InputDropoutCfg),
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       null, //No learning throttle valve configuration
                                       batchSize, //Batch size
                                       NetworkModelConfig.DefaultGradClipNorm,
                                       NetworkModelConfig.DefaultGradClipVal,
                                       NetworkModelConfig.DefaultClassBalancedLoss,
                                       NetStopAttemptPatiency
                                       );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the model of single MLP network having RProp optimizer and BGD.
        /// </summary>
        /// <param name="trainAttempts">Maximum number of training attempts.</param>
        /// <param name="attemptEpochs">Maximum number of epochs within a training attempt.</param>
        /// <param name="aFn">Activation function identifier.</param>
        /// <param name="hiddenLayerSize">Number of neurons on hidden layer.</param>
        /// <param name="numOfHiddenLayers">Number of hidden layers.</param>
        public static NetworkModelConfig CreateRPropNetworkModelConfig(int trainAttempts,
                                                                       int attemptEpochs,
                                                                       ActivationFnID aFn = ActivationFnID.LeakyReLU,
                                                                       int hiddenLayerSize = 50,
                                                                       int numOfHiddenLayers = 2
                                                                       )
        {
            //Network hidden layers configuration
            HiddenLayersConfig hiddenLayersCfg =
                new HiddenLayersConfig(hiddenLayerSize,
                                       aFn,
                                       numOfHiddenLayers,
                                       new DropoutConfig(),
                                       HiddenRegL1Cfg,
                                       HiddenRegL2Cfg,
                                       HiddenNormConsCfg
                                       );
            //Network model configuration
            NetworkModelConfig modelCfg =
                new NetworkModelConfig(trainAttempts, //Maximum number of training attempts
                                       attemptEpochs, //Maximum number of epochs within a training attempt
                                       new RPropConfig(), //Weights updater
                                       hiddenLayersCfg, //Hidden layers
                                       new InputOptionsConfig(new DropoutConfig()),
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       null, //No learning throttle valve configuration
                                       NetworkModelConfig.AutoBatchSizeNumCode, //Batch size
                                       NetworkModelConfig.DefaultGradClipNorm,
                                       NetworkModelConfig.DefaultGradClipVal,
                                       NetworkModelConfig.DefaultClassBalancedLoss,
                                       NetStopAttemptPatiency
                                       );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the model of single MLP network having BGD RProp optimizer and no hidden layers.
        /// </summary>
        /// <param name="trainAttempts">Maximum number of training attempts.</param>
        /// <param name="attemptEpochs">Maximum number of epochs within a training attempt.</param>
        public static NetworkModelConfig CreateRPropOutputOnlyNetworkModelConfig(int trainAttempts,
                                                                                 int attemptEpochs
                                                                                 )
        {
            //Network model configuration
            NetworkModelConfig modelCfg =
                new NetworkModelConfig(trainAttempts, //Maximum number of training attempts
                                       attemptEpochs, //Maximum number of epochs within a training attempt
                                       new RPropConfig(), //Weights updater
                                       null, //No hidden layers
                                       new InputOptionsConfig(new DropoutConfig()),
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       null, //No learning throttle valve configuration
                                       NetworkModelConfig.AutoBatchSizeNumCode, //Batch size
                                       NetworkModelConfig.DefaultGradClipNorm,
                                       NetworkModelConfig.DefaultGradClipVal,
                                       NetworkModelConfig.DefaultClassBalancedLoss,
                                       NetStopAttemptPatiency
                                       );
            return modelCfg;
        }

        /*
         * CrossVal model configurations.
        */
        /// <summary>
        /// Creates configuration of a CrossValModel of networks.
        /// </summary>
        /// <param name="foldDataRatio">Specifies the ratio of training samples constituting validation fold. Number of resulting validation folds then determines number of member networks.</param>
        public static CrossValModelConfig CreateCrossValModelConfig(double foldDataRatio)
        {
            //Model configuration
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(CreateNetworkModelConfig(ActivationFnID.ReLU), //For every validation fold will be trained a member network having this configuration
                                        foldDataRatio //Validation fold data ratio
                                        );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of a CrossValModel of networks trained by RProp optimizer and BGD.
        /// </summary>
        /// <param name="foldDataRatio">Specifies the ratio of training samples constituting validation fold. Number of resulting validation folds then determines number of member networks.</param>
        /// <param name="trainAttempts">Maximum number of training attempts.</param>
        /// <param name="attemptEpochs">Maximum number of epochs within a training attempt.</param>
        public static CrossValModelConfig CreateRPropCrossValModelConfig(double foldDataRatio,
                                                                         int trainAttempts,
                                                                         int attemptEpochs
                                                                         )
        {
            //Model configuration
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(CreateRPropNetworkModelConfig(trainAttempts, attemptEpochs, ActivationFnID.LeakyReLU, 50, 2), //For every validation fold will be trained a member network having this configuration
                                       foldDataRatio //Validation fold data ratio
                                       );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the CrossValModel of networks without hidden layers.
        /// </summary>
        /// <param name="foldDataRatio">Specifies the ratio of training samples constituting validation fold. Number of resulting validation folds then determines number of member networks.</param>
        public static CrossValModelConfig CreateOutputOnlyNetworkCrossValModelConfig(double foldDataRatio)
        {
            //Model configuration
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(CreateOutputOnlyNetworkModelConfig(), //For every validation fold will be trained a member network having this configuration
                                       foldDataRatio //Validation fold data ratio
                                       );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the CrossValModel of networks with BGD RProp optimizer and no hidden layers.
        /// </summary>
        /// <param name="foldDataRatio">Specifies the ratio of training samples constituting validation fold. Number of resulting validation folds then determines number of member networks.</param>
        /// <param name="trainAttempts">Maximum number of training attempts.</param>
        /// <param name="attemptEpochs">Maximum number of epochs within a training attempt.</param>
        public static CrossValModelConfig CreateRPropOutputOnlyNetworkCrossValModelConfig(double foldDataRatio,
                                                                                          int trainAttempts,
                                                                                          int attemptEpochs
                                                                                          )
        {
            //Model configuration
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(CreateRPropOutputOnlyNetworkModelConfig(trainAttempts, attemptEpochs), //For every validation fold will be trained a member network having this configuration
                                       foldDataRatio //Validation fold data ratio
                                       );
            return modelCfg;
        }

        /*
         * Stacking model configurations.
        */
        /// <summary>
        /// Creates configuration of a stacking model.
        /// </summary>
        /// <param name="foldDataRatio">Specifies the ratio of training samples constituting one hold-out fold.</param>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner.</param>
        public static StackingModelConfig CreateStackingModelConfig(double foldDataRatio = StackingModelConfig.DefaultFoldDataRatio,
                                                                    bool routeInput = StackingModelConfig.DefaultRouteInput
                                                                    )
        {
            //Configuration of networks constituing a stack.
            NetworkStackConfig stackCfg =
                new NetworkStackConfig(CreateNetworkModelConfig(ActivationFnID.LeakyReLU),
                                       CreateNetworkModelConfig(ActivationFnID.ReLU),
                                       CreateNetworkModelConfig(ActivationFnID.GELU),
                                       CreateNetworkModelConfig(ActivationFnID.ELU)
                                       );
            //Model configuration
            StackingModelConfig cfg =
                new StackingModelConfig(stackCfg,
                                        CreateNetworkModelConfig(ActivationFnID.ReLU), //Meta-learner model configuration
                                        foldDataRatio, //Specifies the ratio of training samples constituting one hold-out fold
                                        routeInput //Specifies whether to provide original input to meta-learner.
                                        );
            return cfg;
        }

        /*
         * Stacking model configurations.
        */
        /// <summary>
        /// Creates configuration of a stacking model with RProp optimizer (leads to BGD).
        /// </summary>
        /// <param name="foldDataRatio">Specifies the ratio of training samples constituting one hold-out fold.</param>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner.</param>
        public static StackingModelConfig CreateRPropStackingModelConfig(double foldDataRatio = StackingModelConfig.DefaultFoldDataRatio,
                                                                         bool routeInput = StackingModelConfig.DefaultRouteInput
                                                                         )
        {
            //Configuration of networks constituing a stack.
            NetworkStackConfig stackCfg =
                new NetworkStackConfig(CreateRPropNetworkModelConfig(3, 500, ActivationFnID.LeakyReLU),
                                       CreateRPropNetworkModelConfig(3, 500, ActivationFnID.ReLU),
                                       CreateRPropNetworkModelConfig(3, 500, ActivationFnID.GELU),
                                       CreateRPropNetworkModelConfig(3, 500, ActivationFnID.ELU)
                                       );
            //Model configuration
            StackingModelConfig cfg =
                new StackingModelConfig(stackCfg,
                                        CreateRPropNetworkModelConfig(3, 500, ActivationFnID.ReLU), //Meta-learner model configuration
                                        foldDataRatio, //Specifies the ratio of training samples constituting one hold-out fold
                                        routeInput //Specifies whether to provide original input to meta-learner
                                        );
            return cfg;
        }

        /*
         * Composite model configurations.
        */
        /// <summary>
        /// Creates configuration of the small CompositeModel of three network models.
        /// </summary>
        /// <returns></returns>
        public static CompositeModelConfig CreateSmallCompositeModelConfig()
        {
            //Model configuration
            CompositeModelConfig modelCfg =
                new CompositeModelConfig(CreateNetworkModelConfig(ActivationFnID.ReLU),
                                         CreateNetworkModelConfig(ActivationFnID.TanH),
                                         CreateRPropNetworkModelConfig(3, 200)
                                         );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the CompositeModel consisting of StackingModel, CrossValModel and one another CompositeModel.
        /// </summary>
        /// <param name="foldDataRatio">Used for inner CrossValModel model. Specifies the ratio of training samples constituting validation fold.</param>
        public static CompositeModelConfig CreateCompositeModelConfig(double foldDataRatio)
        {
            //Model configuration
            CompositeModelConfig modelCfg =
                new CompositeModelConfig(CreateStackingModelConfig(foldDataRatio),
                                         CreateCrossValModelConfig(foldDataRatio),
                                         CreateSmallCompositeModelConfig()
                                         );
            return modelCfg;
        }


    }//ExampleConfigs
} //Namespace

