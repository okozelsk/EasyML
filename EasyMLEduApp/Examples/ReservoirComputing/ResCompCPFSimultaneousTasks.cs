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
    /// Example code demonstrates use of the ResComp component for simultaneous
    /// Categorical, Binary and Regression tasks on the same input data.
    /// 
    /// Data is organized as fixed length time series input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   MultiTaskPPOutlineAgeGroup_train.csv and MultiTaskPPOutlineAgeGroup_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class ResCompCPFSimultaneousTasks
    {

        //Methods
        private static void ExecuteResCompSimultaneousTasksExample()
        {
            //Output class labels of classification task
            List<string> outputClassLabels = new List<string>()
            {
                "years 0-6",
                "years 7-12",
                "years 13-19"
            };
            //Output feature names of regression task
            List<string> outputRegrFeatureNames = new List<string>()
            {
                "SQRT(SUM(A..F))"
            };
            //Output feature names of binary task
            List<string> outputBinFeatureNames = new List<string>()
            {
                "Is (A) GT (H)"
            };
            //Training and Testing data
            EasyML.Oper.Report("./Data/MultiTaskPPOutlineAgeGroup.txt");
            int totalNumOfOutputFeatures = outputClassLabels.Count + outputRegrFeatureNames.Count + outputBinFeatureNames.Count;
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./Data/MultiTaskPPOutlineAgeGroup_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           totalNumOfOutputFeatures //Total number of output features
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/MultiTaskPPOutlineAgeGroup_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.Separately,
                                           totalNumOfOutputFeatures //Total number of output features
                                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             1, //One variable (outline)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //It does not matter in case of 1 variable
                                                             60d, //Connections density of 1 input variable to hidden neurons
                                                             2, //Synaptic maximum delay
                                                             ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(120, //Number of hidden neurons
                                                                   12d, //Density of hidden to hidden connection
                                                                   1, //Synaptic maximum delay
                                                                   ReservoirHiddenLayerConfig.DefaultRetainment //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Classification
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig catInputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces,
                                                   Reservoir.OutSection.SquaredActivations,
                                                   Reservoir.OutSection.ResInputs
                                                   );
            ResCompTaskConfig catTaskConfig =
                new ResCompTaskConfig("Age Group categorical task",
                                      OutputTaskType.Categorical,
                                      catInputSections,
                                      new FeaturesConfig(outputClassLabels),
                                      MLPModelConfigs.CreateRPropCrossValModelConfig(0.1d, 20, 200) //Model to be used
                                      );
            //Regression
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig regrInputSections =
                new ResCompTaskInputSectionsConfig(//Reservoir.OutSection.Activations//,
                                                   Reservoir.OutSection.SquaredActivations,
                                                   Reservoir.OutSection.ResInputs
                                                   );
            ResCompTaskConfig regrTaskConfig =
                new ResCompTaskConfig("Value regression task",
                                      OutputTaskType.Regression,
                                      regrInputSections,
                                      new FeaturesConfig(outputRegrFeatureNames),
                                      MLPModelConfigs.CreateNetworkModelConfig(ActivationFnID.TanH, 20, 2, 60)
                                      );
            //Binary
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig binInputSections =
                new ResCompTaskInputSectionsConfig(//Reservoir.OutSection.SpikesFadingTraces,
                                                   //Reservoir.OutSection.SquaredActivations,
                                                   Reservoir.OutSection.ResInputs
                                                   );
            ResCompTaskConfig binTaskConfig =
                new ResCompTaskConfig("Binary decision task",
                                      OutputTaskType.Binary,
                                      binInputSections,
                                      new FeaturesConfig(outputBinFeatureNames),
                                      MLPModelConfigs.CreateRPropCrossValModelConfig(0.1d, 10, 100) //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg,
                                  catTaskConfig,
                                  regrTaskConfig,
                                  binTaskConfig
                                  );
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat, //Stat data of the reservoir
                                  true, //Verbose
                                  true //Max detail
                                  );
            //Test
            List<ModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                   testingData, //Testing data
                                   out ResultDataset resultDataset, //Original testing samples together with computed data
                                   true, //Verbose
                                   true //Max detail
                                   );
            //Diagnostic test (an alternative to Test)
            List<ModelDiagnosticData> tasksDiagData =
                EasyML.Oper.DiagnosticTest(resComp, //Our built reservoir computer
                                           testingData, //Testing data
                                           true, //Verbose
                                           false //Max detail
                                           );
            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            ExecuteResCompSimultaneousTasksExample();
            return;
        }

    }//ResCompCPFSimultaneousTasks

}//Namespace
