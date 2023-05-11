using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyMLCore
{
    /// <summary>
    /// Implements basic methods of serializable objects.
    /// </summary>
    [Serializable]
    public class SerializableObject
    {
        //Static methods
        /// <summary>
        /// Deserializes object instance from specified stream.
        /// </summary>
        /// <param name="stream">A stream.</param>
        /// <returns>Deserialized object instance.</returns>
        public static SerializableObject Deserialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Typ nebo člen je zastaralý.
            return (SerializableObject)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Typ nebo člen je zastaralý.
        }

        /// <summary>
        /// Deserializes object instance from specified file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>Deserialized object instance.</returns>
        public static SerializableObject Deserialize(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                return Deserialize(stream);
            }
        }

        //Methods
        /// <summary>
        /// Serializes this instance into the specified stream.
        /// </summary>
        /// <param name="stream">A stream.</param>
        public void Serialize(Stream stream)
        {
            BinaryFormatter formatter = new();
#pragma warning disable SYSLIB0011 // Typ nebo člen je zastaralý.
            formatter.Serialize(stream, this);
#pragma warning restore SYSLIB0011 // Typ nebo člen je zastaralý.
            return;
        }

        /// <summary>
        /// Serializes this instance into the specified file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Serialize(string fileName)
        {
            using (Stream stream = File.Create(fileName))
            {
                Serialize(stream);
            }
            return;
        }

    }//SerializableObject

}//Namespace
