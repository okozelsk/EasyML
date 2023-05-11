using EasyMLCore.Data;
using System.Collections.Generic;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Common interface of components providing task-specific output computation.
    /// </summary>
    public interface IComputableTaskSpecific : IComputable
    {
        /// <inheritdoc cref="OutputTaskType"/>
        OutputTaskType TaskType { get; }

        /// <summary>
        /// Names of task's output features.
        /// </summary>
        public List<string> OutputFeatureNames { get; }

        /// <summary>
        /// Gets the appropriate instance of task specific detailed output.
        /// </summary>
        /// <param name="outputData">Computed or ideal data vector.</param>
        /// <returns>The appropriate instance of task specific detailed output.</returns>
        public TaskOutputDetailBase GetOutputDetail(double[] outputData);


    }//IComputableTaskSpecific

}//Namespace
