﻿using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    //Delegates
    /// <summary>
    /// Delegate of the model's build progress changed event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the model's build process.</param>
    public delegate void ModelBuildProgressChangedHandler(ModelBuildProgressInfo progressInfo);

    /// <summary>
    /// Delegate of the model testing progress changed event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the computation process.</param>
    public delegate void ModelTestProgressChangedHandler(ModelTestProgressInfo progressInfo);

    /// <summary>
    /// Common base class of all models.
    /// </summary>
    [Serializable]
    public abstract class ModelBase : SerializableObject, IComputableTaskSpecific
    {
        //Static variables
        /// <summary>
        /// A number used to initialize pseudo random numbers.
        /// </summary>
        private static int RandomSeed = Common.DefaultRandomSeed;

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        protected static event ModelBuildProgressChangedHandler BuildProgressChanged;

        /// <summary>
        /// This informative event occurs each time the progress of the model
        /// testing process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        protected event ModelTestProgressChangedHandler ModelTestProgressChanged;

        //Attribute properties
        /// <summary>
        /// Model configuration.
        /// </summary>
        public IModelConfig ModelConfig { get; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc/>
        public OutputTaskType TaskType { get; }

        /// <inheritdoc/>
        public List<string> OutputFeatureNames { get; }

        /// <inheritdoc cref="ModelConfidenceMetrics"/>
        public ModelConfidenceMetrics ConfidenceMetrics { get; private set; }

        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="modelConfig">Model configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">A collection of output feature names.</param>
        protected ModelBase(IModelConfig modelConfig,
                            string name,
                            OutputTaskType taskType,
                            IEnumerable<string> outputFeatureNames
                            )
        {
            if (modelConfig == null)
            {
                throw new ArgumentNullException(nameof(modelConfig));
            }
            if (outputFeatureNames == null)
            {
                throw new ArgumentNullException(nameof(outputFeatureNames));
            }
            ModelConfig = (IModelConfig)modelConfig.DeepClone();
            Name = name;
            TaskType = taskType;
            OutputFeatureNames = new List<string>(outputFeatureNames);
            if (OutputFeatureNames.Count < 1)
            {
                throw new ArgumentException("Missing output feature names.", nameof(outputFeatureNames));
            }
            if (TaskType == OutputTaskType.Categorical && OutputFeatureNames.Count < 2)
            {
                throw new ArgumentException("Probably wrong task type or missing output features.", nameof(taskType));
            }
            ConfidenceMetrics = null;
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        protected ModelBase(ModelBase source)
        {
            ModelConfig = (IModelConfig)source.ModelConfig.DeepClone();
            Name = source.Name;
            TaskType = source.TaskType;
            OutputFeatureNames = new List<string>(source.OutputFeatureNames);
            ConfidenceMetrics = source.ConfidenceMetrics?.DeepClone();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public int NumOfOutputFeatures { get { return OutputFeatureNames.Count; } }

        //Static methods
        /// <summary>
        /// Changes a number used to initialize pseudo random numbers.
        /// </summary>
        /// <param name="seed">New seed value.</param>
        public static void SetRandomSeed(int seed)
        {
            RandomSeed = seed;
            return;
        }

        /// <summary>
        /// Gets a number to be used to initialize pseudo random numbers.
        /// </summary>
        /// <param name="seed">New seed value.</param>
        public static int GetRandomSeed()
        {
            return RandomSeed;
        }

        /// <summary>
        /// Invokes BuildProgressChanged.
        /// </summary>
        /// <param name="progressInfo">Progress info.</param>
        protected static void InvokeBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            BuildProgressChanged?.Invoke(progressInfo);
            return;
        }

        //Methods
        /// <summary>
        /// Gets sub-models confidence weights for each output feature.
        /// </summary>
        /// <param name="subModels">The collection of sub-models.</param>
        protected double[][] GetWeights(List<ModelBase> subModels)
        {
            int numOfSubModels = subModels.Count;
            double[][] weights = new double[NumOfOutputFeatures][];
            for (int featureIdx = 0; featureIdx < NumOfOutputFeatures; featureIdx++)
            {
                weights[featureIdx] = new double[numOfSubModels];
                for (int subModelIdx = 0; subModelIdx < numOfSubModels; subModelIdx++)
                {
                    weights[featureIdx][subModelIdx] = subModels[subModelIdx].ConfidenceMetrics.FeatureConfidences[featureIdx];
                }
            }
            return weights;
        }

        /// <summary>
        /// Computes an aggregated output.
        /// </summary>
        /// <param name="subModelsOutputs">The collection of sub-models outputs.</param>
        /// <param name="weights">The collection of sub-models weights specific for each output feature.</param>
        /// <returns>An aggregated output.</returns>
        protected double[] ComputeAggregation(List<double[]> subModelsOutputs, double[][] weights)
        {
            double[] output = new double[NumOfOutputFeatures];
            if (TaskType == OutputTaskType.Regression)
            {
                //Weighted average
                for (int outIdx = 0; outIdx < NumOfOutputFeatures; outIdx++)
                {
                    //Compute single output feature value as the weighted average from all members
                    WeightedAvg wAvg = new WeightedAvg();
                    for (int i = 0; i < subModelsOutputs.Count; i++)
                    {
                        //Add member's sub-result to weighted average
                        wAvg.AddSample(subModelsOutputs[i][outIdx], weights == null ? 1d : weights[outIdx][i]);
                    }
                    //Store averaged output
                    output[outIdx] = wAvg.Result;
                }
            }
            else
            {
                //Probability mixing
                for (int outIdx = 0; outIdx < NumOfOutputFeatures; outIdx++)
                {
                    double[] submodelWeights = (double[])weights[outIdx].Clone();
                    submodelWeights.ScaleToNewSum(1d);
                    double[] predictedProbabilities = new double[subModelsOutputs.Count];
                    for (int i = 0; i < subModelsOutputs.Count; i++)
                    {
                        predictedProbabilities[i] = subModelsOutputs[i][outIdx];
                    }
                    output[outIdx] = PMixer.MixP(predictedProbabilities, submodelWeights);
                }
            }
            if (TaskType == OutputTaskType.Categorical)
            {
                output.ScaleToNewSum(1d);
            }
            //Return outputs
            return output;
        }

        /// <summary>
        /// Finalizes the model.
        /// </summary>
        protected void FinalizeModel(ModelConfidenceMetrics confidenceMetrics)
        {
            //Confidence metrics
            ConfidenceMetrics = confidenceMetrics.DeepClone();
            return;
        }

        /// <inheritdoc/>
        public abstract double[] Compute(double[] input);

        /// <summary>
        /// Performs model test.
        /// </summary>
        /// <remarks>
        /// Samples can be in any range. Data standardization is always performed internally.
        /// </remarks>
        /// <param name="testingData">Testing samples.</param>
        /// <param name="resultDataset">Result dataset.</param>
        /// <param name="progressInfoSubscriber">Subscriber will receive notification event about progress. (Parameter can be null).</param>
        /// <returns>Resulting error statistics.</returns>
        public ModelErrStat Test(SampleDataset testingData,
                                 out ResultDataset resultDataset,
                                 ModelTestProgressChangedHandler progressInfoSubscriber = null
                                 )
        {
            if (progressInfoSubscriber != null)
            {
                ModelTestProgressChanged += progressInfoSubscriber;
            }
            List<double[]> computedVectorCollection = new List<double[]>(testingData.Count);
            int numOfProcessedSamples = 0;
            foreach (Sample sample in testingData.SampleCollection)
            {
                double[] computedVector = Compute(sample.InputVector);
                computedVectorCollection.Add(computedVector);
                ModelTestProgressChanged?.Invoke(new ModelTestProgressInfo(Name, ++numOfProcessedSamples, testingData.Count));
            }
            resultDataset = new ResultDataset(testingData, computedVectorCollection);
            return new ModelErrStat(TaskType, OutputFeatureNames, testingData, computedVectorCollection);
        }

        /// <inheritdoc/>
        public TaskOutputDetailBase GetOutputDetail(double[] outputData)
        {
            return TaskType switch
            {
                OutputTaskType.Regression => new RegressionOutputDetail(OutputFeatureNames, outputData),
                OutputTaskType.Binary => new BinaryOutputDetail(OutputFeatureNames, outputData),
                OutputTaskType.Categorical => new CategoricalOutputDetail(OutputFeatureNames, outputData),
                _ => null,
            };
        }

        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        public abstract ModelBase DeepClone();

        /// <summary>
        /// Gets formatted informative text about this model instance.
        /// </summary>
        /// <param name="detail">Specifies whether to report details if available.</param>
        /// <param name="margin">Specifies left margin.</param>
        /// <returns>Formatted informative text about this model instance.</returns>
        public abstract string GetInfoText(bool detail = false, int margin = 0);


    }//ModelBase

}//Namespace
