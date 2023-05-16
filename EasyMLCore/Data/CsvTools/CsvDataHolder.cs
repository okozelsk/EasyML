using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements the holder of csv data. Supports data loading and saving to a file.
    /// </summary>
    [Serializable]
    public class CsvDataHolder : SerializableObject
    {
        //Constants
        public const int DefaultExpectedNumOfDataRows = 10000;
        //Delimiters
        /// <summary>
        /// The semicolon delimiter.
        /// </summary>
        public const char SemicolonDelimiter = ';';
        /// <summary>
        /// The comma delimiter.
        /// </summary>
        public const char CommaDelimiter = ',';
        /// <summary>
        /// The tabelator delimiter.
        /// </summary>
        public const char TabDelimiter = '\t';
        /// <summary>
        /// Default delimiter.
        /// </summary>
        public const char DefaultDelimiter = SemicolonDelimiter;


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
        /// Creates an initialized instance with empty data.
        /// </summary>
        /// <param name="delimiter">Columns delimiter.</param>
        /// <param name="colNames">Column names.</param>
        public CsvDataHolder(char delimiter = DefaultDelimiter, IEnumerable<string> colNames = null, int expectedNumOfDataRows = DefaultExpectedNumOfDataRows)
        {
            if(delimiter != TabDelimiter && delimiter != SemicolonDelimiter &&  delimiter != CommaDelimiter)
            {
                throw new ArgumentException($"Unsupported delimiter '{delimiter}'.", nameof(delimiter));
            }
            DataDelimiter = delimiter;
            if (colNames != null)
            {
                ColNameCollection = new DelimitedStringValues(colNames);
            }
            else
            {
                ColNameCollection = new DelimitedStringValues(DataDelimiter);
            }
            DataRowCollection = new List<DelimitedStringValues>(expectedNumOfDataRows);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fileName">Name of the file containing data to be loaded.</param>
        public CsvDataHolder(string fileName)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            fileName = Path.Combine(dir, fileName);
            Investigate(fileName, out bool header, out char delimiter);
            DataDelimiter = delimiter;
            DataRowCollection = new List<DelimitedStringValues>();
            using StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open));
            if(header)
            {
                ColNameCollection = new DelimitedStringValues(streamReader.ReadLine(), DataDelimiter);
            }
            while (!streamReader.EndOfStream)
            {
                //Known delimiter
                DataRowCollection.Add(new DelimitedStringValues(streamReader.ReadLine(), DataDelimiter));
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="dataset">Samples.</param>
        /// <param name="delimiter">Data items delimiter.</param>
        public CsvDataHolder(SampleDataset dataset, IEnumerable<string> colNames = null, char delimiter = DefaultDelimiter)
            :this(delimiter, colNames, dataset.Count)
        {
            foreach (Sample sample in dataset.SampleCollection)
            {
                double[] data = (double[])sample.InputVector.Concat(sample.OutputVector);
                DelimitedStringValues dsv = new DelimitedStringValues(DataDelimiter);
                foreach (double value in data)
                {
                    dsv.AddValue(value.ToString(CultureInfo.InvariantCulture));
                }
                DataRowCollection.Add(dsv);
            }
            return;
        }

        //Static methods
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
        /// Investigates given csv file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="header">Indicates that file contains header.</param>
        /// <param name="delimiter">Recognized columns delimiter.</param>
        public static void Investigate(string fileName, out bool header, out char delimiter)
        {
            //Read first two rows
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            fileName = Path.Combine(dir, fileName);
            using StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open));
            List<string> lines = new List<string>(2);
            while (!streamReader.EndOfStream && lines.Count < 2)
            {
                lines.Add(streamReader.ReadLine());
            }
            if(lines.Count < 1 || lines[0].Length == 0)
            {
                throw new ApplicationException($"File {fileName} contains no data or it has invalid format.");
            }
            //Delimiter
            //Check of the presence of candidate chars
            string sampleDelimitedData = lines[0];
            //Is "tab" char the candidate?
            if (sampleDelimitedData.IndexOf(TabDelimiter) != -1)
            {
                //If tab is present then it is the most probable delimiter
                delimiter = TabDelimiter;
            }
            //Is "semicolon" char the candidate?
            else if (sampleDelimitedData.IndexOf(SemicolonDelimiter) != -1)
            {
                //If semicolon is present then it is the next most probable delimiter
                delimiter = SemicolonDelimiter;
            }
            //Is "comma" char the candidate?
            else if (sampleDelimitedData.IndexOf(CommaDelimiter) != -1)
            {
                //Comma is the probable delimiter
                delimiter = CommaDelimiter;
            }
            else
            {
                //Remaining default delimiter
                delimiter = DefaultDelimiter;
            }

            //Header?
            header = false;
            DelimitedStringValues firstRowDSV = new DelimitedStringValues(lines[0], delimiter);
            header = !ContainsDataItems(firstRowDSV);
            if(header && lines.Count == 2)
            {
                //Check that the second row contains data
                DelimitedStringValues secondRowDSV = new DelimitedStringValues(lines[1], delimiter);
                bool containsData = ContainsDataItems(secondRowDSV);
                if(!containsData)
                {
                    throw new ApplicationException($"File {fileName} has invalid format.");
                }
            }
            return;
        }

        //Methods
        /// <summary>
        /// Writes all content to the specified stream.
        /// </summary>
        /// <param name="streamWriter">A stream writer object.</param>
        public void Write(StreamWriter streamWriter)
        {
            if (ColNameCollection.NumOfStringValues > 0)
            {
                streamWriter.WriteLine(ColNameCollection.ToSingleRow(DataDelimiter));
            }
            foreach (DelimitedStringValues dsv in DataRowCollection)
            {
                streamWriter.WriteLine(dsv.ToSingleRow(DataDelimiter));
            }
            return;
        }

        /// <summary>
        /// Saves all content to the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file in which to save the content.</param>
        public void Save(string fileName)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            fileName = Path.Combine(dir, fileName);
            using StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create));
            Write(streamWriter);
            return;
        }

        /// <summary>
        /// Loads single csv datafile containing time-serie data where each row
        /// contains variable(features) data of one time point.
        /// Then converts loaded time-serie data so that input vector contains
        /// features data from specified number of time points and
        /// output vector contains features data from immediately followed time point.
        /// Then splits data to the training and testing datasets and
        /// saves them as two csv files (training and testing).
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
