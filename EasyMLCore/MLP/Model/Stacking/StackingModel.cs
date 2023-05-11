using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Implements a model of the meta-learner, which is trained on outputs of NetworkModel(s)
    /// defined in a stack.
    /// Model output is an output of trained meta-learner. Meta-Learner can be any kind of model.
    /// </summary>
    [Serializable]
    public class StackingModel : ModelBase
    {
        //Constants
        /// <summary>
        /// Short identifier in context path.
        /// </summary>
        public const string ContextPathID = "Stacking";

        //Attributes
        private readonly List<NetworkModel> _stack;
        private ModelBase _metaLearner;
        private readonly bool _routeInput;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="taskType">Output task.</param>
        /// <param name="outputFeatureNames">Names of output features.</param>
        /// <param name="routeInput">Specifies whether to provide original input to meta-learner.</param>
        public StackingModel(string name,
                             OutputTaskType taskType,
                             IEnumerable<string> outputFeatureNames,
                             bool routeInput
                             )
            : base(name, taskType, outputFeatureNames)
        {
            _stack = new List<NetworkModel>();
            _metaLearner = null;
            _routeInput = routeInput;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public StackingModel(StackingModel source)
            : base(source)
        {
            _stack = new List<NetworkModel>(source._stack.Count);
            foreach (NetworkModel sourceStackMember in source._stack)
            {
                _stack.Add((NetworkModel)sourceStackMember.DeepClone());
            }
            _metaLearner = source._metaLearner?.DeepClone();
            return;
        }

        //Methods
        /// <summary>
        /// Adds network into the stack.
        /// </summary>
        /// <param name="stackMember">A network model to be added into the stack.</param>
        public void AddStackMember(NetworkModel stackMember)
        {
            //Check
            if (stackMember.TaskType != TaskType || stackMember.NumOfOutputFeatures != NumOfOutputFeatures)
            {
                throw new ArgumentException("Inconsistent member network in terms of output task type or number of output features.", nameof(stackMember));
            }
            //Add member
            _stack.Add(stackMember);
            return;
        }

        /// <summary>
        /// Sets the model operationable.
        /// </summary>
        /// <param name="metaLearner">A meta-learner model combining outputs od the stack members.</param>
        public void SetOperationable(ModelBase metaLearner)
        {
            //Checks
            if (metaLearner.TaskType != TaskType || metaLearner.NumOfOutputFeatures != NumOfOutputFeatures)
            {
                throw new ArgumentException("Inconsistent meta-learner in terms of output task type or number of output features.", nameof(metaLearner));
            }
            if (_stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }
            _metaLearner = metaLearner;
            //Set metrics
            FinalizeModel(_metaLearner.ConfidenceMetrics);
            return;
        }

        /// <summary>
        /// Computes outputs of stack members.
        /// </summary>
        /// <param name="inputVector">Input vector.</param>
        public List<double[]> ComputeMembers(double[] inputVector)
        {
            List<double[]> outputVectors = new List<double[]>(_stack.Count);
            foreach (NetworkModel net in _stack)
            {
                outputVectors.Add(net.Compute(inputVector));
            }
            return outputVectors;
        }

        /// <inheritdoc/>
        public override double[] Compute(double[] input)
        {
            double[] stackOutputs = ComputeMembers(input).Flattenize();
            return _metaLearner.Compute(_routeInput ? (double[])input.Concat(stackOutputs) : stackOutputs);
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
            sb.Append($"    Ready                      : {Ready.GetXmlCode()}{Environment.NewLine}");
            sb.Append($"    Task type                  : {TaskType.ToString()}{Environment.NewLine}");
            sb.Append($"    Output features info       : {OutputFeatureNames.Count.ToString(CultureInfo.InvariantCulture)}");
            int fIdx = 0;
            foreach (string outputFeatureName in OutputFeatureNames)
            {
                sb.Append($" [{outputFeatureName}, {ConfidenceMetrics.FeatureConfidences[fIdx++].ToString("F3", CultureInfo.InvariantCulture)}]");
            }
            sb.Append(Environment.NewLine);
            sb.Append($"    Number of stacked models   : {_stack.Count.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}");
            if (detail)
            {
                sb.Append($"    Stacked models one by one >>>{Environment.NewLine}");
                for (int i = 0; i < _stack.Count; i++)
                {
                    sb.Append(_stack[i].GetInfoText(detail, 8));
                }
            }
            sb.Append($"    Route input to meta learner: {_routeInput.GetXmlCode()}{Environment.NewLine}");
            sb.Append(_metaLearner.GetInfoText(detail, 4));
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
            return new StackingModel(this);
        }

    }//StackingModel

}//Namespace
