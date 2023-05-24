using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyMLEduApp.Examples.MLP
{
    /// <summary>
    /// Example code demonstrates use of
    /// Network, CrossVal, BHS, Stacking, RVFL and Composite models
    /// to solve Binary decisions tasks (classification).
    /// Code also shows how objects of type dataset, configuration, error
    /// statistics or model can be easily serialized and deserialized.
    /// 
    /// See txt files related to csv files for more info.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   BeetleFly_train.csv and BeetleFly_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class MLPModelsBinaryCompetition
    {
        //Methods
        private static void ExecuteBeetleFlyCompetition()
        {
            string taskName = "Beetle (1) or Fly (0)";
            //Output feature names of binary decision task
            List<string> outputFeatureNames = new List<string>()
            {
                "BeetleFlySwitch"
            };
            //Model configurations to be applied one by one one
            List<IModelConfig> modelConfigCollection = new List<IModelConfig>()
            {
                MLPModelConfigs.CreateNetworkModelConfig(ActivationFnID.LeakyReLU),
                MLPModelConfigs.CreateRVFLModelConfig(),
                MLPModelConfigs.CreateCrossValModelConfig(0.1d),
                MLPModelConfigs.CreateBHSModelConfig(),
                MLPModelConfigs.CreateStackingModelConfig(0.1d),
                MLPModelConfigs.CreateSmallCompositeModelConfig()
            };
            //For each defined model configuration
            //build a trained model and then test its performance.
            //Select the best one and serialize it together with its
            //configuration, error statistics and result dataset for later use.
            EasyML.Oper.Log.Write($"*****************************************************************");
            EasyML.Oper.Log.Write($"MODELS COMPETITION on {taskName} STARTED");
            EasyML.Oper.Log.Write($"*****************************************************************");
            //Load Training and Testing data
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
            EasyML.Oper.Log.Write(string.Empty);
            ModelBase bestModel = null;
            IModelConfig bestModelConfig = null;
            ModelErrStat bestModelErrStat = null;
            ResultDataset bestmodelResultDataset = null;
            foreach (IModelConfig modelCfg in modelConfigCollection)
            {
                ModelBase model =
                    EasyML.Oper.Build(modelCfg, //Model config
                                      taskName, //Out task name
                                      OutputTaskType.Binary, //Our task type
                                      outputFeatureNames, //Output feature names
                                      trainingData, //Sample training data
                                      true, //We want to report progress and results
                                      false //We do not require to report all details
                                      );
                ModelErrStat errStat =
                    EasyML.Oper.Test(model, //Our built model
                                     testingData, //Sample testing data
                                     out ResultDataset resultDataset, //Original testing samples together with computed data
                                     true, //We want to report progress and results
                                     false //We do not require to report all details
                                     );
                if (bestModelErrStat == null || bestModelErrStat.IsBetter(errStat))
                {
                    bestModel = model;
                    bestModelConfig = modelCfg;
                    bestModelErrStat = errStat;
                    bestmodelResultDataset = resultDataset;
                }
            }
            //Serialization
            EasyML.Oper.Log.Write($"Serializing winner's data to Temp folder...");
            string fileName;
            string fileNamePrefix = "./Temp/BeetleFly_winner_";
            //Model
            fileName = fileNamePrefix + "model.bin";
            bestModel.Serialize(fileName);
            EasyML.Oper.Log.Write($"    {fileName}");
            //Config
            fileName = fileNamePrefix + "config.bin";
            ((ConfigBase)bestModelConfig).Serialize(fileName);
            EasyML.Oper.Log.Write($"    {fileName}");
            //Error statistics
            fileName = fileNamePrefix + "errStat.bin";
            bestModelErrStat.Serialize(fileName);
            EasyML.Oper.Log.Write($"    {fileName}");
            //Result dataset from model test
            fileName = fileNamePrefix + "resDataset.bin";
            bestmodelResultDataset.Serialize(fileName);
            EasyML.Oper.Log.Write($"    {fileName}");
            //Finish
            EasyML.Oper.Log.Write($"Serialization completed.");
            EasyML.Oper.Log.Write($"MODELS COMPETITION on {taskName} FINISHED");
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write(string.Empty);
            return;
        }

        private static void ReportBeetleFlyCompetitionResults()
        {
            //Deserialize previously serialized objects
            string fileNamePrefix = "./Temp/BeetleFly_winner_";
            ModelBase bestModel = (ModelBase)SerializableObject.Deserialize(fileNamePrefix + "model.bin");
            IModelConfig bestModelConfig = (IModelConfig)SerializableObject.Deserialize(fileNamePrefix + "config.bin");
            ModelErrStat bestErrStat = (ModelErrStat)SerializableObject.Deserialize(fileNamePrefix + "errStat.bin");
            ResultDataset bestResultDataset = (ResultDataset)SerializableObject.Deserialize(fileNamePrefix + "resDataset.bin");

            //Report the best model
            EasyML.Oper.Log.Write($"COMPETITION RESULTS:");
            EasyML.Oper.Log.Write($"--------------------");
            EasyML.Oper.Log.Write($"The best configuration is:");
            EasyML.Oper.Report((ConfigBase)bestModelConfig, false, 4);
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write($"Trained model detailed info:");
            EasyML.Oper.Report(bestModel, false, 4);
            EasyML.Oper.Log.Write($"Achieved error statistics:");
            EasyML.Oper.Report(bestErrStat, false, 4);
            //Now show first 3 computation details from result dataset
            EasyML.Oper.Log.Write($"First 3 computations of the best model");
            for (int i = 0; i < 3 && i < bestResultDataset.ComputedVectorCollection.Count; i++)
            {
                EasyML.Oper.Log.Write($"Sample {i + 1}");
                EasyML.Oper.Log.Write($"--------------");
                EasyML.Oper.Log.Write($"    Ideal:");
                TaskOutputDetailBase idealOutputDetail = bestModel.GetOutputDetail(bestResultDataset.IdealVectorCollection[i]);
                EasyML.Oper.Log.Write(idealOutputDetail.GetTextInfo(4));
                EasyML.Oper.Log.Write($"    Computed:");
                TaskOutputDetailBase computedOutputDetail = bestModel.GetOutputDetail(bestResultDataset.ComputedVectorCollection[i]);
                EasyML.Oper.Log.Write(computedOutputDetail.GetTextInfo(4));
                EasyML.Oper.Log.Write(string.Empty);
            }
            EasyML.Oper.Log.Write($"*****************************************************************");
            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            ExecuteBeetleFlyCompetition();
            ReportBeetleFlyCompetitionResults();
            return;
        }

    }//MLPModelsBinaryCompetition

}//Namespace
