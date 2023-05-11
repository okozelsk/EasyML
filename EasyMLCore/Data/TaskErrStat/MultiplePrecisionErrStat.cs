using EasyMLCore.MLP;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Text;
using EasyMLCore.Extensions;
using System.Globalization;

namespace EasyMLCore.Data
{
    [Serializable]
    public class MultiplePrecisionErrStat : TaskErrStatBase
    {
        //Attribute properties
        /// <summary>
        /// Holds the precision error statistics for each feature.
        /// </summary>
        public SinglePrecisionErrStat[] FeaturePrecisionStats { get; }

        /// <summary>
        /// The precision statistics. A pallet of statistics indicators about how close or distant are
        /// computed values and ideal values.
        /// </summary>
        public BasicStat TotalPrecisionStat { get; }


        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="outputFeatureNames">Names of output features in this statistics.</param>
        public MultiplePrecisionErrStat(IEnumerable<string> outputFeatureNames)
            :base(outputFeatureNames)
        {
            FeaturePrecisionStats = new SinglePrecisionErrStat[NumOfOutputFeatures];
            for(int i = 0; i < NumOfOutputFeatures; i++)
            {
                FeaturePrecisionStats[i] = new SinglePrecisionErrStat(OutputFeatureNames[i]);
            }
            TotalPrecisionStat = new BasicStat();
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="computableUnit">A computable unit.</param>
        /// <param name="dataset">Sample dataset.</param>
        public MultiplePrecisionErrStat(IComputableTaskSpecific computableUnit, SampleDataset dataset)
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
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public MultiplePrecisionErrStat(MultiplePrecisionErrStat source)
            : base(source)
        {
            FeaturePrecisionStats = new SinglePrecisionErrStat[NumOfOutputFeatures];
            for (int i = 0; i < NumOfOutputFeatures; i++)
            {
                FeaturePrecisionStats[i] = new SinglePrecisionErrStat(source.FeaturePrecisionStats[i]);
            }
            TotalPrecisionStat = new BasicStat(source.TotalPrecisionStat);
            return;
        }

        /// <summary>
        /// Merger constructor.
        /// </summary>
        /// <param name="outputFeatureNames">Names of output features in this statistics.</param>
        /// <param name="sources">Source instances to be merged together.</param>
        public MultiplePrecisionErrStat(IEnumerable<string> outputFeatureNames, IEnumerable<TaskErrStatBase> sources)
            : this(outputFeatureNames)
        {
            Merge(sources);
            return;
        }

        //Properties
        /// <summary>
        /// Mean Squared Error (numerical precision).
        /// </summary>
        public double MSE { get { return TotalPrecisionStat.MeanSquare; } }

        /// <inheritdoc/>
        public override int NumOfSamples { get { return TotalPrecisionStat.NumOfSamples / NumOfOutputFeatures; } }


        //Methods
        /// <summary>
        /// Merges another statistics with this statistics.
        /// </summary>
        /// <param name="source">Another statistics.</param>
        public override void Merge(TaskErrStatBase source)
        {
            MultiplePrecisionErrStat sourceStat = source as MultiplePrecisionErrStat;
            for(int i = 0; i < NumOfOutputFeatures; i++)
            {
                FeaturePrecisionStats[i].Merge(sourceStat.FeaturePrecisionStats[i]);
            }
            TotalPrecisionStat.Merge(sourceStat.TotalPrecisionStat);
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
            for(int i = 0; i < NumOfOutputFeatures; i++)
            {
                FeaturePrecisionStats[i].Update(computedVector[i], idealVector[i]);
                TotalPrecisionStat.AddSample(Math.Abs(idealVector[i] - computedVector[i]));
            }
            return;
        }

        /// <inheritdoc/>
        public override bool IsBetter(TaskErrStatBase other)
        {
            MultiplePrecisionErrStat otherStat = other as MultiplePrecisionErrStat;
            return otherStat.TotalPrecisionStat.RootMeanSquare < TotalPrecisionStat.RootMeanSquare;
        }

        /// <inheritdoc/>
        public override TaskErrStatBase DeepClone()
        {
            return new MultiplePrecisionErrStat(this);
        }

        /// <inheritdoc/>
        public override string GetReportText(bool detail = false, int margin = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Total RMSE: {TotalPrecisionStat.RootMeanSquare.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Samples   : {NumOfSamples.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"Features one by one >>>{Environment.NewLine}");
            for (int featureIdx = 0; featureIdx < NumOfOutputFeatures; featureIdx++)
            {
                sb.Append(FeaturePrecisionStats[featureIdx].GetReportText(detail, 4));
            }
            string report = sb.ToString();
            if (margin > 0)
            {
                report = report.Indent(margin);
            }
            return report;
        }

    }//PrecisionErrStat

}//Namespace
