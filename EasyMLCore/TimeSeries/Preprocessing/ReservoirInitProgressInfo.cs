using EasyMLCore.MiscTools;
using System;
using System.Globalization;
using System.Text;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Holds progress information about the init process.
    /// </summary>
    [Serializable]
    public class ReservoirInitProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Information about the progress of training epochs within the current build attempt.
        /// </summary>
        public ProgressTracker ProcessedInputsTracker { get; }

        /// <summary>
        /// Number of already prepared reservoir outputs.
        /// </summary>
        public int NumOfPreparedOutputs { get; }

        /// <summary>
        /// Reservoir's stat when finished.
        /// </summary>
        public ReservoirStat Stat { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfProcessedInputs">Number of currently processed inputs.</param>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed.</param>
        /// <param name="numOfPreparedOutputs">Number of currently prepared outputs.</param>
        /// <param name="numOfPreparedOutputs">Number of currently prepared outputs.</param>
        /// <param name="stat">Reservoir's stat when finished.</param>
        public ReservoirInitProgressInfo(int numOfProcessedInputs,
                                         int totalNumOfInputs,
                                         int numOfPreparedOutputs,
                                         ReservoirStat stat
                                         )
            :base(Reservoir.ContextPathID)
        {
            ProcessedInputsTracker = new ProgressTracker((uint)totalNumOfInputs, (uint)numOfProcessedInputs);
            NumOfPreparedOutputs = numOfPreparedOutputs;
            Stat = stat;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ShouldBeReported { get { return ProcessedInputsTracker.Last; } }


        /// <summary>
        /// Gets number of currently processed inputs.
        /// </summary>
        public int NumOfProcessedInputs
        {
            get
            {
                return (int)ProcessedInputsTracker.Current;
            }
        }

        //Methods
        /// <summary>
        /// Gets textual information about the number of processed inputs.
        /// </summary>
        public string GetInputsInfoText()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"Processed inputs {ProcessedInputsTracker.Current.ToString(CultureInfo.InvariantCulture)}");
            text.Append($"/{ProcessedInputsTracker.Target.ToString(CultureInfo.InvariantCulture)}");
            return text.ToString();
        }

        /// <summary>
        /// Gets textual information about the number of prepared outputs.
        /// </summary>
        public string GetOutputsInfoText()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"Prepared outputs {NumOfPreparedOutputs.ToString(CultureInfo.InvariantCulture)}");
            return text.ToString();
        }

        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            //Build the progress text message
            StringBuilder progressText = new StringBuilder();
            progressText.Append(new string(' ', margin));
            progressText.Append("[");
            progressText.Append(ContextPath.ToString());
            progressText.Append("] ");
            progressText.Append(GetInputsInfoText());
            progressText.Append(", ");
            progressText.Append(GetOutputsInfoText());
            if(Stat != null)
            {
                progressText.Append(Environment.NewLine);
                progressText.Append(Stat.GetReportText(margin));
            }
            return progressText.ToString();
        }

    }//ReservoirInitProgress

}//Namespace
