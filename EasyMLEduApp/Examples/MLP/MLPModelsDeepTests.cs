using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using EasyMLCore.TimeSeries;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasyMLDemoApp.Examples.MLP
{
    /// <summary>
    /// Example code performs deep tests of MLP models on referential datasets.
    /// A deep test consists of testing the model configuration on X times permuted training
    /// and testing data. The aggregated results of the tests performed in this way are more
    /// objective than the results on a single training and testing data set.
    /// All deep tests can take several hours.
    /// 
    /// Data is organized as fixed length input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   LibrasMovement_train.csv and LibrasMovement_test.csv
    ///   ProximalPhalanxOutlineAgeGroup_train.csv and ProximalPhalanxOutlineAgeGroup_test.csv
    /// </summary>
    public static class MLPModelsDeepTests
    {
        /// <summary>
        /// Specifies number of deep test rounds.
        /// </summary>
        public const int DeepTestRounds = 30;
        
        //Methods
        private static ModelErrStat ExecuteLibrasMovementDeepTest()
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
            //MLP model configuration
            ////////////////////////////////////////////////////////////////////////////
            IModelConfig modelCfg =
                new CrossValModelConfig(EasyML.Oper.GetDefaultNetworkModelConfig(trainingData),
                                        0.05d
                                        );
            //Deep test
            return EasyML.Oper.DeepTest(modelCfg,
                                        taskName,
                                        OutputTaskType.Categorical,
                                        outputFeatureNames,
                                        trainingData,
                                        testingData,
                                        DeepTestRounds
                                        );
        }

        private static ModelErrStat ExecuteProximalPhalanxOutlineAgeGroupDeepTest()
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
            //MLP model configuration
            ////////////////////////////////////////////////////////////////////////////
            IModelConfig modelCfg =
                new CrossValModelConfig(EasyML.Oper.GetDefaultNetworkModelConfig(trainingData),
                                        0.1d
                                        );
            //Deep test
            return EasyML.Oper.DeepTest(modelCfg,
                                        taskName,
                                        OutputTaskType.Categorical,
                                        outputFeatureNames,
                                        trainingData,
                                        testingData,
                                        DeepTestRounds
                                        );
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Tuple<string, ModelErrStat>> modelErrStats = new List<Tuple<string, ModelErrStat>>();
            modelErrStats.Add(new Tuple<string, ModelErrStat>("LibrasMovementDeepTest", ExecuteLibrasMovementDeepTest()));
            modelErrStats.Add(new Tuple<string, ModelErrStat>("ProximalPhalanxOutlineAgeGroupDeepTest", ExecuteProximalPhalanxOutlineAgeGroupDeepTest()));
            stopwatch.Stop();
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write($"Deep tests final results");
            EasyML.Oper.Log.Write($"------------------------");
            foreach (Tuple<string, ModelErrStat> statEntry in modelErrStats)
            {
                EasyML.Oper.Log.Write($"Aggregated result of {statEntry.Item1}:");
                EasyML.Oper.Report(statEntry.Item2, false, 4);
            }
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write($"Finished in {stopwatch.Elapsed.Hours.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            return;
        }

    }//MLPModelsDeepTests

}//Namespace
