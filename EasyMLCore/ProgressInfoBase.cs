using System;
using System.Text;

namespace EasyMLCore
{
    /// <summary>
    /// Implements base class of specialized ProgressInfo classes.
    /// </summary>
    [Serializable]
    public abstract class ProgressInfoBase : SerializableObject
    {
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

        //Properties
        /// <summary>
        /// Indicates important progress information that should be reported.
        /// </summary>
        public abstract bool ShouldBeReported { get; }

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
