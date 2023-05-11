using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Base class for task output features error statistics.
    /// </summary>
    [Serializable]
    public abstract class TaskErrStatBase : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Names of output features in this statistics.
        /// </summary>
        public List<string> OutputFeatureNames { get; }


        //Constructors
        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="outputFeatureNames">Names of output features in this statistics.</param>
        protected TaskErrStatBase(IEnumerable<string> outputFeatureNames)
        {
            OutputFeatureNames = new List<string>(outputFeatureNames);
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        protected TaskErrStatBase(TaskErrStatBase source)
        {
            OutputFeatureNames = new List<string>(source.OutputFeatureNames);
            return;
        }

        //Properties
        /// <summary>
        /// Number of output features in this statistics.
        /// </summary>
        public int NumOfOutputFeatures { get { return OutputFeatureNames.Count; } }

        /// <summary>
        /// Gets number of samples.
        /// </summary>
        public virtual int NumOfSamples { get { throw new NotImplementedException(); } }

        //Static methods
        protected static double ComputeLogLoss(double computedValue, double idealValue)
        {
            if (idealValue >= Common.BinDecisionBorder)
            {
                return -Math.Log(computedValue.Bound(Common.Epsilon, 1d - Common.Epsilon));
            }
            else
            {
                return -Math.Log(1d - computedValue.Bound(Common.Epsilon, 1d - Common.Epsilon));
            }
        }

        //Methods
        /// <summary>
        /// Merges another statistics with this statistics.
        /// </summary>
        /// <param name="source">Another statistics.</param>
        public abstract void Merge(TaskErrStatBase source);

        /// <summary>
        /// Merges other statistics with this statistics.
        /// </summary>
        /// <param name="sources">Other statistics.</param>
        public void Merge(IEnumerable<TaskErrStatBase> sources)
        {
            foreach (TaskErrStatBase source in sources)
            {
                Merge(source);
            }
            return;
        }

        /// <summary>
        /// Updates statistics.
        /// </summary>
        /// <param name="computedValue">Computed value.</param>
        /// <param name="idealValue">Corresponding ideal value.</param>
        public abstract void Update(double computedValue, double idealValue);

        /// <summary>
        /// Updates the statistics.
        /// </summary>
        /// <param name="computedVector">The vector of computed values.</param>
        /// <param name="idealVector">The vector of corresponding ideal values.</param>
        public virtual void Update(double[] computedVector,
                                   double[] idealVector
                                   )
        {
            for(int i = 0; i < computedVector.Length; i++)
            {
                Update(computedVector[i], idealVector[i]);
            }
            return;
        }

        /// <summary>
        /// Updates the statistics.
        /// </summary>
        /// <param name="computedVectorCollection">The collection of computed vectors.</param>
        /// <param name="idealVectorCollection">The collection of ideal vectors.</param>
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
        /// Decides whether another error stat is better than this stat.
        /// </summary>
        /// <param name="other">Other stat to be compared with this statistics.</param>
        /// <returns>True if another error stat is better than this stat.</returns>
        public abstract bool IsBetter(TaskErrStatBase other);

        /// <summary>
        /// Creates the deep copy instance.
        /// </summary>
        public abstract TaskErrStatBase DeepClone();

        /// <summary>
        /// Gets text containing the results report.
        /// </summary>
        /// <param name="detail">Specifies whether to report details if available.</param>
        /// <param name="margin">Specifies left margin.</param>
        public abstract string GetReportText(bool detail = false, int margin = 0);


    }//TaskErrStatBase

}//Namespace
