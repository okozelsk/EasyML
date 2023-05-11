using EasyMLCore.Extensions;
using EasyMLCore.MLP;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements an error statistics of the categorical decisions (multiple features - classes).
    /// </summary>
    [Serializable]
    public class CategoricalErrStat : MultipleDecisionErrStat
    {
        //Attribute properties
        /// <summary>
        /// LogLoss statistics.
        /// </summary>
        public BasicStat ClassificationLogLossStat { get; }

        /// <summary>
        /// Classification error statistics.
        /// </summary>
        public BasicStat WrongClassificationStat { get; }

        /// <summary>
        /// Statistics of the right classifications but with low probability.
        /// </summary>
        public BasicStat LowProbabilityClassificationStat { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="outputFeatureNames">Names of output features in this statistics.</param>
        public CategoricalErrStat(IEnumerable<string> outputFeatureNames)
            : base(outputFeatureNames)
        {
            ClassificationLogLossStat = new BasicStat();
            WrongClassificationStat = new BasicStat();
            LowProbabilityClassificationStat = new BasicStat();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="computableUnit">A computable unit.</param>
        /// <param name="dataset">Sample dataset.</param>
        public CategoricalErrStat(IComputableTaskSpecific computableUnit, SampleDataset dataset)
            : this(computableUnit.OutputFeatureNames)
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CategoricalErrStat(CategoricalErrStat source)
            : base(source)
        {
            ClassificationLogLossStat = new BasicStat(source.ClassificationLogLossStat);
            WrongClassificationStat = new BasicStat(source.WrongClassificationStat);
            LowProbabilityClassificationStat = new BasicStat(source.LowProbabilityClassificationStat);
            return;
        }

        /// <summary>
        /// Merger constructor.
        /// </summary>
        /// <param name="outputFeatureNames">Names of output features in this statistics.</param>
        /// <param name="sources">Source instances to be merged together.</param>
        public CategoricalErrStat(IEnumerable<string> outputFeatureNames, IEnumerable<TaskErrStatBase> sources)
            : this(outputFeatureNames)
        {
            Merge(sources);
            return;
        }

        //Properties
        /// <summary>
        /// Binary accuracy.
        /// </summary>
        public double ClassificationAccuracy { get { return (1d - WrongClassificationStat.ArithAvg); } }

        /// <summary>
        /// Gets total number of inadequate classifications (wrong plus right having low-probability)
        /// </summary>
        public int TotalNumOfInadequateClassifications { get { return (int)(WrongClassificationStat.Sum + LowProbabilityClassificationStat.Sum); } }
        
        /// <summary>
        /// Computed Cross-Entropy.
        /// </summary>
        public double ClassificationCrossEntropy { get { return ClassificationLogLossStat.ArithAvg; } }

        //Methods
        /// <inheritdoc/>
        public override void Merge(TaskErrStatBase source)
        {
            base.Merge(source);
            CategoricalErrStat sourceStat = source as CategoricalErrStat;
            ClassificationLogLossStat.Merge(sourceStat.ClassificationLogLossStat);
            WrongClassificationStat.Merge(sourceStat.WrongClassificationStat);
            LowProbabilityClassificationStat.Merge(sourceStat.LowProbabilityClassificationStat);
            return;
        }

        /// <inheritdoc/>
        public override void Update(double computedValue, double idealValue)
        {
            throw new NotImplementedException("Update method with single double arguments is not relevant for categorical (classification) error statistics.");
        }

        /// <inheritdoc/>
        public override void Update(double[] computedVector, double[] idealVector)
        {
            base.Update(computedVector, idealVector);
            //Update LogLoss
            for (int i = 0; i < NumOfOutputFeatures; i++)
            {
                if (idealVector[i] >= Common.BinDecisionBorder)
                {
                    ClassificationLogLossStat.AddSample(ComputeLogLoss(computedVector[i], idealVector[i]));
                }
            }
            //Update categorical summary statistics
            int computedMaxIdx = computedVector.IndexOfMax(out int count);
            int idealMaxIdx = idealVector.IndexOfMax(out _);
            if (computedMaxIdx != idealMaxIdx || (computedMaxIdx == idealMaxIdx && count > 1))
            {
                //Wrong classification
                WrongClassificationStat.AddSample(1d);
            }
            else
            {
                //Right classification
                WrongClassificationStat.AddSample(0d);
                if (computedVector[computedMaxIdx] < Common.BinDecisionBorder)
                {
                    //Low probability classification
                    LowProbabilityClassificationStat.AddSample(1d);
                }
                else
                {
                    //Enaugh probability classification
                    LowProbabilityClassificationStat.AddSample(0d);
                }

            }
            return;
        }

        /// <inheritdoc/>
        public override bool IsBetter(TaskErrStatBase other)
        {
            CategoricalErrStat otherStat = other as CategoricalErrStat;
            if (otherStat.WrongClassificationStat.Sum < WrongClassificationStat.Sum)
            {
                return true;
            }
            else if (otherStat.WrongClassificationStat.Sum == WrongClassificationStat.Sum)
            {
                if(otherStat.LowProbabilityClassificationStat.Sum < LowProbabilityClassificationStat.Sum)
                {
                    return true;
                }
                else if (otherStat.LowProbabilityClassificationStat.Sum == LowProbabilityClassificationStat.Sum)
                {
                    if (otherStat.ClassificationLogLossStat.RootMeanSquare < ClassificationLogLossStat.RootMeanSquare)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the deep copy instance.
        /// </summary>
        public override TaskErrStatBase DeepClone()
        {
            return new CategoricalErrStat(this);
        }

        /// <inheritdoc/>
        public override string GetReportText(bool detail = false, int margin = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Categorical Accuracy : {(ClassificationAccuracy * 100d).ToString("F2", CultureInfo.InvariantCulture)}%{Environment.NewLine}");
            sb.Append($"Categorical Errors   : {WrongClassificationStat.Sum.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Low Probabilities    : {LowProbabilityClassificationStat.Sum.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Cross Entropy        : {ClassificationCrossEntropy.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Total RMSE           : {TotalPrecisionStat.RootMeanSquare.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Samples              : {NumOfSamples.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if (detail)
            {
                sb.Append($"Class labels one by one >>>{Environment.NewLine}");
                for (int classIdx = 0; classIdx < NumOfOutputFeatures; classIdx++)
                {
                    sb.Append(FeatureBinDecisionStats[classIdx].GetReportText(detail, 4));
                }
            }
            string report = sb.ToString();
            if (margin > 0)
            {
                report = report.Indent(margin);
            }
            return report;
        }



    }//CategoricalErrStat

}//Namespace

