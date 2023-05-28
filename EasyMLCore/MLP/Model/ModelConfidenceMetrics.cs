using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Holds confidence metrics related to a model.
    /// </summary>
    [Serializable]
    public class ModelConfidenceMetrics : SerializableObject
    {
        //Constants
        /// <summary>
        /// If only training err stat is present this penalty is used to scale features confidences of such model.
        /// Aim is to favorize validated models during the bagging of models.
        /// </summary>
        private const double MissingValidationErrStatFConfidencesPenalty = 0.05d;

        /// <summary>
        /// Bellow 1d it favorizes training stat results over validation stat resusults.
        /// Above 1d it favorizes validation stat results over training stat resusults.
        /// </summary>
        private const double TrainingToValidationSamplesRatioCoeff = 1d;

        //Attribute properties
        /// <inheritdoc cref="OutputTaskType"/>
        public OutputTaskType TaskType { get; }

        /// <summary>
        /// Averaged expected cost.
        /// </summary>
        public double CostIndicator { get; }

        /// <summary>
        /// Averaged expected categoriocal accuracy.
        /// </summary>
        public double CategoricalAccuracy { get; }

        /// <summary>
        /// Averaged expected binary accuracy.
        /// </summary>
        public double BinaryAccuracy { get; }

        /// <summary>
        /// Contains confidence indicators of output features.
        /// </summary>
        public double[] FeatureConfidences { get; }

        /// <summary>
        /// Basic stat over confidence indicators of output features.
        /// </summary>
        public BasicStat FeatureConfidencesStat { get; }

        //Constructors
        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="taskType">Computation task type.</param>
        /// <param name="costIndicator">Indicates an average cost indicator.</param>
        /// <param name="categoricalAccuracy">Averaged expected categoriocal accuracy.</param>
        /// <param name="binaryAccuracy">Averaged expected binary accuracy.</param>
        /// <param name="featureConfidences">Contains confidence indicators of output features.</param>
        /// <param name="featureConfidencesStat">Basic stat over confidence indicators of output features.</param>
        public ModelConfidenceMetrics(OutputTaskType taskType,
                                      double costIndicator,
                                      double categoricalAccuracy,
                                      double binaryAccuracy,
                                      double[] featureConfidences,
                                      BasicStat featureConfidencesStat
                                      )
        {
            TaskType = taskType;
            CostIndicator = costIndicator;
            CategoricalAccuracy = categoricalAccuracy;
            BinaryAccuracy = binaryAccuracy;
            FeatureConfidences = (double[])featureConfidences.Clone();
            FeatureConfidencesStat = new BasicStat(featureConfidencesStat);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="trainingErrorStat">Training error statistics.</param>
        /// <param name="validationErrorStat">Validation error statistics.</param>
        public ModelConfidenceMetrics(ModelErrStat trainingErrorStat, ModelErrStat validationErrorStat = null)
        {
            TaskType = trainingErrorStat.TaskType;
            if (validationErrorStat == null)
            {
                if (TaskType == OutputTaskType.Categorical)
                {
                    CostIndicator = ((CategoricalErrStat)trainingErrorStat.StatData).ClassificationLogLossStat.RootMeanSquare;
                    CategoricalAccuracy = ((CategoricalErrStat)trainingErrorStat.StatData).ClassificationAccuracy;
                    BinaryAccuracy = ((CategoricalErrStat)trainingErrorStat.StatData).BinaryAccuracy;
                }
                else if (TaskType == OutputTaskType.Binary)
                {
                    CostIndicator = ((MultipleDecisionErrStat)trainingErrorStat.StatData).TotalBinLogLossStat.RootMeanSquare;
                    CategoricalAccuracy = 0d;
                    BinaryAccuracy = ((MultipleDecisionErrStat)trainingErrorStat.StatData).BinaryAccuracy;
                }
                else
                {
                    CostIndicator = ((MultiplePrecisionErrStat)trainingErrorStat.StatData).TotalPrecisionStat.RootMeanSquare;
                    CategoricalAccuracy = 0d;
                    BinaryAccuracy = 0d;
                }
                //Feature confidences
                FeatureConfidences = trainingErrorStat.GetFeatureConfidences();
                //Apply "missing test statistics" penalty
                FeatureConfidences.Scale(1d - MissingValidationErrStatFConfidencesPenalty);
            }
            else
            {
                double validationErrStatWeight = TrainingToValidationSamplesRatioCoeff * ((double)trainingErrorStat.NumOfSamples / (double)validationErrorStat.NumOfSamples);
                double avgDenominator = trainingErrorStat.NumOfSamples + (double)validationErrorStat.NumOfSamples * validationErrStatWeight;
                if (TaskType == OutputTaskType.Categorical)
                {
                    CategoricalErrStat trainingStat = trainingErrorStat.StatData as CategoricalErrStat;
                    CategoricalErrStat validationStat = validationErrorStat.StatData as CategoricalErrStat;
                    CostIndicator = Math.Sqrt((trainingStat.ClassificationLogLossStat.SumOfSquares + validationErrStatWeight * validationStat.ClassificationLogLossStat.SumOfSquares) / avgDenominator);
                    CategoricalAccuracy = 1d - (trainingStat.WrongClassificationStat.Sum + validationErrStatWeight * validationStat.WrongClassificationStat.Sum) / avgDenominator;
                    BinaryAccuracy = 1d - (trainingStat.TotalBinWrongDecisionStat.Sum + validationErrStatWeight * validationStat.TotalBinWrongDecisionStat.Sum) / avgDenominator;
                }
                else if (TaskType == OutputTaskType.Binary)
                {
                    MultipleDecisionErrStat trainingStat = trainingErrorStat.StatData as MultipleDecisionErrStat;
                    MultipleDecisionErrStat validationStat = validationErrorStat.StatData as MultipleDecisionErrStat;
                    CostIndicator = Math.Sqrt((trainingStat.TotalBinLogLossStat.SumOfSquares + validationErrStatWeight * validationStat.TotalBinLogLossStat.SumOfSquares) / avgDenominator);
                    CategoricalAccuracy = 0d;
                    BinaryAccuracy = 1d - (trainingStat.TotalBinWrongDecisionStat.Sum + validationErrStatWeight * validationStat.TotalBinWrongDecisionStat.Sum) / avgDenominator;
                }
                else
                {
                    MultiplePrecisionErrStat trainingStat = trainingErrorStat.StatData as MultiplePrecisionErrStat;
                    MultiplePrecisionErrStat validationStat = validationErrorStat.StatData as MultiplePrecisionErrStat;
                    CostIndicator = Math.Sqrt((trainingStat.TotalPrecisionStat.SumOfSquares + validationErrStatWeight * validationStat.TotalPrecisionStat.SumOfSquares) / avgDenominator);
                    CategoricalAccuracy = 0d;
                    BinaryAccuracy = 0d;
                }
                //Feature confidences
                double[] trainingConfidences = trainingErrorStat.GetFeatureConfidences();
                double[] validationConfidences = validationErrorStat.GetFeatureConfidences();
                FeatureConfidences = new double[trainingConfidences.Length];
                for (int i = 0; i < trainingConfidences.Length; i++)
                {
                    FeatureConfidences[i] = (trainingConfidences[i] + validationErrStatWeight * validationConfidences[i]) / (1d + validationErrStatWeight);
                }
            }
            FeatureConfidencesStat = new BasicStat(FeatureConfidences);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="taskType">Output task type.</param>
        /// <param name="confidenceMetricsCollection">Collection of confidence metrics.</param>
        public ModelConfidenceMetrics(OutputTaskType taskType, IEnumerable<ModelConfidenceMetrics> confidenceMetricsCollection)
        {
            TaskType = taskType;
            List<double[]> subConfidencies = new List<double[]>();
            WeightedAvg costAvg = new WeightedAvg();
            WeightedAvg catAccAvg = new WeightedAvg();
            WeightedAvg binAccAvg = new WeightedAvg();
            foreach(ModelConfidenceMetrics subMetrics in confidenceMetricsCollection)
            {
                costAvg.AddSample(subMetrics.CostIndicator);
                catAccAvg.AddSample(subMetrics.CategoricalAccuracy);
                binAccAvg.AddSample(subMetrics.BinaryAccuracy);
                subConfidencies.Add(subMetrics.FeatureConfidences);
            }
            CostIndicator = costAvg.Result;
            CategoricalAccuracy = catAccAvg.Result;
            BinaryAccuracy = binAccAvg.Result;
            FeatureConfidences = new double[subConfidencies[0].Length];
            for(int featureIdx = 0; featureIdx < FeatureConfidences.Length; featureIdx++)
            {
                WeightedAvg featureConfAvg = new WeightedAvg();
                for (int subConfIdx = 0; subConfIdx < subConfidencies.Count; subConfIdx++)
                {
                    featureConfAvg.AddSample(subConfidencies[subConfIdx][featureIdx]);
                }
                FeatureConfidences[featureIdx] = featureConfAvg.Result;
            }
            FeatureConfidencesStat = new BasicStat(FeatureConfidences);
            return;
        }

        //Static methods
        /// <summary>
        /// Compares two model confidence metrics.
        /// </summary>
        /// <param name="metrics1">Model confidence metrics 1.</param>
        /// <param name="metrics2">Model confidence metrics 2.</param>
        /// <returns>-1 if metrics1 is better, 1 if metrics2 is better, 0 if no difference.</returns>
        public static int Comparer(ModelConfidenceMetrics metrics1, ModelConfidenceMetrics metrics2)
        {
            if (metrics1.TaskType == OutputTaskType.Categorical)
            {
                if (metrics1.CategoricalAccuracy > metrics2.CategoricalAccuracy)
                {
                    return -1;
                }
                else if (metrics1.CategoricalAccuracy < metrics2.CategoricalAccuracy)
                {
                    return 1;
                }
                else if (metrics1.BinaryAccuracy > metrics2.BinaryAccuracy)
                {
                    return -1;
                }
                else if (metrics1.BinaryAccuracy < metrics2.BinaryAccuracy)
                {
                    return 1;
                }
            }
            else if (metrics1.TaskType == OutputTaskType.Binary)
            {
                if (metrics1.BinaryAccuracy > metrics2.BinaryAccuracy)
                {
                    return -1;
                }
                else if (metrics1.BinaryAccuracy < metrics2.BinaryAccuracy)
                {
                    return 1;
                }
            }
            //Regression or final decision
            if(metrics1.FeatureConfidencesStat.RootMeanSquare > metrics2.FeatureConfidencesStat.RootMeanSquare)
            {
                return -1;
            }
            else if(metrics1.FeatureConfidencesStat.RootMeanSquare < metrics2.FeatureConfidencesStat.RootMeanSquare)
            {
                return 1;
            }
            else if (metrics1.CostIndicator < metrics2.CostIndicator)
            {
                return -1;
            }
            else if (metrics1.CostIndicator > metrics2.CostIndicator)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        /// <returns></returns>
        public ModelConfidenceMetrics DeepClone()
        {
            return new ModelConfidenceMetrics(TaskType, CostIndicator, CategoricalAccuracy, BinaryAccuracy, FeatureConfidences, FeatureConfidencesStat);
        }

    }//ModelConfidenceMetrics
}//Namespace
