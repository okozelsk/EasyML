using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using EasyMLCore.MLP.Model;
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
    /// Data is organized as varying length time series input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   AllGestureWiimoteX_train.csv and AllGestureWiimoteX_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class ResCompVPFCategoricalTasks
    {

        //Methods

        private static void ExecuteResCompAllGestureWiimoteXExample()
        {
            string taskName = "All Gesture WiimoteX";
            //Output class labels of classification task
            List<string> outputFeatureNames = new List<string>()
            {
                "pick-up",
                "shake",
                "one move to the right",
                "one move to the left",
                "one move to up",
                "one move to down",
                "one left circle",
                "one right circle",
                "one move toward the screen",
                "one move away from the screen"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/AllGestureWiimoteX.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/AllGestureWiimoteX_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output features (classes)
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/AllGestureWiimoteX_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output features (classes)
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternVarLength, //Feeding regime
                                                             1, //One variable (X acceleration)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             0.1d, //Connections density of 1 input variable to hidden neurons
                                                             ReservoirInputConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(400, //Number of hidden neurons
                                                                   0.15d, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   0.8d //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces
                                                   );

            NetworkModelConfig taskModelCfg =
                new NetworkModelConfig(1, 10000, new AdamConfig(), null, null);

            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Categorical,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      taskModelCfg
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
            ExecuteResCompAllGestureWiimoteXExample();
            return;
        }

    }//ResCompVPFCategoricalTasks

}//Namespace
