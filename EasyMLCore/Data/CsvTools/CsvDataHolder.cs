using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements the holder of csv data. Supports data loading and saving to a file.
    /// </summary>
    [Serializable]
    public class CsvDataHolder : SerializableObject
    {
        //Constants
        /// <summary>
        /// A special char code identifying the requirement for automatic detection of data delimiter.
        /// </summary>
        public const char AutoDetectDelimiter = (char)0;

        //Attribute properties
        /// <summary>
        /// Current delimiter of data items.
        /// </summary>
        public char DataDelimiter { get; private set; }

        /// <summary>
        /// Column names.
        /// </summary>
        public DelimitedStringValues ColNameCollection { get; private set; }

        /// <summary>
        /// Data rows.
        /// </summary>
        public List<DelimitedStringValues> DataRowCollection { get; private set; }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="delimiter">Data items delimiter.</param>
        public CsvDataHolder(char delimiter)
        {
            DataDelimiter = delimiter;
            ColNameCollection = new DelimitedStringValues(DataDelimiter);
            DataRowCollection = new List<DelimitedStringValues>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="streamReader">Data stream reader.</param>
        /// <param name="header">Specifies whether the first row contains the column names.</param>
        /// <param name="delimiter">Data items delimiter. If CsvDataHolder.AutoDetectDelimiter is specified then delimiter is recognized automatically from the data.</param>
        public CsvDataHolder(StreamReader streamReader, bool header, char delimiter = AutoDetectDelimiter)
        {
            InitFromStream(streamReader, header, delimiter);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="streamReader">Data stream reader.</param>
        /// <param name="delimiter">Data items delimiter. If CsvDataHolder.AutoDetectDelimiter is specified then delimiter is recognized automatically from the data.</param>
        public CsvDataHolder(StreamReader streamReader, char delimiter = AutoDetectDelimiter)
        {
            InitFromStream(streamReader, delimiter);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fileName">Name of the file containing data to be loaded.</param>
        /// <param name="header">Specifies whether the first row contains the column names.</param>
        /// <param name="delimiter">Data items delimiter. If CsvDataHolder.AutoDetectDelimiter is specified then delimiter is recognized automatically from the data.</param>
        public CsvDataHolder(string fileName, bool header, char delimiter = AutoDetectDelimiter)
        {
            using StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open));
            InitFromStream(streamReader, header, delimiter);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fileName">Name of the file containing data to be loaded.</param>
        /// <param name="delimiter">Data items delimiter. If CsvDataHolder.AutoDetectDelimiter is specified then delimiter is recognized automatically from the data.</param>
        public CsvDataHolder(string fileName, char delimiter = AutoDetectDelimiter)
        {
            var dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            fileName = Path.Combine(dir, fileName);
            using StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open));
            InitFromStream(streamReader, delimiter);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="dataset">Samples.</param>
        /// <param name="delimiter">Data items delimiter.</param>
        public CsvDataHolder(SampleDataset dataset, char delimiter = DelimitedStringValues.DefaultDelimiter)
        {
            InitFromSampleDataset(dataset, delimiter);
            return;
        }

        //Methods
        private void InitFromStream(StreamReader streamReader, bool header, char delimiter = AutoDetectDelimiter)
        {
            DataRowCollection = new List<DelimitedStringValues>();
            DataDelimiter = delimiter;
            AppendFromStream(streamReader);
            if (header && DataRowCollection.Count > 0)
            {
                ColNameCollection = DataRowCollection[0];
                DataRowCollection.RemoveAt(0);
            }
            else
            {
                ColNameCollection = new DelimitedStringValues(DataDelimiter);
            }
            return;
        }

        private void InitFromStream(StreamReader streamReader, char delimiter = AutoDetectDelimiter)
        {
            DataRowCollection = new List<DelimitedStringValues>();
            DataDelimiter = delimiter;
            AppendFromStream(streamReader);
            InitColNames();
            return;
        }

        private void InitFromSampleDataset(SampleDataset dataset, char delimiter = AutoDetectDelimiter)
        {
            DataRowCollection = new List<DelimitedStringValues>();
            //Set delimiter
            DataDelimiter = delimiter;
            //No col names
            ColNameCollection = new DelimitedStringValues(DataDelimiter);
            //Append data
            AppendFromSampleDataset(dataset);
            return;
        }

        private void AppendFromSampleDataset(SampleDataset dataset)
        {
            foreach(Sample sample in dataset.SampleCollection)
            {
                double[] data = (double[])sample.InputVector.Concat(sample.OutputVector);
                DelimitedStringValues dsv = new DelimitedStringValues(DataDelimiter);
                foreach(double value in data)
                {
                    dsv.AddValue(value.ToString());
                }
                DataRowCollection.Add(dsv);
            }
            return;
        }

        private static bool ContainsDataItems(DelimitedStringValues dsv)
        {
            foreach (string item in dsv.StringValueCollection)
            {
                //Numerical
                if (!double.IsNaN(item.ParseDouble(false)))
                {
                    return true;
                }
                else if (item.ParseInt(false) != int.MinValue)
                {
                    return true;
                }
                //Datetime
                else if (item.ParseDateTime(false) != DateTime.MinValue)
                {
                    return true;
                }
                //Boolean
                try
                {
                    item.ParseBool(true, "failed");
                    return true;
                }
                catch
                {
                    //Do nothing
                    ;
                }

            }
            return false;
        }

        /// <summary>
        /// Initializes column names.
        /// </summary>
        private void InitColNames()
        {
            if (DataRowCollection.Count == 0)
            {
                //No data
                ColNameCollection = new DelimitedStringValues(DataDelimiter);
            }
            if (ContainsDataItems(DataRowCollection[0]))
            {
                //First row contains data -> Empty column names
                ColNameCollection = new DelimitedStringValues(DataRowCollection[0].Delimiter);
            }
            else
            {
                //First row probably contains column names
                ColNameCollection = DataRowCollection[0];
                DataRowCollection.RemoveAt(0);
            }
            return;
        }

        /// <summary>
        /// Appends data rows from given stream reader.
        /// </summary>
        /// <param name="streamReader">Data stream reader.</param>
        /// <param name="maxRows">Maximum rows to be loaded. If GT 0 is specified then loading stops when maxRows is reached.</param>
        /// <returns>Number of rows loaded.</returns>
        public int AppendFromStream(StreamReader streamReader, int maxRows = 0)
        {
            int numOfLoadedRows = 0;
            while (!streamReader.EndOfStream)
            {
                //Add data row
                if (DataDelimiter == AutoDetectDelimiter)
                {
                    //Unknown delimiter
                    DelimitedStringValues dsv = new DelimitedStringValues(streamReader.ReadLine());
                    //Set recognized delimiter
                    DataDelimiter = dsv.Delimiter;
                    DataRowCollection.Add(dsv);
                }
                else
                {
                    //Known delimiter
                    DataRowCollection.Add(new DelimitedStringValues(streamReader.ReadLine(), DataDelimiter));
                }
                ++numOfLoadedRows;
                if (maxRows > 0 && numOfLoadedRows == maxRows)
                {
                    //Maximim limit reached
                    break;
                }
            }
            return numOfLoadedRows;
        }

        /// <summary>
        /// Changes the data items delimiter for whole content.
        /// </summary>
        /// <param name="delimiter">New data items delimiter.</param>
        public void SetDataDelimiter(char delimiter)
        {
            DataDelimiter = delimiter;
            ColNameCollection.ChangeDelimiter(DataDelimiter);
            foreach (DelimitedStringValues dsv in DataRowCollection)
            {
                dsv.ChangeDelimiter(DataDelimiter);
            }
            return;
        }

        /// <summary>
        /// Writes all content to the specified stream.
        /// </summary>
        /// <param name="streamWriter">A stream writer object.</param>
        public void Write(StreamWriter streamWriter)
        {
            if (ColNameCollection.NumOfStringValues > 0)
            {
                streamWriter.WriteLine(ColNameCollection.ToString());
            }
            foreach (DelimitedStringValues dsv in DataRowCollection)
            {
                streamWriter.WriteLine(dsv.ToString());
            }
            return;
        }

        /// <summary>
        /// Saves all content to the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file in which to save the content.</param>
        public void Save(string fileName)
        {
            using StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create));
            Write(streamWriter);
            return;
        }

        /// <summary>
        /// Loads single csv datafile containing time-serie data where each row
        /// contains variable(features) data of one time-point.
        /// Then converts loaded time-serie data so that input vector contains
        /// features data from specified number of time-points and
        /// output vector contains features data from immediately followed time-point.
        /// Then splits data to the training and testing datasets and
        /// saves them as two csv files (training and testing).
        /// </summary>
        /// <param name="timeSeriesDataFile">The name of a csv datafile containing the time-serie data.</param>
        /// <param name="featureNames">The names of features to be used from every time-serie time-point (for both input and output vectors).</param>
        /// <param name="numOfInputTimePoints">Specifies how many time-points of time-serie should constitute input vector.</param>
        /// <param name="testDataRatio">Specifies what ratio from all data to use as the testing data.</param>
        /// <param name="outputTrainDataFile">The name of a csv datafile where to save training data.</param>
        /// <param name="outputTestDataFile">The name of a csv datafile where to save testing data.</param>
        public static void ConvertAndSaveContinuousTimeSeriesDataAsPatternizedDatasets
            (string timeSeriesDataFile,
             List<string> featureNames,
             int numOfInputTimePoints,
             double testDataRatio,
             string outputTrainDataFile,
             string outputTestDataFile
             )
        {
            //Load csv data and create datasets
            //All data
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
            CsvDataHolder outCsvHolder = new CsvDataHolder(trainingData);
            outCsvHolder.Save(outputTrainDataFile);
            outCsvHolder = new CsvDataHolder(testingData);
            outCsvHolder.Save(outputTestDataFile);
            return;
        }



    }//CsvDataHolder

}//Namespace
