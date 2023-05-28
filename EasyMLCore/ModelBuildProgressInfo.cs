using EasyMLCore.MLP;
using System;
using System.Globalization;
using System.Text;

namespace EasyMLCore
{
    /// <summary>
    /// It provides information about the currently building part of the target
    /// ML model as well as detailed information that always
    /// relates to the build process of a specific underlying NetworkModel.
    /// </summary>
    [Serializable]
    public class ModelBuildProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Information about the progress of preparatory steps (if necessary).
        /// </summary>
        public ProgressInfoBase PreparatoryStepsProgressInfo { get; }

        /// <summary>
        /// Information about the progress of build process of particular underlying network.
        /// </summary>
        public NetworkBuildProgressInfo NetworkProgressInfo { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="preparatoryStepsProgressInfo">Information about the progress of preparatory steps (if necessary).</param>
        /// <param name="networkProgressInfo">Information about the progress of build process of underlaying network.</param>
        public ModelBuildProgressInfo(string name,
                                      ProgressInfoBase preparatoryStepsProgressInfo,
                                      NetworkBuildProgressInfo networkProgressInfo
                                      )
            :base(name)
        {
            PreparatoryStepsProgressInfo = preparatoryStepsProgressInfo;
            NetworkProgressInfo = networkProgressInfo;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ShouldBeReported
        {
            get
            {
                return (NetworkProgressInfo == null) ? PreparatoryStepsProgressInfo.ShouldBeReported : NetworkProgressInfo.ShouldBeReported;
            }
        }

        /// <inheritdoc/>
        public override bool NewInfoBlock
        {
            get
            {
                return (NetworkProgressInfo == null) ? PreparatoryStepsProgressInfo.NewInfoBlock : NetworkProgressInfo.NewInfoBlock;
            }
        }

        //Methods
        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            return (NetworkProgressInfo == null) ? PreparatoryStepsProgressInfo.GetInfoText(margin) : NetworkProgressInfo.GetInfoText(margin);
        }

    }//ModelBuildProgressInfo

}//Namespace
