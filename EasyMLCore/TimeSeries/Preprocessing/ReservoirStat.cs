using EasyMLCore.MathTools;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Holds detailed and aggregated reservoir statistics.
    /// </summary>
    [Serializable]
    public class ReservoirStat : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Contains statistics of each hidden neuron in the reservoir.
        /// </summary>
        public ReservoirNeuronStat[] IndividualNeuronStats { get; }

        /// <summary>
        /// Holds aggregated stat of activation span from all hidden neurons in the reservoir.
        /// </summary>
        public BasicStat NeuronsActivationSpanStat { get; }

        /// <summary>
        /// Holds aggregated stat of negative activations from all hidden neurons in the reservoir.
        /// </summary>
        public BasicStat NeuronsActivationNegRangeUsage { get; }

        /// <summary>
        /// Holds aggregated stat of positive activations from all hidden neurons in the reservoir.
        /// </summary>
        public BasicStat NeuronsActivationPosRangeUsage { get; }

        /// <summary>
        /// Holds stat of min activation from all hidden neurons in the reservoir.
        /// </summary>
        public BasicStat NeuronsAvgSpikeEventStat { get; }

        /// <summary>
        /// Number of neurons never have received stimuli.
        /// </summary>
        public int NumOfNeuronsWithoutStimuli { get; private set; }

        /// <summary>
        /// Counts of blocked predictors.
        /// </summary>
        public int[] NumOfBlockedPredictors { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="neuronsStats">Statistics of each hidden neuron in the reservoir.</param>
        /// <param name="numOfConstantPredictors">Counts of blocked predictors.</param>
        public ReservoirStat(ReservoirNeuronStat[] neuronsStats, int[] numOfBlockedPredictors)
        {
            NumOfBlockedPredictors = numOfBlockedPredictors;
            IndividualNeuronStats = neuronsStats;
            NeuronsActivationSpanStat = new BasicStat();
            NeuronsActivationNegRangeUsage = new BasicStat();
            NeuronsActivationPosRangeUsage = new BasicStat();
            NeuronsAvgSpikeEventStat = new BasicStat();
            NumOfNeuronsWithoutStimuli = IndividualNeuronStats.Length;
            //Aggregation
            foreach (ReservoirNeuronStat neuronStat in IndividualNeuronStats)
            {
                if (neuronStat.ActivationStat.StdDev != 0)
                {
                    --NumOfNeuronsWithoutStimuli;
                }
                NeuronsActivationSpanStat.AddSample(neuronStat.ActivationStat.Span);
                NeuronsActivationNegRangeUsage.AddSample(neuronStat.NegActivationStat.Span);
                NeuronsActivationPosRangeUsage.AddSample(neuronStat.PosActivationStat.Span);
                NeuronsAvgSpikeEventStat.AddSample(neuronStat.SpikeEventStat.ArithAvg);
            }
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance..</param>
        public ReservoirStat(ReservoirStat source)
        {
            NumOfBlockedPredictors = (int[])source.NumOfBlockedPredictors.Clone();
            IndividualNeuronStats = new ReservoirNeuronStat[source.IndividualNeuronStats.Length];
            for (int i = 0; i < IndividualNeuronStats.Length; i++)
            {
                IndividualNeuronStats[i] = new ReservoirNeuronStat(source.IndividualNeuronStats[i]);
            }
            NeuronsActivationSpanStat = new BasicStat(source.NeuronsActivationSpanStat);
            NeuronsActivationNegRangeUsage = new BasicStat(source.NeuronsActivationNegRangeUsage);
            NeuronsActivationPosRangeUsage = new BasicStat(source.NeuronsActivationPosRangeUsage);
            NeuronsAvgSpikeEventStat = new BasicStat(source.NeuronsAvgSpikeEventStat);
            NumOfNeuronsWithoutStimuli = source.NumOfNeuronsWithoutStimuli;
            return;
        }

        //Methods
        /// <summary>
        /// Gets textual report containing key summaries from neurons stats.
        /// </summary>
        /// <param name="margin">Left margin (number of spaces).</param>
        /// <returns>Formatted text.</returns>
        public string GetReportText(int margin = 0)
        {
            //Build the progress text message
            StringBuilder reportText = new StringBuilder();
            string marginStr = new string(' ', margin);
            reportText.Append($"{marginStr}Number of neurons without stimuli: {NumOfNeuronsWithoutStimuli} (should be 0){Environment.NewLine}");
            reportText.Append($"{marginStr}Number of blocked predictors     : {NumOfBlockedPredictors.Sum()} (the lower the better){Environment.NewLine}");
            reportText.Append($"{marginStr}Avg neg-nonlinear range usage    : {NeuronsActivationNegRangeUsage.ArithAvg.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            reportText.Append($"{marginStr}Avg pos-nonlinear range usage    : {NeuronsActivationPosRangeUsage.ArithAvg.ToString("F5", CultureInfo.InvariantCulture)}{Environment.NewLine}");
            reportText.Append($"{marginStr}Neg/Pos activation disbalance    : {Math.Abs(NeuronsActivationPosRangeUsage.ArithAvg - NeuronsActivationNegRangeUsage.ArithAvg).ToString("F5", CultureInfo.InvariantCulture)} (should be close to 0){Environment.NewLine}");
            reportText.Append($"{marginStr}Avg activation span              : {NeuronsActivationSpanStat.ArithAvg.ToString("F5", CultureInfo.InvariantCulture)} (typically between 1 and 2){Environment.NewLine}");
            reportText.Append($"{marginStr}Avg spike event rate             : {NeuronsAvgSpikeEventStat.ArithAvg.ToString("F5", CultureInfo.InvariantCulture)} (typically between 0.3 and 0.5){Environment.NewLine}");
            return reportText.ToString();
        }

    }//ReservoirStat

}//Namespace
