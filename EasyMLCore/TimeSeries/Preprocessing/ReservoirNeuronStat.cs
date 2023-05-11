using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.TimeSeries
{
    /// <summary>
    /// Holds statistics of single neuron.
    /// </summary>
    [Serializable]
    public class ReservoirNeuronStat : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Activation stat.
        /// </summary>
        public BasicStat ActivationStat { get; }

        /// <summary>
        /// Negative activation stat.
        /// </summary>
        public BasicStat NegActivationStat { get; }

        /// <summary>
        /// Positive activation stat.
        /// </summary>
        public BasicStat PosActivationStat { get; }

        /// <summary>
        /// Spike-event stat.
        /// </summary>
        public BasicStat SpikeEventStat { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public ReservoirNeuronStat()
        {
            ActivationStat = new BasicStat();
            PosActivationStat = new BasicStat();
            NegActivationStat = new BasicStat();
            SpikeEventStat = new BasicStat();
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance..</param>
        public ReservoirNeuronStat(ReservoirNeuronStat source)
        {
            ActivationStat = new BasicStat(source.ActivationStat);
            PosActivationStat = new BasicStat(source.PosActivationStat);
            NegActivationStat = new BasicStat(source.NegActivationStat);
            SpikeEventStat = new BasicStat(source.SpikeEventStat);
            return;
        }

        //Methods
        /// <summary>
        /// Updates neuron stats.
        /// </summary>
        /// <param name="neuron">Reservoir's neuron.</param>
        public void Update(ReservoirNeuron neuron)
        {
            ActivationStat.AddSample(neuron.GetActivation());
            if (neuron.GetActivation() < 0d)
            {
                NegActivationStat.AddSample(neuron.GetActivation());
            }
            else if (neuron.GetActivation() > 0d)
            {
                PosActivationStat.AddSample(neuron.GetActivation());
            }
            SpikeEventStat.AddSample(neuron.SpikeEvent ? 1d : 0d);
            return;
        }

    }//ReservoirNeuronStat

}//Namespace
