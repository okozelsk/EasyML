using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements a model of aggregated child models where
    /// child model can be any model (including other composite models).
    /// Model output is weighted average of root child models outputs.
    /// </summary>
    [Serializable]
    public class CompositeModel : ModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "Composite";

        //Attributes
        private readonly List<ModelBase> _members;
        private double[][] _weights;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="name">Model name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        public CompositeModel(string name,
                              OutputTaskType taskType,
                              IEnumerable<string> outputFeatureNames
                              )
            : base(name, taskType, outputFeatureNames)
        {
            _members = new List<ModelBase>();
            _weights = null;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CompositeModel(CompositeModel source)
            : base(source)
        {
            _members = new List<ModelBase>();
            foreach (ModelBase member in source._members)
            {
                _members.Add(member.DeepClone());
            }
            _weights = (double[][])source._weights.Clone();
            return;
        }

        //Methods
        /// <summary>
        /// Adds a new member.
        /// </summary>
        /// <param name="newMember">A new member to be added.</param>
        public void AddMember(ModelBase newMember)
        {
            //Checks
            if (newMember.NumOfOutputFeatures != NumOfOutputFeatures)
            {
                throw new ArgumentException("Different number of new member outputs.", nameof(newMember));
            }
            if (newMember.TaskType != TaskType)
            {
                throw new ArgumentException("Different output task of new member.", nameof(newMember));
            }
            //Add new member
            _members.Add(newMember);
            return;
        }

        /// <summary>
        /// Sets the model operationable.
        /// </summary>
        public void SetOperationable()
        {
            //Checks
            if (_members.Count < 1)
            {
                throw new InvalidOperationException("At least one member must be added before the finalization.");
            }
            //Set weights
            _weights = GetWeights(_members);
            //Set metrics
            //Finalize model
            FinalizeModel(new ModelConfidenceMetrics(TaskType, (from member in _members select member.ConfidenceMetrics)));
            return;
        }

        /// <summary>
        /// Computes outputs of all members.
        /// </summary>
        /// <param name="inputVector">Input vector.</param>
        public List<double[]> ComputeMembers(double[] inputVector)
        {
            List<double[]> outputVectors = new List<double[]>(_members.Count);
            for (int memberIdx = 0; memberIdx < _members.Count; memberIdx++)
            {
                outputVectors.Add(_members[memberIdx].Compute(inputVector));
            }
            return outputVectors;
        }


        /// <inheritdoc/>
        public override double[] Compute(double[] input)
        {
            return ComputeAggregation(ComputeMembers(input), _weights);
        }

        /// <inheritdoc/>
        public override string GetInfoText(bool detail = false, int margin = 0)
        {
            if (!Ready)
            {
                throw new InvalidOperationException("Model is not built yet.");
            }
            margin = Math.Max(margin, 0);
            StringBuilder sb = new StringBuilder($"{Name} [{GetType()}]{Environment.NewLine}");
            sb.Append($"    Ready                  : {Ready.GetXmlCode()}{Environment.NewLine}");
            sb.Append($"    Task type              : {TaskType.ToString()}{Environment.NewLine}");
            sb.Append($"    Output features info   : {OutputFeatureNames.Count.ToString(CultureInfo.InvariantCulture)}");
            int fIdx = 0;
            foreach (string outputFeatureName in OutputFeatureNames)
            {
                sb.Append($" [{outputFeatureName}, {ConfidenceMetrics.FeatureConfidences[fIdx++].ToString("F3", CultureInfo.InvariantCulture)}]");
            }
            sb.Append(Environment.NewLine);
            sb.Append($"    Number of member models: {_members.Count.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if (detail)
            {
                sb.Append($"    Inner models one by one >>>{Environment.NewLine}");
                for (int i = 0; i < _members.Count; i++)
                {
                    sb.Append(_members[i].GetInfoText(detail, 8));
                }
            }
            string infoText = sb.ToString();
            if (margin > 0)
            {
                infoText = infoText.Indent(margin);
            }
            return infoText;
        }



        /// <inheritdoc/>
        public override ModelBase DeepClone()
        {
            return new CompositeModel(this);
        }

    }//CompositeModel

}//Namespace
