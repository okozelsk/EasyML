using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements dataset of input, computed and ideal (desired) data vector triplets.
    /// </summary>
    [Serializable]
    public class ResultDataset : SerializableObject
    {
        //Attributes
        /// <summary>
        /// The collection of input vectors.
        /// </summary>
        public List<double[]> InputVectorCollection { get; }

        /// <summary>
        /// The collection of computed vectors.
        /// </summary>
        public List<double[]> ComputedVectorCollection { get; }

        /// <summary>
        /// The collection of ideal vectors.
        /// </summary>
        public List<double[]> IdealVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputVectorCollection">A collection of input vectors.</param>
        /// <param name="computedVectorCollection">A collection of computed vectors.</param>
        /// <param name="idealVectorCollection">A collection of ideal vectors.</param>
        /// <remarks>Vectors in all three collections has to be in corresponding order.</remarks>
        public ResultDataset(IEnumerable<double[]> inputVectorCollection,
                            IEnumerable<double[]> computedVectorCollection,
                            IEnumerable<double[]> idealVectorCollection
                            )
        {
            InputVectorCollection = new List<double[]>(inputVectorCollection);
            ComputedVectorCollection = new List<double[]>(computedVectorCollection);
            IdealVectorCollection = new List<double[]>(idealVectorCollection);
            if(InputVectorCollection.Count != ComputedVectorCollection.Count ||
                InputVectorCollection.Count != IdealVectorCollection.Count)
            {
                throw new ArgumentException($"Number of vectors is not the same in all three collections.");
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="samples">A sample dataset.</param>
        /// <param name="computedVectorCollection">A collection of computed vectors.</param>
        /// <remarks>Samples and computed vectors has to be in corresponding order.</remarks>
        public ResultDataset(SampleDataset samples,
                            IEnumerable<double[]> computedVectorCollection
                            )
        {
            InputVectorCollection = new List<double[]>((from sample in samples.SampleCollection select sample.InputVector));
            ComputedVectorCollection = new List<double[]>(computedVectorCollection);
            IdealVectorCollection = new List<double[]>((from sample in samples.SampleCollection select sample.OutputVector));
            if (InputVectorCollection.Count != ComputedVectorCollection.Count ||
                InputVectorCollection.Count != IdealVectorCollection.Count)
            {
                throw new ArgumentException($"Number of samples and computed vectors is not the same.");
            }
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public ResultDataset()
        {
            InputVectorCollection = new List<double[]>();
            ComputedVectorCollection = new List<double[]>();
            IdealVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="expectedNumOfRows">The expected number of vector rows.</param>
        public ResultDataset(int expectedNumOfRows)
        {
            InputVectorCollection = new List<double[]>(expectedNumOfRows);
            ComputedVectorCollection = new List<double[]>(expectedNumOfRows);
            IdealVectorCollection = new List<double[]>(expectedNumOfRows);
            return;
        }

        /// <summary>
        /// Adds vectors triplet.
        /// </summary>
        /// <param name="inputVector">An input vector.</param>
        /// <param name="computedVector">A computed vector.</param>
        /// <param name="idealVector">An ideal vector.</param>
        public void AddVectors(double[] inputVector, double[] computedVector, double[] idealVector)
        {
            InputVectorCollection.Add(inputVector);
            ComputedVectorCollection.Add(computedVector);
            IdealVectorCollection.Add(idealVector);
            return;
        }


    }//ResultDataset

}//Namespace
