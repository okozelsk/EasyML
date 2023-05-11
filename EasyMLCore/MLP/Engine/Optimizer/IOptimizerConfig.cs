using System.Xml.Linq;

namespace EasyMLCore.MLP
{
    /// <summary>
    /// Common interface of optimizer configurations.
    /// </summary>
    public interface IOptimizerConfig
    {
        /// <inheritdoc cref="Optimizer"/>
        Optimizer OptimizerID { get; }

        /// <inheritdoc cref="ConfigBase.DeepClone"/>
        ConfigBase DeepClone();

        /// <inheritdoc cref="ConfigBase.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//IOptimizerConfig

}//Namespace
