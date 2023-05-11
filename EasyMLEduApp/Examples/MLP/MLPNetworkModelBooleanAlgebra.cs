using EasyMLCore;
using EasyMLCore.Data;
using EasyMLCore.MLP;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace EasyMLDemoApp.Examples.MLP
{
    /// <summary>
    /// This code example shows how to simply build and test MLP NetworkModel
    /// solving the boolean algebra (AND, OR and XOR).
    /// Example also demonstrates option to initialize model configuration
    /// from xml (available for all EasyML configs by default).
    /// 
    /// </summary>
    /// <seealso cref="EasyML"/>
    public class MLPNetworkModelBooleanAlgebra
    {
        private static readonly List<string> _outputFeatureNames =
            new List<string>() { "AND", "OR", "XOR" };

        //Constructor
        public MLPNetworkModelBooleanAlgebra()
        {
            return;
        }


        //Methods
        /// <summary>
        /// Creates the training data (and in simple case of boolean algebra also testing data).
        /// Input vector contains 0/1 combination and output vector contains appropriate results of the AND, OR and XOR operation.
        /// </summary>
        private static SampleDataset CreateBooleanAlgebraSampleData()
        {
            SampleDataset trainingData = new SampleDataset();
            trainingData.AddSample(trainingData.Count, new double[] { 0, 0 }, new double[] { 0, 0, 0 });
            trainingData.AddSample(trainingData.Count, new double[] { 0, 1 }, new double[] { 0, 1, 1 });
            trainingData.AddSample(trainingData.Count, new double[] { 1, 0 }, new double[] { 0, 1, 1 });
            trainingData.AddSample(trainingData.Count, new double[] { 1, 1 }, new double[] { 1, 1, 0 });
            return trainingData;
        }

        /// <summary>
        /// Creates a configuration of the MLP network having
        /// two LeakyReLU hidden layers of size 10,
        /// associated Resilient Backpropagation optimizer and
        /// training will be performed in two attempts 150 epochs each.
        /// </summary>
        private static NetworkModelConfig CreateNetworkConfig()
        {
            return new NetworkModelConfig(2, //Training attempts
                                          150, //Training attempt epochs
                                          new RPropConfig(), //Weights updater
                                          new HiddenLayersConfig(10, //Hidden layer size (number of neurons)
                                                                 ActivationFnID.LeakyReLU, //Hidden layer activation function
                                                                 2 //Number of hidden layers
                                                                 )
                                         );
        }

        /// <summary>
        /// Creates a configuration of the MLP network having
        /// two LeakyReLU hidden layers of size 10,
        /// associated Resilient Backpropagation optimizer and
        /// training will be performed in two attempts 150 epochs each.
        ///         
        /// This function version is equivalent to CreateNetworkConfig function but
        /// this version shows alternative use of the Xml constructor of the NetworkModelConfig class.
        /// </summary>
        private static NetworkModelConfig CreateNetworkConfigFromXml()
        {
            //Xml text
            string xmlStr = "";
            xmlStr += "<networkModel attempts=\"2\" epochs=\"150\">\n";
            xmlStr += "  <rprop />\n";
            xmlStr += "  <hiddenLayers>\n";
            xmlStr += "    <layer neurons=\"10\" activation=\"LeakyReLU\" />\n";
            xmlStr += "    <layer neurons=\"10\" activation=\"LeakyReLU\" />\n";
            xmlStr += "  </hiddenLayers>\n";
            xmlStr += "</networkModel>\n";
            //Use Xml constructor
            return new NetworkModelConfig(XElement.Parse(xmlStr));
        }

        private void CalculateAndReportOutputs(ModelBase model)
        {
            for (int b1 = 0; b1 <= 1; b1++)
            {
                for (int b2 = 0; b2 <= 1; b2++)
                {
                    EasyML.Oper.Log.Write($"Results of boolean algebra computation when input is ({b1}, {b2}):");
                    double[] computed = model.Compute(new double[] { b1, b2 });
                    TaskOutputDetailBase outputDetail = model.GetOutputDetail(computed);
                    EasyML.Oper.Log.Write(outputDetail.GetTextInfo());
                }
            }
            return;
        }

        /// <summary>
        /// Builds and tests network model to solve boolean algebra.
        /// This version uses CreateNetworkConfig() method which directly
        /// sets configuration parameters.
        /// </summary>
        private void ModelConfigFromSourceCodeExample()
        {
            EasyML.Oper.Log.Write("Example of NetworkModel solving boolean algebra:");
            //Call method to create network model configuration from scratch in its source code.
            NetworkModelConfig networkModelCfg = CreateNetworkConfig();
            SampleDataset samples = CreateBooleanAlgebraSampleData();
            //Build model
            ModelBase model = EasyML.Oper.Build(networkModelCfg, //Network model configuration
                                                "Solving boolean algebra", //Our name of the task
                                                OutputTaskType.Binary, //Type of the task
                                                _outputFeatureNames, //Output feature names
                                                samples, //Training sample data
                                                true, //We want to report progress and results
                                                true //We want to report max detail
                                                );
            //Test model
            ModelErrStat errStat = EasyML.Oper.Test(model, //Our built model
                                                      samples, //Testing samples (here the same as training samples)
                                                      out ResultDataset resultData, //Original testing samples together with computed data
                                                      true, //We want to report progress and results
                                                      true //We want to report max detail
                                                      );

            //Finally, let's try calling the model calculations and displaying the results.
            CalculateAndReportOutputs(model);
            return;
        }

        /// <summary>
        /// Builds and tests network model to solve boolean algebra.
        /// This version uses CreateNetworkConfigFromXml() method which creates
        /// configuration from parameters in a XML.
        /// </summary>
        private void ModelConfigFromXMLExample()
        {
            EasyML.Oper.Log.Write("Example of NetworkModel solving boolean algebra:");
            //Call method to create network model configuration from from xml
            NetworkModelConfig networkModelCfg = CreateNetworkConfigFromXml();
            SampleDataset samples = CreateBooleanAlgebraSampleData();
            //Build model
            ModelBase model = EasyML.Oper.Build(networkModelCfg, //Network model configuration
                                                "Solving boolean algebra", //Our name of the task
                                                OutputTaskType.Binary, //Type of the task
                                                _outputFeatureNames, //Output feature names
                                                samples, //Training sample data
                                                true, //We want to report progress
                                                true //We want to report max detail
                                                );
            //Test model
            ModelErrStat errStat = EasyML.Oper.Test(model, //Our built model
                                                      samples, //Testing samples (here the same as training samples)
                                                      out ResultDataset resultData, //Original testing samples together with computed data
                                                      true, //We want to report progress
                                                      true //We want to report max detail
                                                      );

            //Finally, let's try calling a separate calculation and displaying the results.
            CalculateAndReportOutputs(model);
            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            Console.Clear();
            ModelConfigFromSourceCodeExample();
            EasyML.Oper.Log.Write("Press Enter to continue to second (XML) version...");
            Console.ReadLine();
            EasyML.Oper.Log.Write(string.Empty, true);
            EasyML.Oper.Log.Write(string.Empty);
            ModelConfigFromXMLExample();
            return;
        }//Run

    }//MLPNetworkModelBooleanAlgebra

}//Namespace
