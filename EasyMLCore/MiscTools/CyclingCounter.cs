using System;

namespace EasyMLCore.MiscTools
{
    /// <summary>
    /// Implements cycling counter (from -> to).
    /// </summary>
    public class CyclingCounter
    {
        //Attributes
        private readonly int _from;
        private readonly int _to;
        private readonly int _step;
        private int _counter;

        //Constructor
        /// <summary>
        /// Create initialized instance.
        /// </summary>
        /// <param name="from">From inclusive.</param>
        /// <param name="to">To inclusive.</param>
        /// <param name="step">Increment step.</param>
        public CyclingCounter(int from, int to, int step)
        {
            if(from >= to)
            {
                throw new ArgumentException($"From ({from}) must be LT to ({to}).", nameof(from));
            }
            if (step == 0)
            {
                throw new ArgumentException($"Step can not be 0.", nameof(step));
            }
            int dist = to - from;
            if(dist % step != 0)
            {
                throw new ArgumentException($"Step must divide interval (to - from).", nameof(step));
            }
            _from = from;
            _to = to;
            _step = step;
            Reset();
            return;
        }

        public void Reset()
        {
            _counter = _from - _step;
            return;
        }

        public int GetNext()
        {
            _counter += _step;
            if (_counter > _to)
            {
                _counter = _from;
            }
            return _counter;
        }

    }//CyclingCounter


}//Namespace


