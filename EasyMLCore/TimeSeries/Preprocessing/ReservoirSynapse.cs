using System;
using System.Runtime.CompilerServices;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Implements reservoir synapse.
    /// </summary>
    [Serializable]
    public class ReservoirSynapse : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Synapse's delay.
        /// </summary>
        public int Delay { get; private set; }

        //Attributes
        private readonly ReservoirNeuron _presynapticNeuron;
        private readonly double[] _queueBuffer;
        private int _enqueueIndex;
        private int _dequeueIndex;
        private int _count;
        private double _weight;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="presynapticNeuron">Synapse's presynaptic (stimuli source) neuron.</param>
        /// <param name="weight">Synaptic weight.</param>
        /// <param name="delay">Synaptic delay.</param>
        public ReservoirSynapse(ReservoirNeuron presynapticNeuron, double weight, int delay)
        {
            Delay = delay;
            _presynapticNeuron = presynapticNeuron;
            _weight = weight;
            _queueBuffer = Delay > 0 ? new double[Delay + 1] : null;
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Gets synapse's weight.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetWeight() { return _weight; }

        /// <summary>
        /// Gets synapse's presynaptic neuron.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReservoirNeuron GetPresynapticNeuron() { return _presynapticNeuron; }

        /// <summary>
        /// Resets synapse.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _enqueueIndex = 0;
            _dequeueIndex = 0;
            _count = 0;
            return;
        }

        /// <summary>
        /// Scales weight by given scale factor.
        /// </summary>
        /// <param name="factor">Scale factor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleWeight(double factor)
        {
            _weight *= factor;
            return;
        }

        /// <summary>
        /// Gets stimuli from source neuron.
        /// </summary>
        /// <returns>Stimuli.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Pull()
        {
            //Delay?
            if(_queueBuffer == null)
            {
                //No delay
                return _presynapticNeuron.GetActivation() * _weight;
            }
            //Enqueue
            _queueBuffer[_enqueueIndex++] = _presynapticNeuron.GetActivation() * _weight;
            if (_enqueueIndex == _queueBuffer.Length)
            {
                _enqueueIndex = 0;
            }
            ++_count;
            //Full?
            if(_count == _queueBuffer.Length)
            {
                //Queue is full -> send pending signal
                double stimuli = _queueBuffer[_dequeueIndex++];
                if (_dequeueIndex == _queueBuffer.Length)
                {
                    _dequeueIndex = 0;
                }
                --_count;
                return stimuli;
            }
            else
            {
                //No signal yet
                return 0d;
            }
        }

    }//Reservoir synapse
}//Namespace
