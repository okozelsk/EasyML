using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using EasyMLCore.MLP.Model;
using System;
using System.Collections.Generic;

namespace EasyMLEduApp.Examples.MLP
{
    /// <summary>
    /// Example code demonstrates use of
    /// Network, CrossVal, BHS, Stacking and Composite models
    /// to solve Categorical tasks (classification).
    /// Code also shows DiagnosticTest of models.
    /// 
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   LibrasMovement_train.csv and LibrasMovement_test.csv,
    ///   ProximalPhalanxOutlineAgeGroup_train.csv and ProximalPhalanxOutlineAgeGroup_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    /// <seealso cref="EasyML"/>
    public static class MLPModelsCategoricalCompetition
    {

        //Methods
        private static void ReportDetailOfFirst10Computations(ModelBase model, SampleDataset data)
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

        private static void ExecuteLibrasMovementCompetition()
        {
            string taskName = "Libras Movement";
            //Output class labels of Libras Movement classification task
            List<string> outputClassLabels = new List<string>()
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
                                           outputClassLabels.Count ////Number of output features (classes)
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/LibrasMovement_test.csv",//Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputClassLabels.Count ////Number of output features (classes)
                                           );
            //Model configurations to be applied one by one on Libras Movement data
            List<IModelConfig> modelConfigCollection = new List<IModelConfig>()
            {
                MLPModelConfigs.CreateNetworkModelConfig(ActivationFnID.ReLU),
                MLPModelConfigs.CreateCrossValModelConfig(0.05d),
                MLPModelConfigs.CreateBHSModelConfig(),
                MLPModelConfigs.CreateStackingModelConfig(0.05d),
                MLPModelConfigs.CreateSmallCompositeModelConfig(),
                MLPModelConfigs.CreateCompositeModelConfig(0.05d)
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
                                      OutputTaskType.Categorical, //Our task type
                                      outputClassLabels, //Output feature names
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

                //A diagnostic test as a more informative alternative to standard test
                ModelDiagnosticData diagData =
                    EasyML.Oper.DiagnosticTest(model, //Our built model
                                               testingData, //Sample testing data
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
            //Now show first 10 computation details on testing data
            EasyML.Oper.Log.Write($"First 10 computations of the best model");
            ReportDetailOfFirst10Computations(bestModel, testingData);
            return;
        }

        private static void ExecuteProximalPhalanxOutlineAgeGroupCompetition()
        {
            string taskName = "Age Group";
            //Output class labels of Proximal Phalanx Outline Age Group classification task
            List<string> outputClassLabels = new List<string>()
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
                                           outputClassLabels.Count //Number of output features (classes)
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./Data/ProximalPhalanxOutlineAgeGroup_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.First,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputClassLabels.Count //Number of output features (classes)
                                           );
            //Model configurations to be applied one by one on Proximal Phalanx Outline Age Group data
            List<IModelConfig> modelConfigCollection = new List<IModelConfig>()
            {
                MLPModelConfigs.CreateNetworkModelConfig(ActivationFnID.ReLU),
                MLPModelConfigs.CreateCrossValModelConfig(0.1d),
                MLPModelConfigs.CreateBHSModelConfig(),
                MLPModelConfigs.CreateStackingModelConfig(0.1d),
                MLPModelConfigs.CreateSmallCompositeModelConfig(),
                MLPModelConfigs.CreateCompositeModelConfig(0.1d)
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
                                      OutputTaskType.Categorical, //Our task type
                                      outputClassLabels, //Output feature names
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

                //A diagnostic test as a more informative alternative to standard test
                ModelDiagnosticData diagData =
                    EasyML.Oper.DiagnosticTest(model, //Our built model
                                               testingData, //Sample testing data
                                               true, //We want to report progress and results
                                               false //We do not require to report all details
                                               );

                if (bestErrStat == null)
                {
                    bestModel = model;
                    bestModelConfig = modelCfg;
                    bestErrStat = errStat;
                }
                else if(bestErrStat.IsBetter(errStat))
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
            //Now show first 10 computation details on testing data
            EasyML.Oper.Log.Write($"First 10 computations of the best model");
            ReportDetailOfFirst10Computations(bestModel, testingData);
            return;
        }


        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            ExecuteLibrasMovementCompetition();
            ExecuteProximalPhalanxOutlineAgeGroupCompetition();
            return;
        }


    }//MLPModelsCategoricalCompetition

}//Namespace
