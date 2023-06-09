﻿using EasyMLCore.MiscTools;
using System;
using System.Globalization;
using System.Text;

namespace EasyMLCore
{
    /// <summary>
    /// Holds progress information about the ML model testing process.
    /// </summary>
    [Serializable]
    public class ModelTestProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Name of the model.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Information about the progress of inputs processing.
        /// </summary>
        public ProgressTracker ProcessedInputsTracker { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">Name of the model.</param>
        /// <param name="numOfProcessedInputs">Number of currently processed inputs.</param>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed.</param>
        public ModelTestProgressInfo(string name,
                                     int numOfProcessedInputs,
                                     int totalNumOfInputs
                                     )
            :base(name)
        {
            Name = name;
            ProcessedInputsTracker = new ProgressTracker((uint)totalNumOfInputs, (uint)numOfProcessedInputs);
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
                return ProcessedInputsTracker.Last || NumOfProcessedInputs == 1 || (NumOfProcessedInputs % InformInterval == 0);
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
            return progressText.ToString();
        }

    }//ModelTestProgressInfo

}//Namespace
