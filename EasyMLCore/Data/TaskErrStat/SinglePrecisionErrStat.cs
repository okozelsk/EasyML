using EasyMLCore.MLP;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Text;
using EasyMLCore.Extensions;
using System.Globalization;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Holds precision error statistics for a single feature.
    /// </summary>
    [Serializable]
    public class SinglePrecisionErrStat : TaskErrStatBase
    {
        //Attribute properties
        /// <summary>
        /// The precision statistics. A pallet of statistics indicators about how close or distant are
        /// computed values and ideal values.
        /// </summary>
        public BasicStat FeaturePrecisionStat { get; }


        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="outputFeatureName">Name of output feature name.</param>
        public SinglePrecisionErrStat(string outputFeatureName)
            :base(new List<string>() { outputFeatureName })
        {
            FeaturePrecisionStat = new BasicStat();
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="computableUnit">A computable unit.</param>
        /// <param name="dataset">Sample dataset.</param>
        public SinglePrecisionErrStat(IComputableTaskSpecific computableUnit, SampleDataset dataset)
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
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SinglePrecisionErrStat(SinglePrecisionErrStat source)
            : base(source)
        {
            FeaturePrecisionStat = new BasicStat(source.FeaturePrecisionStat);
            return;
        }

        /// <summary>
        /// Merger constructor.
        /// </summary>
        /// <param name="outputFeatureName">Name of output feature name.</param>
        /// <param name="sources">Source instances to be merged together.</param>
        public SinglePrecisionErrStat(string outputFeatureName, IEnumerable<TaskErrStatBase> sources)
            : this(outputFeatureName)
        {
            Merge(sources);
            return;
        }

        //Properties
        /// <summary>
        /// Mean Squared Error (numerical precision).
        /// </summary>
        public double MSE { get { return FeaturePrecisionStat.MeanSquare; } }

        /// <summary>
        /// Gets the precision score.
        /// </summary>
        public double PScore
        {
            get
            {
                return 1d / (Common.Epsilon + FeaturePrecisionStat.RootMeanSquare);

            }
        }

        /// <inheritdoc/>
        public override int NumOfSamples { get { return FeaturePrecisionStat.NumOfSamples; } }

        /// <summary>
        /// Gets name of the output feature.
        /// </summary>
        public string OutputFeatureName { get { return OutputFeatureNames[0]; } }

        //Methods
        /// <summary>
        /// Merges another statistics with this statistics.
        /// </summary>
        /// <param name="source">Another statistics.</param>
        public override void Merge(TaskErrStatBase source)
        {
            SinglePrecisionErrStat sourceStat = source as SinglePrecisionErrStat;
            FeaturePrecisionStat.Merge(sourceStat.FeaturePrecisionStat);
            return;
        }

        /// <inheritdoc/>
        public override void Update(double computedValue, double idealValue)
        {
            FeaturePrecisionStat.AddSample(Math.Abs(idealValue - computedValue));
            return;
        }

        /// <inheritdoc/>
        public override bool IsBetter(TaskErrStatBase other)
        {
            SinglePrecisionErrStat otherStat = other as SinglePrecisionErrStat;
            return otherStat.FeaturePrecisionStat.RootMeanSquare < FeaturePrecisionStat.RootMeanSquare;
        }

        /// <inheritdoc/>
        public override TaskErrStatBase DeepClone()
        {
            return new SinglePrecisionErrStat(this);
        }

        /// <inheritdoc/>
        public override string GetReportText(bool detail = false, int margin = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{OutputFeatureName}]{Environment.NewLine}");
            sb.Append($"    MinError: {FeaturePrecisionStat.Min.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    MaxError: {FeaturePrecisionStat.Max.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    PScore  : {PScore.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    RMSE    : {FeaturePrecisionStat.RootMeanSquare.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Samples : {NumOfSamples.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            string report = sb.ToString();
            if(margin > 0)
            {
                report = report.Indent(margin);
            }
            return report;
        }

    }//SinglePrecisionErrStat

}//Namespace
