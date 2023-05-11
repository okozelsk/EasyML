using System;
using System.Text;

namespace EasyMLCore.Log
{
    /// <summary>
    /// Implements basic output log using system console.
    /// </summary>
    public sealed class ConsoleLog : IOutputLog
    {
        //Constants
        public const int ConsoleBufferMinWidth = 500;
        public const int ConsoleBufferMinHeight = 1000;

        //Static attributes
        private static readonly object _monitor = new object();

        //Attributes
        private int _lastMessageLength;
        private int _lastCursorLeft;
        private int _lastCursorTop;

        /// <summary>
        /// Provides simple thread safe output journal using system console.
        /// </summary>
        public ConsoleLog()
        {
            lock (_monitor)
            {
                #if Windows
                    //Set console buffer size
                    Console.SetBufferSize(Math.Max(ConsoleBufferMinWidth, Console.BufferWidth), Math.Max(ConsoleBufferMinHeight, Console.BufferHeight));
                    //Adjust console window position and size
                    Console.WindowLeft = 0;
                    Console.WindowTop = 0;
                    Console.WindowWidth = Console.LargestWindowWidth;
                #endif
                //Clear the console
                Console.Clear();
                //Store current cursor position
                _lastCursorLeft = Console.CursorLeft;
                _lastCursorTop = Console.CursorTop;
            }
            //Reset last message length
            _lastMessageLength = 0;
            return;
        }

        //Methods
        /// <inheritdoc/>
        /// <remarks>
        /// Writes a message to the system console.
        /// </remarks>
        public void Write(string message, bool replaceLastMessage = false)
        {
            lock (_monitor)
            {
                if (replaceLastMessage)
                {
                    Console.CursorLeft = _lastCursorLeft;
                    Console.CursorTop = _lastCursorTop;
                    if (_lastMessageLength > 0)
                    {
                        StringBuilder emptyMsg = new StringBuilder(_lastMessageLength);
                        for (int i = 0; i < _lastMessageLength; i++)
                        {
                            emptyMsg.Append(" ");
                        }
                        Console.Write(emptyMsg.ToString());
                        Console.CursorLeft = _lastCursorLeft;
                        Console.CursorTop = _lastCursorTop;
                    }
                    Console.Write(message);
                }
                else
                {
                    Console.WriteLine();
                    _lastCursorLeft = Console.CursorLeft;
                    _lastCursorTop = Console.CursorTop;
                    Console.Write(message);
                }
                _lastMessageLength = message.Length;
            }
            return;
        }

    }//ConsoleLog

}//Namespace
