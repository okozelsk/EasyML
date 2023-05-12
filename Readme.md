# Machine Learning Library for .NET (EasyML)
This solution is a successor of my [older project](https://github.com/okozelsk/NET). 
Unlike the older solution, there is full [**MLP**](https://en.wikipedia.org/wiki/Multilayer_perceptron) support (many optimizers, standard regularization techniques and types of network models). There is also implemented the [**Reservoir Computer**](https://en.wikipedia.org/wiki/Reservoir_computing), which is now much easier to configure and has a more powerful spike-trace predictor for time series classifications. Overall, the components are much simpler to configure and easier to use than in the older solution. 
The entire solution is written for .net 6 and consists of the main library (**EasyMLCore**) and an educational console application (**EasyMLEduApp**), where it is shown how to work with the library.

## EasyMLCore (namespace EasyMLCore)
The source code of the library is independent on any third party and entire code is written for .net 6.0 in C# 10.
The purpose of this library is to support the usual machine-learning scenario in an easy way.
<br />
![Typical ML scenario](./EasyMLCore/Docs/ML_scenario.png)

### Activation (namespace EasyMLCore.Activation)
Contains activation functions. Currently implemented activations are: BentIdentity, ElliotSig, ELU, GELU, LeakyReLU, Linear, ReLU, SELU, Sigmoid, Softmax, Softplus, TanH.

### Data (namespace EasyMLCore.Data)
Contains data manipulation/evaluation/description components.

|Main content|Description|
|--|--|
|[CsvDataHolder](./EasyMLCore/Data/CsvTools/CsvDataHolder.cs)|Provides easy reading and writing of data in csv format.|
|[SampleDataset](./EasyMLCore/Data/Dataset/SampleDataset.cs)|Implements a set of samples, where each sample holds an input vector and corresponding output vector (vector is a 1D array of double numbers). Component provides samples for Training and Test methods and can be initialized from a CsvDataHolder, another SampleDataset or directly by you, from your custom code.|
|[ResultDataset](./EasyMLCore/Data/Dataset/ResultDataset.cs)|Holds triplets of vectors and usually it is an output from Test methods. Each triplet consists of sample input vector, sample output vector (ideal) and computed vector (computation output of tested model).|
|[TaskErrStat](./EasyMLCore/Data/TaskErrStat)|A set of task-specific error statistics holders. Task can be Categorical (multi-class classification), Binary (single or multiple decisions) or Regression (single or multiple forecasting). Each task type has associated its root summary error statistics class, which inherits and/or encapsulates other more granular/detailed error statistics classes.|



The detailed description is being worked on and will be added gradually and as soon as possible.
