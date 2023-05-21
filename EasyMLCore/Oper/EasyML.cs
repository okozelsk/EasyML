using EasyMLCore.Data;
using EasyMLCore.MLP;
using EasyMLCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using EasyMLCore.TimeSeries;
using EasyMLCore.Extensions;
using System.IO;
using System.Diagnostics;
using static EasyMLCore.Data.SampleDataset;
using EasyMLCore.MLP.Model;

namespace EasyMLCore
{
    /// <summary>
    /// Implements the basic standard operational interface for EasyML users.
    /// </summary>
    public sealed class EasyML
    {
        //Static attributes
        //One and only instance
        private static readonly Lazy<EasyML> _lazy = new Lazy<EasyML>(() => new EasyML());

        //Attribute properties
        /// <inheritdoc cref="StdHandlers"/>>
        public StdHandlers Handlers { get; }

        //Attributes
        //Lock object
        private readonly object _monitor;
        //Changeable log.
        private IOutputLog _log;

        //Constructor
        /// <summary>
        /// Creates the initialized instance.
        /// </summary>
        private EasyML()
        {
            _monitor = new object();
            _log = new ConsoleLog();
            Handlers = new StdHandlers(_log);
            return;
        }

        //Static properties
        /// <summary>
        /// Gets the one and only instance through which is available EasyML's operational interface.
        /// </summary>
        public static EasyML Oper { get { return _lazy.Value; } }

        //Properties
        /// <summary>
        /// Gets the instance of associated output log.
        /// </summary>
        public IOutputLog Log { get { lock (_monitor) { return _log; } } }

        //Static methods

        /// <summary>
        /// Gets the appropriate instance of task specific detailed output.
        /// </summary>
        /// <param name="outputData">Computed or ideal data vector.</param>
        /// <returns>The appropriate instance of task specific detailed output.</returns>
        public static TaskOutputDetailBase GetTaskOutputDetail(OutputTaskType taskType,
                                                               IEnumerable<string> outputFeatureNames,
                                                               double[] outputData
                                                               )
        {
            if (outputFeatureNames == null)
            {
                throw new ArgumentNullException(nameof(outputFeatureNames));
            }
            if (outputData == null)
            {
                throw new ArgumentNullException(nameof(outputData));
            }
            return taskType switch
            {
                OutputTaskType.Regression => new RegressionOutputDetail(outputFeatureNames.ToList(), outputData),
                OutputTaskType.Binary => new BinaryOutputDetail(outputFeatureNames.ToList(), outputData),
                OutputTaskType.Categorical => new CategoricalOutputDetail(outputFeatureNames.ToList(), outputData),
                _ => null,
            };
        }


        //Methods
        /// <summary>
        /// Changes the output log.
        /// </summary>
        /// <param name="newLog">Another output log instance to be used.</param>
        public void ChangeOutputLog(IOutputLog newLog)
        {
            if (newLog == null)
            {
                throw new ArgumentNullException(nameof(newLog), "Output log instance can not be null.");
            }
            lock (_monitor)
            { 
                _log = newLog;
                Handlers.ChangeOutputLog(newLog);
            }
            return;
        }

        /// <summary>
        /// Reports a text file.
        /// </summary>
        /// <param name="fileName">Path to text file to be reported.</param>
        public void Report(string fileName)
        {
            try
            {
                using StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open));
                Log.Write($"Content of file {fileName}:");
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    Log.Write(line);
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Can't report file {fileName}. Exception:");
                Log.Write(ex.Message);
            }
            Log.Write($"{string.Empty}");
            return;
        }

        /// <summary>
        /// Reports model computation detailed output.
        /// </summary>
        /// <param name="detailedOutput">Detailed output of model computation.</param>
        /// <param name="margin">Specifies left margin.</param>
        public void Report(TaskOutputDetailBase detailedOutput, int margin = 0)
        {
            if (detailedOutput == null)
            {
                Log.Write($"Received {nameof(detailedOutput)} is null.");
                return;
            }
            Log.Write(detailedOutput.GetTextInfo(margin));
            return;
        }

        /// <summary>
        /// Reports ResComp computation detailed output.
        /// </summary>
        /// <param name="detailedOutput">Detailed output of ResComp computation.</param>
        /// <param name="margin">Specifies left margin.</param>
        public void Report(List<Tuple<string, TaskOutputDetailBase>> detailedOutputs, int margin = 0)
        {
            if (detailedOutputs == null)
            {
                Log.Write($"Received {nameof(detailedOutputs)} is null.");
                return;
            }
            margin = Math.Max( margin, 0 );
            foreach(Tuple<string, TaskOutputDetailBase> tuple in detailedOutputs)
            {
                Log.Write($"{tuple.Item1.Indent(margin)}");
                Report(tuple.Item2, margin + 4);
                Log.Write($"{string.Empty}");
            }

            return;
        }

        /// <summary>
        /// Reports error statistics.
        /// </summary>
        /// <param name="errStat">Error statistics to be reported.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <param name="margin">Specifies left margin.</param>
        public void Report(TaskErrStatBase errStat, bool detail = false, int margin = 0)
        {
            if (errStat == null)
            {
                Log.Write($"Received {nameof(errStat)} is null.");
                return;
            }
            string report = errStat.GetReportText(detail, margin);
            Log.Write(report);
            return;
        }

        /// <summary>
        /// Reports error statistics.
        /// </summary>
        /// <param name="errStat">Error statistics to be reported.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <param name="margin">Specifies left margin.</param>
        public void Report(ModelErrStat errStat, bool detail = false, int margin = 0)
        {
            if (errStat == null)
            {
                Log.Write($"Received {nameof(errStat)} is null.");
                return;
            }
            string report = errStat.GetReportText(detail, margin);
            Log.Write(report);
            return;
        }

        /// <summary>
        /// Reports a configuration.
        /// </summary>
        /// <param name="config">A configuration to be reported.</param>
        /// <param name="detail">Specifies whether to report max available detail (default values).</param>
        /// <param name="margin">Specifies left margin.</param>
        public void Report(ConfigBase config, bool detail = false, int margin = 0)
        {
            if (config == null)
            {
                Log.Write($"Received {nameof(config)} is null.");
                return;
            }
            string report = config.GetXml(!detail).ToString();
            if (margin > 0)
            {
                report = report.Indent(margin);
            }
            Log.Write(report);
            return;
        }

        /// <summary>
        /// Reports the state of given model.
        /// </summary>
        /// <param name="model">Model to be reported.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <param name="margin">Specifies left margin.</param>
        public void Report(ModelBase model, bool detail = false, int margin = 0)
        {
            if (model == null)
            {
                Log.Write($"Received {nameof(model)} is null.");
                return;
            }
            string report = model.GetInfoText(detail, margin);
            Log.Write(report);
            return;
        }

        /// <summary>
        /// Reports the state of reservoir computer.
        /// </summary>
        /// <param name="resComp">Reservoir computer to be reported.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <param name="margin">Specifies left margin.</param>
        public void Report(ResComp resComp, bool detail = false, int margin = 0)
        {
            if (resComp == null)
            {
                Log.Write($"Received {nameof(resComp)} is null.");
                return;
            }
            string report = resComp.GetInfoText(detail, margin);
            Log.Write(report);
            return;
        }

        /// <summary>
        /// Loads csv datafile containing the sample input and output data in a one row and converts data to sample dataset.
        /// </summary>
        /// <param name="csvFileName">The name of a csv dataset file containing the sample input and output data in a one row.</param>
        /// <param name="outputFeaturesPosition">Specifies where are output features in csv data row.</param>
        /// <param name="outputFeaturesPresence">Specifies how are output features presented in csv data row.</param>
        /// <param name="numOfOutputFeatures">Number of output features. Important: in case of classification it is a number of classes.</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        public SampleDataset LoadSampleData(string csvFileName,
                                            CsvOutputFeaturesPosition outputFeaturesPosition,
                                            CsvOutputFeaturesPresence outputFeaturesPresence,
                                            int numOfOutputFeatures,
                                            bool verbose = true
                                            )
        {
            //Load csv dataset and create dataset
            if (verbose)
            {
                Log.Write($"Loading {csvFileName}...");
            }
            CsvDataHolder csvData = new CsvDataHolder(csvFileName);
            SampleDataset dataset = SampleDataset.Load(csvData,
                                                       outputFeaturesPosition,
                                                       outputFeaturesPresence,
                                                       numOfOutputFeatures
                                                       );
            return dataset;
        }

        /// <summary>
        /// Loads csv datafile (time points format) and converts data to sample dataset.
        /// </summary>
        /// <param name="csvFileName">The name of a csv dataset file containing the sample input and output data in a one row.</param>
        /// <param name="inputFieldNameCollection">Input field names.</param>
        /// <param name="outputFieldNameCollection">Output field names.</param>
        /// <param name="remainingInputVector">The last unused input vector (next input).</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        public SampleDataset LoadSampleData(string csvFileName,
                                            List<string> inputFieldNameCollection,
                                            List<string> outputFieldNameCollection,
                                            out double[] remainingInputVector,
                                            bool verbose = true
                                            )
        {
            //Load csv dataset and create dataset
            if (verbose)
            {
                Log.Write($"Loading {csvFileName}...");
            }
            CsvDataHolder csvData = new CsvDataHolder(csvFileName);
            SampleDataset dataset = SampleDataset.Load(csvData,
                                                       inputFieldNameCollection,
                                                       outputFieldNameCollection,
                                                       out remainingInputVector
                                                       );
            return dataset;
        }

        /// <summary>
        /// Prepares default network model configuration for given complexity.
        /// </summary>
        /// <param name="trainingData">Available training data.</param>
        /// <param name="verbose">Specifies whether to report configuration.</param>
        /// <returns>Default network model configuration for given task complexity.</returns>
        public NetworkModelConfig GetDefaultNetworkModelConfig(SampleDataset trainingData, bool verbose = true)
        {
            NetworkModelConfig config =
                NetworkModelConfig.GetDefaultNetworkModelConfig(trainingData.InputVectorLength,
                                                                trainingData.OutputVectorLength,
                                                                trainingData.Count
                                                                );
            if (verbose)
            {
                Log.Write($"Default network config for InpLength={trainingData.InputVectorLength}, OutpLength={trainingData.OutputVectorLength} and NumOfSamples={trainingData.Count} is:");
                Report(config, false, 0);
            }
            return config;
        }

        /// <summary>
        /// Prepares default model configuration for given complexity.
        /// </summary>
        /// <param name="trainingData">Available training data.</param>
        /// <param name="verbose">Specifies whether to report configuration.</param>
        /// <returns>Default MLP model configuration for given task complexity.</returns>
        public IModelConfig GetDefaultMLPModelConfig(SampleDataset trainingData, bool verbose = true)
        {
            NetworkModelConfig netCfg = GetDefaultNetworkModelConfig(trainingData, false);
            CrossValModelConfig modelCfg =
                new CrossValModelConfig(netCfg, CrossValModelConfig.DefaultFoldDataRatio);

            if (verbose)
            {
                Log.Write($"Default model config for InpLength={trainingData.InputVectorLength}, OutpLength={trainingData.OutputVectorLength} and NumOfSamples={trainingData.Count} is:");
                Report(modelCfg, false, 0);
            }
            return modelCfg;
        }

        /// <summary>
        /// Builds a model based on given configuration and given training data.
        /// </summary>
        /// <param name="modelCfg">Model configuration.</param>
        /// <param name="taskName">Task name.</param>
        /// <param name="taskType">Task type.</param>
        /// <param name="outputFeatureNames">Output feature names.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <returns>Built model.</returns>
        public ModelBase Build(IModelConfig modelCfg,
                               string taskName,
                               OutputTaskType taskType,
                               List<string> outputFeatureNames,
                               SampleDataset trainingData,
                               bool verbose = true,
                               bool detail = false
                               )
        {
            if (verbose)
            {
                Log.Write($"Build model for {taskType} task {taskName}.");
                Log.Write($"{modelCfg.GetType()}:");
                Log.Write(modelCfg.ToString());
                Log.Write(string.Empty);
                Log.Write($"Build is running...");
            }

            //Build a model
            Type modelCfgType = modelCfg.GetType();
            string modelNamePrefix = $"({taskName})-";
            ModelBase model = null;
            if (modelCfgType == typeof(NetworkModelConfig))
            {
                model = NetworkModel.Build(modelCfg,
                                           modelNamePrefix,
                                           taskType,
                                           outputFeatureNames,
                                           trainingData,
                                           null,
                                           verbose ? Handlers.OnModelBuildProgressChanged : null
                                           );
            }
            else if (modelCfgType == typeof(CrossValModelConfig))
            {
                model = CrossValModel.Build(modelCfg,
                                            modelNamePrefix,
                                            taskType,
                                            outputFeatureNames,
                                            trainingData,
                                            verbose ? Handlers.OnModelBuildProgressChanged : null
                                            );
            }
            else if (modelCfgType == typeof(StackingModelConfig))
            {
                model = StackingModel.Build(modelCfg,
                                            modelNamePrefix,
                                            taskType,
                                            outputFeatureNames,
                                            trainingData,
                                            verbose ? Handlers.OnModelBuildProgressChanged : null
                                            );
            }
            else if (modelCfgType == typeof(BHSModelConfig))
            {
                model = BHSModel.Build(modelCfg,
                                       modelNamePrefix,
                                       taskType,
                                       outputFeatureNames,
                                       trainingData,
                                       verbose ? Handlers.OnModelBuildProgressChanged : null
                                       );
            }
            else if (modelCfgType == typeof(CompositeModelConfig))
            {
                model = CompositeModel.Build(modelCfg,
                                             modelNamePrefix,
                                             taskType,
                                             outputFeatureNames,
                                             trainingData,
                                             verbose ? Handlers.OnModelBuildProgressChanged : null
                                             );
            }
            else
            {
                throw new ArgumentException($"Unsupported model configuration {modelCfgType}.", nameof(modelCfg));
            }
            if(verbose)
            {
                Log.Write(string.Empty);
                if(detail)
                {
                    Report(model, detail, 0);
                }
            }
            return model;
        }

        /// <summary>
        /// Tests a model on given data.
        /// </summary>
        /// <param name="model">Model to be tested.</param>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="resultDataset">Result dataset.</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <returns>Error statistics of the model test.</returns>
        public ModelErrStat Test(ModelBase model,
                                 SampleDataset testingData,
                                 out ResultDataset resultDataset,
                                 bool verbose = true,
                                 bool detail = false
                                 )
        {
            if (verbose)
            {
                Log.Write($"Test of the model {model.Name} is running...");
            }
            ModelErrStat errStat = model.Test(testingData,
                                                out resultDataset,
                                                verbose ? Handlers.OnModelTestProgressChanged : null
                                                );
            if (verbose)
            {
                Report(errStat, detail, 0);
                Log.Write(string.Empty);
            }
            return errStat;
        }

        /// <summary>
        /// Performs diagnostic test of a model and all its sub-models on given data.
        /// </summary>
        /// <param name="model">Model to be tested.</param>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <returns>Diagnostics data of the model and all its sub-models.</returns>
        public ModelDiagnosticData DiagnosticTest(ModelBase model,
                                                  SampleDataset testingData,
                                                  bool verbose = true,
                                                  bool detail = false
                                                  )
        {
            if (verbose)
            {
                Log.Write($"Diagnostic test of the model {model.Name} is running...");
            }
            ModelDiagnosticData diagnosticData = model.DiagnosticTest(testingData, verbose ? Handlers.OnModelTestProgressChanged : null);
            if (verbose)
            {
                Log.Write(diagnosticData.GetInfoText(detail, 0));
                Log.Write(string.Empty);
            }
            return diagnosticData;
        }


        /// <summary>
        /// Performs the deep test of the MLP model's configuration on given data.
        /// For each round:
        ///   Available sample data is shuffled and divided to new training and testing dataset.
        ///   A new instance of MLP model is created, trained and tested. Error stat is collected.
        /// </summary>
        /// <param name="cfg">MLP model's configuration.</param>
        /// <param name="taskName">Task name.</param>
        /// <param name="taskType">Task type.</param>
        /// <param name="outputFeatureNames">Output feature names.</param>
        /// <param name="origTrainingData">Original training samples.</param>
        /// <param name="origTestingData">Original testing samples.</param>
        /// <param name="rounds">Specifies number of deep test rounds.</param>
        /// <returns>Aggregated error stat.</returns>
        public ModelErrStat DeepTest(IModelConfig cfg,
                                     string taskName,
                                     OutputTaskType taskType,
                                     List<string> outputFeatureNames,
                                     SampleDataset origTrainingData,
                                     SampleDataset origTestingData,
                                     int rounds = 10
                                     )
        {
            if (cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }
            if (rounds < 1)
            {
                throw new ArgumentException($"Number of deep test rounds must be GT 0.", nameof(rounds));
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            ModelErrStat aggregatedErrStat = null;
            Random rand = new Random(0);
            Log.Write($"DEEP TEST started...");
            for (int round = 0; round < rounds; round++)
            {
                Log.Write($"Round {round + 1} of {rounds}");
                Log.Write($"Creating new variant of data...");
                SampleDataset.CreateShuffledSimilar(rand,
                                                    taskType,
                                                    origTrainingData,
                                                    origTestingData,
                                                    out SampleDataset newTrainingData,
                                                    out SampleDataset newTestingData
                                                    );
                Log.Write($"Data prepared.");
                ////////////////////////////////////////////////////////////////////////////
                //Build and testing
                //Build
                ModelBase model =
                    Oper.Build(cfg, //Model configuration
                               taskName,
                               taskType,
                               outputFeatureNames,
                               newTrainingData
                               );
                //Testing
                ModelErrStat roundErrStats =
                    Oper.Test(model, //Our built model
                              newTestingData, //Testing data
                              out _ //Testing samples together with computed data
                              );
                if (aggregatedErrStat == null)
                {
                    aggregatedErrStat = roundErrStats;
                }
                else
                {
                    aggregatedErrStat.Merge(roundErrStats);
                }
            }
            stopwatch.Stop();
            Log.Write(string.Empty);
            Log.Write($"DEEP TEST finished in {stopwatch.Elapsed.Hours.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            Log.Write($"------------------------------");
            Log.Write($"Task ({taskName})");
            Report(aggregatedErrStat, false, 4);
            Log.Write(string.Empty);
            return aggregatedErrStat;
        }


        /// <summary>
        /// Builds a reservoir computer based on given configuration and training data.
        /// </summary>
        /// <param name="cfg">Reservoir computer's configuration.</param>
        /// <param name="trainingData">Training samples.</param>
        /// <param name="reservoirStat">Statistics of computer's inner reservoir.</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <returns>Built reservoir computer.</returns>
        public ResComp Build(ResCompConfig cfg,
                             SampleDataset trainingData,
                             out ReservoirStat reservoirStat,
                             bool verbose = true,
                             bool detail = false
                             )
        {
            if (verbose)
            {
                Log.Write($"Build reservoir computer.");
                Log.Write($"{cfg.GetType()}:");
                Log.Write(cfg.ToString());
                Log.Write(string.Empty);
                Log.Write($"Build is running...");
            }
            //Build
            ResComp resComp = ResComp.Build(cfg, trainingData, out reservoirStat, verbose ? Handlers.OnResCompBuildProgressChanged : null);
            if (verbose)
            {
                Log.Write(string.Empty);
                if(detail)
                {
                    Report(resComp, detail, 0);
                }
            }
            return resComp;
        }

        /// <summary>
        /// Tests a reservoir computer on given data.
        /// </summary>
        /// <param name="resComp">Reservoir computer to be tested.</param>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="resultDataset">Result dataset containing original samples together with computed data.</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <returns>Error statistics of the RC test.</returns>
        public List<ModelErrStat> Test(ResComp resComp,
                                       SampleDataset testingData,
                                       out ResultDataset resultDataset,
                                       bool verbose = true,
                                       bool detail = false
                                       )
        {
            if (verbose)
            {
                Log.Write($"Test of the reservoir computer is running...");
            }
            List<ModelErrStat> errStats = resComp.Test(testingData,
                                                         out resultDataset,
                                                         verbose ? Handlers.OnResCompTestProgressChanged : null
                                                         );
            if (verbose)
            {
                Log.Write(string.Empty);
                for (int i = 0; i < errStats.Count; i++)
                {
                    Log.Write($"Task ({resComp.Tasks[i].Name})");
                    Report(errStats[i], detail, 4);
                }
                Log.Write(string.Empty);
            }
            return errStats;
        }

        /// <summary>
        /// Performs the deep test of the reservoir computer's configuration on given data.
        /// For each round:
        ///   Available sample data is shuffled and divided to new training and testing dataset.
        ///   A new instance of RC is created, trained and tested. Error stat is collected.
        /// </summary>
        /// <param name="cfg">Reservoir computer's configuration.</param>
        /// <param name="origTrainingData">Original training samples.</param>
        /// <param name="origTestingData">Original testing samples.</param>
        /// <param name="rounds">Specifies number of deep test rounds.</param>
        /// <returns>Aggregated error stat of the RC task.</returns>
        public ModelErrStat DeepTest(ResCompConfig cfg,
                                     SampleDataset origTrainingData,
                                     SampleDataset origTestingData,
                                     int rounds = 10
                                     )
        {
            if (cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }
            if(cfg.TaskCfgCollection.Count != 1)
            {
                throw new ArgumentException($"Only single task configuration is supported.", nameof(cfg));
            }
            if (rounds < 1)
            {
                throw new ArgumentException($"Number of deep test rounds must be GT 0.", nameof(rounds));
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            ModelErrStat aggregatedErrStat = null;
            Random rand = new Random(0);
            Log.Write($"DEEP TEST started...");
            for(int round = 0; round < rounds; round++)
            {
                Log.Write($"Round {round + 1} of {rounds}");
                Log.Write($"Creating new variant of data...");
                SampleDataset.CreateShuffledSimilar(rand,
                                                    cfg.TaskCfgCollection[0].TaskType,
                                                    origTrainingData,
                                                    origTestingData,
                                                    out SampleDataset newTrainingData,
                                                    out SampleDataset newTestingData
                                                    );
                Log.Write($"Data prepared.");
                ////////////////////////////////////////////////////////////////////////////
                //Build and testing
                //Build
                ResComp resComp =
                    Oper.Build(cfg, //Reservoir computer configuration
                               newTrainingData, //Training samples
                               out _ //Stat data of the reservoir
                               );
                //Testing
                List<ModelErrStat> roundErrStats =
                    EasyML.Oper.Test(resComp, //Our built reservoir computer
                                     newTestingData, //Testing data
                                     out _ //Original testing samples together with computed data
                                     );
                if(aggregatedErrStat == null)
                {
                    aggregatedErrStat = roundErrStats[0];
                }
                else
                {
                    aggregatedErrStat.Merge(roundErrStats[0]);
                }
            }
            stopwatch.Stop();
            Log.Write(string.Empty);
            Log.Write($"DEEP TEST finished in {stopwatch.Elapsed.Hours.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Minutes.ToString().PadLeft(2, '0')}:{stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0')}");
            Log.Write($"------------------------------");
            Log.Write($"Task ({cfg.TaskCfgCollection[0].Name})");
            Report(aggregatedErrStat, false, 4);
            Log.Write(string.Empty);
            return aggregatedErrStat;
        }

        /// <summary>
        /// Performs diagnostic test of a model and all its sub-models on given data.
        /// </summary>
        /// <param name="resComp">Model to be tested.</param>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="verbose">Specifies whether to report progress.</param>
        /// <param name="detail">Specifies whether to report max available detail.</param>
        /// <returns>Diagnostics data of the model and all its sub-models.</returns>
        public List<ModelDiagnosticData> DiagnosticTest(ResComp resComp,
                                                        SampleDataset testingData,
                                                        bool verbose = true,
                                                        bool detail = false
                                                        )
        {
            if (verbose)
            {
                Log.Write($"Diagnostic test of the Reservoir Computer is running...");
            }
            List<ModelDiagnosticData> tasksDiagData = resComp.DiagnosticTest(testingData, verbose ? Handlers.OnResCompTestProgressChanged : null);
            if (verbose)
            {
                //Log.Write(string.Empty);
                for (int i = 0; i < tasksDiagData.Count; i++)
                {
                    //Log.Write($"Task ({model.Tasks[i].Name})");
                    Log.Write(tasksDiagData[i].GetInfoText(detail, 4));
                    Log.Write(string.Empty);
                }
                Log.Write(string.Empty);
            }
            return tasksDiagData;
        }






    }//EasyML


}//Namespace
