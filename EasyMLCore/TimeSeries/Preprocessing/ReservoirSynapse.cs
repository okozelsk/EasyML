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
        private readonly int _presynapticNeuronIndex;
        private readonly double[] _queueBuffer;
        private int _enqueueIndex;
        private int _dequeueIndex;
        private int _count;
        private double _weight;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="presynapticNeuronIndex">Index of synapse's presynaptic (stimuli source) neuron.</param>
        /// <param name="weight">Synaptic weight.</param>
        /// <param name="delay">Synaptic delay.</param>
        public ReservoirSynapse(int presynapticNeuronIndex, double weight, int delay)
        {
            Delay = delay;
            _presynapticNeuronIndex = presynapticNeuronIndex;
            _weight = weight;
            _queueBuffer = Delay > 0 ? new double[Delay + 1] : null;
            Reset();
            return;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">Source instance.</param>
        public ReservoirSynapse(ReservoirSynapse source)
        {
            Delay = source.Delay;
            _presynapticNeuronIndex = source._presynapticNeuronIndex;
            _weight = source._weight;
            _queueBuffer = (double[])source._queueBuffer?.Clone();
            _enqueueIndex = source._enqueueIndex;
            _dequeueIndex = source._dequeueIndex;
            _count = source._dequeueIndex;
            return;
        }

        //Methods
        /// <summary>
        /// Gets synapse's weight.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetWeight() { return _weight; }

        /// <summary>
        /// Gets presynaptic neuron's index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPresynapticNeuronIndex() { return _presynapticNeuronIndex; }

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
        public double Pull(ReservoirNeuron[] neurons)
        {
            //Delay?
            if(_queueBuffer == null)
            {
                //No delay
                return neurons[_presynapticNeuronIndex].GetActivation() * _weight;
            }
            //Enqueue
            _queueBuffer[_enqueueIndex++] = neurons[_presynapticNeuronIndex].GetActivation() * _weight;
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

        /// <summary>
        /// Creates deep clone.
        /// </summary>
        public ReservoirSynapse DeepClone()
        {
            return new ReservoirSynapse(this);
        }

    }//Reservoir synapse
}//Namespace
