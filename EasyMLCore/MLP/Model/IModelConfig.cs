using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Common interface of all model configurations.
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
