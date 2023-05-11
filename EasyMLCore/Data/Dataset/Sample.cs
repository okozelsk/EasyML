using System;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Holds a sample data vector pair and its identifier.
    /// </summary>
    public class Sample
    {
        //Attribute properties
        /// <summary>
        /// Sample ID.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Input vector.
        /// </summary>
        public double[] InputVector { get; }

        /// <summary>
        /// Output vector.
        /// </summary>
        public double[] OutputVector { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="id">Sample ID.</param>
        /// <param name="inputVector">Input vector.</param>
        /// <param name="outputVector">Output vector.</param>
        public Sample(int id, double[] inputVector, double[] outputVector)
        {
            ID = id;
            InputVector = inputVector ?? throw new ArgumentNullException(nameof(inputVector));
            OutputVector = outputVector ?? throw new ArgumentNullException(nameof(outputVector));
            return;
        }

        /// <summary>
        /// Shallow copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public Sample(Sample source)
            : this(source.ID, source.InputVector, source.OutputVector)
        {
            return;
        }

        //Static methods
        /// <summary>
        /// Comparer for samples sorting in ascending order of by sample ID.
        /// </summary>
        /// <param name="item1">Sample 1.</param>
        /// <param name="item2">Sample 2.</param>
        /// <returns></returns>
        public static int IDComparer(Sample item1, Sample item2)
        {
            return Math.Sign((double)(item1.ID - item2.ID));
        }

        //Methods
        /// <summary>
        /// Creates a shallow clone.
        /// </summary>
        public Sample ShallowClone()
        {
            return new Sample(this);
        }

    }//FlatSample

}//Namespace
