using EasyMLCore.MLP;
using System;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Holds progress information about the reservoir computer's build process.
    /// </summary>
    [Serializable]
    public class ResCompBuildProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Information about the reservoir's progress.
        /// </summary>
        public ReservoirInitProgressInfo ReservoirProgressInfo { get; }

        /// <summary>
        /// Information about the progress of the underlying model build progress.
        /// </summary>
        public ModelBuildProgressInfo ModelProgressInfo { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirProgressInfo">Information about the reservoir's progress.</param>
        /// <param name="modelProgressInfo">Information about the progress of the underlying model build progress..</param>
        public ResCompBuildProgressInfo(ReservoirInitProgressInfo reservoirProgressInfo,
                                                  ModelBuildProgressInfo modelProgressInfo
                                                  )
            :base(ResComp.ContextPathID)
        {
            ReservoirProgressInfo = reservoirProgressInfo;
            ModelProgressInfo = modelProgressInfo;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ShouldBeReported
        {
            get
            {
                return (ModelProgressInfo == null) ? ReservoirProgressInfo.ShouldBeReported : ModelProgressInfo.ShouldBeReported;
            }
        }

        //Methods
        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            return (ModelProgressInfo == null) ? ReservoirProgressInfo.GetInfoText(margin) : ModelProgressInfo.GetInfoText(margin);
        }

    }//ResCompBuildProgressInfo

}//Namespace
