using EasyMLCore.MLP;
using EasyMLCore;
using System;

namespace EasyMLEduApp.Examples.MLP
{
    /// <summary>
    /// Shows how to create configurations of the NetworkModel, CrossValModel,
    /// StackingModel, BHSModel, RVFLModel and CompositeModel.
    /// These configurations are used by other examples.
    /// </summary>
    public static class MLPModelConfigs
    {
        /*
         * Default constants and weights regularization options used in examples.
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
        private const double NetStopAttemptPatiency = 0.2d;

        //Static members
        //Dropout configuration
        //No input dropout
        private static readonly DropoutConfig InputDropoutCfg = new DropoutConfig(0d, DropoutMode.None);
        //50% Bernouli dropout on networks hidden layers
        private static readonly DropoutConfig HiddenDropoutCfg = new DropoutConfig(0.5d, DropoutMode.Bernoulli);
        //L1 regularization configuration
        private static readonly RegL1Config HiddenRegL1Cfg = new RegL1Config(0d, false);
        private static readonly RegL1Config OutputRegL1Cfg = new RegL1Config(0d, false);
        //L2 regularization configuration
        private static readonly RegL2Config HiddenRegL2Cfg = new RegL2Config(0d, false);
        private static readonly RegL2Config OutputRegL2Cfg = new RegL2Config(0d, false);
        //Norm constraints configuration
        //No norm constraint on networks hidden layers
        private static readonly NormConsConfig HiddenNormConsCfg = new NormConsConfig(0d, 0d, false);
        //No norm constraint on networks output layers
        private static readonly NormConsConfig OutputNormConsCfg = new NormConsConfig(0d, 0d, false);
        //Learning throttle valve
        //No throttling
        private static readonly LearningThrottleValveConfig LTVCfg = null;

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
                                       new InputOptionsConfig(InputDropoutCfg), //Input dropout
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       LTVCfg, //Learning throttle valve
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
                                       new InputOptionsConfig(InputDropoutCfg), //Input dropout
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       LTVCfg, //Learning throttle valve
                                       batchSize, //Batch size
                                       NetworkModelConfig.DefaultGradClipNorm,
                                       NetworkModelConfig.DefaultGradClipVal,
                                       NetworkModelConfig.DefaultClassBalancedLoss,
                                       NetStopAttemptPatiency
                                       );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the model of single MLP network having hidden layers and RProp optimizer (leads to BGD).
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
                                       null, //No input options (no input dropout because the RPRop)
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       LTVCfg, //Learning throttle valve
                                       NetworkModelConfig.AutoBatchSizeNumCode, //Batch size
                                       NetworkModelConfig.DefaultGradClipNorm,
                                       NetworkModelConfig.DefaultGradClipVal,
                                       NetworkModelConfig.DefaultClassBalancedLoss,
                                       NetStopAttemptPatiency
                                       );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the model of single MLP network having no hidden layers and RProp optimizer (leads to BGD).
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
                                       null, //No input options (no input dropout because the RPRop)
                                       new OutputOptionsConfig(OutputRegL1Cfg, OutputRegL2Cfg, OutputNormConsCfg),
                                       LTVCfg, //Learning throttle valve
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
        /// Creates configuration of a CrossValModel of networks trained by RProp optimizer (leads toBGD).
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
                new CrossValModelConfig(CreateRPropNetworkModelConfig(trainAttempts, attemptEpochs, ActivationFnID.LeakyReLU, 50, 1), //For every validation fold will be trained a member network having this configuration
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
                new NetworkStackConfig(CreateNetworkModelConfig(ActivationFnID.ReLU, 50, 2),
                                       CreateNetworkModelConfig(ActivationFnID.ReLU, 50, 1),
                                       CreateNetworkModelConfig(ActivationFnID.LeakyReLU, 26, 1),
                                       CreateOutputOnlyNetworkModelConfig()
                                       );
            //Model configuration
            StackingModelConfig cfg =
                new StackingModelConfig(stackCfg,
                                        CreateOutputOnlyNetworkModelConfig(), //Meta-learner model configuration
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
                new NetworkStackConfig(CreateRPropNetworkModelConfig(3, 500, ActivationFnID.ReLU, 50, 2),
                                       CreateRPropNetworkModelConfig(3, 500, ActivationFnID.ReLU, 50, 1),
                                       CreateRPropNetworkModelConfig(3, 500, ActivationFnID.LeakyReLU, 26, 1),
                                       CreateRPropOutputOnlyNetworkModelConfig(3, 500)
                                       );
            //Model configuration
            StackingModelConfig cfg =
                new StackingModelConfig(stackCfg,
                                        CreateRPropOutputOnlyNetworkModelConfig(3, 500), //Meta-learner model configuration
                                        foldDataRatio, //Specifies the ratio of training samples constituting one hold-out fold
                                        routeInput //Specifies whether to provide original input to meta-learner
                                        );
            return cfg;
        }

        /*
         * BHS model configurations.
        */
        /// <summary>
        /// Creates configuration of a BHS model.
        /// </summary>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner.</param>
        public static BHSModelConfig CreateBHSModelConfig(bool routeInput = HSModelConfig.DefaultRouteInput)
        {
            //Configuration of networks constituing a stack.
            NetworkStackConfig stackCfg =
                new NetworkStackConfig(CreateNetworkModelConfig(ActivationFnID.ReLU, 50, 2),
                                       CreateNetworkModelConfig(ActivationFnID.ReLU, 50, 1),
                                       CreateNetworkModelConfig(ActivationFnID.LeakyReLU, 26, 1),
                                       CreateOutputOnlyNetworkModelConfig()
                                       );
            //HS model configuration
            HSModelConfig hsModelcfg =
                new HSModelConfig(stackCfg,
                                  CreateOutputOnlyNetworkModelConfig(), //Meta-learner model configuration
                                  routeInput //Specifies whether to provide original input to meta-learner.
                                  );
            //BHS model configuration
            BHSModelConfig cfg =
                new BHSModelConfig(hsModelcfg, //HS model configuration
                                   1 //Halfing repetitions
                                   );
            return cfg;
        }

        /// <summary>
        /// Creates configuration of a BHS model with RProp optimizer (leading to BGD).
        /// </summary>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner.</param>
        public static BHSModelConfig CreateRPropBHSModelConfig(bool routeInput = HSModelConfig.DefaultRouteInput)
        {
            //Configuration of networks constituing a stack.
            NetworkStackConfig stackCfg =
                new NetworkStackConfig(CreateRPropNetworkModelConfig(3, 500, ActivationFnID.ReLU, 25, 2),
                                       CreateRPropNetworkModelConfig(3, 500, ActivationFnID.ReLU, 20, 1),
                                       CreateRPropNetworkModelConfig(3, 500, ActivationFnID.LeakyReLU, 15, 1),
                                       CreateRPropOutputOnlyNetworkModelConfig(3, 500)
                                       );
            //HS model configuration
            HSModelConfig hsModelcfg =
                new HSModelConfig(stackCfg,
                                  CreateRPropOutputOnlyNetworkModelConfig(3, 500), //Meta-learner model configuration
                                  routeInput //Specifies whether to provide original input to meta-learner.
                                  );
            //BHS model configuration
            BHSModelConfig cfg =
                new BHSModelConfig(hsModelcfg, //HS model configuration
                                   1 //Halfing repetitions
                                   );
            return cfg;
        }

        /*
         * RVFL model configurations
        */

        /// <summary>
        /// Creates RVFL model configuration having 1 hidden layer.
        /// </summary>
        public static RVFLModelConfig CreateSingleLayerRVFLModelConfig()
        {
            int poolSize = 512;
            //RVFL hidden layers
            RVFLHiddenLayersConfig hlsc =
                new RVFLHiddenLayersConfig(new RVFLHiddenLayerConfig(new RVFLHiddenPoolConfig(poolSize, ActivationFnID.Sigmoid, true),
                                                                     new RVFLHiddenPoolConfig(poolSize, ActivationFnID.HardLim, true)
                                                                     )
                                           );

            //RVFL model
            RVFLModelConfig rvflCfg =
                new RVFLModelConfig(hlsc,
                                    CreateRPropOutputOnlyNetworkCrossValModelConfig(0.1d, 3, 1000), //end-model
                                    RVFLModelConfig.DefaultScaleFactor, //Scale factor of the first layer's weights (first layer has uniform distribution of random weights)
                                    false //Do not provide original input to end-model
                                    );
            return rvflCfg;
        }

        /// <summary>
        /// Creates RVFL model configuration having 3 hidden layers.
        /// </summary>
        public static RVFLModelConfig CreateDeepRVFLModelConfig()
        {
            //Pools to be used within hidden layers
            int poolSize = 512;
            RVFLHiddenPoolConfig pool1Cfg = new RVFLHiddenPoolConfig(poolSize, ActivationFnID.TanH, true);
            RVFLHiddenPoolConfig pool2Cfg = new RVFLHiddenPoolConfig(poolSize, ActivationFnID.LeakyReLU, true);
            RVFLHiddenPoolConfig pool3Cfg = new RVFLHiddenPoolConfig(poolSize, ActivationFnID.SELU, true);
            //RVFL hidden layers
            RVFLHiddenLayersConfig hlsc =
                new RVFLHiddenLayersConfig(new RVFLHiddenLayerConfig(pool1Cfg, pool2Cfg, pool3Cfg), //First layer
                                           new RVFLHiddenLayerConfig(pool1Cfg), //Second layer
                                           new RVFLHiddenLayerConfig(pool2Cfg) //Third layer
                                           );
            //RVFL model
            RVFLModelConfig rvflCfg =
                new RVFLModelConfig(hlsc,
                                    CreateOutputOnlyNetworkModelConfig(), //Simple output only network
                                    RVFLModelConfig.DefaultScaleFactor, // Scale factor of the first layer's weights (first layer has uniform distribution of random weights)
                                    false //Do not provide original input to end-model
                                    );
            return rvflCfg;
        }



        /*
         * Composite model configurations.
        */
        /// <summary>
        /// Creates configuration of the small CompositeModel of three network models and one RVFL model.
        /// </summary>
        /// <returns></returns>
        public static CompositeModelConfig CreateSmallCompositeModelConfig()
        {
            //Model configuration
            CompositeModelConfig modelCfg =
                new CompositeModelConfig(CreateNetworkModelConfig(ActivationFnID.ReLU, 50, 2),
                                         CreateNetworkModelConfig(ActivationFnID.TanH, 50, 2),
                                         CreateNetworkModelConfig(ActivationFnID.ReLU, 44, 1),
                                         CreateDeepRVFLModelConfig()
                                         );
            return modelCfg;
        }

        /// <summary>
        /// Creates configuration of the CompositeModel consisting of BHSModel, CrossValModel and one another CompositeModel.
        /// </summary>
        /// <param name="foldDataRatio">Used for inner CrossVal model. Specifies the ratio of training samples constituting hold-out/validation fold.</param>
        public static CompositeModelConfig CreateCompositeModelConfig(double foldDataRatio)
        {
            //Model configuration
            CompositeModelConfig modelCfg =
                new CompositeModelConfig(CreateBHSModelConfig(),
                                         CreateCrossValModelConfig(foldDataRatio),
                                         CreateSmallCompositeModelConfig()
                                         );
            return modelCfg;
        }


    }//MLPModelConfigs

} //Namespace

