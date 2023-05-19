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
    /// Binary tasks (binary decisions).
    /// 
    /// Data is organized as fixed length time series input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   BeetleFly_train.csv and BeetleFly_test.csv
    ///   Earthquakes_train.csv and Earthquakes_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class ResCompPFBinaryTasks
    {

        //Methods
        private static void ExecuteResCompBeetleFlyExample()
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
                new ReservoirConfig(new ReservoirInputConfig(trainingData.InputVectorLength, //512 Flat input pattern length
                                                             1, //One variable (Distance from centre)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //In case of 1 variable is valid any var schema
                                                             Reservoir.InputFeeding.Pattern, //Feeding regime
                                                             10d, //Connections density of 1 input variable to hidden neurons
                                                             4,//Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(45, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.625d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces);

            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Binary,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      MLPModelConfigs.CreateCrossValModelConfig(0.1) //Model to be used
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

        private static void ExecuteResCompEarthquakesExample()
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
                new ReservoirConfig(new ReservoirInputConfig(trainingData.InputVectorLength, //512 Flat input pattern length
                                                             1, //One variable (sensor signal)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //In case of 1 variable is valid any var schema
                                                             Reservoir.InputFeeding.Pattern, //Feeding regime
                                                             32d, //Connections density of 1 input variable to hidden neurons
                                                             15,//Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
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

            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Binary,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      MLPModelConfigs.CreateRPropCrossValModelConfig(0.3333, 5, 500)
                                      //MLPModelConfigs.CreateCrossValModelConfig(0.3333) //Model to be used
                                      //MLPModelConfigs.CreateNetworkModelConfig() //Model to be used
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
            ExecuteResCompBeetleFlyExample();
            ExecuteResCompEarthquakesExample();
            return;
        }

    }//ResCompPFBinaryTasks

}//Namespace
