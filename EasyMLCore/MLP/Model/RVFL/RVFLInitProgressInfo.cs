using EasyMLCore.MiscTools;
using System;
using System.Globalization;
using System.Text;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Holds progress information about the RVFL preprocessor init process.
    /// </summary>
    [Serializable]
    public class RVFLInitProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Information about the progress of inputs processing.
        /// </summary>
        public ProgressTracker ProcessedInputsTracker { get; }

        /// <summary>
        /// Preprocessor stats if available.
        /// </summary>
        public RVFLPreprocessorStat PreprocessorStat { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfProcessedInputs">Number of currently processed inputs.</param>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed.</param>
        /// <param name="preprocessorStat">Preprocessor stats if available.</param>
        public RVFLInitProgressInfo(int numOfProcessedInputs,
                                    int totalNumOfInputs,
                                    RVFLPreprocessorStat preprocessorStat
                                    )
            :base(RVFLPreprocessor.ContextPathID)
        {
            ProcessedInputsTracker = new ProgressTracker((uint)totalNumOfInputs, (uint)numOfProcessedInputs);
            PreprocessorStat = preprocessorStat;
            return;
        }

        //Properties
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

        /// <inheritdoc/>
        public override bool ShouldBeReported
        {
            get
            {
                return ProcessedInputsTracker.Last || NumOfProcessedInputs == 1 || (NumOfProcessedInputs % InformInterval) == 0;
            }
        }

        /// <inheritdoc/>
        public override bool NewInfoBlock
        {
            get
            {
                return NumOfProcessedInputs == 1;
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

        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            //Build the progress text message
            StringBuilder progressText = new StringBuilder();
            progressText.Append(new string(' ', margin));
            progressText.Append('[');
            progressText.Append(ContextPath);
            progressText.Append("] ");
            progressText.Append(GetInputsInfoText());
            if(PreprocessorStat != null)
            {
                progressText.Append(Environment.NewLine);
                progressText.Append(PreprocessorStat.GetReportText(margin + 4));
            }
            return progressText.ToString();
        }

    }//RVFLInitProgressInfo

}//Namespace
