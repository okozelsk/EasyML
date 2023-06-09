﻿using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using EasyMLCore.TimeSeries;
using EasyMLEduApp.Examples.MLP;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasyMLEduApp.Examples.ReservoirComputing
{
    /// <summary>
    /// Example code performs deep tests of Reservoir Computer on referential datasets.
    /// A deep test consists of testing the RC configuration on X times permuted training
    /// and testing data. The aggregated results of the tests performed in this way are more
    /// objective than the results on a single version of training and testing data set.
    /// All deep tests can take several hours.
    /// 
    /// Data is organized as time series input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   LibrasMovement_train.csv and LibrasMovement_test.csv
    ///   ProximalPhalanxOutlineAgeGroup_train.csv and ProximalPhalanxOutlineAgeGroup_test.csv
    ///   LargeKitchenAppliances_train.csv and LargeKitchenAppliances_test.csv
    ///   CricketX_train.csv and CricketX_test.csv
    ///   Worms_train.csv and Worms_test.csv
    ///   BeetleFly_train.csv and BeetleFly_test.csv
    ///   Earthquakes_train.csv and Earthquakes_test.csv
    /// </summary>
    public static class ResCompPFDeepTests
    {
        /// <summary>
        /// Specifies number of deep test rounds.
        /// </summary>
        public const int DeepTestRounds = 30;
        
        //Methods
        private static MLPModelErrStat ExecuteResCompLibrasMovementDeepTest()
        {
            string taskName = "Libras Movement";
            //Output class labels of classification task
            List<string> outputFeatureNames = new List<string>()
            {
                "curved swing",
                "horizontal swing",
                "vertical swing",
                "anti-clockwise arc",
                "clockwise arc",
                "circle",
                "horizontal straight-line",
                "vertical straight-line",
                "horizontal zigzag",
                "vertical zigzag",
                "horizontal wavy",
                "vertical wavy",
                "face-up curve",
                "face-down curve",
                "tremble"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/LibrasMovement.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/LibrasMovement_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count ////Number of output features (classes)
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/LibrasMovement_test.csv",//Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count ////Number of output features (classes)
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             2, //Two variables (coordinates X and Y)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //Coordinates are two noodles 45xX and 45xY in a flat input pattern
                                                             0.2d, //ReservoirInputConfig.DefaultDensity, //Connections density of 1 input variable to hidden neurons
                                                             4, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(150, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   0, //ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.21d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Task config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces);

            NetworkModelConfig networkCfg =
                new NetworkModelConfig(3, //Maximum number of training attempts
                                       100, //Maximum number of epochs within a training attempt
                                       new RPropConfig() //Weights updater
                                       );

            CrossValModelConfig modelCfg =
                new CrossValModelConfig(networkCfg, //For every validation fold will be trained a cluster member network having this configuration
                                        15d / 180d //Validation fold data ratio
                                        );

            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      modelCfg //Model to be used
                                      );

            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Deep test
            return EasyML.Oper.DeepTest(resCompCfg, trainingData, testingData, DeepTestRounds);
        }

        private static MLPModelErrStat ExecuteResCompProximalPhalanxOutlineAgeGroupDeepTest()
        {
            string taskName = "Age Group";
            //Output class labels of Proximal Phalanx Outline Age Group classification task
            List<string> outputFeatureNames = new List<string>()
            {
                "years 0-6",
                "years 7-12",
                "years 13-19"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/ProximalPhalanxOutlineAgeGroup.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/ProximalPhalanxOutlineAgeGroup_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.First,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output features (classes)
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/ProximalPhalanxOutlineAgeGroup_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.First,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output features (classes)
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             1, //One variable (outline)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             (2 * 80 * 1), //ReservoirInputConfig.DefaultDensity, //Connections density of 1 input variable to hidden neurons
                                                             (trainingData.FirstInputVectorLength - 1), //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig((2 * 80 * 1), //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.05d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Task config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces);

            NetworkModelConfig networkCfg =
                new NetworkModelConfig(5, //Maximum number of training attempts
                                       500, //Maximum number of epochs within a training attempt
                                       new RPropConfig() //Weights updater
                                       );
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(networkCfg, //For every validation fold will be trained a cluster member network having this configuration
                                        0.5d //Validation fold data ratio
                                        );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      modelCfg
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Deep test
            return EasyML.Oper.DeepTest(resCompCfg, trainingData, testingData, DeepTestRounds);
        }

        private static MLPModelErrStat ExecuteResCompLargeKitchenAppliancesDeepTest()
        {
            //Our task name
            string taskName = "Kitchen Appliance";
            //Output class labels
            List<string> outputFeatureNames = new List<string>()
            {
                "Washing Machine",
                "Tumble Dryer",
                "Dishwasher"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/LargeKitchenAppliances.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/LargeKitchenAppliances_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output classes
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/LargeKitchenAppliances_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output classes
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             1, //One variable (electricity consumption)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             ReservoirInputConfig.DefaultDensity, //Connections density of 1 input variable to hidden neurons
                                                             ReservoirInputConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(360, //Number of hidden neurons
                                                                   5d, //ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   1,//ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.25d //ReservoirHiddenLayerConfig.DefaultRetainment //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Task config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            NetworkModelConfig networkCfg =
                new NetworkModelConfig(3, //Maximum number of training attempts
                                       1500, //Maximum number of epochs within a training attempt
                                       new RPropConfig() //Weights updater
                                       );
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(networkCfg, //For every validation fold will be trained a cluster member network having this configuration
                                        1d/3d //Validation fold data ratio
                                        );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      modelCfg //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Deep test
            return EasyML.Oper.DeepTest(resCompCfg, trainingData, testingData, DeepTestRounds);
        }

        private static MLPModelErrStat ExecuteResCompCricketXDeepTest()
        {
            string taskName = "CricketX";
            //Output class labels of classification task
            List<string> outputFeatureNames = new List<string>()
            {
                "Cancel Call",
                "Dead Ball",
                "Four",
                "Last Hour",
                "Leg Bye",
                "No Ball",
                "One Short",
                "Out",
                "Penalty Runs",
                "Six",
                "TV Replay",
                "Wide"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/CricketX.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/CricketX_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output classes
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/CricketX_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output classes
                                           );
            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             1, //One variable (Acceleration on X axis)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             300d, //Connections density of 1 input variable to hidden neurons
                                                             29, //Synaptic maximum delay
                                                             2d //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(900, //Number of hidden neurons
                                                                   5d, //Density of hidden to hidden connection
                                                                   1,////Synaptic maximum delay
                                                                   0.8d //Very high retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Task config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            NetworkModelConfig networkCfg =
                new NetworkModelConfig(5, //Maximum number of training attempts
                                       200, //Maximum number of epochs within a training attempt
                                       new RPropConfig() //Weights updater
                                       );
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(networkCfg, //For every validation fold will be trained a cluster member network having this configuration
                                        0.1d //Validation fold data ratio
                                        );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      modelCfg //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Deep test
            return EasyML.Oper.DeepTest(resCompCfg, trainingData, testingData, DeepTestRounds);
        }

        private static MLPModelErrStat ExecuteResCompWormsDeepTest()
        {
            string taskName = "Worms";
            //Output class labels of classification task
            List<string> outputFeatureNames = new List<string>()
            {
                "Wild",
                "Mutant-goa-1",
                "Mutant-unc-1",
                "Mutant-unc-38",
                "Mutant-unc-63"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/Worms.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/Worms_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output classes
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/Worms_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output classes
                                           );
            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             1, //One variable (eigenworm1D)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             0.2d, //Connections density of 1 input variable to hidden neurons
                                                             ReservoirInputConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(180, //Number of hidden neurons
                                                                   0.5d, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.5d //retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Task config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            NetworkModelConfig networkCfg =
                new NetworkModelConfig(2, //Maximum number of training attempts
                                       500, //Maximum number of epochs within a training attempt
                                       new RPropConfig() //Weights updater
                                       );
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(networkCfg, //For every validation fold will be trained a cluster member network having this configuration
                                        0.05d //Validation fold data ratio
                                        );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      modelCfg //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Deep test
            return EasyML.Oper.DeepTest(resCompCfg, trainingData, testingData, DeepTestRounds);
        }

        private static MLPModelErrStat ExecuteResCompBeetleFlyDeepTest()
        {
            string taskName = "Beetle (1) or Fly (0)";
            //Output feature names of binary decision task
            List<string> outputFeatureNames = new List<string>()
            {
                "BeetleFlySwitch"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/BeetleFly.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/BeetleFly_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           outputFeatureNames.Count //Number of output features
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/BeetleFly_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           outputFeatureNames.Count //Number of output features
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             1, //One variable (Distance from centre)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //In case of 1 variable is valid any var schema
                                                             ReservoirInputConfig.DefaultDensity, //Connections density of 1 input variable to hidden neurons
                                                             4,//Synaptic maximum delay
                                                             1d //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(60, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.625d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Task config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces);

            NetworkModelConfig networkCfg =
                new NetworkModelConfig(5, //Maximum number of training attempts
                                       200, //Maximum number of epochs within a training attempt
                                       new AdamConfig(), //Weights updater
                                       new HiddenLayersConfig(new HiddenLayerConfig(30, ActivationFnID.ReLU, new DropoutConfig(0.5d, DropoutMode.Bernoulli)))
                                       );
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(networkCfg, //For every validation fold will be trained a cluster member network having this configuration
                                        0.2d //Validation fold data ratio
                                        );

            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Binary,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      modelCfg //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Deep test
            return EasyML.Oper.DeepTest(resCompCfg, trainingData, testingData, DeepTestRounds);
        }

        private static MLPModelErrStat ExecuteResCompEarthquakesDeepTest()
        {
            string taskName = "Earthquakes";
            //Output feature names of binary decision task
            List<string> outputFeatureNames = new List<string>()
            {
                "EventIndicator"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/Earthquakes.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/Earthquakes_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           outputFeatureNames.Count //Number of output features
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/Earthquakes_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           outputFeatureNames.Count //Number of output features
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             1, //One variable (sensor signal)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //In case of 1 variable is valid any var schema
                                                             32d, //Connections density of 1 input variable to hidden neurons
                                                             15, //Synaptic maximum delay
                                                             1.8 ////Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(100, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.7d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );

            NetworkModelConfig networkCfg =
                new NetworkModelConfig(5, //Maximum number of training attempts
                                       500, //Maximum number of epochs within a training attempt
                                       new RPropConfig(), //Weights updater
                                       new HiddenLayersConfig(new HiddenLayerConfig(10, ActivationFnID.LeakyReLU),
                                                              new HiddenLayerConfig(10, ActivationFnID.LeakyReLU)
                                                              )
                                       );
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(networkCfg, //For every validation fold will be trained a cluster member network having this configuration
                                        1d/3d //Validation fold data ratio
                                        );

            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Binary,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      modelCfg
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Deep test
            return EasyML.Oper.DeepTest(resCompCfg, trainingData, testingData, DeepTestRounds);
        }


        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Tuple<string, MLPModelErrStat>> modelErrStats = new List<Tuple<string, MLPModelErrStat>>();
            modelErrStats.Add(new Tuple<string, MLPModelErrStat>("LibrasMovementDeepTest", ExecuteResCompLibrasMovementDeepTest()));
            modelErrStats.Add(new Tuple<string, MLPModelErrStat>("ProximalPhalanxOutlineAgeGroupDeepTest", ExecuteResCompProximalPhalanxOutlineAgeGroupDeepTest()));
            modelErrStats.Add(new Tuple<string, MLPModelErrStat>("LargeKitchenAppliancesDeepTest", ExecuteResCompLargeKitchenAppliancesDeepTest()));
            modelErrStats.Add(new Tuple<string, MLPModelErrStat>("CricketXDeepTest", ExecuteResCompCricketXDeepTest()));
            modelErrStats.Add(new Tuple<string, MLPModelErrStat>("WormsDeepTest", ExecuteResCompWormsDeepTest()));
            modelErrStats.Add(new Tuple<string, MLPModelErrStat>("BeetleFlyDeepTest", ExecuteResCompBeetleFlyDeepTest()));
            modelErrStats.Add(new Tuple<string, MLPModelErrStat>("EarthquakesDeepTest", ExecuteResCompEarthquakesDeepTest()));
            stopwatch.Stop();
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write($"Deep tests final results");
            EasyML.Oper.Log.Write($"------------------------");
            foreach (Tuple<string, MLPModelErrStat> statEntry in modelErrStats)
            {
                EasyML.Oper.Log.Write($"Aggregated result of {statEntry.Item1}:");
                EasyML.Oper.Report(statEntry.Item2, false, 4);
            }
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write($"Finished in {stopwatch.Elapsed.Hours.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            return;
        }

    }//ResCompPFDeepTests

}//Namespace
