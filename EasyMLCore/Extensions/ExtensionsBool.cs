using System.Runtime.CompilerServices;

namespace EasyMLCore.Extensions
{
    /// <summary>
    /// Implements extensions of the bool type.
    /// </summary>
    public static class ExtensionsBool
    {
        //Constants
        /// <summary>
        /// The xml code of the true value.
        /// </summary>
        private const string TrueXmlCode = "true";

        /// <summary>
        /// The xml code of the false value.
        /// </summary>
        private const string FalseXmlCode = "false";

        /// <summary>
        /// Gets the xml code corresponding with boolean value.
        /// </summary>
        /// <param name="x">Bool value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetXmlCode(this bool x)
        {
            return x ? TrueXmlCode : FalseXmlCode;
        }

    }//ExtensionsBool

}//Namespace
