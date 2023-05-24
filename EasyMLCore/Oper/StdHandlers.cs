using EasyMLCore.Log;
using EasyMLCore.MLP;
using EasyMLCore.TimeSeries;
using System;

namespace EasyMLCore
{
    /// <summary>
    /// Provides the set of standard handlers of EasyML's informative progress events.
    /// </summary>
    public class StdHandlers
    {
        //Constants
        public const int DefaultInformInterval = 5;

        //Attributes
        //Lock object
        private readonly object _monitor;
        //Output log
        private IOutputLog _log;
        private int _informInterval;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="outputLog">An output log to be used.</param>
        public StdHandlers(IOutputLog outputLog) 
        {
            _monitor = new object();
            _log = outputLog;
            _informInterval = DefaultInformInterval;
            return;
        }

        //Properties
        /// <summary>
        /// Gets the instance of associated output log.
        /// </summary>
        public IOutputLog Log { get { lock (_monitor) { return _log; } } }

        //Methods
        /// <summary>
        /// Changes the output log.
        /// </summary>
        /// <param name="newLog">Another output log instance to be used.</param>
        public void ChangeOutputLog(IOutputLog newLog)
        {
            if (newLog == null)
            {
                throw new ArgumentNullException(nameof(newLog), "Output log instance can not be null.");
            }
            lock ( _monitor ) { _log = newLog; }
            return;
        }

        /// <summary>
        /// Changes the progress info interval (step).
        /// </summary>
        /// <param name="newInterval">New value of the progress info step.</param>
        public void ChangeProgressInfoInterval(int newInterval)
        {
            if (newInterval <= 0)
            {
                throw new ArgumentException($"New interval must be GT 0 (received {newInterval}).", nameof(newInterval));
            }
            lock (_monitor) { _informInterval = newInterval; }
            return;
        }

        /// <summary>
        /// Continuously reports information about the model build process.
        /// </summary>
        /// <param name="progressInfo">The current state of the build process.</param>
        public void OnModelBuildProgressChanged(ModelBuildProgressInfo progressInfo)
        {
            if(progressInfo.NetworkProgressInfo != null)
            {
                //Progress info
                if (progressInfo.NetworkProgressInfo.ShouldBeReported || progressInfo.NetworkProgressInfo.CurrEpochNum == 1 || (progressInfo.NetworkProgressInfo.CurrEpochNum % _informInterval == 0))
                {
                    //Build progress report message
                    string progressText = progressInfo.GetInfoText(0);
                    //Report the progress
                    Log.Write(progressText, !(progressInfo.NetworkProgressInfo.NewNet));
                }
            }
            else
            {
                if (progressInfo.ShouldBeReported || progressInfo.PreparatoryStepsTracker.Current == 1 || (progressInfo.PreparatoryStepsTracker.Current % _informInterval == 0))
                {
                    //Build progress report message
                    string progressText = progressInfo.GetInfoText(0);
                    //Report the progress
                    Log.Write(progressText, progressInfo.PreparatoryStepsTracker.Current > 1);
                }
            }
            return;
        }

        /// <summary>
        /// Continuously reports information about the RVFL preprocessor's initialization progress.
        /// </summary>
        /// <param name="progressInfo">The current state of the RVFL preprocessor's initialization.</param>
        public void OnRVFLInitProgressChanged(RVFLInitProgressInfo progressInfo)
        {
            //Progress info
            if (progressInfo.ShouldBeReported || progressInfo.NumOfProcessedInputs == 1 || (progressInfo.NumOfProcessedInputs % _informInterval == 0))
            {
                //Build progress report message
                string progressText = progressInfo.GetInfoText(0);
                //Report the progress
                Log.Write(progressText, progressInfo.NumOfProcessedInputs > 1);
            }
            return;
        }

        /// <summary>
        /// Continuously reports information about the model test progress.
        /// </summary>
        /// <param name="progressInfo">The current state of the model testing.</param>
        public void OnModelTestProgressChanged(ModelTestProgressInfo progressInfo)
        {
            //Progress info
            if (progressInfo.ShouldBeReported || progressInfo.NumOfProcessedInputs == 1 || (progressInfo.NumOfProcessedInputs % _informInterval == 0))
            {
                //Build progress report message
                string progressText = progressInfo.GetInfoText(0);
                //Report the progress
                Log.Write(progressText, progressInfo.NumOfProcessedInputs > 1);
            }
            return;
        }

        /// <summary>
        /// Continuously reports information about the reservoir's initialization progress.
        /// </summary>
        /// <param name="progressInfo">The current state of the reservoir's initialization.</param>
        public void OnReservoirInitProgressChanged(ReservoirInitProgressInfo progressInfo)
        {
            //Progress info
            if (progressInfo.ShouldBeReported || progressInfo.NumOfProcessedInputs == 1 || (progressInfo.NumOfProcessedInputs % _informInterval == 0))
            {
                //Build progress report message
                string progressText = progressInfo.GetInfoText(0);
                //Report the progress
                Log.Write(progressText, progressInfo.NumOfProcessedInputs > 1);
            }
            return;
        }

        /// <summary>
        /// Continuously reports information about the reservoir computer's build progress.
        /// </summary>
        /// <param name="progressInfo">The current state of the reservoir computer's build process.</param>
        public void OnResCompBuildProgressChanged(ResCompBuildProgressInfo progressInfo)
        {
            if (progressInfo.ReservoirProgressInfo != null)
            {
                OnReservoirInitProgressChanged(progressInfo.ReservoirProgressInfo);
            }
            else
            {
                OnModelBuildProgressChanged(progressInfo.ModelProgressInfo);
            }
            return;
        }

        /// <summary>
        /// Continuously reports information about the reservoir computer test progress.
        /// </summary>
        /// <param name="progressInfo">The current state of the reservoir computer testing.</param>
        public void OnResCompTestProgressChanged(ResCompTestProgressInfo progressInfo)
        {
            //Progress info
            if (progressInfo.PreprocessedInputsTracker != null)
            {
                if (progressInfo.ShouldBeReported ||
                    progressInfo.PreprocessedInputsTracker.Current == 1 ||
                    (progressInfo.PreprocessedInputsTracker.Current % _informInterval == 0))
                {
                    //Progress report message
                    string progressText = progressInfo.GetInfoText(0);
                    //Report the progress
                    Log.Write(progressText, progressInfo.PreprocessedInputsTracker.Current > 1);
                }
            }
            else
            {
                OnModelTestProgressChanged(progressInfo.ModelTestProgressInfo);
            }
            return;
        }


    }//StdHandlers

}//Namespace
