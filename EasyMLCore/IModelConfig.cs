using System.Xml.Linq;

namespace EasyMLCore
{
    /// <summary>
    /// Common interface of all ML models configurations.
    /// </summary>
    public interface IModelConfig
    {
        /// <inheritdoc cref="ConfigBase"/>
        ConfigBase DeepClone();

        /// <inheritdoc cref="ConfigBase"/>
        XElement GetXml(string rootElemName, bool suppressDefaults);

        /// <inheritdoc cref="ConfigBase"/>
        XElement GetXml(bool suppressDefaults);

    }//IModelConfig

}//Namespace
