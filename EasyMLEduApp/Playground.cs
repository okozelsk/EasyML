using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using EasyMLCore.MiscTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace EasyMLDemoApp
{
    /// <summary>
    /// This class is a free "playground", the place where it is possible to test new concepts or somewhat else.
    /// </summary>
    class Playground
    {
        //Attributes
        private readonly Random _rand;

        //Constructor
        public Playground()
        {
            _rand = new Random();
            return;
        }

        //Methods
        private void TestFolderization(string dataFile, int numOfClasses)
        {
            //Load csv data
            CsvDataHolder csvData = new CsvDataHolder(dataFile);
            //Load csv data to a Dataset
            SampleDataset dataset = SampleDataset.Load(csvData, numOfClasses);
            double[] foldDataRatios = { -1d, 0d, 0.1d, 0.5d, 0.75d, 1d, 2d };
            Console.WriteLine($"Folderization test of {dataFile}. NumOfSamples={dataset.Count.ToString(CultureInfo.InvariantCulture)}, NumOfFoldDataRatios={foldDataRatios.Length.ToString(CultureInfo.InvariantCulture)}");
            foreach (double foldDataRatio in foldDataRatios)
            {
                Console.WriteLine($"  Testing fold data ratio = {foldDataRatio.ToString(CultureInfo.InvariantCulture)}");
                List<SampleDataset> folds = dataset.Folderize(foldDataRatio, (numOfClasses > 1) ? OutputTaskType.Categorical : OutputTaskType.Binary);
                Console.WriteLine($"    Number of resulting folds = {folds.Count.ToString(CultureInfo.InvariantCulture)}");
                for (int foldIdx = 0; foldIdx < folds.Count; foldIdx++)
                {
                    int numOfFoldSamples = folds[foldIdx].Count;
                    Console.WriteLine($"      FoldIdx={foldIdx.ToString(CultureInfo.InvariantCulture),-4} FoldSize={numOfFoldSamples.ToString(CultureInfo.InvariantCulture),-4}");
                    int[] classesBin1Counts = new int[numOfClasses];
                    Array.Fill(classesBin1Counts, 0);
                    for (int sampleIdx = 0; sampleIdx < numOfFoldSamples; sampleIdx++)
                    {
                        for (int classIdx = 0; classIdx < numOfClasses; classIdx++)
                        {
                            if (folds[foldIdx].SampleCollection[sampleIdx].OutputVector[classIdx] >= BinFeatureFilter.GetBinaryBorder(FeatureFilterBase.FeatureUse.Output))
                            {
                                ++classesBin1Counts[classIdx];
                            }
                        }
                    }
                    Console.WriteLine($"        Number of positive samples per class");
                    for (int classIdx = 0; classIdx < numOfClasses; classIdx++)
                    {
                        Console.WriteLine($"          ClassID={classIdx.ToString(CultureInfo.InvariantCulture),-3}, Bin1Samples={classesBin1Counts[classIdx].ToString(CultureInfo.InvariantCulture)}");
                    }
                }
                Console.WriteLine("Press enter to leave TestFolderization...");
                Console.ReadLine();
            }
            return;
        }

        private void TestGaussianRandom()
        {
            double reqStdDev = Math.Sqrt(1d / 80d);
            double[] buffer = new double[200];
            _rand.FillGaussianDouble(buffer, 0d, reqStdDev);
            BasicStat stat = new BasicStat(buffer);
            Console.WriteLine($"Gaussian test reqStdDev={reqStdDev.ToString(CultureInfo.InvariantCulture)}, AchievedStdDev={stat.StdDev.ToString(CultureInfo.InvariantCulture)}, AchievedMean={stat.ArithAvg.ToString(CultureInfo.InvariantCulture)}");
            Console.WriteLine("Press enter to leave TestGaussianRandom...");
            Console.ReadLine();
            return;
        }


        private void TestThrottleValve()
        {
            int from = 1, to = 200;
            ParamValMapper throttleValve = new ParamValMapper(from, to, 1d, 1e-6d, 1e-6d, 1d);
            for(int i = from; i <= to; i++)
            {
                double permeability = throttleValve.Map(i);
                Console.WriteLine($"{i,5} permeability = {permeability.ToString(CultureInfo.InvariantCulture)}");
            }
            Console.WriteLine("Press enter to leave TestThrottleValve...");
            Console.ReadLine();
            return;
        }

        private void AtomicOper(double[] atomicInArray, double[] atomicOutArray, int i)
        {
            atomicOutArray[i] = Math.Sqrt(atomicInArray[i]);
            return;
        }

        private void TestParallelEfficiency()
        {
            Stopwatch sw = new Stopwatch();
            int numOfAtomicOperations = 160000;
            List<Tuple<int, int>> ranges = new(Partitioner.Create(0, numOfAtomicOperations).GetDynamicPartitions());   
            double[] atomicInArray = new double[numOfAtomicOperations];
            double[] atomicOutArray = new double[numOfAtomicOperations];
            _rand.FillUniform(atomicInArray, 0d, 100d, false);

            //Sequential version
            sw.Reset();
            sw.Start();
            for (int i = 0; i < numOfAtomicOperations; i++)
            {
                AtomicOper(atomicInArray, atomicOutArray, i);
            }
            sw.Stop();
            Console.WriteLine($"Sequential: {sw.ElapsedMilliseconds} ms");

            //Parallel version - fixed ranges
            sw.Reset();
            sw.Start();
            Parallel.ForEach(ranges, range =>
            {
                for(int i = range.Item1; i < range.Item2; i++)
                {
                    AtomicOper(atomicInArray, atomicOutArray, i);
                }
            });
            sw.Stop();
            Console.WriteLine($"F-Parallel: {sw.ElapsedMilliseconds} ms");

            //Parallel version - dynamic ranges
            sw.Reset();
            sw.Start();
            Parallel.ForEach(Partitioner.Create(0, numOfAtomicOperations), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    AtomicOper(atomicInArray, atomicOutArray, i);
                }
            });
            sw.Stop();
            Console.WriteLine($"D-Parallel: {sw.ElapsedMilliseconds} ms");

            Console.WriteLine("Press enter to leave TestParallelEfficiency...");
            Console.ReadLine();
            return;
        }

        private void TestParttitoning()
        {
            List<int> numOfTasksToTest = new List<int>() { 1, 2, 3, 4, 7, 8, 9, 14, 15,
                                                          16, 17, 18, 31, 32, 33, 159,
                                                          160, 161};
            foreach (int numOfTasks in numOfTasksToTest)
            {
                List<Tuple<int, int, int>> partitions = Common.GetFixedPartitions(numOfTasks);
                Console.WriteLine($"Test: {numOfTasks}:");
                for(int i = 0; i < partitions.Count; i++)
                {
                    Console.WriteLine($"    PIdx: {partitions[i].Item3, 4} FromIdx: {partitions[i].Item1, 4} ToIdx: {partitions[i].Item2, 4} Count: {partitions[i].Item2 - partitions[i].Item1}");
                }
            }
            Console.WriteLine("Press enter to leave TestParttitoning...");
            Console.ReadLine();
            return;
        }


        /// <summary>
        /// Playground's entry point.
        /// </summary>
        public void Run()
        {
            Console.Clear();
            //TODO - place your code here
            //TestFolderization("./Data/Yoga_train.csv", 1);
            //TestFolderization("./Data/ProximalPhalanxOutlineAgeGroup_train.csv", 3);
            //TestGaussianRandom();
            //TestThrottleValve();
            //TestParallelEfficiency();
            //TestParttitoning();

            Console.WriteLine("Press enter to return to menu...");
            Console.ReadLine();
            return;
        }


    }//Playground
}
