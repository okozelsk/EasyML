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
    /// Regression tasks (forecasting).
    /// 
    /// Data is organized as fixed length time series input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   TTOO_patterns_train.csv and TTOO_patterns_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class ResCompCPFRegressionTasks
    {

        //Methods
        private static void ExecuteResCompTTOOExample()
        {
            string taskName = "TTOO Biosystems Share Prices";
            //Output feature names of TTOO Biosystems Share Prices regression task
            List<string> outputFeatureNames = new List<string>() { "High", "Low", "Adj Close" };
            //Training and Testing data
            EasyML.Oper.Report("./Data/TTOO_patterns.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/TTOO_patterns_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           outputFeatureNames.Count //Number of output features
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/TTOO_patterns_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           outputFeatureNames.Count //Number of output features
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             3, //Three variables "High", "Low", "Adj Close"
                                                             TimeSeriesPattern.FlatVarSchema.Groupped, //A row of 15 x "High", "Low", "Adj Close" triplets
                                                             20d, //Connections density of 1 input variable to hidden neurons
                                                             ReservoirInputConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(60, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   ReservoirHiddenLayerConfig.DefaultRetainment //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.Activations,
                                                   Reservoir.OutSection.SquaredActivations
                                                   );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Regression,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      MLPModelConfigs.CreateRPropCrossValModelConfig(0.1d, 2, 500) //Model to be used
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
            ExecuteResCompTTOOExample();
            return;
        }

    }//ResCompCPFRegressionTasks

}//Namespace
