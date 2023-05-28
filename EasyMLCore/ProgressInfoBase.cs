using System;
using System.Text;

namespace EasyMLCore
{
    //Delegates
    /// <summary>
    /// Common delegate of the progress changed event handler.
    /// </summary>
    /// <param name="progressInfo">Current state of the process progress.</param>
    public delegate void ProgressChangedHandler(ProgressInfoBase progressInfo);

    /// <summary>
    /// Implements base class of specialized ProgressInfo classes.
    /// </summary>
    [Serializable]
    public abstract class ProgressInfoBase : SerializableObject
    {
        //Constants
        public const int DefaultInformInterval = 5;

        //Static attributes
        private static readonly object _monitor = new object();
        private static int _informInterval = DefaultInformInterval;

        //Attribute properties
        /// <summary>
        /// Context path of the currently being initiated reservoir.
        /// </summary>
        public StringBuilder ContextPath { get; }

        //Constructor
        /// <summary>
        /// Initializes context path.
        /// </summary>
        protected ProgressInfoBase(string contextPathText)
        { 
            ContextPath = new StringBuilder(contextPathText);
            return;
        }

        //Static properties
        /// <summary>
        /// Gets current inform-interval.
        /// </summary>
        public static int InformInterval
        {
            get
            {
                lock (_monitor)
                {
                    return _informInterval;
                }
            }
        }

        //Properties
        /// <summary>
        /// Indicates important progress information that should be reported.
        /// </summary>
        public abstract bool ShouldBeReported { get; }

        /// <summary>
        /// Indicates that progress info is related to the new information block.
        /// </summary>
        public abstract bool NewInfoBlock { get; }

        //Static methods
        /// <summary>
        /// Changes the progress info interval (step).
        /// </summary>
        /// <param name="newInterval">New value of the progress info step.</param>
        public static void ChangeProgressInfoInterval(int newInterval)
        {
            if (newInterval <= 0)
            {
                throw new ArgumentException($"New interval must be GT 0 (received {newInterval}).", nameof(newInterval));
            }
            lock (_monitor) { _informInterval = newInterval; }
            return;
        }

        //Methods
        /// <summary>
        /// Extends the build context path.
        /// </summary>
        /// <param name="pathElem">Path element to be inserted into the context path.</param>
        public void ExtendContextPath(string pathElem)
        {
            ContextPath.Insert(0, pathElem + ".");
            return;
        }

        /// <summary>
        /// Gets textual information about the progress.
        /// </summary>
        /// <param name="margin">Left margin (number of spaces).</param>
        /// <returns>Built text message.</returns>
        public abstract string GetInfoText(int margin = 0);


    }//ProgressInfoBase

}//Namespace
