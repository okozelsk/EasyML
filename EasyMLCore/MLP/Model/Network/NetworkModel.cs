using EasyMLCore.Data;
using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements a MLP network model encapsulating the core MLP engine.
    /// </summary>
    [Serializable]
    public class NetworkModel : ModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "MLP";

        //Attribute properties
        /// <summary>
        /// MLP network engine.
        /// </summary>
        public MLPEngine Engine { get; }

        /// <summary>
        /// Error statistics of the network on training data.
        /// </summary>
        public ModelErrStat TrainingErrorStat { get; }

        /// <summary>
        /// Error statistics of the network on validation data.
        /// </summary>
        public ModelErrStat ValidationErrorStat { get; }

        //Attributes
        private readonly FeatureFilterBase[] _inputFilters;
        private readonly FeatureFilterBase[] _outputFilters;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="outputFeatureNames">A collection of output feature names.</param>
        /// <param name="engine">MLP network engine.</param>
        /// <param name="inputFilters">Prepared input filters.</param>
        /// <param name="outputFilters">Prepared output filters.</param>
        /// <param name="trainingErrStat">Error statistics from the training.</param>
        /// <param name="validationData">Validation dataset (can be null).</param>
        public NetworkModel(string name,
                            IEnumerable<string> outputFeatureNames,
                            MLPEngine engine,
                            FeatureFilterBase[] inputFilters,
                            FeatureFilterBase[] outputFilters,
                            ModelErrStat trainingErrStat,
                            SampleDataset validationData
                            )
            : base(name, engine.TaskType, outputFeatureNames)
        {
            if (OutputFeatureNames.Count != engine.NumOfOutputFeatures)
            {
                throw new ArgumentException("Number of feature names differs from number of network's output features.", nameof(outputFeatureNames));
            }
            Engine = engine.DeepClone();
            _inputFilters = new FeatureFilterBase[inputFilters.Length];
            for (int i = 0; i < _inputFilters.Length; i++)
            {
                _inputFilters[i] = inputFilters[i].DeepClone();
            }
            _outputFilters = new FeatureFilterBase[outputFilters.Length];
            for (int i = 0; i < _outputFilters.Length; i++)
            {
                _outputFilters[i] = outputFilters[i].DeepClone();
            }
            //Training error statistics
            TrainingErrorStat = trainingErrStat.DeepClone();
            //Validation data and error statistics
            if (validationData != null)
            {
                ValidationErrorStat = ComputeBatchErrStat(validationData);
            }
            else
            {
                ValidationErrorStat = null;
            }
            //Finalize model
            FinalizeModel(new ModelConfidenceMetrics(TrainingErrorStat, ValidationErrorStat));
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="outputFeatureNames">A collection of output feature names.</param>
        /// <param name="engine">MLP network engine.</param>
        /// <param name="inputFilters">Prepared input filters.</param>
        /// <param name="outputFilters">Prepared output filters.</param>
        /// <param name="trainingErrStat">Error statistics from the training.</param>
        /// <param name="validationErrStat">Error statistics from the validation (can be null).</param>
        public NetworkModel(string name,
                            IEnumerable<string> outputFeatureNames,
                            MLPEngine engine,
                            FeatureFilterBase[] inputFilters,
                            FeatureFilterBase[] outputFilters,
                            ModelErrStat trainingErrStat,
                            ModelErrStat validationErrStat
                            )
            : base(name, engine.TaskType, outputFeatureNames)
        {
            if (OutputFeatureNames.Count != engine.NumOfOutputFeatures)
            {
                throw new ArgumentException("Number of feature names differs from number of network's output features.", nameof(outputFeatureNames));
            }
            Engine = engine.DeepClone();
            _inputFilters = new FeatureFilterBase[inputFilters.Length];
            for (int i = 0; i < _inputFilters.Length; i++)
            {
                _inputFilters[i] = inputFilters[i].DeepClone();
            }
            _outputFilters = new FeatureFilterBase[outputFilters.Length];
            for (int i = 0; i < _outputFilters.Length; i++)
            {
                _outputFilters[i] = outputFilters[i].DeepClone();
            }
            //Training error statistics
            TrainingErrorStat = trainingErrStat.DeepClone();
            //Validation error statistics
            ValidationErrorStat = validationErrStat?.DeepClone();
            //Finalize model
            FinalizeModel(new ModelConfidenceMetrics(TrainingErrorStat, ValidationErrorStat));
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NetworkModel(NetworkModel source)
            : base(source)
        {
            Engine = source.Engine.DeepClone();
            _inputFilters = new FeatureFilterBase[source._inputFilters.Length];
            for (int i = 0; i < _inputFilters.Length; i++)
            {
                _inputFilters[i] = source._inputFilters[i].DeepClone();
            }
            _outputFilters = new FeatureFilterBase[source._outputFilters.Length];
            for (int i = 0; i < _outputFilters.Length; i++)
            {
                _outputFilters[i] = source._outputFilters[i].DeepClone();
            }
            TrainingErrorStat = source.TrainingErrorStat.DeepClone();
            ValidationErrorStat = source.ValidationErrorStat?.DeepClone();
            return;
        }

        //Static methods
        /// <summary>
        /// Compares two network models.
        /// </summary>
        /// <param name="model1">Network model 1.</param>
        /// <param name="model2">Network model 2.</param>
        /// <param name="trainingStatOnly">Specifies whether to consider only training results or also validation results (if available).</param>
        /// <returns>-1 if model1 is better, 1 if model2 is better, 0 if no difference.</returns>
        public bool IsBetter(NetworkModel otherModel, bool trainingStatOnly = false)
        {
            if (trainingStatOnly)
            {
                return TrainingErrorStat.IsBetter(otherModel.TrainingErrorStat);
            }
            else
            {
                return ModelConfidenceMetrics.Comparer(ConfidenceMetrics,
                                                       otherModel.ConfidenceMetrics
                                                       ) == 1;
            }
        }

        //Methods
        /// <summary>
        /// Version for single computation.
        /// </summary>
        private double[] Normalize(double[] natVector)
        {
            double[] normVector = new double[natVector.Length];
            for (int i = 0; i < natVector.Length; i ++)
            {
                normVector[i] = _inputFilters[i].ApplyFilter(natVector[i]);
            }
            return normVector;
        }

        /// <summary>
        /// Version for multiple computations.
        /// </summary>
        private void Normalize(double[] natVector, double[] normVector)
        {
            for (int i = 0; i < natVector.Length; i++)
            {
                normVector[i] = _inputFilters[i].ApplyFilter(natVector[i]);
            }
            return;
        }

        /// <summary>
        /// Version for single computation.
        /// </summary>
        private double[] Naturalize(double[] vector)
        {
            double[] nVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                nVector[i] = _outputFilters[i].ApplyReverse(vector[i]);
            }
            return nVector;
        }

        /// <summary>
        /// Version for multiple computations.
        /// </summary>
        private void Naturalize(double[] activations, int aIdx, double[] natVector)
        {
            for (int i = 0; i < natVector.Length; i++, aIdx++)
            {
                natVector[i] = _outputFilters[i].ApplyReverse(activations[aIdx]);
            }
            return;
        }

        /// <inheritdoc/>
        public override double[] Compute(double[] input)
        {
            return Naturalize(Engine.Compute(Normalize(input)));
        }

        /// <inheritdoc/>
        public override ModelBase DeepClone()
        {
            return new NetworkModel(this);
        }

        /// <summary>
        /// Computes given sample dataset.
        /// </summary>
        /// <param name="dataset">Sample dataset to be computed.</param>
        /// <param name="errStat">Resulting error statistics.</param>
        /// <returns>Computed vectors in the same order as in sample dataset.</returns>
        public double[][] ComputeSampleDataset(SampleDataset dataset, out ModelErrStat errStat)
        {
            double[][] computedVectors = new double[dataset.Count][];
            ModelErrStat batchErrStat = new ModelErrStat(TaskType, OutputFeatureNames);
            object monitor = new object();
            Parallel.ForEach(Partitioner.Create(0, dataset.Count), range =>
            {
                ModelErrStat rangeStat = new ModelErrStat(Engine.TaskType, OutputFeatureNames);
                //Reusable buffers
                double[] sums = new double[Engine.NumOfNeurons];
                double[] activations = new double[Engine.NumOfInputFeatures + Engine.NumOfNeurons];
                double[] computed = new double[Engine.NumOfOutputFeatures];
                //Worker thread loop
                for (int sampleIdx = range.Item1; sampleIdx < range.Item2; sampleIdx++)
                {
                    //Input
                    Normalize(dataset.SampleCollection[sampleIdx].InputVector, activations);
                    //Compute
                    int aOutIdx = Engine.Compute(activations, sums);
                    //Output
                    Naturalize(activations, aOutIdx, computed);
                    computedVectors[sampleIdx] = (double[])computed.Clone();
                    //Update stat
                    rangeStat.Update(computed, dataset.SampleCollection[sampleIdx].OutputVector);
                }
                lock (monitor)
                {
                    batchErrStat.Merge(rangeStat);
                }
            });
            errStat = batchErrStat;
            return computedVectors;
        }

        private ModelErrStat ComputeBatchErrStat(SampleDataset dataset)
        {
            ModelErrStat batchErrStat = new ModelErrStat(TaskType, OutputFeatureNames);
            object monitor = new object();
            Parallel.ForEach(Partitioner.Create(0, dataset.Count), range =>
            {
                ModelErrStat rangeStat = new ModelErrStat(Engine.TaskType, OutputFeatureNames);
                //Reusable buffers
                double[] sums = new double[Engine.NumOfNeurons];
                double[] activations = new double[Engine.NumOfInputFeatures + Engine.NumOfNeurons];
                double[] computed = new double[Engine.NumOfOutputFeatures];
                //Worker thread loop
                for (int sampleIdx = range.Item1; sampleIdx < range.Item2; sampleIdx++)
                {
                    //Input
                    Normalize(dataset.SampleCollection[sampleIdx].InputVector, activations);
                    //Compute
                    int aOutIdx = Engine.Compute(activations, sums);
                    //Output
                    Naturalize(activations, aOutIdx, computed);
                    //Update stat
                    rangeStat.Update(computed, dataset.SampleCollection[sampleIdx].OutputVector);
                }
                lock (monitor)
                {
                    batchErrStat.Merge(rangeStat);
                }
            });
            return batchErrStat;
        }

        public override string GetInfoText(bool detail = false, int margin = 0)
        {
            margin = Math.Max(margin, 0);
            StringBuilder sb = new StringBuilder($"{Name} [{GetType()}]{Environment.NewLine}");
            sb.Append($"    Ready                   : {Ready.GetXmlCode()}{Environment.NewLine}");
            sb.Append($"    Task type               : {Engine.TaskType.ToString()}{Environment.NewLine}");
            sb.Append($"    Number of input features: {Engine.NumOfInputFeatures.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Output features info    : {Engine.NumOfOutputFeatures.ToString(CultureInfo.InvariantCulture)}");
            int fIdx = 0;
            foreach(string outputFeatureName in OutputFeatureNames)
            {
                sb.Append($" [{outputFeatureName}, {ConfidenceMetrics.FeatureConfidences[fIdx++].ToString("F3", CultureInfo.InvariantCulture)}]");
            }
            sb.Append(Environment.NewLine);
            sb.Append($"    Total number of neurons : {Engine.NumOfNeurons.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            sb.Append($"    Number of layers        : {Engine.LayerCollection.Count.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if(detail)
            {
                sb.Append($"    Layers one by one >>>{Environment.NewLine}");
                for(int i = 0; i < Engine.LayerCollection.Count; i++)
                {
                    string layerName = (i == Engine.LayerCollection.Count - 1) ? "Output layer" : "Hidden layer";
                    sb.Append($"        {layerName}{Environment.NewLine}");
                    sb.Append($"            Number of neurons: {Engine.LayerCollection[i].NumOfLayerNeurons.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
                    sb.Append($"            Activation       : {Engine.LayerCollection[i].Activation.ID.ToString()}{Environment.NewLine}");
                    sb.Append($"            Number of weights: {(Engine.LayerCollection[i].NumOfInputNodes * Engine.LayerCollection[i].NumOfLayerNeurons + Engine.LayerCollection[i].NumOfLayerNeurons).ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");

                }
            }
            sb.Append($"    Total number of weights : {Engine.NumOfWeights.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if(detail)
            {
                if (Engine.LayerCollection.Count > 1)
                {
                    sb.Append($"        Hidden weights ({Engine.HLWeightsStat.NumOfSamples.ToString(CultureInfo.InvariantCulture)}){Environment.NewLine}");
                    sb.Append($"{Engine.HLWeightsStat.GetInfoText(12, BasicStat.StatisticalFigure.Min, BasicStat.StatisticalFigure.Max, BasicStat.StatisticalFigure.ArithAvg, BasicStat.StatisticalFigure.RootMeanSquare, BasicStat.StatisticalFigure.StdDev)}");
                }
                sb.Append($"        Output weights ({Engine.OLWeightsStat.NumOfSamples.ToString(CultureInfo.InvariantCulture)}){Environment.NewLine}");
                sb.Append($"{Engine.OLWeightsStat.GetInfoText(12, BasicStat.StatisticalFigure.Min, BasicStat.StatisticalFigure.Max, BasicStat.StatisticalFigure.ArithAvg, BasicStat.StatisticalFigure.RootMeanSquare, BasicStat.StatisticalFigure.StdDev)}");
            }
            string infoText = sb.ToString();
            if (margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }


    }//NetworkModel

}//Namespace
