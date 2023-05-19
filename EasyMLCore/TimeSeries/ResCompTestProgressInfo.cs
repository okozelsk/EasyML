using EasyMLCore.MiscTools;
using EasyMLCore.MLP;
using System;
using System.Globalization;
using System.Text;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Holds progress information about the reservoir computer testing process.
    /// </summary>
    [Serializable]
    public class ResCompTestProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Information about the data preprocessing progress.
        /// </summary>
        public ProgressTracker PreprocessedInputsTracker { get; }

        /// <summary>
        /// Information about the readout layer ResCompTask's model testing progress.
        /// </summary>
        public ModelTestProgressInfo ModelTestProgressInfo { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="preprocessedInputsTracker">Information about the data preprocessing progress.</param>
        /// <param name="modelTestProgressInfo">Information about the readout layer ResCompTask's model testing progress.</param>
        public ResCompTestProgressInfo(ProgressTracker preprocessedInputsTracker,
                                         ModelTestProgressInfo modelTestProgressInfo
                                         )
            :base(ResComp.ContextPathID)
        {
            PreprocessedInputsTracker = preprocessedInputsTracker;
            ModelTestProgressInfo = modelTestProgressInfo;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ShouldBeReported
        {
            get
            {
                return (ModelTestProgressInfo == null) ? PreprocessedInputsTracker.Last : ModelTestProgressInfo.ShouldBeReported;
            }
        }

        //Methods
        /// <summary>
        /// Gets textual information about the number of processed inputs.
        /// </summary>
        public string GetInputsInfoText()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"Preprocessed inputs {PreprocessedInputsTracker.Current.ToString(CultureInfo.InvariantCulture)}");
            text.Append($"/{PreprocessedInputsTracker.Target.ToString(CultureInfo.InvariantCulture)}");
            return text.ToString();
        }

        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            if(PreprocessedInputsTracker != null)
            {
                //Build the progress text message
                StringBuilder progressText = new StringBuilder();
                progressText.Append(new string(' ', margin));
                progressText.Append('[');
                progressText.Append(ContextPath);
                progressText.Append("] ");
                progressText.Append(GetInputsInfoText());
                return progressText.ToString();
            }
            else
            {
                return ModelTestProgressInfo.GetInfoText(margin);
            }
        }

    }//ResCompTestProgressInfo

}//Namespace
