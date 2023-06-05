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
    /// Implements an error statistics of the single feature binary decisions.
    /// </summary>
    [Serializable]
    public class SingleDecisionErrStat : SinglePrecisionErrStat
    {
        //Constants
        /// <summary>
        /// F-score real factor, which means that recall is considered beta times as important as precision.
        /// </summary>
        public const double Beta = 0.5d;

        //Attribute properties
        /// <summary>
        /// Simple samples statistics of ideal samples where Sum tells number of positive ideal samples.
        /// </summary>
        public BasicStat IdealStat { get; }

        /// <summary>
        /// Error statistics of wrong (false flag) decisions.
        /// </summary>
        /// <remarks>
        /// FalseFlagStat[0] contains error statistics where ideal=0 and predicted=1.
        /// FalseFlagStat[1] contains error statistics where ideal=1 and predicted=0.
        /// </remarks>
        public BasicStat[] FalseFlagStat { get; }

        /// <summary>
        /// Total statistics of the wrong decisions.
        /// </summary>
        public BasicStat WrongDecisionStat { get; }

        /// <summary>
        /// LogLoss statistics.
        /// </summary>
        public BasicStat LogLossStat { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="outputFeatureName">Name of output feature name.</param>
        public SingleDecisionErrStat(string outputFeatureName)
            : base(outputFeatureName)
        {
            IdealStat = new BasicStat();
            FalseFlagStat = new BasicStat[2];
            FalseFlagStat[0] = new BasicStat();
            FalseFlagStat[1] = new BasicStat();
            WrongDecisionStat = new BasicStat();
            LogLossStat = new BasicStat();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="computableUnit">A computable unit.</param>
        /// <param name="dataset">Sample dataset.</param>
        public SingleDecisionErrStat(IComputableTaskSpecific computableUnit, SampleDataset dataset)
            : this(computableUnit.OutputFeatureNames[0])
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
        public SingleDecisionErrStat(SingleDecisionErrStat source)
            : base(source)
        {
            IdealStat = source.IdealStat.DeepClone();
            FalseFlagStat = new BasicStat[2];
            FalseFlagStat[0] = source.FalseFlagStat[0].DeepClone();
            FalseFlagStat[1] = source.FalseFlagStat[1].DeepClone();
            WrongDecisionStat = source.WrongDecisionStat.DeepClone();
            LogLossStat = source.LogLossStat.DeepClone();
            return;
        }

        /// <summary>
        /// Merger constructor.
        /// </summary>
        /// <param name="outputFeatureName">Name of output feature name.</param>
        /// <param name="sources">Source instances to be merged together.</param>
        public SingleDecisionErrStat(string outputFeatureName, IEnumerable<TaskErrStatBase> sources)
            : this(outputFeatureName)
        {
            Merge(sources);
            return;
        }

        //Properties
        /// <summary>
        /// Computed Cross-Entropy.
        /// </summary>
        public double CrossEntropy { get { return LogLossStat.ArithAvg; } }

        /// <summary>
        /// Binary accuracy.
        /// </summary>
        public double BinaryAccuracy
        {
            get
            {
                return (1d - WrongDecisionStat.ArithAvg);
            }
        }

        /// <summary>
        /// Gets the F-Score.
        /// </summary>
        public double FScore
        {
            get
            {
                double sqBeta = Beta * Beta;
                double truePositive = IdealStat.Sum - FalseFlagStat[1].Sum;
                double falsePositive = FalseFlagStat[0].Sum;
                double falseNegative = FalseFlagStat[1].Sum;
                double precision = truePositive / (Common.Epsilon + truePositive + falsePositive);
                double recall = truePositive / (Common.Epsilon + truePositive + falseNegative);
                double score = (1d + sqBeta) * ((precision * recall) / ((sqBeta * precision) + recall + Common.Epsilon));
                return score;
            }
        }

    //Instance methods
    /// <inheritdoc/>
    public override void Merge(TaskErrStatBase source)
        {
            base.Merge(source);
            SingleDecisionErrStat sourceStat = source as SingleDecisionErrStat;
            IdealStat.Merge(sourceStat.IdealStat);
            FalseFlagStat[0].Merge(sourceStat.FalseFlagStat[0]);
            FalseFlagStat[1].Merge(sourceStat.FalseFlagStat[1]);
            WrongDecisionStat.Merge(sourceStat.WrongDecisionStat);
            LogLossStat.Merge(sourceStat.LogLossStat);
            return;
        }

        /// <inheritdoc/>
        public override void Update(double computedValue, double idealValue)
        {
            base.Update(computedValue, idealValue);
            int idealBinVal = (idealValue >= Common.BinDecisionBorder) ? 1 : 0;
            int errValue = Common.HaveSameBinaryMeaning(computedValue, idealValue) ? 0 : 1;
            IdealStat.AddSample(idealBinVal);
            FalseFlagStat[idealBinVal].AddSample(errValue);
            WrongDecisionStat.AddSample(errValue);
            LogLossStat.AddSample(ComputeLogLoss(computedValue, idealValue));
            return;
        }

        /// <inheritdoc/>
        public override bool IsBetter(TaskErrStatBase other)
        {
            MultipleDecisionErrStat otherStat = other as MultipleDecisionErrStat;
            if(otherStat.TotalBinWrongDecisionStat.Sum < WrongDecisionStat.Sum)
            {
                return true;
            }
            else if (otherStat.TotalBinWrongDecisionStat.Sum == WrongDecisionStat.Sum)
            {
                if(otherStat.TotalBinLogLossStat.RootMeanSquare < LogLossStat.RootMeanSquare)
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
            return new SingleDecisionErrStat(this);
        }

        /// <inheritdoc/>
        public override string GetReportText(bool detail = false, int margin = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{OutputFeatureName}]{Environment.NewLine}");
            sb.Append($"    Binary Accuracy: {(BinaryAccuracy * 100d).ToString("F2", CultureInfo.InvariantCulture)}%{Environment.NewLine}");
            sb.Append($"    Total Errors   : {WrongDecisionStat.Sum.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    False Positive : {FalseFlagStat[0].Sum.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    False Negative : {FalseFlagStat[1].Sum.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    FScore         : {FScore.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Cross Entropy  : {CrossEntropy.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    RMSE           : {FeaturePrecisionStat.RootMeanSquare.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Samples        : {NumOfSamples.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Ideal Positives: {IdealStat.Sum.ToString("F0", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Ideal Negatives: {(NumOfSamples - IdealStat.Sum).ToString("F0", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            string report = sb.ToString();
            if (margin > 0)
            {
                report = report.Indent(margin);
            }
            return report;
        }


    }//SingleBinDecisionErrStat

}//Namespace
