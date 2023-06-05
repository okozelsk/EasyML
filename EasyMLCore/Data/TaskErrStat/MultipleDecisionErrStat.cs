using EasyMLCore.MLP;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EasyMLCore.Extensions;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements an error statistics of the multiple binary decisions.
    /// </summary>
    [Serializable]
    public class MultipleDecisionErrStat : MultiplePrecisionErrStat
    {
        //Constants
        //Attribute properties
        /// <summary>
        /// Holds the precision error statistics for each feature.
        /// </summary>
        public SingleDecisionErrStat[] FeatureBinDecisionStats { get; }

        /// <summary>
        /// Total error statistics of wrong (false flag) decisions.
        /// </summary>
        /// <remarks>
        /// FalseFlagStat[0] contains error statistics about the "false" samples.
        /// FalseFlagStat[1] contains error statistics about the "true" samples.
        /// </remarks>
        public BasicStat[] TotalBinFalseFlagStat { get; }

        /// <summary>
        /// Total statistics of the wrong decisions.
        /// </summary>
        public BasicStat TotalBinWrongDecisionStat { get; }

        /// <summary>
        /// Total LogLoss statistics.
        /// </summary>
        public BasicStat TotalBinLogLossStat { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="outputFeatureNames">Names of output features in this statistics.</param>
        public MultipleDecisionErrStat(IEnumerable<string> outputFeatureNames)
            : base(outputFeatureNames)
        {
            FeatureBinDecisionStats = new SingleDecisionErrStat[NumOfOutputFeatures];
            for(int i = 0; i < NumOfOutputFeatures; i++)
            {
                FeatureBinDecisionStats[i] = new SingleDecisionErrStat(OutputFeatureNames[i]);
            }
            TotalBinFalseFlagStat = new BasicStat[2];
            TotalBinFalseFlagStat[0] = new BasicStat();
            TotalBinFalseFlagStat[1] = new BasicStat();
            TotalBinWrongDecisionStat = new BasicStat();
            TotalBinLogLossStat = new BasicStat();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="computableUnit">A computable unit.</param>
        /// <param name="dataset">Sample dataset.</param>
        public MultipleDecisionErrStat(IComputableTaskSpecific computableUnit, SampleDataset dataset)
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
        public MultipleDecisionErrStat(MultipleDecisionErrStat source)
            : base(source)
        {
            FeatureBinDecisionStats = new SingleDecisionErrStat[NumOfOutputFeatures];
            for (int i = 0; i < NumOfOutputFeatures; i++)
            {
                FeatureBinDecisionStats[i] = new SingleDecisionErrStat(source.FeatureBinDecisionStats[i]);
            }
            TotalBinFalseFlagStat = new BasicStat[2];
            TotalBinFalseFlagStat[0] = source.TotalBinFalseFlagStat[0].DeepClone();
            TotalBinFalseFlagStat[1] = source.TotalBinFalseFlagStat[1].DeepClone();
            TotalBinWrongDecisionStat = source.TotalBinWrongDecisionStat.DeepClone();
            TotalBinLogLossStat = source.TotalBinLogLossStat.DeepClone();
            return;
        }

        /// <summary>
        /// Merger constructor.
        /// </summary>
        /// <param name="outputFeatureNames">Names of output features in this statistics.</param>
        /// <param name="sources">Source instances to be merged together.</param>
        public MultipleDecisionErrStat(IEnumerable<string> outputFeatureNames, IEnumerable<TaskErrStatBase> sources)
            : this(outputFeatureNames)
        {
            Merge(sources);
            return;
        }

        //Properties
        /// <summary>
        /// Computed Cross-Entropy.
        /// </summary>
        public double TotalBinCrossEntropy { get { return TotalBinLogLossStat.ArithAvg; } }

        /// <summary>
        /// Binary accuracy.
        /// </summary>
        public double BinaryAccuracy { get { return (1d - TotalBinWrongDecisionStat.ArithAvg); } }

        //Instance methods
        /// <inheritdoc/>
        public override void Merge(TaskErrStatBase source)
        {
            base.Merge(source);
            MultipleDecisionErrStat sourceStat = source as MultipleDecisionErrStat;
            for (int i = 0; i < NumOfOutputFeatures; i++)
            {
                FeatureBinDecisionStats[i].Merge(sourceStat.FeatureBinDecisionStats[i]);
            }
            TotalBinFalseFlagStat[0].Merge(sourceStat.TotalBinFalseFlagStat[0]);
            TotalBinFalseFlagStat[1].Merge(sourceStat.TotalBinFalseFlagStat[1]);
            TotalBinWrongDecisionStat.Merge(sourceStat.TotalBinWrongDecisionStat);
            TotalBinLogLossStat.Merge(sourceStat.TotalBinLogLossStat);
            return;
        }

        /// <inheritdoc/>
        public override void Update(double computedValue, double idealValue)
        {
            throw new NotImplementedException("Update method with single double arguments is not relevant for multiple error statistics.");
        }

        /// <inheritdoc/>
        public override void Update(double[] computedVector, double[] idealVector)
        {
            base.Update(computedVector, idealVector);
            for (int i = 0; i < NumOfOutputFeatures; i++)
            {
                double computedValue = computedVector[i];
                double idealValue = idealVector[i];
                FeatureBinDecisionStats[i].Update(computedVector[i], idealVector[i]);
                int idealBinVal = (idealValue >= Common.BinDecisionBorder) ? 1 : 0;
                int errValue = Common.HaveSameBinaryMeaning(computedValue, idealValue) ? 0 : 1;
                TotalBinFalseFlagStat[idealBinVal].AddSample(errValue);
                TotalBinWrongDecisionStat.AddSample(errValue);
                TotalBinLogLossStat.AddSample(ComputeLogLoss(computedValue, idealValue));
            }
            return;
        }

        /// <inheritdoc/>
        public override bool IsBetter(TaskErrStatBase other)
        {
            MultipleDecisionErrStat otherStat = other as MultipleDecisionErrStat;
            if(otherStat.TotalBinWrongDecisionStat.Sum < TotalBinWrongDecisionStat.Sum)
            {
                return true;
            }
            else if (otherStat.TotalBinWrongDecisionStat.Sum == TotalBinWrongDecisionStat.Sum)
            {
                if(otherStat.TotalBinLogLossStat.RootMeanSquare < TotalBinLogLossStat.RootMeanSquare)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the deep copy instance.
        /// </summary>
        public override TaskErrStatBase DeepClone()
        {
            return new MultipleDecisionErrStat(this);
        }

        /// <inheritdoc/>
        public override string GetReportText(bool detail = false, int margin = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Total Binary Accuracy: {(BinaryAccuracy * 100d).ToString("F2", CultureInfo.InvariantCulture)}%{Environment.NewLine}");
            sb.Append($"Total Decision Errors: {TotalBinWrongDecisionStat.Sum.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Total Cross Entropy  : {TotalBinCrossEntropy.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Total RMSE           : {TotalPrecisionStat.RootMeanSquare.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Samples              : {NumOfSamples.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Binary features one by one >>>{Environment.NewLine}");
            for (int featureIdx = 0; featureIdx < NumOfOutputFeatures; featureIdx++)
            {
                sb.Append(FeatureBinDecisionStats[featureIdx].GetReportText(detail, 4));
            }
            string report = sb.ToString();
            if (margin > 0)
            {
                report = report.Indent(margin);
            }
            return report;
        }


    }//MultipleDecisionErrStat

}//Namespace
