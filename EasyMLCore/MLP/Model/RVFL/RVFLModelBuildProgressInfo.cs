using EasyMLCore.MLP;
using System;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Holds progress information about the RVFL model's build process.
    /// </summary>
    [Serializable]
    public class RVFLModelBuildProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Information about the RVFL preprocessor's progress.
        /// </summary>
        public RVFLInitProgressInfo PreprocessorProgressInfo { get; }

        /// <summary>
        /// Information about the progress of the end-model build progress.
        /// </summary>
        public ModelBuildProgressInfo ModelProgressInfo { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="preprocessorProgressInfo">Information about the RVFL preprocessor's progress.</param>
        /// <param name="modelProgressInfo">Information about the progress of the end-model build progress..</param>
        public RVFLModelBuildProgressInfo(RVFLInitProgressInfo preprocessorProgressInfo,
                                          ModelBuildProgressInfo modelProgressInfo
                                          )
            :base(ResComp.ContextPathID)
        {
            PreprocessorProgressInfo = preprocessorProgressInfo;
            ModelProgressInfo = modelProgressInfo;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ShouldBeReported
        {
            get
            {
                return (ModelProgressInfo == null) ? PreprocessorProgressInfo.ShouldBeReported : ModelProgressInfo.ShouldBeReported;
            }
        }

        //Methods
        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            return (ModelProgressInfo == null) ? PreprocessorProgressInfo.GetInfoText(margin) : ModelProgressInfo.GetInfoText(margin);
        }

    }//RVFLModelBuildProgressInfo

}//Namespace
