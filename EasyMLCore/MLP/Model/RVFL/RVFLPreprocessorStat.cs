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
        /// Activations statistics of pools within the layers.
        /// </summary>
        public BasicStat[][] ActivationStats { get; }

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
        /// <param name="activationStats">Activations statistics of pools within the layers.</param>
        /// <param name="weightStats">Weights statistics of pools within the layers.</param>
        /// <param name="biasStats">Biases statistics of pools within the layers.</param>
        public RVFLPreprocessorStat(RVFLHiddenLayersConfig hlsCfg,
                                    BasicStat[][] activationStats,
                                    BasicStat[][] weightStats,
                                    BasicStat[][] biasStats
                                    )
        {
            HLsCfg = (RVFLHiddenLayersConfig)hlsCfg.DeepClone();
            ActivationStats = activationStats;
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
            ActivationStats = new BasicStat[source.ActivationStats.Length][];
            WeightStats = new BasicStat[source.WeightStats.Length][];
            BiasStats = new BasicStat[source.BiasStats.Length][];
            for(int i = 0; i < ActivationStats.Length; i++)
            {
                ActivationStats[i] = new BasicStat[source.ActivationStats[i].Length];
                WeightStats[i] = new BasicStat[source.WeightStats[i].Length];
                BiasStats[i] = new BasicStat[source.BiasStats[i].Length];
                for(int j = 0; j < ActivationStats[i].Length; j++)
                {
                    ActivationStats[i][j] = new BasicStat(source.ActivationStats[i][j]);
                    WeightStats[i][j] = new BasicStat(source.WeightStats[i][j]);
                    BiasStats[i][j] = new BasicStat(source.BiasStats[i][j]);
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
                    reportText.Append($"{marginStr}        Artivations: Min={ActivationStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture)} Max={ActivationStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture)} Avg={ActivationStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture)} SDev={ActivationStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture)}{Environment.NewLine}");
                    reportText.Append($"{marginStr}        Weights    : Min={WeightStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture)} Max={WeightStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture)} Avg={WeightStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture)} SDev={WeightStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture)}{Environment.NewLine}");
                    reportText.Append($"{marginStr}        Biases     : Min={BiasStats[layerIdx][poolIdx].Min.ToString("F10", CultureInfo.InvariantCulture)} Max={BiasStats[layerIdx][poolIdx].Max.ToString("F10", CultureInfo.InvariantCulture)} Avg={BiasStats[layerIdx][poolIdx].ArithAvg.ToString("F10", CultureInfo.InvariantCulture)} SDev={BiasStats[layerIdx][poolIdx].StdDev.ToString("F10", CultureInfo.InvariantCulture)}{Environment.NewLine}");
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
