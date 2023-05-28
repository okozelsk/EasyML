using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MiscTools;
using EasyMLCore.MLP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Implements the Reservoir computer doing Regression, Categorical and Binary decision tasks on multivariate time series data.
    /// </summary>
    [Serializable]
    public class ResComp : SerializableObject, IComputable
    {
        //Constants
        /// <summary>
        /// Short identifier for context path.
        /// </summary>
        public const string ContextPathID = "RC";

        //Events
        /// <summary>
        /// This informative event occurs each time the progress takes a step forward.
        /// </summary>
        [field: NonSerialized]
        private event ProgressChangedHandler ProgressChanged;

        //Attribute properties
        /// <summary>
        /// Reservoir computer's configuration.
        /// </summary>
        public ResCompConfig ResCompCfg { get; }
        
        /// <summary>
        /// Reservoir.
        /// </summary>
        public Reservoir Res { get; }

        /// <summary>
        /// Reservoir computer's tasks.
        /// </summary>
        public List<ResCompTask> Tasks { get; }

        //Attributes
        private readonly int[][] _taskInputSectionIdxs;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="cfg">Reservoir computer's configuration.</param>
        private ResComp(ResCompConfig cfg)
        {
            ResCompCfg = (ResCompConfig)cfg.DeepClone();
            //Reservoir instance
            Res = new Reservoir(ResCompCfg.ReservoirCfg);
            Tasks = new List<ResCompTask>(ResCompCfg.TaskCfgCollection.Count);
            _taskInputSectionIdxs = new int[ResCompCfg.TaskCfgCollection.Count][];
            for(int i = 0; i < ResCompCfg.TaskCfgCollection.Count; i++)
            {
                _taskInputSectionIdxs[i] = new int[ResCompCfg.TaskCfgCollection[i].InputSectionsCfg.InputSectionCfgCollection.Count];
                for(int j = 0; j < _taskInputSectionIdxs[i].Length;  j++)
                {
                    _taskInputSectionIdxs[i][j] = (int)ResCompCfg.TaskCfgCollection[i].InputSectionsCfg.InputSectionCfgCollection[j].Name;
                }
                Array.Sort(_taskInputSectionIdxs[i]);
            }
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">Source instance.</param>
        public ResComp(ResComp source)
        {
            ResCompCfg = (ResCompConfig)source.ResCompCfg.DeepClone();
            Res = source.Res.DeepClone();
            Tasks = new List<ResCompTask>(source.Tasks.Count);
            foreach(ResCompTask task in source.Tasks)
            {
                Tasks.Add(task.DeepClone());
            }
            _taskInputSectionIdxs = new int[source._taskInputSectionIdxs.Length][];
            for(int i = 0; i < source._taskInputSectionIdxs.Length; i++)
            {
                _taskInputSectionIdxs[i] = (int[])source._taskInputSectionIdxs[i].Clone();
            }
            return;
        }

        //Properties
        /// <inheritdoc/>
        public int NumOfOutputFeatures
        {
            get
            {
                int num = 0;
                foreach (ResCompTaskConfig taskCfg in ResCompCfg.TaskCfgCollection)
                {
                    num += taskCfg.OutputFeaturesCfg.FeatureCfgCollection.Count;
                }
                return num;
            }
        }

        //Methods
        private void OnReservoirInitProgressChanged(ProgressInfoBase progressInfo)
        {
            progressInfo.ExtendContextPath(ContextPathID);
            ModelBuildProgressInfo trainProgressInfo =
                new ModelBuildProgressInfo(ContextPathID, progressInfo, null);
            ProgressChanged?.Invoke(trainProgressInfo);
            return;
        }

        private void OnModelBuildProgressChanged(ProgressInfoBase progressInfo)
        {
            progressInfo.ExtendContextPath(ContextPathID);
            ProgressChanged?.Invoke(progressInfo);
            return;
        }

        private void OnModelTestProgressChanged(ProgressInfoBase progressInfo)
        {
            progressInfo.ExtendContextPath(ContextPathID);
            ProgressChanged?.Invoke(progressInfo);
            return;
        }

        private double[] GetTaskInputVector(int taskIdx, List<Tuple<string, double[]>> resOutput)
        {
            List<double[]> taskInputVectorParts = new List<double[]>(_taskInputSectionIdxs[taskIdx].Length);
            for (int i = 0; i < _taskInputSectionIdxs[taskIdx].Length; i++)
            {
                taskInputVectorParts.Add(resOutput[_taskInputSectionIdxs[taskIdx][i]].Item2);
            }
            return taskInputVectorParts.Flattenize();
        }

        private double[] GetTaskOutputVector(int taskIdx, double[] flatOutVector)
        {
            int taskOutputFeaturesStartIdx = 0;
            for(int i = 0; i < taskIdx; i++)
            {
                taskOutputFeaturesStartIdx += ResCompCfg.TaskCfgCollection[i].OutputFeaturesCfg.FeatureCfgCollection.Count;
            }
            double[] taskOutputVector = flatOutVector.Extract(taskOutputFeaturesStartIdx, ResCompCfg.TaskCfgCollection[taskIdx].OutputFeaturesCfg.FeatureCfgCollection.Count);
            return taskOutputVector;
        }

        private SampleDataset GetTaskDataset(int taskIdx, SampleDataset allData, List<List<Tuple<string, double[]>>> bulkResOutSectionsData)
        {
            //Not all input samples are available for tasks
            int trainingDataStartIdx = allData.Count - bulkResOutSectionsData.Count;
            //Extract task inputs and outputs and prepare task-specific data
            SampleDataset taskDataset = new SampleDataset(bulkResOutSectionsData.Count);
            for (int sampleIdx = trainingDataStartIdx, resOutIdx = 0; sampleIdx < allData.SampleCollection.Count; sampleIdx++, resOutIdx++)
            {
                double[] taskInputVector = GetTaskInputVector(taskIdx, bulkResOutSectionsData[resOutIdx]);
                double[] taskOutputVector = GetTaskOutputVector(taskIdx, allData.SampleCollection[sampleIdx].OutputVector);
                taskDataset.AddSample(allData.SampleCollection[sampleIdx].ID,
                                      taskInputVector,
                                      taskOutputVector
                                      );
            }
            return taskDataset;
        }

        /// <summary>
        /// Builds the Reservoir Computer.
        /// </summary>
        /// <param name="cfg">Reservoir Computer configuration.</param>
        /// <param name="trainingData">Training data.</param>
        /// <param name="reservoirStat">Statistics of inner reservoir.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Built Reservoir Computer.</returns>
        public static ResComp Build(ResCompConfig cfg,
                                    SampleDataset trainingData,
                                    out ReservoirStat reservoirStat,
                                    ProgressChangedHandler progressInfoSubscriber = null
                                    )
        {
            ResComp resComp = new ResComp(cfg);
            reservoirStat = null;
            if (progressInfoSubscriber != null)
            {
                resComp.ProgressChanged += progressInfoSubscriber;
            }
            try
            {
                //Init reservoir and obtain inputs for build tasks' models
                resComp.Res.Init((from sample in trainingData.SampleCollection select sample.InputVector).ToList(),
                                  out List<List<Tuple<string, double[]>>> bulkResOutSectionsData,
                                  out reservoirStat,
                                  resComp.OnReservoirInitProgressChanged
                                  );
                //Build task's models
                for (int taskIdx = 0; taskIdx < cfg.TaskCfgCollection.Count; taskIdx++)
                {
                    //Extract task inputs and outputs and prepare task-specific data
                    SampleDataset taskDataset = resComp.GetTaskDataset(taskIdx, trainingData, bulkResOutSectionsData);
                    //Build task
                    resComp.Tasks.Add(ResCompTask.Build(cfg.TaskCfgCollection[taskIdx], taskDataset, resComp.OnModelBuildProgressChanged));
                }
                return resComp;
            }
            finally
            {
                if(progressInfoSubscriber != null)
                {
                    resComp.ProgressChanged -= progressInfoSubscriber;
                }
            }
        }

        /// <summary>
        /// Computes an output.
        /// </summary>
        /// <param name="input">Input vector.</param>
        /// <param name="detailedOutputs">The appropriate instances of task specific detailed output per task.</param>
        /// <returns>Computed tlat output vector.</returns>
        public double[] Compute(double[] input, out List<Tuple<string, TaskOutputDetailBase>> detailedOutputs)
        {
            Res.Compute(input, out List<Tuple<string, double[]>> outSectionsData);
            detailedOutputs = new List<Tuple<string, TaskOutputDetailBase>>(ResCompCfg.TaskCfgCollection.Count);
            List<double[]> taskFlatOutputs = new List<double[]>(ResCompCfg.TaskCfgCollection.Count);
            for(int i = 0; i < ResCompCfg.TaskCfgCollection.Count; i++)
            {
                double[] taskInput = GetTaskInputVector(i, outSectionsData);
                taskFlatOutputs.Add(Tasks[i].Compute(taskInput, out TaskOutputDetailBase taskOutDetail));
                detailedOutputs.Add(new Tuple<string, TaskOutputDetailBase>(ResCompCfg.TaskCfgCollection[i].Name, taskOutDetail));
            }
            return taskFlatOutputs.Flattenize();
        }

        /// <inheritdoc/>
        public double[] Compute(double[] input)
        {
            return Compute(input, out _);
        }

        /// <summary>
        /// Tests a reservoir computer.
        /// </summary>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="resultDataset">Result dataset containing triplets (input, computed, ideal).</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Resulting error stats for each inner ResCompTask.</returns>
        public List<MLPModelErrStat> Test(SampleDataset testingData,
                                          out ResultDataset resultDataset,
                                          ProgressChangedHandler progressInfoSubscriber = null
                                          )
        {
            if (progressInfoSubscriber != null)
            {
                ProgressChanged += progressInfoSubscriber;
            }
            try
            {
                //Prepare specific datasets for tasks
                List<SampleDataset> taskTestDatasets = new List<SampleDataset>(ResCompCfg.TaskCfgCollection.Count);
                for (int taskIdx = 0; taskIdx < ResCompCfg.TaskCfgCollection.Count; taskIdx++)
                {
                    taskTestDatasets.Add(new SampleDataset(testingData.Count));
                }
                int sampleIdx = 0;
                foreach (Sample sample in testingData.SampleCollection)
                {
                    double[] resFlatData = Res.Compute(sample.InputVector, out List<Tuple<string, double[]>> resOutSectionsData);
                    for (int taskIdx = 0; taskIdx < ResCompCfg.TaskCfgCollection.Count; taskIdx++)
                    {
                        double[] taskInputVector = GetTaskInputVector(taskIdx, resOutSectionsData);
                        double[] taskOutputVector = GetTaskOutputVector(taskIdx, sample.OutputVector);
                        taskTestDatasets[taskIdx].AddSample(sample.ID, taskInputVector, taskOutputVector);
                    }
                    ++sampleIdx;
                    ModelTestProgressInfo pinfo = new ModelTestProgressInfo(ContextPathID, sampleIdx, testingData.SampleCollection.Count);
                    ProgressChanged?.Invoke(pinfo);
                }
                //Test tasks
                List<MLPModelErrStat> taskErrStats = new List<MLPModelErrStat>(ResCompCfg.TaskCfgCollection.Count);
                List<ResultDataset> taskResultDatasets = new List<ResultDataset>(ResCompCfg.TaskCfgCollection.Count);
                for (int taskIdx = 0; taskIdx < ResCompCfg.TaskCfgCollection.Count; taskIdx++)
                {
                    taskErrStats.Add(Tasks[taskIdx].Test(taskTestDatasets[taskIdx], out ResultDataset taskResultDataset, OnModelTestProgressChanged));
                    taskResultDatasets.Add(taskResultDataset);
                }
                resultDataset = new ResultDataset(testingData.Count);
                for (int i = 0; i < testingData.Count; i++)
                {
                    List<double[]> tasksComputed = new List<double[]>(ResCompCfg.TaskCfgCollection.Count);
                    for (int taskIdx = 0; taskIdx < ResCompCfg.TaskCfgCollection.Count; taskIdx++)
                    {
                        tasksComputed.Add(taskResultDatasets[taskIdx].ComputedVectorCollection[i]);
                    }
                    double[] flatComputedVector = tasksComputed.Flattenize();
                    resultDataset.AddVectors(testingData.SampleCollection[i].InputVector,
                                            flatComputedVector,
                                            testingData.SampleCollection[i].OutputVector
                                            );
                }
                return taskErrStats;
            }
            finally
            {
                if (progressInfoSubscriber != null)
                {
                    ProgressChanged -= progressInfoSubscriber;
                }
            }
        }

        /// <summary>
        /// Performs diagnostic test of each RC task's model and all its inner sub-models.
        /// </summary>
        /// <remarks>
        /// Samples can be in any range. Data standardization is always performed internally.
        /// </remarks>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Resulting diagnostics data of each RC task's model and all its inner sub-models.</returns>
        public List<MLPModelDiagnosticData> DiagnosticTest(SampleDataset testingData,
                                                           ProgressChangedHandler progressInfoSubscriber = null
                                                           )
        {
            if (progressInfoSubscriber != null)
            {
                ProgressChanged += progressInfoSubscriber;
            }
            try
            {
                //Prepare specific datasets for tasks
                List<SampleDataset> taskTestDatasets = new List<SampleDataset>(ResCompCfg.TaskCfgCollection.Count);
                for (int taskIdx = 0; taskIdx < ResCompCfg.TaskCfgCollection.Count; taskIdx++)
                {
                    taskTestDatasets.Add(new SampleDataset(testingData.Count));
                }
                int sampleIdx = 0;
                foreach (Sample sample in testingData.SampleCollection)
                {
                    double[] resFlatData = Res.Compute(sample.InputVector, out List<Tuple<string, double[]>> resOutSectionsData);
                    for (int taskIdx = 0; taskIdx < ResCompCfg.TaskCfgCollection.Count; taskIdx++)
                    {
                        double[] taskInputVector = GetTaskInputVector(taskIdx, resOutSectionsData);
                        double[] taskOutputVector = GetTaskOutputVector(taskIdx, sample.OutputVector);
                        taskTestDatasets[taskIdx].AddSample(sample.ID, taskInputVector, taskOutputVector);
                    }
                    ++sampleIdx;
                    ModelTestProgressInfo pinfo = new ModelTestProgressInfo(ContextPathID, sampleIdx, testingData.SampleCollection.Count);
                    ProgressChanged?.Invoke(pinfo);
                }
                //Diagnostic tests
                List<MLPModelDiagnosticData> tasksDiagData = new List<MLPModelDiagnosticData>(ResCompCfg.TaskCfgCollection.Count);
                for (int taskIdx = 0; taskIdx < ResCompCfg.TaskCfgCollection.Count; taskIdx++)
                {
                    tasksDiagData.Add(Tasks[taskIdx].DiagnosticTest(taskTestDatasets[taskIdx], OnModelTestProgressChanged));
                }
                return tasksDiagData;
            }
            finally
            {
                if (progressInfoSubscriber != null)
                {
                    ProgressChanged -= progressInfoSubscriber;
                }
            }
        }

        /// <summary>
        /// Gets the appropriate instances of task specific detailed outputs.
        /// </summary>
        /// <param name="outputData">Computed or ideal data vector.</param>
        /// <returns>The list of appropriate instances of task specific detailed outputs.</returns>
        public List<Tuple<string, TaskOutputDetailBase>> GetOutputDetails(double[] outputData)
        {
            List<Tuple<string, TaskOutputDetailBase>> outputs = new List<Tuple<string, TaskOutputDetailBase>>(ResCompCfg.TaskCfgCollection.Count);
            int idx = 0;
            foreach(ResCompTask task in Tasks)
            {
                double[] taskOutput = outputData.Extract(idx, task.NumOfOutputFeatures);
                outputs.Add(new Tuple<string, TaskOutputDetailBase>(task.Name, task.GetOutputDetail(taskOutput)));
                idx += task.NumOfOutputFeatures;
            }
            return outputs;
        }


        /// <summary>
        /// Gets formatted informative text about this reservoir computer instance.
        /// </summary>
        /// <param name="detail">Specifies whether to include details about inner output tasks.</param>
        /// <param name="margin">Specifies left margin.</param>
        /// <returns>Formatted informative text about this reservoir computer instance.</returns>
        public string GetInfoText(bool detail = false, int margin = 0)
        {
            margin = Math.Max(margin, 0);
            StringBuilder sb = new StringBuilder($"Reservoir Computer:{Environment.NewLine}");
            sb.Append($"    Output features: {NumOfOutputFeatures.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"{Res.GetInfoText(detail, 4)}");
            sb.Append($"    Output tasks   : {Tasks.Count.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if (detail)
            {
                sb.Append($"    Output tasks one by one >>>{Environment.NewLine}");
                foreach (ResCompTask task in Tasks)
                {
                    sb.Append(task.GetInfoText(detail, 8));
                }
            }
            string infoText = sb.ToString();
            if (margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }

        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        public ResComp DeepClone()
        {
            return new ResComp(this);
        }

    }//ResComp
}//Namespace
