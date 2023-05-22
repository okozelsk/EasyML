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
    /// Regression tasks (forecasting) in TimePoint feeding regime.
    /// 
    /// 
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   TSLA.csv
    ///   MackeyGlass.csv
    /// See txt files related to csv files for more info.
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class ResCompTPRegressionTasks
    {

        //Methods
        private static void ExecuteResCompTSLAExample()
        {
            string taskName = "Tesla Share Prices Forecast";
            EasyML.Oper.Log.Write($"Example {taskName} started.");
            //Input feature names
            List<string> inputFeatureNames = new List<string>() { "Open", "High", "Low", "Adj Close", "Volume" };
            //Output feature names
            List<string> outputFeatureNames = new List<string>() { "High", "Low", "Adj Close" };
            //Full data
            EasyML.Oper.Report("./Data/TSLA.txt");
            SampleDataset fullData =
                EasyML.Oper.LoadSampleData("./Data/TSLA.csv", //Csv data file name
                                           inputFeatureNames, //Identification of features we want to use as the input variables
                                           outputFeatureNames, //Identification of output features we want to forecast
                                           out double[] remainingInputVector, //Last time point input variables where the corresponding output features are not known
                                           true //Verbose
                                           );
            //Split full data to Training and Testing data
            fullData.Split(10, //Our test data will be, let's say, the last 10 timepoints
                           out SampleDataset trainingData,
                           out SampleDataset testingData
                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.TimePoint, //Feeding regime
                                                             trainingData.FirstInputVectorLength, //5 variables "High", "Low", "Adj Close" and "Volume" at time point (1:1 to flat input vector length)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //In case of time point feeding variables schema does not matter (both schemas are valid)
                                                             0.05d, //Connections density of 1 input variable to hidden neurons
                                                             0,//ReservoirInputConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                             2d //ReservoirInputConfig.DefaultMaxStrength //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(500, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   ReservoirHiddenLayerConfig.DefaultRetainment //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.Activations
                                                   );
            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Regression,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      MLPModelConfigs.CreateRPropOutputOnlyNetworkCrossValModelConfig(0.1d, 2, 2000) //Model to be used
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            ResComp resComp =
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
            //And now, the following remaining prediction
            EasyML.Oper.Log.Write($"And now, the following remaining prediction for 2023-05-10...");
            EasyML.Oper.Log.Write($"We know that right prices on 2023-05-10 were High=174.43, Low=166.68 and Adj Close=168.54.");
            EasyML.Oper.Log.Write($"So what prices predicts our trained Reservoir Computer:");
            double[] outputVector = resComp.Compute(remainingInputVector,
                                                    out List<Tuple<string, TaskOutputDetailBase>> detailedOutputs
                                                    );
            EasyML.Oper.Report(detailedOutputs);

            return;
        }

        private static void ExecuteResCompMackeyGlassExample()
        {
            string taskName = "Mackey Glass Forecast";
            EasyML.Oper.Log.Write($"Example {taskName} started.");
            //Input feature names
            List<string> inputFeatureNames = new List<string>() { "Value" };
            //Output feature names
            List<string> outputFeatureNames = new List<string>() { "Value" };
            //Full data
            EasyML.Oper.Report("./Data/MackeyGlass.txt");
            SampleDataset fullData =
                EasyML.Oper.LoadSampleData("./Data/MackeyGlass.csv", //Csv data file name
                                           inputFeatureNames, //Identification of features we want to use as the input variables
                                           outputFeatureNames, //Identification of output features we want to forecast
                                           out double[] remainingInputVector, //Last time point input variables where the corresponding output features are not known
                                           true //Verbose
                                           );
            //Split full data to Training and Testing data
            fullData.Split(5, //Our test data will be, let's say, the last 5 timepoints
                           out SampleDataset trainingData,
                           out SampleDataset testingData
                           );
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.TimePoint, //Feeding regime
                                                             trainingData.FirstInputVectorLength, //1 variable "Value" (1:1 to flat input vector length)
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //In case of time point feeding variables schema does not matter (both schemas are valid)
                                                             ReservoirInputConfig.DefaultDensity, //Connections density of 1 input variable to hidden neurons
                                                             ReservoirInputConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                             1d //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig(100, //Number of hidden neurons
                                                                   ReservoirHiddenLayerConfig.DefaultDensity, //Density of hidden to hidden connection
                                                                   ReservoirHiddenLayerConfig.DefaultMaxDelay, //Synaptic maximum delay
                                                                   ReservoirHiddenLayerConfig.DefaultRetainment //Retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Tasks config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.Activations);

            ResCompTaskConfig taskConfig =
                new ResCompTaskConfig(taskName,
                                      OutputTaskType.Regression,
                                      inputSections,
                                      new FeaturesConfig(outputFeatureNames),
                                      MLPModelConfigs.CreateRPropOutputOnlyNetworkModelConfig(3, 10000)
                                      );
            //Reservoir computer config
            ResCompConfig resCompCfg =
                new ResCompConfig(reservoirCfg, taskConfig);
            ////////////////////////////////////////////////////////////////////////////
            //Build and testing
            //Build
            ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat //Stat data of the reservoir
                                  );
            //Store RC at this point for later use
            ResComp savedResComp = resComp.DeepClone();
            //Classical testing
            List<ModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                 testingData, //Testing data
                                 out ResultDataset resultDataset //Original testing samples together with computed data
                                 );

            //Show computed and ideal values
            EasyML.Oper.Log.Write($"Here are computed MackeyGlass values using continuous input from testing samples...");
            for (int i = 0; i < resultDataset.ComputedVectorCollection.Count; i++)
            {
                //Report ideal value from testing data
                EasyML.Oper.Log.Write($"Sample {i + 1}");
                EasyML.Oper.Log.Write($"Ideal:");
                EasyML.Oper.Report(resComp.GetOutputDetails(resultDataset.IdealVectorCollection[i]));
                //And report our computed value
                EasyML.Oper.Log.Write($"Computed:");
                EasyML.Oper.Report(resComp.GetOutputDetails(resultDataset.ComputedVectorCollection[i]));
            }
            EasyML.Oper.Log.Write(string.Empty);


            EasyML.Oper.Log.Write($"And now let's try to compute testing samples without the feed of samples.");
            EasyML.Oper.Log.Write($"Instead of samples, we use directly RC's computed output as the next input (aka feedback).");
            //The first input is the last output vector from training samples
            double[] inputVector = trainingData.SampleCollection[trainingData.Count - 1].OutputVector;
            for(int i = 0; i < testingData.Count; i++)
            {
                //Compute next MackeyGlass value using previously saved ResComp instance
                double[] outputVector = savedResComp.Compute(inputVector, out List<Tuple<string, TaskOutputDetailBase>> outputDetails);
                //Report ideal value from testing data
                EasyML.Oper.Log.Write($"Sample {i+1}");
                EasyML.Oper.Log.Write($"Ideal:");
                EasyML.Oper.Report(savedResComp.GetOutputDetails(testingData.SampleCollection[i].OutputVector));
                //And report our computed value
                EasyML.Oper.Log.Write($"Computed:");
                EasyML.Oper.Report(outputDetails);
                //Set our computed output as the next RC input
                inputVector = outputVector;
            }

            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            //ExecuteResCompTSLAExample();
            ExecuteResCompMackeyGlassExample();
            return;
        }

    }//ResCompTPRegressionTasks

}//Namespace
