using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Holds RVFL preprocessor's statistics.
    /// </summary>
    [Serializable]
    public class RVFLPreprocessorStat : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Configuration of RVFL preprocessor's hidden layers.
        /// </summary>
        public RVFLHiddenLayersConfig HLsCfg { get; }

        /// <summary>
        /// Activations averages statistics of pool's nodes within the layers.
        /// </summary>
        public BasicStat[][] ActivationAvgsStats { get; }

        /// <summary>
        /// Activations spans statistics of pool's nodes within the layers.
        /// </summary>
        public BasicStat[][] ActivationSpansStats { get; }

        /// <summary>
        /// Activations mins statistics of pool's nodes within the layers.
        /// </summary>
        public BasicStat[][] ActivationMinsStats { get; }

        /// <summary>
        /// Activations maxs statistics of pool's nodes within the layers.
        /// </summary>
        public BasicStat[][] ActivationMaxsStats { get; }

        /// <summary>
        /// Weights statistics of pools within the layers.
        /// </summary>
        public BasicStat[][] WeightStats { get; }

        /// <summary>
        /// Biases statistics of pools within the layers.
        /// </summary>
        public BasicStat[][] BiasStats { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="hlsCfg">Configuration of RVFL preprocessor's hidden layers.</param>
        /// <param name="activationStats">Activations statistics of pools nodes within the layers.</param>
        /// <param name="weightStats">Weights statistics of pools within the layers.</param>
        /// <param name="biasStats">Biases statistics of pools within the layers.</param>
        public RVFLPreprocessorStat(RVFLHiddenLayersConfig hlsCfg,
                                    BasicStat[][][] activationStats,
                                    BasicStat[][] weightStats,
                                    BasicStat[][] biasStats
                                    )
        {
            HLsCfg = (RVFLHiddenLayersConfig)hlsCfg.DeepClone();
            ActivationAvgsStats = new BasicStat[activationStats.Length][];
            ActivationSpansStats = new BasicStat[activationStats.Length][];
            ActivationMinsStats = new BasicStat[activationStats.Length][];
            ActivationMaxsStats = new BasicStat[activationStats.Length][];
            for (int i = 0; i < activationStats.Length; i++)
            {
                ActivationAvgsStats[i] = new BasicStat[activationStats[i].Length];
                ActivationSpansStats[i] = new BasicStat[activationStats[i].Length];
                ActivationMinsStats[i] = new BasicStat[activationStats[i].Length];
                ActivationMaxsStats[i] = new BasicStat[activationStats[i].Length];
                for (int j = 0; j < activationStats[i].Length; j++)
                {
                    ActivationAvgsStats[i][j] = new BasicStat();
                    ActivationSpansStats[i][j] = new BasicStat();
                    ActivationMinsStats[i][j] = new BasicStat();
                    ActivationMaxsStats[i][j] = new BasicStat();
                    for (int k = 0; k < activationStats[i][j].Length; k++)
                    {
                        ActivationAvgsStats[i][j].AddSample(activationStats[i][j][k].ArithAvg);
                        ActivationSpansStats[i][j].AddSample(activationStats[i][j][k].Span);
                        ActivationMinsStats[i][j].AddSample(activationStats[i][j][k].Min);
                        ActivationMaxsStats[i][j].AddSample(activationStats[i][j][k].Max);
                    }
                }
            }
            WeightStats = weightStats;
            BiasStats = biasStats;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance.</param>
        public RVFLPreprocessorStat(RVFLPreprocessorStat source)
        {
            HLsCfg = (RVFLHiddenLayersConfig)source.HLsCfg.DeepClone();
            ActivationAvgsStats = new BasicStat[source.ActivationAvgsStats.Length][];
            ActivationSpansStats = new BasicStat[source.ActivationSpansStats.Length][];
            ActivationMinsStats = new BasicStat[source.ActivationMinsStats.Length][];
            ActivationMaxsStats = new BasicStat[source.ActivationMaxsStats.Length][];
            WeightStats = new BasicStat[source.WeightStats.Length][];
            BiasStats = new BasicStat[source.BiasStats.Length][];
            for(int i = 0; i < ActivationAvgsStats.Length; i++)
            {
                ActivationAvgsStats[i] = new BasicStat[source.ActivationAvgsStats[i].Length];
                ActivationSpansStats[i] = new BasicStat[source.ActivationSpansStats[i].Length];
                ActivationMinsStats[i] = new BasicStat[source.ActivationMinsStats[i].Length];
                ActivationMaxsStats[i] = new BasicStat[source.ActivationMaxsStats[i].Length];
                WeightStats[i] = new BasicStat[source.WeightStats[i].Length];
                BiasStats[i] = new BasicStat[source.BiasStats[i].Length];
                for(int j = 0; j < ActivationSpansStats[i].Length; j++)
                {
                    ActivationAvgsStats[i][j] = source.ActivationAvgsStats[i][j].DeepClone();
                    ActivationSpansStats[i][j] = source.ActivationSpansStats[i][j].DeepClone();
                    ActivationMinsStats[i][j] = source.ActivationMinsStats[i][j].DeepClone();
                    ActivationMaxsStats[i][j] = source.ActivationMaxsStats[i][j].DeepClone();
                    WeightStats[i][j] = source.WeightStats[i][j].DeepClone();
                    BiasStats[i][j] = source.BiasStats[i][j].DeepClone();
                }
            }
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
            for(int layerIdx = 0; layerIdx < HLsCfg.LayerCfgCollection.Count; layerIdx++)
            {
                reportText.Append($"{marginStr}Layer {(layerIdx + 1)}{Environment.NewLine}");
                for(int poolIdx = 0; poolIdx < HLsCfg.LayerCfgCollection[layerIdx].PoolCfgCollection.Count; poolIdx++)
                {
                    RVFLHiddenPoolConfig poolCfg = HLsCfg.LayerCfgCollection[layerIdx].PoolCfgCollection[poolIdx];
                    reportText.Append($"{marginStr}    Pool ({poolCfg.NumOfNeurons}x{poolCfg.ActivationID}){Environment.NewLine}");
                    reportText.Append($"{marginStr}        Activations Spans: Min={ActivationSpansStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Max={ActivationSpansStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Avg={ActivationSpansStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} SDev={ActivationSpansStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)}{Environment.NewLine}");
                    reportText.Append($"{marginStr}        Activations Avgs : Min={ActivationAvgsStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Max={ActivationAvgsStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Avg={ActivationAvgsStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} SDev={ActivationAvgsStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)}{Environment.NewLine}");
                    reportText.Append($"{marginStr}        Activations Mins : Min={ActivationMinsStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Max={ActivationMinsStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Avg={ActivationMinsStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} SDev={ActivationMinsStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)}{Environment.NewLine}");
                    reportText.Append($"{marginStr}        Activations Maxs : Min={ActivationMaxsStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Max={ActivationMaxsStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Avg={ActivationMaxsStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} SDev={ActivationMaxsStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)}{Environment.NewLine}");
                    reportText.Append($"{marginStr}        Weights          : Min={WeightStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Max={WeightStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Avg={WeightStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} SDev={WeightStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)}{Environment.NewLine}");
                    reportText.Append($"{marginStr}        Biases           : Min={BiasStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Max={BiasStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} Avg={BiasStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)} SDev={BiasStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture).PadRight(14)}{Environment.NewLine}");
                }
            }
            return reportText.ToString();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        /// <returns></returns>
        public RVFLPreprocessorStat DeepClone()
        {
            return new RVFLPreprocessorStat(this);
        }

    }//RVFLPreprocessorStat
}//Namespace
