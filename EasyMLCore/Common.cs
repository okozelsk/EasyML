using System.Collections.Generic;
using System;

namespace EasyMLCore
{

    //Namespace enumerations
    /// <summary>
    /// Type of computation output task.
    /// </summary>
    public enum OutputTaskType
    {
        /// <summary>
        /// A real-value(s) quantity computation.
        /// </summary>
        Regression,
        /// <summary>
        /// Binary classification (one or more independent real-world binary decisions).
        /// </summary>
        Binary,
        /// <summary>
        /// Multi-Class classification (more than two classes where one takes all in the real-world).
        /// </summary>
        Categorical
    }

    /// <summary>
    /// Activation function identifier.
    /// </summary>
    public enum ActivationFnID
    {
        /// <summary>
        /// The Bent Identity activation function.
        /// </summary>
        BentIdentity,
        /// <summary>
        /// The Elliot activation function.
        /// </summary>
        ElliotSig,
        /// <summary>
        /// The Exponential Linear Unit activation function.
        /// </summary>
        ELU,
        /// <summary>
        /// The Gaussian Error Linear Unit activation function.
        /// </summary>
        GELU,
        /// <summary>
        /// The Leaky Rectified Linear Unit activation function.
        /// </summary>
        LeakyReLU,
        /// <summary>
        /// The Linear activation function.
        /// </summary>
        Linear,
        /// <summary>
        /// The Radial Basis (aka Gaussian) activation function.
        /// </summary>
        RadBas,
        /// <summary>
        /// The Rectified Linear Unit activation function.
        /// </summary>
        ReLU,
        /// <summary>
        /// The Scaled Exponential Linear Unit activation function.
        /// </summary>
        SELU,
        /// <summary>
        /// The Sigmoid activation function.
        /// </summary>
        Sigmoid,
        /// <summary>
        /// The Softmax activation function.
        /// </summary>
        Softmax,
        /// <summary>
        /// The Softplus activation function.
        /// </summary>
        Softplus,
        /// <summary>
        /// The Hyperbolic Tangent activation function.
        /// </summary>
        TanH
    }//ActivationFnID

    /// <summary>
    /// Dropout mode.
    /// </summary>
    public enum DropoutMode
    {
        /// <summary>
        /// No dropout.
        /// </summary>
        None,
        /// <summary>
        /// Bernoulli dropout mode.
        /// </summary>
        Bernoulli,
        /// <summary>
        /// Gaussian dropout mode.
        /// </summary>
        Gaussian
    }

    /// <summary>
    /// Optimizer identificator.
    /// </summary>
    public enum Optimizer
    {
        /// <summary>
        /// The RProp optimizer (Resilient Back Propagation).
        /// </summary>
        RProp,
        /// <summary>
        /// The SGD optimizer with momentum option (Stochastic Gradient Descent).
        /// </summary>
        SGD,
        /// <summary>
        /// The Adam optimizer with Amsgrad option (Adaptive Moment Estimation).
        /// </summary>
        Adam,
        /// <summary>
        /// The Adabelief optimizer (Adapting Stepsizes by the Belief in Observed Gradients).
        /// </summary>
        Adabelief,
        /// <summary>
        /// The Padam optimizer (Partially Adaptive Moment Estimation).
        /// </summary>
        Padam,
        /// <summary>
        /// The Adamax optimizer (variant of Adam based on the infinity norm).
        /// </summary>
        Adamax,
        /// <summary>
        /// The Adagrad optimizer.
        /// </summary>
        Adagrad,
        /// <summary>
        /// The Adadelta optimizer (an extension of Adagrad that attempts to solve its radically diminishing learning rates).
        /// </summary>
        Adadelta,
        /// <summary>
        /// The RMSProp optimizer with Centered option.
        /// </summary>
        RMSProp
    }

    /// <summary>
    /// Holds common constants, static variables and routines.
    /// </summary>
    public static class Common
    {
        //Constants
        /// <summary>
        /// Epsilon. A small constant close to zero.
        /// </summary>
        public const double Epsilon = 1E-8d;

        /// <summary>
        /// The binary decision border.
        /// </summary>
        /// <remarks>
        /// A value less than this border is considered as the False, True otherwise.
        /// </remarks>
        public const double BinDecisionBorder = 0.5d;

        /// <summary>
        /// A default number used to initialize pseudo random number generators.
        /// </summary>
        public const int DefaultRandomSeed = 0;

        //Static methods
        /// <summary>
        /// Gets 0 or 1.
        /// </summary>
        /// <param name="value">Floating point value between 0 and 1.</param>
        /// <returns>O or 1.</returns>
        public static int GetBinary(double value)
        {
            return value < Common.BinDecisionBorder ? 0 : 1;
        }

        /// <summary>
        /// Decides whether two given floating point values represent the same binary meaning.
        /// </summary>
        /// <param name="val1">Floating point value1.</param>
        /// <param name="val2">Floating point value2.</param>
        public static bool HaveSameBinaryMeaning(double val1, double val2)
        {
            return GetBinary(val1) == GetBinary(val2);
        }

        /// <summary>
        /// Simple partitioner. It splits tasks into the partitions in order to utilize
        /// all fully avilable cores, ideally by the same amount of work.
        /// </summary>
        /// <param name="numOfTasks">Total number of tasks to be processed.</param>
        /// <param name="desiredNumOfPartitions">If possible, this num of patitions will be prepared. Value LE 0 means to use automatic partitioning.</param>
        /// <returns>List of tuples (partitions) where each contains tasks zero based index range (Item1: starting task index inclusive, Item2: ending task index exclusive) and zero based partition index (Item3).</returns>
        public static List<Tuple<int, int, int>> GetFixedPartitions(int numOfTasks, int desiredNumOfPartitions = -1)
        {
            //Checks
            if(numOfTasks <= 0)
            {
                throw new ArgumentException("Number of tasks has to be greater than 0.", nameof(numOfTasks));
            }
            int maxProcessorsToUtilize = Environment.ProcessorCount - 1;
            int maxPartitions = Math.Max(1, desiredNumOfPartitions <= 0 ? maxProcessorsToUtilize : desiredNumOfPartitions);
            double partitionSize = Math.Max(1d, numOfTasks / (double)maxPartitions);
            int numOfPartitions = Math.Min(numOfTasks, maxPartitions);
            List<Tuple<int, int, int>> partitions = new List<Tuple<int, int, int>>(numOfPartitions);
            //Create partitions
            int fromIdx = 0;
            for (int i = 0; i < numOfPartitions; i++)
            {
                int shoulBeDone = (int)Math.Round((i + 1) * partitionSize, MidpointRounding.AwayFromZero);
                int thisStepSize = shoulBeDone - fromIdx;
                if(fromIdx + thisStepSize > numOfTasks)
                {
                    thisStepSize = numOfTasks - fromIdx;
                }
                partitions.Add(new Tuple<int, int, int>(fromIdx, fromIdx +thisStepSize, i));
                fromIdx += thisStepSize;
            }
            return partitions;
        }


    }//Common


}//Namespace
