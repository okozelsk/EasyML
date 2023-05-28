using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements a time series data pattern (univariate or multivariate).
    /// </summary>
    [Serializable]
    public class TimeSeriesPattern : SerializableObject
    {
        //Enums
        /// <summary>
        /// Schema of variables organization in a time series 1D (flat) format.
        /// </summary>
        public enum FlatVarSchema
        {
            /// <summary>
            /// {v1[t1],v2[t1],v1[t2],v2[t2],v1[t3],v2[t3],...}
            /// where "v" means variable and "t" means time point.
            /// </summary>
            Groupped,
            /// <summary>
            /// {v1[t1],v1[t2],v1[t3],v2[t1],v2[t2],v2[t3],...}
            /// where "v" means variable and "t" means time point.
            /// </summary>
            VarSequence
        }

        /// <summary>
        /// Pattern data in a form where each variable has its own row of time ordered data.
        /// </summary>
        public List<double[]> VariablesDataCollection { get; }

        //Constructors
        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source input pattern.</param>
        public TimeSeriesPattern(TimeSeriesPattern source)
        {
            VariablesDataCollection = new List<double[]>(source.VariablesDataCollection.Count);
            foreach (double[] vector in source.VariablesDataCollection)
            {
                VariablesDataCollection.Add((double[])vector.Clone());
            }
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="numOfVariables">Number of input pattern variables.</param>
        public TimeSeriesPattern(int numOfVariables = 1)
        {
            VariablesDataCollection = new List<double[]>(numOfVariables);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputData">1D array of doubles containing input pattern data.</param>
        /// <param name="numOfVariables">Number of input pattern variables.</param>
        /// <param name="varSchema">Variables schema specifies an organization of variables' data in a flat array.</param>
        public TimeSeriesPattern(double[] inputData,
                                 int numOfVariables,
                                 FlatVarSchema varSchema
                                 )
        {
            VariablesDataCollection = VariablesDataFromArray(inputData, 0, inputData.Length, numOfVariables, varSchema);
            return;
        }

        //Properties
        /// <summary>
        /// Indicates whether the pattern data is initialized and consistent.
        /// </summary>
        public bool Consistent
        {
            get
            {
                if (VariablesDataCollection.Count == 0 ||
                    VariablesDataCollection == null ||
                    VariablesDataCollection[0].Length == 0)
                {
                    return false;
                }
                for(int i = 1;  i < VariablesDataCollection.Count; i++)
                {
                    if (VariablesDataCollection[i].Length != VariablesDataCollection[i - 1].Length) 
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets pattern length (number of time points).
        /// </summary>
        public int Length
        {
            get
            {
                if (!Consistent)
                {
                    return 0;
                }
                return VariablesDataCollection[0].Length;
            }
        }

        /// <summary>
        /// Gets number of variables.
        /// </summary>
        public int NumOfVariables { get { return VariablesDataCollection.Count; } }

        //Static methods
        /// <summary>
        /// Extracts variables' data from a 1D collection.
        /// </summary>
        /// <param name="inputData">A 1D (flat) collection of doubles containing input pattern data.</param>
        /// <param name="dataStartIndex">Specifies the zero-based starting index of pattern's data in the flat inputData.</param>
        /// <param name="dataLength">Specifies the length of pattern's data in the flat inputData.</param>
        /// <param name="numOfVariables">Number of pattern's variables.</param>
        /// <param name="varSchema">Specifies an organization of variables in a flat (1D) data format.</param>
        public static List<double[]> VariablesDataFromArray(IList<double> inputData,
                                                            int dataStartIndex,
                                                            int dataLength,
                                                            int numOfVariables,
                                                            FlatVarSchema varSchema
                                                            )
        {
            //Check data length
            if (dataLength < numOfVariables ||
                dataLength % numOfVariables != 0 ||
                dataStartIndex + dataLength > inputData.Count)
            {
                throw new FormatException("Incorrect num of variables or flat data length or both.");
            }
            //Pattern data
            int timePoints = dataLength / numOfVariables;
            List<double[]> varDataCollection = new List<double[]>(numOfVariables);
            for (int i = 0; i < numOfVariables; i++)
            {
                varDataCollection.Add(new double[timePoints]);
            }
            for (int timeIdx = 0; timeIdx < timePoints; timeIdx++)
            {
                for (int i = 0; i < numOfVariables; i++)
                {
                    double varValue = varSchema == FlatVarSchema.Groupped ? inputData[dataStartIndex + timeIdx * numOfVariables + i] : inputData[dataStartIndex + i * timePoints + timeIdx];
                    varDataCollection[i][timeIdx] = varValue;
                }
            }//timeIdx
            return varDataCollection;
        }

        //Methods
        private double[] GetDataAtTimepointInternal(int timePointIndex)
        {
            double[] data = new double[VariablesDataCollection.Count];
            for (int i = 0; i < VariablesDataCollection.Count; i++)
            {
                data[i] = VariablesDataCollection[i][timePointIndex];
            }
            return data;
        }

        /// <summary>
        /// Gets variables' data at specified time point.
        /// </summary>
        /// <param name="timePointIndex">Zero based index of time point.</param>
        public double[] GetDataAt(int timePointIndex)
        {
            if(Length <= timePointIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(timePointIndex));
            }
            return GetDataAtTimepointInternal(timePointIndex);
        }

        /// <summary>
        /// Gets variables' data in time points as a list of double arrays.
        /// </summary>
        public List<double[]> GetTimePointsData() 
        {
            if (!Consistent)
            {
                throw new InvalidOperationException("Pattern is not correctly initialized (Consistent property is false).");
            }
            List<double[]> timePoints = new List<double[]>(VariablesDataCollection[0].Length);
            for(int i = 0; i < VariablesDataCollection[0].Length; i++)
            {
                timePoints.Add(GetDataAtTimepointInternal(i));
            }
            return timePoints;
        }

        /// <summary>
        /// Gets variables' data as a 1D flat array of doubles.
        /// Data of variables is organized according to specified schema.
        /// </summary>
        /// <param name="variablesSchema">Specifies an organization of variables in flat (1D) data.</param>
        public double[] Flattenize(FlatVarSchema variablesSchema = FlatVarSchema.Groupped)
        {
            if(variablesSchema == FlatVarSchema.VarSequence)
            {
                if (!Consistent)
                {
                    throw new InvalidOperationException("Pattern is not correctly initialized (Consistent property is false).");
                }
                return VariablesDataCollection.Flattenize();
            }
            else
            {
                return GetTimePointsData().Flattenize();
            }
        }

        /// <summary>
        /// Standardizes variables' data using given filters.
        /// </summary>
        /// <param name="filters">Feature filters to be used (one per variable, in the same order).</param>
        /// <param name="centered">Specifies whether to center value between -1 an 1, so min value is -1 and max value is 1. If false, 0 is not the interval center but represents the average value and -1 or 1 represents the magnitude.</param>
        public void StandardizeData(IEnumerable<FeatureFilterBase> filters, bool centered)
        {
            if (!Consistent)
            {
                throw new InvalidOperationException("Pattern is not correctly initialized (Consistent property is false).");
            }
            int varIdx = 0;
            foreach(FeatureFilterBase filter in filters)
            {
                for(int i = 0; i < VariablesDataCollection[varIdx].Length; i++)
                {
                    VariablesDataCollection[varIdx][i] = filter.ApplyFilter(VariablesDataCollection[varIdx][i], centered);
                }
                ++varIdx;
            }
            return;
        }

        /// <summary>
        /// Naturalizes variables' data using given filters.
        /// </summary>
        /// <param name="filters">Feature filters to be used (one per variable, in the same order).</param>
        /// <param name="centered">Specifies whether data was centered.</param>
        public void NaturalizeData(IEnumerable<FeatureFilterBase> filters, bool centered)
        {
            if (!Consistent)
            {
                throw new InvalidOperationException("Pattern is not correctly initialized (Consistent property is false).");
            }
            int varIdx = 0;
            foreach (FeatureFilterBase filter in filters)
            {
                for (int i = 0; i < VariablesDataCollection[varIdx].Length; i++)
                {
                    VariablesDataCollection[varIdx][i] = filter.ApplyReverse(VariablesDataCollection[varIdx][i], centered);
                }
                ++varIdx;
            }
            return;
        }


    }//TimeSeriesPattern

}//Namespace
