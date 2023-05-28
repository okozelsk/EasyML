using EasyMLCore.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Encapsulates MLP model error statistics and provides user friendly access.
    /// </summary>
    [Serializable]
    public class MLPModelErrStat : SerializableObject
    {
        //Attribute properties
        /// <inheritdoc cref="OutputTaskType"/>
        public OutputTaskType TaskType { get; }

        /// <summary>
        /// Error stat data.
        /// </summary>
        public TaskErrStatBase StatData { get; }


        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="taskType">Type of model's computation task.</param>
        /// <param name="outputFeatureNames">Names of model task's output features.</param>
        public MLPModelErrStat(OutputTaskType taskType, IEnumerable<string> outputFeatureNames)
        {
            TaskType = taskType;
            StatData = TaskType switch
            {
                OutputTaskType.Categorical => new CategoricalErrStat(outputFeatureNames),
                OutputTaskType.Binary => new MultipleDecisionErrStat(outputFeatureNames),
                _ => new MultiplePrecisionErrStat(outputFeatureNames),
            };
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="computableUnit">A computable unit.</param>
        /// <param name="dataset">Sample dataset.</param>
        public MLPModelErrStat(IComputableTaskSpecific computableUnit, SampleDataset dataset)
            : this(computableUnit.TaskType, computableUnit.OutputFeatureNames)
        {
            for (int i = 0; i < dataset.Count; i++)
            {
                Update(computableUnit.Compute(dataset.SampleCollection[i].InputVector),
                       dataset.SampleCollection[i].OutputVector
                       );
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="taskType">Output computation task.</param>
        /// <param name="outputFeatureNames">Names of model task's output features.</param>
        /// <param name="dataset">Sample dataset.</param>
        /// <param name="computedVectorCollection">The collection of computed vectors.</param>
        public MLPModelErrStat(OutputTaskType taskType,
                            IEnumerable<string> outputFeatureNames,
                            SampleDataset dataset,
                            IEnumerable<double[]> computedVectorCollection
                            )
            : this(taskType, outputFeatureNames)
        {
            IEnumerator enumerator = computedVectorCollection.GetEnumerator();
            for (int i = 0; i < dataset.Count; i++)
            {
                enumerator.MoveNext();
                double[] computedVector = (double[])enumerator.Current;
                Update(computedVector, dataset.SampleCollection[i].OutputVector);
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="taskType">Output computation task.</param>
        /// <param name="outputFeatureNames">Names of model task's output features.</param>
        /// <param name="computedVectorCollection">The collection of computed vectors.</param>
        /// <param name="idealVectorCollection">The collection of ideal vectors.</param>
        public MLPModelErrStat(OutputTaskType taskType,
                            IEnumerable<string> outputFeatureNames,
                            IEnumerable<double[]> computedVectorCollection,
                            IEnumerable<double[]> idealVectorCollection
                            )
            : this(taskType, outputFeatureNames)
        {
            Update(computedVectorCollection, idealVectorCollection);
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public MLPModelErrStat(MLPModelErrStat source)
        {
            TaskType = source.TaskType;
            StatData = TaskType switch
            {
                OutputTaskType.Categorical => new CategoricalErrStat((CategoricalErrStat)source.StatData),
                OutputTaskType.Binary => new MultipleDecisionErrStat((MultipleDecisionErrStat)source.StatData),
                _ => new MultiplePrecisionErrStat((MultiplePrecisionErrStat)source.StatData),
            };
            return;
        }

        /// <summary>
        /// Merger constructor.
        /// </summary>
        /// <param name="taskType">Output computation task.</param>
        /// <param name="outputFeatureNames">Names of model task's output features.</param>
        /// <param name="sources">Source instances.</param>
        public MLPModelErrStat(OutputTaskType taskType, IEnumerable<string> outputFeatureNames, IEnumerable<MLPModelErrStat> sources)
            : this(taskType, outputFeatureNames)
        {
            Merge(sources);
            return;
        }

        //Properties
        /// <inheritdoc cref="TaskErrStatBase.NumOfOutputFeatures"/>
        public int NumOfOutputFeatures { get { return StatData.NumOfOutputFeatures; } }

        /// <inheritdoc cref="TaskErrStatBase.OutputFeatureNames"/>
        public List<string> OutputFeatureNames { get { return StatData.OutputFeatureNames; } }

        /// <inheritdoc cref="TaskErrStatBase.NumOfSamples"/>
        public int NumOfSamples { get { return StatData.NumOfSamples; } }

        //Methods
        /// <summary>
        /// Merges another statistics with this statistics.
        /// </summary>
        /// <param name="source">Another statistics.</param>
        public void Merge(MLPModelErrStat source)
        {
            if (TaskType != source.TaskType)
            {
                throw new InvalidOperationException("Source instance is incompatible (OutputTask differs).");
            }
            if (NumOfOutputFeatures != source.NumOfOutputFeatures)
            {
                throw new InvalidOperationException("Source instances is incompatible (NumOfOutputFeatures differs).");
            }
            StatData.Merge(source.StatData);
            return;
        }

        /// <summary>
        /// Merges collection of other statistics with this statistics.
        /// </summary>
        /// <param name="sources">Collection of other statistics.</param>
        public void Merge(IEnumerable<MLPModelErrStat> sources)
        {
            foreach (MLPModelErrStat source in sources)
            {
                Merge(source);
            }
            return;
        }

        /// <summary>
        /// Updates statistics.
        /// </summary>
        /// <param name="computedVector">Computed values.</param>
        /// <param name="idealVector">Ideal values.</param>
        public void Update(double[] computedVector, double[] idealVector)
        {
            StatData.Update(computedVector, idealVector);
            return;
        }

        /// <summary>
        /// Updates the statistics.
        /// </summary>
        /// <param name="computedVectorCollection">A collection of computed vectors.</param>
        /// <param name="idealVectorCollection">A collection of ideal vectors.</param>
        public void Update(IEnumerable<double[]> computedVectorCollection,
                           IEnumerable<double[]> idealVectorCollection
                           )
        {
            IEnumerator<double[]> idealVectorEnumerator = idealVectorCollection.GetEnumerator();
            foreach (double[] computedVector in computedVectorCollection)
            {
                idealVectorEnumerator.MoveNext();
                double[] idealVector = idealVectorEnumerator.Current;
                Update(computedVector, idealVector);
            }
            return;
        }

        /// <summary>
        /// Gets confidence indicator for each output feature, based on error statistics.
        /// </summary>
        /// <returns>Array of indicators between 0 and 1. Size of the array equals to number of output features.</returns>
        public double[] GetFeatureConfidences()
        {
            double[] confidences = new double[NumOfOutputFeatures];
            for(int i = 0; i < NumOfOutputFeatures; i++)
            {
                confidences[i] = TaskType switch
                {
                    OutputTaskType.Categorical => ((CategoricalErrStat)StatData).FeatureBinDecisionStats[i].FScore,
                    OutputTaskType.Binary => ((MultipleDecisionErrStat)StatData).FeatureBinDecisionStats[i].FScore,
                    _ => ((MultiplePrecisionErrStat)StatData).FeaturePrecisionStats[i].PScore,
                };
            }
            return confidences;
        }

        /// <inheritdoc cref="TaskErrStatBase.IsBetter(TaskErrStatBase)"/>
        public bool IsBetter(MLPModelErrStat otherStat)
        {
            return StatData.IsBetter(otherStat.StatData);
        }

        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        public MLPModelErrStat DeepClone()
        {
            return new MLPModelErrStat(this);
        }

        /// <summary>
        /// Gets text containing the results report.
        /// </summary>
        /// <param name="detail">Specifies whether to report details if available.</param>
        /// <param name="margin">Specifies left margin.</param>
        public string GetReportText(bool detail = false, int margin = 0)
        {
            return StatData.GetReportText(detail, margin);
        }

    }//MLPModelErrStat

}//Namespace
