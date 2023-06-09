﻿using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using System;
using System.Collections.Generic;

namespace EasyMLEduApp.Examples.MLP
{
    /// <summary>
    /// Example code demonstrates use of
    /// Network, CrossVal, BHS, Stacking, RVFL and Composite models
    /// to solve Regression tasks (forecasting).
    /// 
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   TTOO_patterns_train.csv and TTOO_patterns_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class MLPModelsRegressionCompetition
    {
        //Methods
        private static void ReportDetailOfFirstNComputations(MLPModelBase model, SampleDataset data, int n)
        {
            for (int i = 0; i < n && i < data.Count; i++)
            {
                EasyML.Oper.Log.Write($"Sample {i + 1}");
                EasyML.Oper.Log.Write($"--------------");
                EasyML.Oper.Log.Write($"    Ideal:");
                TaskOutputDetailBase idealOutputDetail = model.GetOutputDetail(data.SampleCollection[i].OutputVector);
                EasyML.Oper.Log.Write(idealOutputDetail.GetTextInfo(4));
                EasyML.Oper.Log.Write($"    Computed:");
                double[] computed = model.Compute(data.SampleCollection[i].InputVector);
                TaskOutputDetailBase computedOutputDetail = model.GetOutputDetail(computed);
                EasyML.Oper.Log.Write(computedOutputDetail.GetTextInfo(4));
                EasyML.Oper.Log.Write(string.Empty);
            }
            return;
        }

        //Methods
        private static void ExecuteTTOOCompetition()
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
            //Model configurations to be applied one by one on TTOO Biosystems Share Prices data
            List<IModelConfig> modelConfigCollection = new List<IModelConfig>()
            {
                MLPModelConfigs.CreateNetworkModelConfig(ActivationFnID.ELU),
                MLPModelConfigs.CreateSingleLayerRVFLModelConfig(),
                MLPModelConfigs.CreateDeepRVFLModelConfig(),
                MLPModelConfigs.CreateRPropNetworkModelConfig(2, 2000, ActivationFnID.ELU),
                MLPModelConfigs.CreateRPropCrossValModelConfig(0.25d, 2, 2000),
                MLPModelConfigs.CreateBHSModelConfig(),
                MLPModelConfigs.CreateRPropStackingModelConfig(0.25d),
                MLPModelConfigs.CreateSmallCompositeModelConfig()
            };

            //For each defined model configuration
            //build a trained model and then test its performance.
            //Select the best one and report its configuration and testing results again.
            EasyML.Oper.Log.Write($"MODELS COMPETITION STARTED");
            EasyML.Oper.Log.Write($"--------------------------");
            MLPModelBase bestModel = null;
            IModelConfig bestModelConfig = null;
            MLPModelErrStat bestErrStat = null;
            foreach (IModelConfig modelCfg in modelConfigCollection)
            {
                MLPModelBase model =
                    EasyML.Oper.Build(modelCfg, //Model config
                                      taskName, //Out task name
                                      OutputTaskType.Regression, //Our task type
                                      outputFeatureNames, //Output feature names
                                      trainingData, //Sample training data
                                      true, //We want to report progress and results
                                      false //We do not require to report all details
                                      );
                MLPModelErrStat errStat =
                    EasyML.Oper.Test(model, //Our built model
                                       testingData, //Sample testing data
                                       out ResultDataset resultData, //Original testing samples together with computed data
                                       true, //We want to report progress and results
                                       false //We do not require to report all details
                                       );
                if (bestErrStat == null)
                {
                    bestModel = model;
                    bestModelConfig = modelCfg;
                    bestErrStat = errStat;
                }
                else if (bestErrStat.IsBetter(errStat))
                {
                    bestModel = model;
                    bestModelConfig = modelCfg;
                    bestErrStat = errStat;
                }
                EasyML.Oper.Log.Write(string.Empty);
                EasyML.Oper.Log.Write(string.Empty);
            }
            //Report the best model
            EasyML.Oper.Log.Write($"MODELS COMPETITION FINISHED");
            EasyML.Oper.Log.Write($"---------------------------");
            EasyML.Oper.Log.Write($"The best configuration is:");
            EasyML.Oper.Report((ConfigBase)bestModelConfig, false, 4);
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write($"The best model info:");
            EasyML.Oper.Report(bestModel, false, 4);
            EasyML.Oper.Log.Write($"The best model results:");
            EasyML.Oper.Report(bestErrStat, false, 4);
            //Now show first 3 computation details on testing data
            EasyML.Oper.Log.Write($"First 3 computations of the best model");
            ReportDetailOfFirstNComputations(bestModel, testingData, 3);
            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            ExecuteTTOOCompetition();
            return;
        }

    }//MLPModelsRegressionCompetition

}//Namespace
