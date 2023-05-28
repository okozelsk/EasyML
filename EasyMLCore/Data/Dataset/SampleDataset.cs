using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements sample dataset of identifiable samples of input and output vector pairs.
    /// </summary>
    [Serializable]
    public class SampleDataset : SerializableObject
    {
        //Enums
        /// <summary>
        /// Specifies where are output features in csv data row.
        /// </summary>
        public enum CsvOutputFeaturesPosition
        {
            /// <summary>
            /// Csv data row begins with output features.
            /// </summary>
            First,
            /// <summary>
            /// Csv data row ends with output features.
            /// </summary>
            Last
        }

        /// <summary>
        /// Specifies how are output features presented in csv data row.
        /// </summary>
        public enum CsvOutputFeaturesPresence
        {
            /// <summary>
            /// Each output feature has its own output value. In case of classification task, each class has own 0/1 column.
            /// </summary>
            Separately,
            /// <summary>
            /// Task is a classification and classes are represented as a 0-based index.
            /// </summary>
            ClassesAsNumberFrom0,
            /// <summary>
            /// Task is a classification and classes are represented as a 1-based index.
            /// </summary>
            ClassesAsNumberFrom1
        }

        //Constants
        /// <summary>
        /// The maximum ratio of one data fold.
        /// </summary>
        public const double MaxRatioOfFoldData = 0.5d;


        //Attribute properties
        /// <summary>
        /// The collection of vector pair samples.
        /// </summary>
        public List<Sample> SampleCollection { get; }

        //Attributes
        private readonly Dictionary<int, Sample> _sampleIDRef;

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public SampleDataset()
        {
            SampleCollection = new List<Sample>();
            _sampleIDRef = new Dictionary<int, Sample>();
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="expectedNumOfSamples">Expected number of vector pair samples.</param>
        public SampleDataset(int expectedNumOfSamples)
        {
            SampleCollection = new List<Sample>(expectedNumOfSamples);
            _sampleIDRef = new Dictionary<int, Sample>(expectedNumOfSamples);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The collection of vector pair samples.</param>
        public SampleDataset(ICollection<Sample> sampleCollection)
            : this(sampleCollection.Count)
        {
            foreach (Sample sample in sampleCollection)
            {
                AddSample(sample);
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="datasetCollection">The collection of datasets (folds).</param>
        public SampleDataset(ICollection<SampleDataset> datasetCollection)
            : this()
        {
            foreach (SampleDataset dataset in datasetCollection)
            {
                foreach (Sample sample in dataset.SampleCollection)
                {
                    AddSample(sample);
                }
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputVectorCollection">The collection of input vectors.</param>
        /// <param name="outputVectorCollection">The collection of output vectors.</param>
        public SampleDataset(IList<double[]> inputVectorCollection, IList<double[]> outputVectorCollection)
            : this(Math.Min(inputVectorCollection.Count, outputVectorCollection.Count))
        {
            int count = Math.Min(inputVectorCollection.Count, outputVectorCollection.Count);
            for (int i = 0; i < count; i++)
            {
                AddSampleInternal(new Sample(i, inputVectorCollection[i], outputVectorCollection[i]));
            }
            return;
        }

        //Properties
        /// <summary>
        /// Checks basic consistency.
        /// </summary>
        public bool IsConsistent
        {
            get
            {
                if (SampleCollection.Count > 0)
                {
                    int outputLength = SampleCollection[0].OutputVector.Length;
                    for (int i = 1; i < SampleCollection.Count; i++)
                    {
                        if (SampleCollection[i].OutputVector.Length != outputLength)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Checks equality of input vector lengths and output vector lengths.
        /// </summary>
        public bool IsUniform
        {
            get
            {
                if (SampleCollection.Count > 0)
                {
                    int inputLength = SampleCollection[0].InputVector.Length;
                    int outputLength = SampleCollection[0].OutputVector.Length;
                    for (int i = 1; i < SampleCollection.Count; i++)
                    {
                        if (SampleCollection[i].InputVector.Length != inputLength ||
                            SampleCollection[i].OutputVector.Length != outputLength
                            )
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets number of contained sample vector pairs.
        /// </summary>
        public int Count { get { return SampleCollection.Count; } }

        /// <summary>
        /// Gets input vector length of the first sample in the collection
        /// or -1 if there are no samples.
        /// </summary>
        public int FirstInputVectorLength
        {
            get
            {
                if (Count > 0)
                {
                    return SampleCollection[0].InputVector.Length;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets output vector length of the first sample in the collection
        /// or 0 if there are no samples.
        /// </summary>
        public int FirstOutputVectorLength
        {
            get
            {
                if (Count > 0)
                {
                    return SampleCollection[0].OutputVector.Length;
                }
                else
                {
                    return 0;
                }
            }
        }


        //Static methods
        /// <summary>
        /// Loads a dataset from csv data.
        /// </summary>
        /// <param name="csvData">Csv data holder.</param>
        /// <param name="outputFeaturesPosition">Specifies where are output features in csv data row.</param>
        /// <param name="outputFeaturesPresence">Specifies how are output features presented in csv data row.</param>
        /// <param name="numOfOutputFeatures">Number of output features.</param>
        public static SampleDataset Load(CsvDataHolder csvData,
                                         CsvOutputFeaturesPosition outputFeaturesPosition,
                                         CsvOutputFeaturesPresence outputFeaturesPresence,
                                         int numOfOutputFeatures
                                         )
        {
            SampleDataset dataset = new SampleDataset();
            int numOfOutputFeaturesInCsv = outputFeaturesPresence == CsvOutputFeaturesPresence.Separately ? numOfOutputFeatures : 1;
            foreach (DelimitedStringValues dataRow in csvData.DataRowCollection)
            {
                int numOfInputValues = dataRow.NumOfStringValues - numOfOutputFeaturesInCsv;
                //Check data length
                if (numOfInputValues <= 0)
                {
                    throw new ArgumentException("Incorrect length of data row.", nameof(csvData));
                }
                //Input data
                int inputDataOffset = outputFeaturesPosition == CsvOutputFeaturesPosition.First ? numOfOutputFeaturesInCsv : 0;
                double[] inputData = new double[numOfInputValues];
                for (int i = 0; i < numOfInputValues; i++)
                {
                    inputData[i] = dataRow.GetValueAt(inputDataOffset + i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValueAt(inputDataOffset + i)}.");
                }
                //Output data
                int outputDataOffset = outputFeaturesPosition == CsvOutputFeaturesPosition.First ? 0 : numOfInputValues;
                double[] outputData = new double[numOfOutputFeatures];
                if(outputFeaturesPresence == CsvOutputFeaturesPresence.Separately)
                {
                    for (int i = 0; i < numOfOutputFeaturesInCsv; i++)
                    {
                        outputData[i] = dataRow.GetValueAt(outputDataOffset + i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValueAt(outputDataOffset + i)}.");
                    }
                }
                else
                {
                    int classIndex = (int)dataRow.GetValueAt(outputDataOffset).ParseDouble(true, $"Can't parse class index {dataRow.GetValueAt(outputDataOffset)}.");
                    if(outputFeaturesPresence == CsvOutputFeaturesPresence.ClassesAsNumberFrom1)
                    {
                        --classIndex;
                    }
                    if(classIndex < 0 || classIndex >= outputData.Length)
                    {
                        throw new ApplicationException($"Invalid class index {classIndex} at row {dataset.Count + 1}.");
                    }
                    outputData[classIndex] = 1d;
                }
                dataset.AddSample(dataset.Count, inputData, outputData);
            }
            return dataset;
        }

        /// <summary>
        /// Loads time-serie csv data (each row = 1 timepoint) and creates patternized sample dataset.
        /// </summary>
        /// <param name="csvData">Csv data holder.</param>
        /// <param name="numOfInputTimePoints">Requested number of timepoints in an input pattern.</param>
        /// <param name="outputFieldNameCollection">Output field names.</param>
        /// <param name="inputFieldNameCollection">Input field names (when null, output field names are used).</param>
        /// <remarks>
        /// Useable for time-series regression task.
        /// In resulting dataset, multivariate flat input vector is always in a groupped form:
        /// {v1[t1],v2[t1],v1[t2],v2[t2],v1[t3],v2[t3],...}
        /// where "v" means variable and "t" means time point.
        /// </remarks>
        public static SampleDataset LoadAndPatternize(CsvDataHolder csvData,
                                                     int numOfInputTimePoints,
                                                     List<string> outputFieldNameCollection,
                                                     List<string> inputFieldNameCollection = null
                                                     )
        {
            //Indexes of input/output fields
            List<int> inputFieldIndexes = new List<int>();
            List<int> outputFieldIndexes = new List<int>();
            //Collect indexes of output fields
            foreach (string name in outputFieldNameCollection)
            {
                int fieldIdx = csvData.ColNameCollection.IndexOf(name);
                if (fieldIdx == -1)
                {
                    throw new ArgumentException($"Output field name {name} was not found in the csv data column names.", nameof(csvData));
                }
                outputFieldIndexes.Add(fieldIdx);
            }
            //Input fields
            if (inputFieldNameCollection == null)
            {
                //Not specified -> use output fields
                inputFieldIndexes = new List<int>(outputFieldIndexes);
            }
            else
            {
                //Collect indexes of input fields
                foreach (string name in inputFieldNameCollection)
                {
                    int fieldIdx = csvData.ColNameCollection.IndexOf(name);
                    if (fieldIdx == -1)
                    {
                        throw new ArgumentException($"Input field name {name} was not found in the csv data column names.", nameof(csvData));
                    }
                    inputFieldIndexes.Add(fieldIdx);
                }
            }
            //Patternize data
            //Prepare input and output vectors
            List<double[]> inputVectorCollection = new List<double[]>(csvData.DataRowCollection.Count);
            List<double[]> outputVectorCollection = new List<double[]>(csvData.DataRowCollection.Count);
            for (int masterRowIdx = 0; masterRowIdx < csvData.DataRowCollection.Count - numOfInputTimePoints; masterRowIdx++)
            {
                //Input vector
                double[] inputVector = new double[inputFieldIndexes.Count * numOfInputTimePoints];
                for (int timepointRowSubIdx = 0, vectorIdx = 0; timepointRowSubIdx < numOfInputTimePoints; timepointRowSubIdx++)
                {
                    for (int i = 0; i < inputFieldIndexes.Count; i++, vectorIdx++)
                    {
                        string value = csvData.DataRowCollection[masterRowIdx + timepointRowSubIdx].GetValueAt(inputFieldIndexes[i]);
                        inputVector[vectorIdx] = value.ParseDouble(true, $"Can't parse double value {value}.");
                    }
                }
                inputVectorCollection.Add(inputVector);
                //Output vector
                double[] outputVector = new double[outputFieldIndexes.Count];
                for (int i = 0; i < outputFieldIndexes.Count; i++)
                {
                    string value = csvData.DataRowCollection[masterRowIdx + numOfInputTimePoints].GetValueAt(outputFieldIndexes[i]);
                    outputVector[i] = value.ParseDouble(true, $"Can't parse double value {value}.");
                }
                outputVectorCollection.Add(outputVector);
            }
            //Create and return new dataset
            return new SampleDataset(inputVectorCollection, outputVectorCollection);
        }

        /// <summary>
        /// Creates dataset from time-serie csv data.
        /// </summary>
        /// <param name="csvData">Csv data holder.</param>
        /// <param name="inputFieldNameCollection">Input field names.</param>
        /// <param name="outputFieldNameCollection">Output field names.</param>
        /// <param name="remainingInputVector">The last unused input vector (next input).</param>
        /// <remarks>
        /// Useable for time-series regression task.
        /// </remarks>
        public static SampleDataset Load(CsvDataHolder csvData,
                                         List<string> inputFieldNameCollection,
                                         List<string> outputFieldNameCollection,
                                         out double[] remainingInputVector
                                         )
        {
            remainingInputVector = null;
            List<int> inputFieldIndexes = new List<int>();
            List<int> outputFieldIndexes = new List<int>();
            if (inputFieldNameCollection != null)
            {
                //Check the number of fields
                if (csvData.ColNameCollection.NumOfStringValues < inputFieldNameCollection.Count)
                {
                    throw new ArgumentException("The number of column names in csv data is less than the number of the input fields.", nameof(csvData));
                }
                //Collect indexes of allowed input fields
                foreach (string name in inputFieldNameCollection)
                {
                    int fieldIdx = csvData.ColNameCollection.IndexOf(name);
                    if (fieldIdx == -1)
                    {
                        throw new ArgumentException($"The input field name {name} was not found in the csv data column names.", nameof(csvData));
                    }
                    inputFieldIndexes.Add(fieldIdx);
                }
            }
            else
            {
                int[] indexes = new int[csvData.ColNameCollection.NumOfStringValues];
                indexes.Indices();
                inputFieldIndexes = new List<int>(indexes);
            }
            for (int i = 0; i < outputFieldNameCollection.Count; i++)
            {
                int fieldIdx = csvData.ColNameCollection.IndexOf(outputFieldNameCollection[i]);
                if (fieldIdx == -1)
                {
                    throw new ArgumentException($"The output field name {outputFieldNameCollection[i]} was not found in the csv data column names.", nameof(csvData));
                }
                outputFieldIndexes.Add(fieldIdx);
            }
            //Prepare input and output vectors
            List<double[]> inputVectorCollection = new List<double[]>(csvData.DataRowCollection.Count);
            List<double[]> outputVectorCollection = new List<double[]>(csvData.DataRowCollection.Count);
            for (int i = 0; i < csvData.DataRowCollection.Count; i++)
            {
                //Input vector
                double[] inputVector = new double[inputFieldIndexes.Count];
                for (int j = 0; j < inputFieldIndexes.Count; j++)
                {
                    inputVector[j] = csvData.DataRowCollection[i].GetValueAt(inputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {csvData.DataRowCollection[i].GetValueAt(inputFieldIndexes[j])}.");
                }
                if (i < csvData.DataRowCollection.Count - 1)
                {
                    //Within the dataset
                    inputVectorCollection.Add(inputVector);
                }
                else
                {
                    //Remaining input vector out of the dataset
                    remainingInputVector = inputVector;
                }
                if (i > 0)
                {
                    //Output vector
                    double[] outputVector = new double[outputFieldIndexes.Count];
                    for (int j = 0; j < outputFieldIndexes.Count; j++)
                    {
                        outputVector[j] = csvData.DataRowCollection[i].GetValueAt(outputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {csvData.DataRowCollection[i].GetValueAt(outputFieldIndexes[j])}.");
                    }
                    outputVectorCollection.Add(outputVector);
                }
            }
            //Create and return dataset
            return new SampleDataset(inputVectorCollection, outputVectorCollection);
        }

        /// <summary>
        /// Loads csv datafile containing time-serie data where each row
        /// contains variable(features) data of one time point.
        /// Then converts loaded time-serie data so that input vector contains
        /// features data from specified number of time points and
        /// output vector contains features data from immediately followed time point.
        /// Then splits data to the training and testing datasets and
        /// saves them as two csv files (training and testing).
        /// Output features are at the end of data line.
        /// </summary>
        /// <remarks>
        /// Useable for regression tasks when you want to work with fixed-length patterns instead of continuous time-series.
        /// </remarks>
        /// <param name="timeSeriesDataFile">The name of a csv datafile containing the time-serie data.</param>
        /// <param name="featureNames">The names of features to be used from every time-serie time-point (for both input and output vectors).</param>
        /// <param name="numOfInputTimePoints">Specifies how many time-points of time-serie should constitute input vector.</param>
        /// <param name="testDataRatio">Specifies what ratio from all data to use as the testing data.</param>
        /// <param name="outputTrainDataFile">The name of a csv datafile where to save training data.</param>
        /// <param name="outputTestDataFile">The name of a csv datafile where to save testing data.</param>
        /// <param name="delimiter">Data items delimiter.</param>
        public static void LoadPatternizeAndSave(string timeSeriesDataFile,
                                                 List<string> featureNames,
                                                 int numOfInputTimePoints,
                                                 double testDataRatio,
                                                 string outputTrainDataFile,
                                                 string outputTestDataFile,
                                                 char delimiter = CsvDataHolder.DefaultDelimiter
                                                 )
        {
            //Time series data
            CsvDataHolder csvData = new CsvDataHolder(timeSeriesDataFile);
            SampleDataset allData = SampleDataset.LoadAndPatternize(csvData, numOfInputTimePoints, featureNames);
            //Split data to training and testing data
            int numOfTestingSamples = (int)Math.Round(allData.Count * testDataRatio, MidpointRounding.AwayFromZero);
            if (numOfTestingSamples < 1)
            {
                throw new ArgumentException("Too low testDataRatio or few data samples.", nameof(testDataRatio));
            }
            SampleDataset trainingData = new SampleDataset();
            SampleDataset testingData = new SampleDataset();
            for (int i = 0; i < allData.Count; i++)
            {
                if (i < allData.Count - numOfTestingSamples)
                {
                    trainingData.AddSample(trainingData.Count,
                                        (double[])allData.SampleCollection[i].InputVector.Clone(),
                                        (double[])allData.SampleCollection[i].OutputVector.Clone()
                                        );
                }
                else
                {
                    testingData.AddSample(testingData.Count,
                                        (double[])allData.SampleCollection[i].InputVector.Clone(),
                                        (double[])allData.SampleCollection[i].OutputVector.Clone()
                                        );
                }
            }
            //Save the data
            trainingData.SaveAsCsv(outputTrainDataFile, CsvOutputFeaturesPosition.Last, CsvOutputFeaturesPresence.Separately, delimiter);
            testingData.SaveAsCsv(outputTestDataFile, CsvOutputFeaturesPosition.Last, CsvOutputFeaturesPresence.Separately, delimiter);
            return;
        }

        //Methods
        /// <summary>
        /// Saves dataset as csv file.
        /// </summary>
        /// <param name="fileName">Name of the output csv file.</param>
        /// <param name="outputFeaturesPosition">Specifies where are output features in csv data row.</param>
        /// <param name="outputFeaturesPresence">Specifies how are output features presented in csv data row.</param>
        /// <param name="delimiter">Data delimiter to be used.</param>
        public void SaveAsCsv(string fileName,
                              CsvOutputFeaturesPosition outputFeaturesPosition,
                              CsvOutputFeaturesPresence outputFeaturesPresence,
                              char delimiter = CsvDataHolder.DefaultDelimiter
                              )
        {
            CsvDataHolder csvDataHolder = new CsvDataHolder(delimiter, null, Count);
            foreach (Sample sample in SampleCollection)
            {
                double[] outputValues;
                if (outputFeaturesPresence == CsvOutputFeaturesPresence.Separately)
                {
                    outputValues = sample.OutputVector;
                }
                else
                {
                    outputValues = new double[1];
                    outputValues[0] = sample.OutputVector.IndexOfMax(out _) + (outputFeaturesPresence == CsvOutputFeaturesPresence.ClassesAsNumberFrom1 ? 1 : 0);
                }
                double[] allValues = (double[])(outputFeaturesPosition == CsvOutputFeaturesPosition.First ? outputValues.Concat(sample.InputVector) : sample.InputVector.Concat(outputValues));
                DelimitedStringValues dsv = new DelimitedStringValues(allValues.Length);
                foreach (double value in allValues)
                {
                    dsv.AddValue(value.ToString(CultureInfo.InvariantCulture));
                }
                csvDataHolder.DataRowCollection.Add(dsv);
            }
            csvDataHolder.Save(fileName);
            return;
        }

        /// <summary>
        /// Data in input vectors is considered as a time series data.
        /// This function changes an order of data in input vectors to be organized according
        /// to given variable schema. See the TimeSeriesPattern class and its FlatVarSchema enum
        /// for detailed information about the multivariate schemas.
        /// </summary>
        /// <param name="numOfVariables">Number of varibles in input vector.</param>
        /// <param name="newFlatVarSchema">New multivariate schema to be applied.</param>
        /// <returns>New dataset with converted input patterns.</returns>
        /// <seealso cref="TimeSeriesPattern"/>
        public SampleDataset ConvertInputFlatVarSchema(int numOfVariables, TimeSeriesPattern.FlatVarSchema newFlatVarSchema)
        {
            if(numOfVariables <= 0 || FirstInputVectorLength % numOfVariables != 0)
            {
                throw new ArgumentException($"Inconsistent or invalid specified number of variables ({numOfVariables}) for input vector length ({FirstInputVectorLength})", nameof(numOfVariables));
            }
            //Loop and convert samples
            SampleDataset dataset = new SampleDataset();
            foreach(Sample sample in SampleCollection)
            {
                TimeSeriesPattern tsp = new TimeSeriesPattern(sample.InputVector,
                                                              numOfVariables,
                                                              newFlatVarSchema == TimeSeriesPattern.FlatVarSchema.VarSequence ? TimeSeriesPattern.FlatVarSchema.Groupped : TimeSeriesPattern.FlatVarSchema.VarSequence
                                                              );
                double[] convertedInputVector = tsp.Flattenize(newFlatVarSchema);
                dataset.AddSample(sample.ID, convertedInputVector, sample.OutputVector);
            }
            return dataset;
        }

        /// <summary>
        /// Adds specified sample instance directly.
        /// </summary>
        /// <param name="sample">A sample instance to be directly added.</param>
        private void AddSampleInternal(Sample sample)
        {
            try
            {
                _sampleIDRef.Add(sample.ID, sample);
            }
            catch (Exception)
            {
                throw new ArgumentException("Sample with the same ID already exists.", nameof(sample));
            }
            SampleCollection.Add(sample);
            return;
        }

        /// <summary>
        /// Adds new sample into the dataset.
        /// </summary>
        /// <param name="sample">A sample to be added.</param>
        public void AddSample(Sample sample)
        {
            AddSampleInternal(new Sample(sample));
            return;
        }

        /// <summary>
        /// Adds new sample into the dataset.
        /// </summary>
        /// <param name="id">Sample ID.</param>
        /// <param name="inputVector">Input vector.</param>
        /// <param name="outputVector">Output vector.</param>
        public void AddSample(int id, double[] inputVector, double[] outputVector)
        {
            AddSampleInternal(new Sample(id, inputVector, outputVector));
            return;
        }

        /// <summary>
        /// Gets sample by sample ID.
        /// </summary>
        /// <param name="sampleID">Sample ID.</param>
        public Sample GetSample(int sampleID)
        {
            if (_sampleIDRef.TryGetValue(sampleID, out Sample sample))
            {
                return sample;
            }
            return null;
        }

        /// <summary>
        /// Sorts samples in ascending order by ID.
        /// </summary>
        public void SortByID()
        {
            SampleCollection.Sort(Sample.IDComparer);
            return;
        }

        /// <summary>
        /// Adds all samples from another dataset.
        /// </summary>
        /// <param name="dataset">Another dataset.</param>
        public void Add(SampleDataset dataset)
        {
            foreach (Sample sample in dataset.SampleCollection)
            {
                AddSample(sample);
            }
            return;
        }

        /// <summary>
        /// Creates a shallow clone.
        /// </summary>
        public SampleDataset ShallowClone()
        {
            return new SampleDataset(SampleCollection);
        }

        /// <summary>
        /// Randomly shuffles samples.
        /// </summary>
        /// <param name="rand">Random generator to be used.</param>
        public void Shuffle(Random rand)
        {
            List<Sample> tmp = new List<Sample>(SampleCollection);
            SampleCollection.Clear();
            int[] shuffledIndices = new int[tmp.Count];
            shuffledIndices.Indices();
            rand.Shuffle(shuffledIndices);
            for (int i = 0; i < shuffledIndices.Length; i++)
            {
                SampleCollection.Add(tmp[shuffledIndices[i]]);
            }
            return;
        }

        /// <summary>
        /// Simply minces this dataset to sub-datasets having specified nuber of samples.
        /// Note that the last sub-dataset can have smaller number of samples.
        /// </summary>
        /// <param name="numOfSubDatasetSamples">Desired nuber of samples in sub-dataset.</param>
        /// <returns>List of smaller sub-datasets.</returns>
        public List<SampleDataset> Folderize(int numOfSubDatasetSamples)
        {
            if (numOfSubDatasetSamples <= 0) numOfSubDatasetSamples = 1;
            int numOfSubDatasets = Count / numOfSubDatasetSamples;
            if (numOfSubDatasets == 0) ++numOfSubDatasets;
            if (numOfSubDatasets * numOfSubDatasetSamples < Count) ++numOfSubDatasets;
            List<SampleDataset> subDatasets = new List<SampleDataset>(numOfSubDatasets);
            for (int subDatasetIdx =  0, sampleIdx = 0; subDatasetIdx < numOfSubDatasets; subDatasetIdx++)
            {
                SampleDataset subDataset = new SampleDataset(numOfSubDatasetSamples);
                for(int i = 0; i < numOfSubDatasetSamples && sampleIdx < Count; i++, sampleIdx++)
                {
                    subDataset.AddSample(SampleCollection[sampleIdx]);
                }
                subDatasets.Add(subDataset);
            }
            return subDatasets;
        }

        /// <summary>
        /// Minces this dataset to a collection of smaller folds (sub-datasets).
        /// </summary>
        /// <remarks>
        /// When output task is Categorical then is checked consistency that always only one feature is true
        /// in the output vector.
        /// When output task is Categorical then is ensured that every fold contains all categories in the +- same distribution as on whole dataset.
        /// When output task is Binary with one output feature then is ensured that every fold contains 0 and 1 in the +- same distribution as on whole dataset.
        /// </remarks>
        /// <param name="foldDataRatio">Requested samples ratio constituting single fold (sub-dataset).</param>
        /// <param name="taskType">Type of computation output task (purpose of folderization).</param>
        /// <returns>Created folds (sub-datasets).</returns>
        public List<SampleDataset> Folderize(double foldDataRatio, OutputTaskType taskType)
        {
            if (Count < 2)
            {
                throw new InvalidOperationException($"Insufficient number of samples ({Count.ToString(CultureInfo.InvariantCulture)}). Minimum is 2.");
            }
            int numOfOutputs = FirstOutputVectorLength;
            List<SampleDataset> foldCollection = new List<SampleDataset>();
            //Fold data ratio basic correction
            if (foldDataRatio > MaxRatioOfFoldData)
            {
                foldDataRatio = MaxRatioOfFoldData;
            }
            //Prelimitary fold size estimation
            int foldSize = Math.Max(1, (int)Math.Round(Count * foldDataRatio, 0, MidpointRounding.AwayFromZero));
            //Prelimitary number of folds
            int numOfFolds = (int)Math.Round((double)Count / foldSize, MidpointRounding.AwayFromZero);
            //Folds creation
            if (taskType == OutputTaskType.Regression || (taskType == OutputTaskType.Binary && numOfOutputs > 1))
            {
                //Simple split
                int samplesPos = 0;
                for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                {
                    SampleDataset fold = new SampleDataset(foldSize);
                    for (int i = 0; i < foldSize && samplesPos < Count; i++)
                    {
                        fold.AddSample(SampleCollection[samplesPos]);
                        ++samplesPos;
                    }
                    foldCollection.Add(fold);
                }
                //Remaining samples
                for (int i = 0; i < Count - samplesPos; i++)
                {
                    int foldIdx = i % foldCollection.Count;
                    foldCollection[foldIdx].AddSample(SampleCollection[samplesPos + i]);
                }
            }//Non-balanced output
            else
            {
                double binBorder = BinFeatureFilter.GetBinaryBorder(FeatureFilterBase.FeatureUse.Output);
                //Keep balanced 0/1 ratios on output
                if (numOfOutputs == 1)
                {
                    //Only one binary output
                    //Investigation of the output data metrics
                    BinDistribution refBinDistr = new BinDistribution(binBorder);
                    refBinDistr.Update(from sample in SampleCollection select sample.OutputVector, 0);
                    int min01 = Math.Min(refBinDistr.NumOf[0], refBinDistr.NumOf[1]);
                    if (min01 < 2)
                    {
                        throw new InvalidOperationException($"Insufficient bin 0 or 1 samples (less than 2).");
                    }
                    if (numOfFolds > min01)
                    {
                        numOfFolds = min01;
                    }
                    //Scan data
                    int[] bin0SampleIdxs = new int[refBinDistr.NumOf[0]];
                    int bin0SamplesPos = 0;
                    int[] bin1SampleIdxs = new int[refBinDistr.NumOf[1]];
                    int bin1SamplesPos = 0;
                    for (int i = 0; i < Count; i++)
                    {
                        if (SampleCollection[i].OutputVector[0] >= refBinDistr.BinBorder)
                        {
                            bin1SampleIdxs[bin1SamplesPos++] = i;
                        }
                        else
                        {
                            bin0SampleIdxs[bin0SamplesPos++] = i;
                        }
                    }
                    //Determine distributions of 0 and 1 for one fold
                    int datasetBin0Count = Math.Max(1, refBinDistr.NumOf[0] / numOfFolds);
                    int datasetBin1Count = Math.Max(1, refBinDistr.NumOf[1] / numOfFolds);
                    //Datasets creation
                    bin0SamplesPos = 0;
                    bin1SamplesPos = 0;
                    for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                    {
                        SampleDataset fold = new SampleDataset();
                        //Bin 0
                        for (int i = 0; i < datasetBin0Count; i++)
                        {
                            fold.AddSample(SampleCollection[bin0SampleIdxs[bin0SamplesPos]]);
                            ++bin0SamplesPos;
                        }
                        //Bin 1
                        for (int i = 0; i < datasetBin1Count; i++)
                        {
                            fold.AddSample(SampleCollection[bin1SampleIdxs[bin1SamplesPos]]);
                            ++bin1SamplesPos;
                        }
                        foldCollection.Add(fold);
                    }
                    //Remaining samples
                    for (int i = 0; i < bin0SampleIdxs.Length - bin0SamplesPos; i++)
                    {
                        int foldIdx = i % foldCollection.Count;
                        foldCollection[foldIdx].AddSample(SampleCollection[bin0SampleIdxs[bin0SamplesPos + i]]);
                    }
                    for (int i = 0; i < bin1SampleIdxs.Length - bin1SamplesPos; i++)
                    {
                        int foldIdx = i % foldCollection.Count;
                        foldCollection[foldIdx].AddSample(SampleCollection[bin1SampleIdxs[bin1SamplesPos + i]]);
                    }
                }//Only 1 binary output
                else
                {
                    //There is more than 1 binary output -> classification
                    //Investigation of the output data metrics
                    //Collect bin 1 sample indexes and check one truth consistency
                    List<int>[] outBin1SampleIdxs = new List<int>[numOfOutputs];
                    for (int i = 0; i < numOfOutputs; i++)
                    {
                        outBin1SampleIdxs[i] = new List<int>();
                    }
                    for (int sampleIdx = 0; sampleIdx < Count; sampleIdx++)
                    {
                        int numOf1 = 0;
                        for (int outFeatureIdx = 0; outFeatureIdx < numOfOutputs; outFeatureIdx++)
                        {
                            if (SampleCollection[sampleIdx].OutputVector[outFeatureIdx] >= binBorder)
                            {
                                outBin1SampleIdxs[outFeatureIdx].Add(sampleIdx);
                                ++numOf1;
                            }
                        }
                        if (numOf1 != 1)
                        {
                            throw new ApplicationException($"Inconsistency on data index {sampleIdx.ToString(CultureInfo.InvariantCulture)}. Output vector has {numOf1.ToString(CultureInfo.InvariantCulture)} feature(s) having bin value 1.");
                        }
                    }
                    //Determine max possible number of folds
                    int maxNumOfFolds = Count;
                    for (int outFeatureIdx = 0; outFeatureIdx < numOfOutputs; outFeatureIdx++)
                    {
                        int outFeatureMaxFolds = Math.Min(outBin1SampleIdxs[outFeatureIdx].Count, Count - outBin1SampleIdxs[outFeatureIdx].Count);
                        maxNumOfFolds = Math.Min(outFeatureMaxFolds, maxNumOfFolds);
                    }
                    //Correct the number of folds to be created
                    if (numOfFolds > maxNumOfFolds)
                    {
                        numOfFolds = maxNumOfFolds;
                    }
                    //Create the folds
                    for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                    {
                        foldCollection.Add(new SampleDataset());
                    }
                    //Samples distribution
                    for (int outFeatureIdx = 0; outFeatureIdx < numOfOutputs; outFeatureIdx++)
                    {
                        for (int bin1SampleRefIdx = 0; bin1SampleRefIdx < outBin1SampleIdxs[outFeatureIdx].Count; bin1SampleRefIdx++)
                        {
                            int foldIdx = bin1SampleRefIdx % foldCollection.Count;
                            int dataIdx = outBin1SampleIdxs[outFeatureIdx][bin1SampleRefIdx];
                            foldCollection[foldIdx].AddSample(SampleCollection[dataIdx]);
                        }
                    }
                }//More binary outputs
            }//Balanced binary output

            return foldCollection;
        }

        /// <summary>
        /// Splits dataset to two parts.
        /// Keeps the samples order and IDs.
        /// </summary>
        /// <param name="numOfSamplesInSecondDataset">Required number of samples in the second part.</param>
        /// <param name="firstDataset">The first part dataset.</param>
        /// <param name="secondDataset">The second part dataset.</param>
        public void Split(int numOfSamplesInSecondDataset, out SampleDataset firstDataset, out SampleDataset secondDataset)
        {
            if(numOfSamplesInSecondDataset <= 0)
            {
                throw new ArgumentException($"Requested number of samples in second part dataset has to be GT 0. Received: {numOfSamplesInSecondDataset}.", nameof(numOfSamplesInSecondDataset));
            }
            if (numOfSamplesInSecondDataset >= Count)
            {
                throw new ArgumentException($"Requested number of samples in second part dataset has to be LT total number of samples: {Count}. Received {numOfSamplesInSecondDataset}.", nameof(numOfSamplesInSecondDataset));
            }
            firstDataset = new SampleDataset(SampleCollection.GetRange(0, Count - numOfSamplesInSecondDataset));
            secondDataset = new SampleDataset(SampleCollection.GetRange(Count - numOfSamplesInSecondDataset, numOfSamplesInSecondDataset));
            return;
        }

        /// <summary>
        /// Prepares input and output feature filters.
        /// </summary>
        /// <param name="taskType">Network's output task type.</param>
        /// <param name="inputFilters">Prepared input filters.</param>
        /// <param name="outputFilters">Prepared output filters.</param>
        public void PrepareFeatureFilters(OutputTaskType taskType,
                                          out FeatureFilterBase[] inputFilters,
                                          out FeatureFilterBase[] outputFilters
                                          )
        {
            if(!IsUniform)
            {
                throw new InvalidOperationException($"Dataset is not uniform so method can not be performed.");
            }
            //Input filters
            FeatureFilterBase[] iFilters = new FeatureFilterBase[FirstInputVectorLength];
            for (int i = 0; i < iFilters.Length; i++)
            {
                iFilters[i] = new RealFeatureFilter(FeatureFilterBase.FeatureUse.Input);
            }
            Parallel.For(0, iFilters.Length, featureIdx =>
            {
                for (int sampleIdx = 0; sampleIdx < SampleCollection.Count; sampleIdx++)
                {
                    iFilters[featureIdx].Update(SampleCollection[sampleIdx].InputVector[featureIdx]);
                }
            });
            inputFilters = iFilters;
            //Output filters
            FeatureFilterBase[] oFilters = new FeatureFilterBase[FirstOutputVectorLength];
            for (int i = 0; i < oFilters.Length; i++)
            {
                oFilters[i] = (taskType == OutputTaskType.Regression) ? (FeatureFilterBase)new RealFeatureFilter(FeatureFilterBase.FeatureUse.Output) : (FeatureFilterBase)new BinFeatureFilter(FeatureFilterBase.FeatureUse.Output);
            }
            Parallel.For(0, oFilters.Length, featureIdx =>
            {
                for (int sampleIdx = 0; sampleIdx < SampleCollection.Count; sampleIdx++)
                {
                    oFilters[featureIdx].Update(SampleCollection[sampleIdx].OutputVector[featureIdx]);
                }
            });
            outputFilters = oFilters;
            return;
        }


        /// <summary>
        /// Creates new dataset from this dataset, prepares in/out filters and standardize data.
        /// </summary>
        /// <param name="taskType">Network's output task type.</param>
        /// <param name="inputFilters">Prepared input filters.</param>
        /// <param name="outputFilters">Prepared output filters.</param>
        /// <param name="centered">Specifies whether to center value between -1 an 1, so min value is -1 and max value is 1. If false, 0 is not the interval center but represents the average value and -1 or 1 represents the magnitude.</param>
        public SampleDataset CreateStandardized(OutputTaskType taskType,
                                                out FeatureFilterBase[] inputFilters,
                                                out FeatureFilterBase[] outputFilters,
                                                bool centered
                                                )
        {
            PrepareFeatureFilters(taskType,
                                  out inputFilters,
                                  out outputFilters
                                  );
            double[][] stdInputs = new double[SampleCollection.Count][];
            double[][] stdOutputs = new double[SampleCollection.Count][];
            for (int i = 0; i < SampleCollection.Count; i++)
            {
                stdInputs[i] = new double[FirstInputVectorLength];
                stdOutputs[i] = new double[FirstOutputVectorLength];
            }
            FeatureFilterBase[] iFilters = inputFilters;
            FeatureFilterBase[] oFilters = outputFilters;
            Parallel.For(0, iFilters.Length, featureIdx =>
            {
                for (int sampleIdx = 0; sampleIdx < SampleCollection.Count; sampleIdx++)
                {
                    stdInputs[sampleIdx][featureIdx] = iFilters[featureIdx].ApplyFilter(SampleCollection[sampleIdx].InputVector[featureIdx], centered);
                }
            });
            Parallel.For(0, oFilters.Length, featureIdx =>
            {
                for (int sampleIdx = 0; sampleIdx < SampleCollection.Count; sampleIdx++)
                {
                    stdOutputs[sampleIdx][featureIdx] = oFilters[featureIdx].ApplyFilter(SampleCollection[sampleIdx].OutputVector[featureIdx], centered);
                }
            });
            SampleDataset stdDataset = new SampleDataset(SampleCollection.Count);
            for (int sampleIdx = 0; sampleIdx < SampleCollection.Count; sampleIdx++)
            {
                stdDataset.AddSampleInternal(new Sample(SampleCollection[sampleIdx].ID, stdInputs[sampleIdx], stdOutputs[sampleIdx]));
            }
            return stdDataset;
        }

        /// <summary>
        /// Returns list of all input vectors in the same order as they are in inner collection.
        /// </summary>
        public List<double[]> GettInputVectors()
        {
            List<double[]> inputVectors = new List<double[]>(Count);
            foreach(Sample sample in SampleCollection)
            {
                inputVectors.Add(sample.InputVector);
            }
            return inputVectors;
        }

        /// <summary>
        /// Returns list of all output vectors in the same order as they are in inner collection.
        /// </summary>
        public List<double[]> GettOutputVectors()
        {
            List<double[]> outputVectors = new List<double[]>(Count);
            foreach (Sample sample in SampleCollection)
            {
                outputVectors.Add(sample.OutputVector);
            }
            return outputVectors;
        }

        /// <summary>
        /// Based on original datasets creates new variant of training and testing dataset.
        /// For Binary and Categorical task types method keeps samples distribution.
        /// Note that original sample's ID is not kept.
        /// </summary>
        /// <param name="rand">Random object to be used.</param>
        /// <param name="taskType">Type of the task.</param>
        /// <param name="origTrainingData">Original training dataset.</param>
        /// <param name="origTestingData">Original testing dataset.</param>
        /// <param name="newTrainingData">Created training dataset.</param>
        /// <param name="newTestingData">Created testing dataset.</param>
        public static void CreateShuffledSimilar(Random rand,
                                                 OutputTaskType taskType,
                                                 SampleDataset origTrainingData,
                                                 SampleDataset origTestingData,
                                                 out SampleDataset newTrainingData,
                                                 out SampleDataset newTestingData
                                                 )
        {
            //Checks
            if(rand == null)
            {
                throw new ArgumentNullException(nameof(rand));
            }
            if (origTrainingData == null)
            {
                throw new ArgumentNullException(nameof(origTrainingData));
            }
            if (origTestingData == null)
            {
                throw new ArgumentNullException(nameof(origTestingData));
            }
            if (!origTrainingData.IsConsistent)
            {
                throw new ArgumentException($"Original training data is not consistent.", nameof(origTrainingData));
            }
            if (!origTestingData.IsConsistent)
            {
                throw new ArgumentException($"Original test data is not consistent.", nameof(origTestingData));
            }
            if(origTrainingData.FirstOutputVectorLength != origTestingData.FirstOutputVectorLength)
            {
                throw new ArgumentException($"The original test data has a different output vector length than the original training data.", nameof(origTestingData));
            }
            //Allocations
            newTrainingData = new SampleDataset(origTrainingData.Count);
            newTestingData = new SampleDataset(origTestingData.Count);
            SampleDataset allSamples = new SampleDataset(origTrainingData.Count + origTestingData.Count);
            foreach (Sample sample in origTrainingData.SampleCollection)
            {
                allSamples.AddSample(allSamples.Count, sample.InputVector, sample.OutputVector);
            }
            foreach (Sample sample in origTestingData.SampleCollection)
            {
                allSamples.AddSample(allSamples.Count, sample.InputVector, sample.OutputVector);
            }
            allSamples.Shuffle(rand);
            if (taskType == OutputTaskType.Regression)
            {
                //Simple split
                for(int sampleNum = 0; sampleNum < allSamples.Count; sampleNum++)
                {
                    if(sampleNum < origTrainingData.Count)
                    {
                        newTrainingData.AddSample(allSamples.SampleCollection[sampleNum]);
                    }
                    else
                    {
                        newTestingData.AddSample(allSamples.SampleCollection[sampleNum]);
                    }
                }
            }
            else
            {
                //Balanced split
                LinkedList<Sample> linkedSamples = new LinkedList<Sample>(allSamples.SampleCollection);
                foreach(Sample origSample in origTrainingData.SampleCollection)
                {
                    foreach(Sample sample in linkedSamples)
                    {
                        //Compare output vectors
                        bool theSame = true;
                        for(int i = 0; i < origSample.OutputVector.Length; i++)
                        {
                            if (origSample.OutputVector[i] != sample.OutputVector[i])
                            {
                                theSame = false;
                                break;
                            }
                        }
                        if(theSame)
                        {
                            newTrainingData.AddSample(sample);
                            linkedSamples.Remove(sample);
                            break;
                        }
                    }
                }
                //Remaining samples are new testing samples
                foreach (Sample sample in linkedSamples)
                {
                    newTestingData.AddSample(sample);
                }
            }//taskType
            return;
        }

    }//SampleDataset

}//Namespace
