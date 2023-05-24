using EasyMLCore.Data;
using EasyMLCore.MiscTools;
using System;
using System.Globalization;
using System.Text;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// It provides information about the currently building part of the target
    /// model (context path) as well as detailed information that always
    /// relates to the build process of a specific underlying NetworkModel.
    /// </summary>
    [Serializable]
    public class ModelBuildProgressInfo : ProgressInfoBase
    {
        //Attribute properties
        /// <summary>
        /// Preparatory steps tracker (if necessary).
        /// </summary>
        public ProgressTracker PreparatoryStepsTracker { get; }

        /// <summary>
        /// Information about the progress of build process of underlaying network.
        /// </summary>
        public NetworkBuildProgressInfo NetworkProgressInfo { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="preparatoryStepsTracker">Preparatory steps tracker (if necessary).</param>
        /// <param name="networkProgressInfo">Information about the progress of build process of underlaying network.</param>
        public ModelBuildProgressInfo(string name,
                                      ProgressTracker preparatoryStepsTracker,
                                      NetworkBuildProgressInfo networkProgressInfo
                                      )
            :base(name)
        {
            PreparatoryStepsTracker = preparatoryStepsTracker;
            NetworkProgressInfo = networkProgressInfo;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ShouldBeReported
        {
            get
            {
                return (NetworkProgressInfo == null) ? PreparatoryStepsTracker.Last : NetworkProgressInfo.ShouldBeReported;
            }
        }

        //Methods
        /// <summary>
        /// Gets textual information about the number of processed preparatory steps.
        /// </summary>
        public string GetPreparatoryInfoText(int margin = 0)
        {
            StringBuilder text = new StringBuilder();
            text.Append($"{new string(' ', margin)}[{ContextPath}] Preparatory step {PreparatoryStepsTracker.Current.ToString(CultureInfo.InvariantCulture)}");
            text.Append($"/{PreparatoryStepsTracker.Target.ToString(CultureInfo.InvariantCulture)}");
            return text.ToString();
        }

        /// <inheritdoc/>
        public override string GetInfoText(int margin = 0)
        {
            return (NetworkProgressInfo == null) ? GetPreparatoryInfoText(margin) : NetworkProgressInfo.GetInfoText(margin);
        }

    }//ModelBuildProgressInfo

}//Namespace
