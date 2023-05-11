using EasyMLCore.Activation;
using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Implements reservoir neuron.
    /// </summary>
    [Serializable]
    public class ReservoirNeuron : SerializableObject
    {
        //Enums
        /// <summary>
        /// Supported predictors.
        /// </summary>
        public enum Predictor
        {
            /// <summary>
            /// Neuron's activation.
            /// </summary>
            Activation,
            /// <summary>
            /// Neuron's squared activation.
            /// </summary>
            SquaredActivation,
            /// <summary>
            /// Fading trace of neuron's spikes.
            /// </summary>
            SpikesFadingTrace
        };

        //Attribute properties
        /// <summary>
        /// Internal index within the reservoir neurons.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Synapses from input neurons.
        /// </summary>
        public List<ReservoirSynapse> InputSynapses { get; }

        /// <summary>
        /// Synapses from hidden neurons.
        /// </summary>
        public List<ReservoirSynapse> HiddenSynapses { get; }

        /// <summary>
        /// Indicates whether spike event has currently occured.
        /// </summary>
        public bool SpikeEvent { get; private set; }

        /// <summary>
        /// Power of spike event.
        /// </summary>
        public double SpikePower { get; private set; }

        /// <summary>
        /// Predictors.
        /// </summary>
        public double[] Predictors { get; }

        /// <summary>
        /// Predictors.
        /// </summary>
        public bool[] PredictorSwitches { get; }

        //Attributes
        private readonly ActivationBase _activationFn;
        private readonly double _retainment;
        private readonly double _spikeEventThreshold;
        private readonly double _fadingCoeff;
        private double _stimuliSum;
        private double _prevActivation;
        private double _activation;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="index">Internal index within the reservoir neurons.</param>
        /// <param name="activationFn">Activation function.</param>
        /// <param name="spikeEventThreshold">Spike event threshold. When the new activation is higher than the previous by this threshold, a spike event is emitted.</param>
        /// <param name="fadingCoeff">Fading coeff for fading type predictors.</param>
        /// <param name="retainment">Retainment strength of a previous activation.</param>
        public ReservoirNeuron(int index,
                               ActivationBase activationFn,
                               double spikeEventThreshold,
                               double fadingCoeff,
                               double retainment
                               )
        {
            Index = index;
            _activationFn = activationFn;
            _prevActivation = 0d;
            _activation = 0d;
            _retainment = retainment;
            _spikeEventThreshold = spikeEventThreshold;
            _fadingCoeff = fadingCoeff;
            _stimuliSum = 0d;
            InputSynapses = new List<ReservoirSynapse>();
            HiddenSynapses = new List<ReservoirSynapse>();
            Predictors = new double[Enum.GetValues(typeof(Predictor)).Length];
            PredictorSwitches = new bool[Enum.GetValues(typeof(Predictor)).Length];
            Array.Fill(PredictorSwitches, true);
            SpikeEvent = false;
            SpikePower = 0d;
            return;
        }

        //Methods
        /// <summary>
        /// Gets current neuron activation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetActivation() { return _activation; }

        /// <summary>
        /// Synaptically connects input neuron.
        /// </summary>
        /// <param name="sourceNeuron">Source neuron to be connected with.</param>
        /// <param name="weight">Synaptic weight.</param>
        /// <param name="delay">Synaptic delay.</param>
        public void ConnectInputNeuron(ReservoirNeuron sourceNeuron, double weight, int delay)
        {
            ReservoirSynapse synapse = new ReservoirSynapse(sourceNeuron, weight, delay);
            InputSynapses.Add(synapse);
            return;
        }

        /// <summary>
        /// Synaptically connects hidden neuron.
        /// </summary>
        /// <param name="sourceNeuron">Source neuron to be connected with.</param>
        /// <param name="weight">Synaptic weight.</param>
        /// <param name="delay">Synaptic delay.</param>
        public void ConnectHiddenNeuron(ReservoirNeuron sourceNeuron, double weight, int delay)
        {
            ReservoirSynapse synapse = new ReservoirSynapse(sourceNeuron, weight, delay);
            HiddenSynapses.Add(synapse);
            return;
        }

        /// <summary>
        /// Selects synapses connecting input neurons and scales their weight
        /// by synapses count to keep max total strength of summed weights.
        /// </summary>
        public void AdjustInputSynapsesWeight()
        {
            if (InputSynapses.Count > 1)
            {
                foreach (ReservoirSynapse synapse in InputSynapses)
                {
                    synapse.ScaleWeight(1d / InputSynapses.Count);
                }
            }
            return;
        }

        /// <summary>
        /// Selects synapses connecting hidden neurons and scales their weight
        /// by given scale factor.
        /// </summary>
        /// <param name="factor">Scale factor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleHiddenSynapsesWeight(double factor)
        {
            foreach (ReservoirSynapse synapse in HiddenSynapses)
            {
                synapse.ScaleWeight(factor);
            }
            return;
        }

        /// <summary>
        /// Collects stimuli from an external input and connected neurons.
        /// </summary>
        /// <param name="externalInput">External input.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CollectStimuli(double externalInput = 0)
        {
            _stimuliSum = externalInput;
            foreach (ReservoirSynapse synapse in InputSynapses)
            {
                _stimuliSum += synapse.Pull();
            }
            foreach (ReservoirSynapse synapse in HiddenSynapses)
            {
                _stimuliSum += synapse.Pull();
            }
            return;
        }

        /// <summary>
        /// Recomputes neuron according to new stimuli collected by previously called CollectStimuli method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recompute()
        {
            //Save previous activation
            _prevActivation = _activation;
            //Compute new activation
            _activation = (_retainment * _prevActivation) + (1d - _retainment) * _activationFn.Compute(_stimuliSum);
            //Predictors
            //Activation
            if (PredictorSwitches[(int)Predictor.Activation])
            {
                Predictors[(int)Predictor.Activation] = _activation;
            }
            else
            {
                Predictors[(int)Predictor.Activation] = 0d;
            }
            //Squared activation
            if (PredictorSwitches[(int)Predictor.SquaredActivation])
            {
                //Square it but keep sign
                Predictors[(int)Predictor.SquaredActivation] = _activation * _activation * Math.Sign(_activation);
            }
            else
            {
                Predictors[(int)Predictor.SquaredActivation] = 0d;
            }
            //Spikes fading trace
            if (PredictorSwitches[(int)Predictor.SpikesFadingTrace])
            {
                Predictors[(int)Predictor.SpikesFadingTrace] *= _fadingCoeff;
                //Original analog spike event implementation firstly invented in predecessor project (NET, StateMachine)
                //SpikeEvent = (_activation - _prevActivation) >= _spikeEventThreshold;
                //Enhanced spike implementation (firstly invented here in EasyML)
                //Spike power as Logarithmized Threshold Quants
                SpikeEvent = ((_activation - _prevActivation) >= _spikeEventThreshold);
                SpikePower = 0d;
                if (SpikeEvent)
                {
                    double denominator = Math.Log(1d / _spikeEventThreshold) + 1d;
                    SpikePower = Math.Min(denominator, 1d + Math.Log((_activation - _prevActivation) / _spikeEventThreshold)) / denominator;
                }
                Predictors[(int)Predictor.SpikesFadingTrace] += SpikePower;
            }
            else
            {
                Predictors[(int)Predictor.SpikesFadingTrace] = 0d;
            }
            return;
        }

        /// <summary>
        /// Resets neuron and all its synapses.
        /// </summary>
        public void Reset()
        {
            _activation = 0d;
            _prevActivation = 0d;
            SpikeEvent = false;
            SpikePower = 0d;
            _stimuliSum = 0d;
            Array.Fill(Predictors, 0d);
            foreach(ReservoirSynapse synapse in InputSynapses)
            {
                synapse.Reset();
            }
            foreach (ReservoirSynapse synapse in HiddenSynapses)
            {
                synapse.Reset();
            }
            return;
        }

        /// <summary>
        /// Updates stats of input and hidden synapses weight.
        /// </summary>
        /// <param name="inputWStat">Input synapses weight stat.</param>
        /// <param name="hiddenWStat">Hidden synapses weight stat.</param>
        public void UpdateSynapsesWeightStat(BasicStat inputWStat, BasicStat hiddenWStat)
        {
            foreach(ReservoirSynapse synapse in InputSynapses)
            {
                inputWStat.AddSample(synapse.GetWeight());
            }
            foreach (ReservoirSynapse synapse in HiddenSynapses)
            {
                hiddenWStat.AddSample(synapse.GetWeight());
            }
            return;
        }

    }//ReservoirNeuron

}//Namespace
