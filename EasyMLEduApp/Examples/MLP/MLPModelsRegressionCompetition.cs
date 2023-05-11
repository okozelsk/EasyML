using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using System;
using System.Collections.Generic;

namespace EasyMLDemoApp.Examples.MLP
{
    /// <summary>
    /// Example code demonstrates use of
    /// Network, CrossVal, Stacking and Composite models
    /// for Regression tasks (forecasting).
    /// 
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   TTOO_patterns_train.csv and TTOO_patterns_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public class MLPModelsRegressionCompetition
    {
        //Constructor
        public MLPModelsRegressionCompetition()
        {
            return;
        }

        private void ReportDetailOfFirst10Computations(ModelBase model, SampleDataset data)
        {
            for (int i = 0; i < 10 && i < data.Count; i++)
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
        private void ExecuteTTOOCompetition()
        {
            string taskName = "TTOO Biosystems Share Prices";
            //Output feature names of TTOO Biosystems Share Prices regression task
            List<string> outputFeatureNames = new List<string>() { "High", "Low", "Adj Close" };
            //Training and Testing data
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData(outputFeatureNames.Count, ////Number of output features
                                           "./Data/TTOO_patterns_train.csv"//Training csv data file name
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData(outputFeatureNames.Count, ////Number of output features
                                           "./Data/TTOO_patterns_test.csv"//Testing csv data file name
                                           );
            //Model configurations to be applied one by one on TTOO Biosystems Share Prices data
            List<IModelConfig> modelConfigCollection = new List<IModelConfig>()
            {
                MLPModelConfigs.CreateNetworkModelConfig(ActivationFnID.ELU),
                MLPModelConfigs.CreateCrossValModelConfig(0.25d),
                MLPModelConfigs.CreateStackingModelConfig(0.25d),
                MLPModelConfigs.CreateCompositeModelConfig(0.25d)
            };

            //For each defined model configuration
            //build a trained model and then test its performance.
            //Select the best one and report its configuration and testing results again.
            EasyML.Oper.Log.Write($"MODELS COMPETITION STARTED");
            EasyML.Oper.Log.Write($"--------------------------");
            ModelBase bestModel = null;
            IModelConfig bestModelConfig = null;
            ModelErrStat bestErrStat = null;
            foreach (IModelConfig modelCfg in modelConfigCollection)
            {
                ModelBase model =
                    EasyML.Oper.Build(modelCfg, //Model config
                                      taskName, //Out task name
                                      OutputTaskType.Regression, //Our task type
                                      outputFeatureNames, //Output feature names
                                      trainingData, //Sample training data
                                      true, //We want to report progress and results
                                      false //We do not require to report all details
                                      );
                ModelErrStat errStat =
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
            EasyML.Oper.Log.Write($"The best model info:");
            EasyML.Oper.Report(bestModel, false, 4);
            EasyML.Oper.Log.Write($"The best model results:");
            EasyML.Oper.Report(bestErrStat, false, 4);
            //Now show first 10 computation details on testing data
            EasyML.Oper.Log.Write($"First 10 computations of the best model");
            ReportDetailOfFirst10Computations(bestModel, testingData);
            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            Console.Clear();
            ExecuteTTOOCompetition();
            return;
        }

    }//MLPModelsRegressionCompetition

}//Namespace
