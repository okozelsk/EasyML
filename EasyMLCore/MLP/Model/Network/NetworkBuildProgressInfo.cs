using EasyMLCore.Data;
using EasyMLCore.MiscTools;
using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// It provides information about the currently building Network.
    /// </summary>
    [Serializable]
    public class NetworkBuildProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Information about the progress of MLP engine training attempts.
        /// </summary>
        public ProgressTracker AttemptsTracker { get; }

        /// <summary>
        /// Information about the progress of training epochs within the current training attempt.
        /// </summary>
        public ProgressTracker AttemptEpochsTracker { get; }

        /// <summary>
        /// Current NetworkModel and its error statistics.
        /// </summary>
        public NetworkModel CurrNet { get; }

        /// <summary>
        /// The best NetworkModel so far and its error statistics.
        /// </summary>
        public NetworkModel BestNet { get; }

        /// <summary>
        /// Training attempt number in which was found the best NetworkModel so far.
        /// </summary>
        public int BestNetAttemptNum { get; }

        /// <summary>
        /// An epoch number within the BestNetAttemptNum in which was found the best NetworkModel so far.
        /// </summary>
        public int BestNetAttemptEpochNum { get; }

        /// <summary>
        /// Indicates that current training attempt or whole build process will be stopped.
        /// </summary>
        public bool WillBeStopped { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="attemptNum">Current training attempt number.</param>
        /// <param name="maxNumOfAttempts">Maximum number of training attempts.</param>
        /// <param name="attemptEpochNum">Current training epoch number within the current attempt.</param>
        /// <param name="maxNumOfAttemptEpochs">Maximum number of training epochs.</param>
        /// <param name="currNet">Currently tried NetworkModel and its error statistics.</param>
        /// <param name="bestNet">The best NetworkModel so far.</param>
        /// <param name="bestNetAttemptNum">An attempt number in which was found the best NetworkModel so far.</param>
        /// <param name="bestNetAttemptEpochNum">An epoch number within the bestNetAttemptNum in which was found the best NetworkModel so far.</param>
        /// <param name="willBeStopped">Indicates that current training attempt or whole build process of current NetworkModel will be stopped.</param>
        public NetworkBuildProgressInfo(int attemptNum,
                                        int maxNumOfAttempts,
                                        int attemptEpochNum,
                                        int maxNumOfAttemptEpochs,
                                        NetworkModel currNet,
                                        NetworkModel bestNet,
                                        int bestNetAttemptNum,
                                        int bestNetAttemptEpochNum,
                                        bool willBeStopped
                                        )
            :base(currNet.Name)
        {
            AttemptsTracker = new ProgressTracker((uint)maxNumOfAttempts, (uint)attemptNum);
            AttemptEpochsTracker = new ProgressTracker((uint)maxNumOfAttemptEpochs, (uint)attemptEpochNum);
            CurrNet = currNet;
            BestNet = bestNet;
            BestNetAttemptNum = bestNetAttemptNum;
            BestNetAttemptEpochNum = bestNetAttemptEpochNum;
            WillBeStopped = willBeStopped;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the current network is also the best network so far.
        /// </summary>
        public bool CurrentIsBest { get { return (BestNetAttemptNum == AttemptsTracker.Current && BestNetAttemptEpochNum == AttemptEpochsTracker.Current); } }

        /// <summary>
        /// Indicates that the build of a new network has started.
        /// </summary>
        public bool NewNet
        {
            get
            {
                return AttemptsTracker.Current == 1 && AttemptEpochsTracker.Current == 1;
            }
        }

        /// <summary>
        /// Gets currently processed build epoch number.
        /// </summary>
        public int CurrEpochNum
        {
            get
            {
                return (int)AttemptEpochsTracker.Current;
            }
        }

        /// <inheritdoc/>
        public override bool ShouldBeReported
        {
            get
            {
                return WillBeStopped || NewNet || CurrentIsBest || AttemptEpochsTracker.Last ||
                       CurrEpochNum == 1 || (CurrEpochNum % InformInterval == 0);
            }
        }

        /// <inheritdoc/>
        public override bool NewInfoBlock
        {
            get
            {
                return NewNet;
            }
        }


        //Methods
        /// <summary>
        /// Gets basic textual information about the network build progress.
        /// </summary>
        public string GetBasicProgressInfoText()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"Attempt {AttemptsTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(AttemptsTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
            text.Append($", Epoch {AttemptEpochsTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(AttemptEpochsTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
            return text.ToString();
        }

        /// <summary>
        /// Gets textual information about the number of training and test samples.
        /// </summary>
        public string GetSamplesInfoText()
        {
            StringBuilder text = new StringBuilder();
            int numOfTrainingSamples = CurrNet.TrainingErrorStat.NumOfSamples;
            text.Append($"Samples {numOfTrainingSamples.ToString(CultureInfo.InvariantCulture)}");
            if (CurrNet.ValidationErrorStat != null)
            {
                int numOfTestingSamples = CurrNet.ValidationErrorStat.NumOfSamples;
                text.Append($"/{numOfTestingSamples.ToString(CultureInfo.InvariantCulture)}");
            }
            return text.ToString();
        }

        /// <summary>
        /// Gets textual information about the specified network instance.
        /// </summary>
        /// <param name="network">An instance of network.</param>
        public static string GetNetworkInfoText(NetworkModel network)
        {
            StringBuilder text = new StringBuilder();
            text.Append("RMSE: Train ");
            text.Append(((MultiplePrecisionErrStat)network.TrainingErrorStat.StatData).TotalPrecisionStat.RootMeanSquare.ToString("E3", CultureInfo.InvariantCulture));
            if (network.TaskType == OutputTaskType.Binary)
            {
                MultipleDecisionErrStat stat = (MultipleDecisionErrStat)network.TrainingErrorStat.StatData;
                text.Append("/" + stat.TotalBinWrongDecisionStat.Sum.ToString(CultureInfo.InvariantCulture));
                text.Append("/" + stat.TotalBinFalseFlagStat[0].Sum.ToString(CultureInfo.InvariantCulture));
                text.Append("/" + stat.TotalBinFalseFlagStat[1].Sum.ToString(CultureInfo.InvariantCulture));
            }
            else if (network.TaskType == OutputTaskType.Categorical)
            {
                CategoricalErrStat stat = (CategoricalErrStat)network.TrainingErrorStat.StatData;
                text.Append("/" + stat.TotalNumOfInadequateClassifications.ToString(CultureInfo.InvariantCulture));
                text.Append("/" + stat.WrongClassificationStat.Sum.ToString(CultureInfo.InvariantCulture));
            }
            if (network.ValidationErrorStat != null)
            {
                text.Append(", Val ");
                text.Append(((MultiplePrecisionErrStat)network.ValidationErrorStat.StatData).TotalPrecisionStat.RootMeanSquare.ToString("E3", CultureInfo.InvariantCulture));
                if (network.TaskType == OutputTaskType.Binary)
                {
                    MultipleDecisionErrStat stat = (MultipleDecisionErrStat)network.ValidationErrorStat.StatData;
                    text.Append("/" + stat.TotalBinWrongDecisionStat.Sum.ToString(CultureInfo.InvariantCulture));
                    text.Append("/" + stat.TotalBinFalseFlagStat[0].Sum.ToString(CultureInfo.InvariantCulture));
                    text.Append("/" + stat.TotalBinFalseFlagStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                else if (network.TaskType == OutputTaskType.Categorical)
                {
                    CategoricalErrStat stat = (CategoricalErrStat)network.ValidationErrorStat.StatData;
                    text.Append("/" + stat.TotalNumOfInadequateClassifications.ToString(CultureInfo.InvariantCulture));
                    text.Append("/" + stat.WrongClassificationStat.Sum.ToString(CultureInfo.InvariantCulture));
                }
            }
            text.Append(", RMS: W-Out ");
            text.Append(network.Engine.OLWeightsStat.RootMeanSquare.ToString("E3", CultureInfo.InvariantCulture));
            if (network.Engine.HLWeightsStat.NumOfSamples > 0)
            {
                text.Append(", W-Hid ");
                text.Append(network.Engine.HLWeightsStat.RootMeanSquare.ToString("E3", CultureInfo.InvariantCulture));
            }
            return text.ToString();
        }

        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            //Build the progress text message
            StringBuilder progressText = new StringBuilder();
            progressText.Append(new string(' ', margin));
            progressText.Append('[');
            progressText.Append(ContextPath);
            progressText.Append("] ");
            progressText.Append(GetBasicProgressInfoText());
            progressText.Append(", ");
            progressText.Append(GetSamplesInfoText());
            progressText.Append(", BestNet-{");
            progressText.Append(GetNetworkInfoText(BestNet));
            progressText.Append("}, CurrNet-{");
            progressText.Append(GetNetworkInfoText(CurrNet));
            progressText.Append('}');
            return progressText.ToString();
        }

    }//NetworkBuildProgressInfo

}//Namespace
