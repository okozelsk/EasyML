using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MiscTools;
using EasyMLCore.MLP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EasyMLCore.TimeSeries
{
    //Delegates
    /// <summary>
    /// Delegate of the reservoir computer's build progress changed event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the reservoir computer's build process.</param>
    public delegate void ResCompBuildProgressChangedHandler(ResCompBuildProgressInfo progressInfo);

    /// <summary>
    /// Delegate of the reservoir computer test progress changed event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the reservoir computer test process.</param>
    public delegate void ResCompTestProgressChangedHandler(ResCompTestProgressInfo progressInfo);

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
        /// This informative event occurs each time the progress of the reservoir's build process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        private event ResCompBuildProgressChangedHandler BuildProgressChanged;

        /// <summary>
        /// This informative event occurs each time the progress of the reservoir computer
        /// test process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        private event ResCompTestProgressChangedHandler TestProgressChanged;

        //Attribute properties
        /// <summary>
        /// Reservoir.
        /// </summary>
        public Reservoir Res { get; }

        /// <summary>
        /// Reservoir computer's tasks.
        /// </summary>
        public List<ResCompTask> Tasks { get; }

        //Attributes
        private readonly ResCompConfig _cfg;
        private readonly int[][] _taskInputSectionIdxs;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="cfg">Reservoir computer's configuration.</param>
        private ResComp(ResCompConfig cfg)
        {
            _cfg = (ResCompConfig)cfg.DeepClone();
            //Reservoir instance
            Res = new Reservoir(_cfg.ReservoirCfg);
            Tasks = new List<ResCompTask>(_cfg.TaskCfgCollection.Count);
            _taskInputSectionIdxs = new int[_cfg.TaskCfgCollection.Count][];
            for(int i = 0; i < _cfg.TaskCfgCollection.Count; i++)
            {
                _taskInputSectionIdxs[i] = new int[_cfg.TaskCfgCollection[i].InputSectionsCfg.InputSectionCfgCollection.Count];
                for(int j = 0; j < _taskInputSectionIdxs[i].Length;  j++)
                {
                    _taskInputSectionIdxs[i][j] = (int)_cfg.TaskCfgCollection[i].InputSectionsCfg.InputSectionCfgCollection[j].Name;
                }
                Array.Sort(_taskInputSectionIdxs[i]);
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
                foreach (ResCompTaskConfig taskCfg in _cfg.TaskCfgCollection)
                {
                    num += taskCfg.OutputFeaturesCfg.FeatureCfgCollection.Count;
                }
                return num;
            }
        }

        //Methods
        private void OnReservoirInitProgressChanged(ReservoirInitProgressInfo progressInfo)
        {
            progressInfo.ExtendContextPath(ContextPathID);
            ResCompBuildProgressInfo trainProgressInfo =
                new ResCompBuildProgressInfo(progressInfo, null);
            BuildProgressChanged?.Invoke(trainProgressInfo);
            return;
        }

        private void OnModelBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            progressInfo.ExtendContextPath(ContextPathID);
            ResCompBuildProgressInfo trainingProgressInfo =
                new ResCompBuildProgressInfo(null, progressInfo);
            BuildProgressChanged?.Invoke(trainingProgressInfo);
            return;
        }

        private void OnModelTestProgressChanged(ModelTestProgressInfo progressInfo)
        {
            progressInfo.ExtendContextPath(ContextPathID);
            ResCompTestProgressInfo testingProgressInfo =
                new ResCompTestProgressInfo(null, progressInfo);
            TestProgressChanged?.Invoke(testingProgressInfo);
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
                taskOutputFeaturesStartIdx += _cfg.TaskCfgCollection[i].OutputFeaturesCfg.FeatureCfgCollection.Count;
            }
            double[] taskOutputVector = flatOutVector.Extract(taskOutputFeaturesStartIdx, _cfg.TaskCfgCollection[taskIdx].OutputFeaturesCfg.FeatureCfgCollection.Count);
            return taskOutputVector;
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
                                    ResCompBuildProgressChangedHandler progressInfoSubscriber = null
                                    )
        {
            ResComp resComp = new ResComp(cfg);
            reservoirStat = null;
            if (progressInfoSubscriber != null)
            {
                resComp.BuildProgressChanged += progressInfoSubscriber;
            }
            try
            {
                //Init reservoir and obtain inputs for build tasks' models
                resComp.Res.Init((from sample in trainingData.SampleCollection select sample.InputVector).ToList(),
                                  out List<List<Tuple<string, double[]>>> bulkResOutSectionsData,
                                  out reservoirStat,
                                  resComp.OnReservoirInitProgressChanged
                                  );
                //Not all input samples are available for tasks training
                int trainingDataStartIdx = trainingData.Count - bulkResOutSectionsData.Count;
                //Build task's models
                //_taskInputSectionIdxs
                int taskOutputFeaturesStartIdx = 0;
                for (int taskIdx = 0; taskIdx < cfg.TaskCfgCollection.Count; taskIdx++)
                {
                    //Extract task inputs and outputs and prepare task-specific data
                    SampleDataset taskDataset;
                    taskDataset = new SampleDataset(bulkResOutSectionsData.Count);
                    for (int sampleIdx = trainingDataStartIdx, resOutIdx = 0; sampleIdx < trainingData.SampleCollection.Count; sampleIdx++, resOutIdx++)
                    {
                        double[] taskInputVector = resComp.GetTaskInputVector(taskIdx, bulkResOutSectionsData[resOutIdx]);
                        double[] taskOutputVector = trainingData.SampleCollection[sampleIdx].OutputVector.Extract(taskOutputFeaturesStartIdx, cfg.TaskCfgCollection[taskIdx].OutputFeaturesCfg.FeatureCfgCollection.Count);
                        taskDataset.AddSample(trainingData.SampleCollection[sampleIdx].ID,
                                              taskInputVector,
                                              taskOutputVector
                                              );
                    }
                    //Build task
                    resComp.Tasks.Add(ResCompTask.Build(cfg.TaskCfgCollection[taskIdx], taskDataset, resComp.OnModelBuildProgressChanged));
                    taskOutputFeaturesStartIdx += cfg.TaskCfgCollection[taskIdx].OutputFeaturesCfg.FeatureCfgCollection.Count;
                }
                return resComp;
            }
            finally
            {
                if(progressInfoSubscriber != null)
                {
                    resComp.BuildProgressChanged -= progressInfoSubscriber;
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
            detailedOutputs = new List<Tuple<string, TaskOutputDetailBase>>(_cfg.TaskCfgCollection.Count);
            List<double[]> taskFlatOutputs = new List<double[]>(_cfg.TaskCfgCollection.Count);
            for(int i = 0; i < _cfg.TaskCfgCollection.Count; i++)
            {
                double[] taskInput = GetTaskInputVector(i, outSectionsData);
                taskFlatOutputs.Add(Tasks[i].Compute(taskInput, out TaskOutputDetailBase taskOutDetail));
                detailedOutputs.Add(new Tuple<string, TaskOutputDetailBase>(_cfg.TaskCfgCollection[i].Name, taskOutDetail));
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
        public List<ModelErrStat> Test(SampleDataset testingData,
                                       out ResultDataset resultDataset,
                                       ResCompTestProgressChangedHandler progressInfoSubscriber = null
                                       )
        {
            if (progressInfoSubscriber != null)
            {
                TestProgressChanged += progressInfoSubscriber;
            }
            try
            {
                //Prepare specific datasets for tasks
                List<SampleDataset> taskTestDatasets = new List<SampleDataset>(_cfg.TaskCfgCollection.Count);
                for (int taskIdx = 0; taskIdx < _cfg.TaskCfgCollection.Count; taskIdx++)
                {
                    taskTestDatasets.Add(new SampleDataset(testingData.Count));
                }
                int sampleIdx = 0;
                foreach (Sample sample in testingData.SampleCollection)
                {
                    double[] resFlatData = Res.Compute(sample.InputVector, out List<Tuple<string, double[]>> resOutSectionsData);
                    for (int taskIdx = 0; taskIdx < _cfg.TaskCfgCollection.Count; taskIdx++)
                    {
                        double[] taskInputVector = GetTaskInputVector(taskIdx, resOutSectionsData);
                        double[] taskOutputVector = GetTaskOutputVector(taskIdx, sample.OutputVector);
                        taskTestDatasets[taskIdx].AddSample(sample.ID, taskInputVector, taskOutputVector);
                    }
                    ++sampleIdx;
                    ResCompTestProgressInfo pinfo = new ResCompTestProgressInfo(new ProgressTracker((uint)testingData.SampleCollection.Count, (uint)sampleIdx), null);
                    TestProgressChanged?.Invoke(pinfo);
                }
                List<ModelErrStat> taskErrStats = new List<ModelErrStat>(_cfg.TaskCfgCollection.Count);
                List<ResultDataset> taskResultDatasets = new List<ResultDataset>(_cfg.TaskCfgCollection.Count);
                for (int taskIdx = 0; taskIdx < _cfg.TaskCfgCollection.Count; taskIdx++)
                {
                    taskErrStats.Add(Tasks[taskIdx].Test(taskTestDatasets[taskIdx], out ResultDataset taskResultDataset, OnModelTestProgressChanged));
                    taskResultDatasets.Add(taskResultDataset);
                }
                resultDataset = new ResultDataset(testingData.Count);
                for (int i = 0; i < testingData.Count; i++)
                {
                    List<double[]> tasksComputed = new List<double[]>(_cfg.TaskCfgCollection.Count);
                    for (int taskIdx = 0; taskIdx < _cfg.TaskCfgCollection.Count; taskIdx++)
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
                    TestProgressChanged -= progressInfoSubscriber;
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
            List<Tuple<string, TaskOutputDetailBase>> outputs = new List<Tuple<string, TaskOutputDetailBase>>(_cfg.TaskCfgCollection.Count);
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

    }//ResComp
}//Namespace
