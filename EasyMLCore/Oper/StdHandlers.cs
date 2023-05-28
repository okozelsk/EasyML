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
        //Attributes
        //Lock object
        private readonly object _monitor;
        //Output log
        private IOutputLog _log;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="outputLog">An output log to be used.</param>
        public StdHandlers(IOutputLog outputLog) 
        {
            _monitor = new object();
            _log = outputLog;
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
        /// Continuously reports information about the particular process progress.
        /// </summary>
        /// <param name="progressInfo">The current state of the progress.</param>
        public void OnProgressChanged(ProgressInfoBase progressInfo)
        {
            if (progressInfo.ShouldBeReported)
            {
                //Build progress report message
                string progressText = progressInfo.GetInfoText(0);
                //Report the progress
                Log.Write(progressText, !(progressInfo.NewInfoBlock));
            }
            return;
        }


    }//StdHandlers

}//Namespace
