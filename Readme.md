# Machine Learning Library for .NET (EasyML)
This solution is a successor of my [older project](https://github.com/okozelsk/NET). 
Unlike the older solution, there is full [**MLP**](https://en.wikipedia.org/wiki/Multilayer_perceptron) support (many optimizers, standard regularization techniques and types of network models). There is also implemented the [**Reservoir Computer**](https://en.wikipedia.org/wiki/Reservoir_computing), which is now much easier to configure and has a more powerful spike-trace predictor for time series classifications. Overall, the components are much simpler to configure and easier to use than in the older solution. 
The entire solution is written for .net 6 and consists of the main library (**EasyMLCore**) and an educational console application (**EasyMLEduApp**), where it is shown how to work with the library.

## EasyMLCore (namespace EasyMLCore)
The source code of the library is independent on any third party and entire code is written for .net 6.0 in C# 10.
The purpose of this library is to support the usual machine-learning scenario in an easy way.
<br />
![Typical ML scenario](./EasyMLCore/Docs/ML_scenario.png)

The root namespace contains some common elements such as xsd describing configuration xml(s).
But the main thing it contains is the [EasyML class with its Oper](./EasyMLCore/EasyML.cs) interface, which provides basic functionalities supporting the ML process in a user-friendly way. The EasyML.Oper interface is a singleton. It is immediately usable and its main methods are LoadSampleData, Report, Build and Test. Unless otherwise stated, the methods log the progress of the operation in the system console by default. To redirect logs elsewhere, it is sufficient to set any instance of a custom object implementing the trivial [IOutputLog](./EasyMLCore/Log/IOutputLog.cs) interface using the EasyML.Oper.ChangeOutputLog method.
If you want to write anything of your own to the active log, use the EasyML.Oper.Log.Write method.
<br />
<br />
**General characteristics and limitations**
* No GPU utilization, only CPUs
* Common floating point data type is double (not float)
* Every ML model class has associated its own Builder class (or directly contains Build method), which requires appropriate Config class. Every Config class holds model's properties, ensures the basic consistency and has constructor(s) for setup from scratch and also the constructor accepting XElement. Every Config class provides GetXml method
* Almost every component is derived from [SerializableObject](./EasyMLCore/SerializableObject.cs) base class and is easily serializable/deserializable using methods of that base class. Serialization uses the BinaryFormatter
* EasyML does not support the use of distributed resources and is intended for the preparation of models solving small to medium-sized tasks. It is not intended for massive ML tasks with hundreds of thousands of samples
* Supported ML task types are: Categorical (multi-class classification), Binary (single or multiple decisions) and Regression (single or multiple forecasting)

### Activation (namespace EasyMLCore.Activation)
Contains activation functions. Currently implemented [activations](./EasyMLCore/Activation) are: BentIdentity, ElliotSig, ELU, GELU, LeakyReLU, Linear, ReLU, SELU, Sigmoid, Softmax, Softplus, TanH.
<br />
![TanH activation](./EasyMLCore/Docs/TanH.png)

### Data (namespace EasyMLCore.Data)
Contains data manipulation/evaluation/description components.

|Main content|Description|
|--|--|
|[CsvDataHolder](./EasyMLCore/Data/CsvTools/CsvDataHolder.cs)|Provides easy reading and writing of data in csv format.|
|[SampleDataset](./EasyMLCore/Data/Dataset/SampleDataset.cs)|Implements a set of samples, where each sample holds an input vector and corresponding output vector (vector is a 1D array of double numbers). Component provides samples for Training and Test methods and can be initialized from a CsvDataHolder, another SampleDataset or directly by you, from your custom code.|
|[ResultDataset](./EasyMLCore/Data/Dataset/ResultDataset.cs)|Holds triplets of vectors and usually it is an output from Test methods. Each triplet consists of sample input vector, sample output vector (ideal) and computed vector (computation output of tested model).|
|[TaskErrStat](./EasyMLCore/Data/TaskErrStat)|A set of ML task-specific error statistics holders. Each ML task type has associated its root summary error statistics class, which inherits and/or encapsulates other more granular/detailed error statistics classes.|
|[TaskOutputDetail](./EasyMLCore/Data/TaskOutputDetail)|A set of ML task-specific computed output descriptors. Each ML task type has associated its xxxOutputDetail class, which provides detailed information about computed values (also in textual form).|

### MLP (namespace EasyMLCore.MLP)

![MLP models](./EasyMLCore/Docs/MLP_models.png)

### TimeSeries (namespace EasyMLCore.TimeSeries)


The detailed description is being worked on and will be added gradually and as soon as possible.
