using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using EasyMLCore.TimeSeries;
using EasyMLEduApp.Examples.MLP;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasyMLEduApp.Examples.ReservoirComputing
{
    /// <summary>
    /// Example code demonstrates ability of the ResComp component to
    /// outperform winners of the "ICANN/PASCAL2 Challenge: MEG MindReading".
    /// It is a categorical task (classification).
    /// The goal of the challenge is to determine what type of video the subject (a young man)
    /// is watching, based on magnetoencephalography (MEG) data.
    /// Input data is 1 second long read from MEG, giving 204 variables in 200 timepoints.
    /// ResComp achieves accuracy 71.36%. The winner of challenge achieved 68% and
    /// combined result of top 10 challenge participating teams was amost 70%.
    /// ResComp outperforms both challenge results.
    /// The whole example takes 44 seconds on my notebook with SSD and RYZEN 7 CPU.
    /// Here is the link to challenge paper describing the challenge and its results in detail:
    /// https://www.researchgate.net/publication/239918465_ICANNPASCAL2_Challenge_MEG_Mind_Reading_--_Overview_and_Results
    /// 
    /// Data must be downloaded and unzipped in "./LargeData" subfolder.
    /// Data is organized as fixed length time series input pattern (input vector)
    /// followed by expected output values (output vector).
    /// See txt file related to csv files for more info.
    /// Example expects following files in ./LargeData subfolder:
    ///   MindReading.txt
    ///   MindReading_train.csv
    ///   MindReading_test.csv
    /// </summary>
    /// <seealso cref="MLPModelConfigs"/>
    public static class ResCompMindReadingChallenge
    {

        //Methods
        private static void ExecuteMindReadingChallenge()
        {
            Stopwatch challengeSW = Stopwatch.StartNew();
            string taskName = "Type of Video Stimulus";
            //Output class labels of classification task
            List<string> outputFeatureNames = new List<string>()
            {
                "Artificial screen saves",
                "Nature clips from nature documentaries",
                "Foolball",
                "Mr. Bean",
                "Chaplin"
            };
            //Training and Testing data
            Stopwatch datasetsLoadingSW = Stopwatch.StartNew();
            EasyML.Oper.Report("./LargeData/MindReading.txt");
            SampleDataset trainingData =
                EasyML.Oper.LoadSampleData("./LargeData/MindReading_train.csv", //Training csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output features (classes)
                                           );
            SampleDataset testingData =
                EasyML.Oper.LoadSampleData("./LargeData/MindReading_test.csv", //Testing csv data file name
                                           SampleDataset.CsvOutputFeaturesPosition.Last,
                                           SampleDataset.CsvOutputFeaturesPresence.ClassesAsNumberFrom1,
                                           outputFeatureNames.Count //Number of output features (classes)
                                           );
            datasetsLoadingSW.Stop();
            Stopwatch buildRCSW = Stopwatch.StartNew();
            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////
            //Reservoir config
            ReservoirConfig reservoirCfg =
                new ReservoirConfig(new ReservoirInputConfig(Reservoir.InputFeeding.PatternConstLength, //Feeding regime
                                                             204, //204 variables
                                                             TimeSeriesPattern.FlatVarSchema.VarSequence, //Variables are organized as noodles
                                                             4d, //4 synaptical connections of 1 input variable to hidden neurons
                                                             ReservoirInputConfig.DefaultMaxDelay, //No delay
                                                             0.5d //Max strength of input per hidden neuron
                                                             ),
                                    new ReservoirHiddenLayerConfig((8 * 204), //Number of hidden neurons
                                                                   3d, //Each hidden neuron to be connected with 3 other hidden neurons
                                                                   1,//Synaptic maximum delay
                                                                   0.86 //retainment
                                                                   )
                                    );
            ////////////////////////////////////////////////////////////////////////////
            //Task config
            //Allowed inputs (outputs from raservoir)
            ResCompTaskInputSectionsConfig inputSections =
                new ResCompTaskInputSectionsConfig(Reservoir.OutSection.SpikesFadingTraces);

            //Task MLP model
            //ResCompTask has very simple end-model: output only network (no hidden layers).
            //Without input dropout option, accuracy on test dataset is 70.29%.
            //With input dropout option, accuracy on test dataset is 71.52%.
            InputOptionsConfig ioc = new InputOptionsConfig(new DropoutConfig(0.5d, DropoutMode.Bernoulli));
            NetworkModelConfig taskModelCfg =
                new NetworkModelConfig(1, 100, new AdamConfig(), null, ioc);

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
            ResComp resComp =
                EasyML.Oper.Build(resCompCfg, //Reservoir computer configuration
                                  trainingData, //Training samples
                                  out ReservoirStat resStat //Stat data of the reservoir
                                  );
            buildRCSW.Stop();
            Stopwatch testRCSW = Stopwatch.StartNew();
            //Testing
            List<MLPModelErrStat> errStats =
                EasyML.Oper.Test(resComp, //Our built reservoir computer
                                 testingData, //Testing data
                                 out _, //We do not need original testing samples together with computed data
                                 true, //Verbose yes
                                 true //And with full detail
                                 );
            testRCSW.Stop();
            challengeSW.Stop();
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write($"Challenge finished in {challengeSW.Elapsed.Hours.ToString().PadLeft(2, '0')}:{challengeSW.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{challengeSW.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            EasyML.Oper.Log.Write($"   of which:");
            EasyML.Oper.Log.Write($"       Loading data took: {datasetsLoadingSW.Elapsed.Hours.ToString().PadLeft(2, '0')}:{datasetsLoadingSW.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{datasetsLoadingSW.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            EasyML.Oper.Log.Write($"       ResComp buid took: {buildRCSW.Elapsed.Hours.ToString().PadLeft(2, '0')}:{buildRCSW.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{buildRCSW.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            EasyML.Oper.Log.Write($"       ResComp test took: {testRCSW.Elapsed.Hours.ToString().PadLeft(2, '0')}:{testRCSW.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{testRCSW.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            EasyML.Oper.Log.Write(string.Empty);
            EasyML.Oper.Log.Write(string.Empty);
            return;
        }


        /// <summary>
        /// Runs the example code.
        /// </summary>
        public static void Run()
        {
            Console.Clear();
            ExecuteMindReadingChallenge();
            return;
        }

    }//ResCompMindReadingChallenge

}//Namespace
