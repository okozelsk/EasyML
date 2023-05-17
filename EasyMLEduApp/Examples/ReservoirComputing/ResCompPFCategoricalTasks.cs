using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using EasyMLCore.TimeSeries;
using EasyMLEduApp.Examples.MLP;
using System;
using System.Collections.Generic;

namespace EasyMLEduApp.Examples.ReservoirComputing
{
    /// <summary>
    /// Example code demonstrates use of the ResComp component for
    /// Categorical tasks (classification).
    /// 
    /// Data is organized as fixed length time series input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   LibrasMovement_train.csv and LibrasMovement_test.csv
    ///   ProximalPhalanxOutlineAgeGroup_train.csv and ProximalPhalanxOutlineAgeGroup_test.csv
    ///   LargeKitchenAppliances_train.csv and LargeKitchenAppliances_test.csv
    ///   CricketX_train.csv and CricketX_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class ResCompPFCategoricalTasks
    {

        //Methods
        private static void ExecuteResCompLibrasMovementExample()
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
                new ReservoirConfig(new ReservoirInputConfig(trainingData.InputVectorLength, //90 Flat input pattern length
                                                             2, //Two variables (coordinates X and Y)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //Coordinates are two noodles 45xX and 45xY in a flat input pattern
                                                             Reservoir.InputFeeding.Pattern, //Feeding regime
                                                             0.2d, //Connections density of 1 input variable to hidden neurons
                                                             5, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(150, //Number of hidden neurons
                                                                   2, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.25d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      MLPModelConfigs.CreateRPropCrossValModelConfig(0.05d, 5, 200) //Model to be used
                                      //MLPModelConfigs.CreateRPropOutputOnlyNetworkCrossValModelConfig(0.05d, 20, 500) //Model to be used
                                      //MLPModelConfigs.CreateOutputOnlyNetworkCrossValModelConfig(0.05)
                                      //MLPModelConfigs.CreateStackingModelConfig(0.05d)
                                      //MLPModelConfigs.CreateRPropStackingModelConfig(0.05d)
                                      //MLPModelConfigs.CreateNetworkModelConfig()
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            EasyMLCore.TimeSeries.ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat //Stat data of the reservoir
                                  );
            //Testing
            List<ModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                   testingData, //Testing data
                                   out ResultDataset resultDataset //Original testing samples together with computed data
                                   );
            return;
        }

        private static void ExecuteResCompProximalPhalanxOutlineAgeGroupExample()
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
                new ReservoirConfig(new ReservoirInputConfig(trainingData.InputVectorLength, //80 Flat input pattern length
                                                             1, //One variable (outline)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             Reservoir.InputFeeding.Pattern, //Feeding regime
                                                             ReservoirInputConfig.DefaultDensity, //Connections density of 1 input variable to hidden neurons
                                                             2, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(160, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   ReservoirHiddenLayerConfig.DefaultRetainment //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      //MLPModelConfigs.CreateCrossValModelConfig(0.1d) //Model to be used
                                      MLPModelConfigs.CreateRPropCrossValModelConfig(0.2d, 5, 200) //Model to be used
                                      //MLPModelConfigs.CreateRPropOutputOnlyNetworkCrossValModelConfig(0.1d, 5, 1000) //Model to be used
                                      //MLPModelConfigs.CreateOutputOnlyNetworkCrossValModelConfig(0.1d)
                                      //MLPModelConfigs.CreateRPropStackingModelConfig(0.1d)
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            EasyMLCore.TimeSeries.ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat //Stat data of the reservoir
                                  );
            //Testing
            List<ModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                   testingData, //Testing data
                                   out ResultDataset resultDataset //Original testing samples together with computed data
                                   );
            return;
        }

        private static void ExecuteResCompLargeKitchenAppliancesExample()
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
                new ReservoirConfig(new ReservoirInputConfig(trainingData.InputVectorLength, //720 Flat input pattern length
                                                             1, //One variable (electricity consumption)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             Reservoir.InputFeeding.Pattern, //Feeding regime
                                                             ReservoirInputConfig.DefaultDensity, //Connections density of 1 input variable to hidden neurons
                                                             ReservoirInputConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(360, //Number of hidden neurons
                                                                   5d, //Density of hidden to hidden connection
                                                                   1, //Synaptic maximum delay
                                                                   0.2d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      //MLPModelConfigs.CreateRPropCrossValModelConfig(0.333333d, 3, 1500) //Model to be used
                                      MLPModelConfigs.CreateRPropOutputOnlyNetworkCrossValModelConfig(0.33333d, 3, 1500) //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            EasyMLCore.TimeSeries.ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat //Stat data of the reservoir
                                  );
            //Testing
            List<ModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                   testingData, //Testing data
                                   out ResultDataset resultDataset //Original testing samples together with computed data
                                   );
            return;
        }

        private static void ExecuteResCompCricketXExample()
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
                new ReservoirConfig(new ReservoirInputConfig(trainingData.InputVectorLength, //300 Flat input pattern length
                                                             1, //One variable (Acceleration on X axis)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             Reservoir.InputFeeding.Pattern, //Feeding regime
                                                             300d, //Connections density of 1 input variable to hidden neurons
                                                             29, //Synaptic maximum delay
                                                             1.5d //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(900, //Number of hidden neurons
                                                                   5d, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.835d //Very high retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      //MLPModelConfigs.CreateRPropCrossValModelConfig(0.1d, 20, 100) //Model to be used
                                      MLPModelConfigs.CreateRPropOutputOnlyNetworkCrossValModelConfig(0.1d, 5, 200) //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            EasyMLCore.TimeSeries.ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat //Stat data of the reservoir
                                  );
            //Testing
            List<ModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                   testingData, //Testing data
                                   out ResultDataset resultDataset //Original testing samples together with computed data
                                   );
            return;
        }

        private static void ExecuteResCompWormsExample()
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
                new ReservoirConfig(new ReservoirInputConfig(trainingData.InputVectorLength, //900 Flat input pattern length
                                                             1, //One variable (eigenworm1D)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             Reservoir.InputFeeding.Pattern, //Feeding regime
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
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      //MLPModelConfigs.CreateRPropCrossValModelConfig(0.1d, 20, 100) //Model to be used
                                      MLPModelConfigs.CreateRPropOutputOnlyNetworkCrossValModelConfig(0.05d, 2, 500) //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            EasyMLCore.TimeSeries.ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat //Stat data of the reservoir
                                  );
            //Testing
            List<ModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                   testingData, //Testing data
                                   out ResultDataset resultDataset //Original testing samples together with computed data
                                   );
            return;
        }


        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            ExecuteResCompLibrasMovementExample();
            ExecuteResCompProximalPhalanxOutlineAgeGroupExample();
            ExecuteResCompLargeKitchenAppliancesExample();
            ExecuteResCompCricketXExample();
            ExecuteResCompWormsExample();
            return;
        }

    }//ResCompPFCategoricalTasks

}//Namespace
