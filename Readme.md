# Machine Learning Library for .NET (EasyML)
This repo is a successor of my [older repo](https://github.com/okozelsk/NET). 
Now there is full [**MLP**](https://en.wikipedia.org/wiki/Multilayer_perceptron) support (many optimizers, standard regularization techniques and types of network models). There is also implemented the [**Reservoir Computer**](https://en.wikipedia.org/wiki/Reservoir_computing), which is now much easier to configure and has a more powerful spike-trace predictor for time series classifications. Overall, the components are much simpler to configure and easier to use than in the older solution. 
Repo consists of the main library (**EasyMLCore**) and a small educational console application (**EasyMLEduApp**), where it is shown how to work with the library.

## EasyMLCore (namespace EasyMLCore)
The purpose is to support the usual machine-learning scenario in an easy way.
<br />
![Typical ML scenario](./EasyMLCore/Docs/ML_scenario.png)

The EasyMLCore namespace is a root namespace of the library. It contains some common elements but the main thing it contains is the [EasyML class with its Oper](./EasyMLCore/EasyML.cs) interface, which provides basic functionalities supporting the ML process in a user-friendly way. The EasyML.Oper interface is a singleton. It is immediately usable and its main methods are LoadSampleData, Report, Build and Test. Unless otherwise stated, the methods log the progress of the operation in the system console by default. To redirect logs elsewhere, it is sufficient to set any instance of a custom object implementing the trivial [IOutputLog](./EasyMLCore/Log/IOutputLog.cs) interface using the EasyML.Oper.ChangeOutputLog method.
If you want to write anything of your own to the active log, use the EasyML.Oper.Log.Write method.
<br />
<br />
**General characteristics and limitations**
* The source code is independent on any third party and entire code is written for .net 6.0 in C# 10
* No GPU utilization, only CPUs
* Common floating point data type is double (not float)
* Each ML model class provides static method Build, which creates valid and trained instance (there is no public constructors). The Build method always requires an instance of the appropriate Config class. Config class specifies model's properties, ensures the basic consistency and has constructor(s) for setup from scratch and also the constructor accepting XElement. Config class of any type always provides GetXml method
* Each ML model provides a Compute method (respectively the IComputable interface). The method expects a 1D array of doubles on input and returns the result as a 1D array of doubles as well
* Each ML model provides a Test method that computes a test dataset and returns the results along with error statistics
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
|[SampleDataset](./EasyMLCore/Data/Dataset/SampleDataset.cs)|Implements a set of samples, where each sample holds an input vector and corresponding output vector (vector is a 1D array of double numbers). Component provides samples for Build (training) and Test methods and can be initialized from a CsvDataHolder, another SampleDataset or directly by you, from your custom code.|
|[ResultDataset](./EasyMLCore/Data/Dataset/ResultDataset.cs)|Holds triplets of vectors and usually it is an output from Test methods. Each triplet consists of sample input vector, sample output vector (ideal) and computed vector (computation output of tested model).|
|[TaskErrStat](./EasyMLCore/Data/TaskErrStat)|A set of ML task-specific error statistics holders. Each ML task type has associated its root summary error statistics class, which inherits and/or encapsulates other more granular/detailed error statistics classes.|
|[TaskOutputDetail](./EasyMLCore/Data/TaskOutputDetail)|A set of ML task-specific computed output descriptors. Each ML task type has associated its xxxOutputDetail class, which provides detailed information about computed values (also in textual form).|

### MLP (namespace EasyMLCore.MLP)
Contains MLP engine and MLP models. The mutual relationship is shown schematically in the following figure and will be described in more detail below.

![MLP models](./EasyMLCore/Docs/MLP_models.png)

|Main content|Description|
|--|--|
|[MLPEngine](./EasyMLCore/MLP/Engine/MLPEngine.cs)|Implements MLP, a classical fully connected Feed Forward network that may or may not have hidden layers. A [Trainer](./EasyMLCore/MLP/Engine/Trainer.cs) component is dedicated to the MLPEngine. Trainer in iterations (epochs) modifies the weights so that the outputs of the network are as close as possible to the expected outputs. The trainer can be instructed to try to train the network several times from the beginning (attempts). This is mainly due to the random nature of the initialization of the network weights, where repeating with a different initial setting of the weights increases the chance of finding the optimum. The trainer ensures packaging of training data (mini-batches or BGD). As part of the training iteration, it applies the set of required [regularization techniques](./EasyMLCore/MLP/Engine) and calculates the gradients, subsequently modifies the weights using one of the [implemented optimizers](./EasyMLCore/MLP/Engine/Optimizer), and finally modifies the weights if any regularization post-rules are set. The implemented regularizations are: Dropout, L1, L2, WeightsNorm, ClassBalancedLoss, GradClipVal and GradClipNorm.|
|[NetworkModel](./EasyMLCore/MLP/Model/Network/NetworkModel.cs)|Encapsulates the MLPEngine. NetworkModel's Build method uses Trainer component and adds functionality such is keeping the best MLPEngine produced during the training attempts/epochs and early stopping. Build method requires instance of the [NetworkModelConfig](./EasyMLCore/MLP/Model/Network/NetworkModelConfig.cs) class.|
|[CrossValModel](./EasyMLCore/MLP/Model/Network/NetworkModel.cs)|Implements the cross-validated model. CrossValModel's Build method builds N NetworkModel(s), where each is trained on different training and validation datasets. Configuration contains "fold ratio" parameter, which determines what part of the available training data will be used for the "validation fold". In other words, before starting the build process, available training data is divided into N folds. For each NetworkModel, one different fold is designated for validation and the rest for training. The CrossValModel finally works by letting all inner NetworkModel(s) perform the calculation and the final result is a weighted average (bagging). Average is weighted according to the confidence metrics achieved by the NetworkModel(s) in the build process. Build method requires instance of the [CrossValModelConfig](./EasyMLCore/MLP/Model/CrossVal/CrossValModelConfig.cs) class.|
|[StackingModel](./EasyMLCore/MLP/Model/Stacking/StackingModel.cs)|Implements the stacking model, which is a stack of several trained NetworkModel(s), the results of which are combined by the meta-learner. Meta-learner can be any MLP model. StackingModel's Build method builds stack members (weak/strong NetworkModels), from the weak members outputs on hold-out folds prepares training data for meta-learner and then builds the meta-learner. Weak stack members are temporary and they are wiped out during the build process. Meta-learner combines outputs from strong stack members and it's output is the final output of StackingModel. Build method requires instance of the [StackingModelConfig](./EasyMLCore/MLP/Model/Stacking/StackingModelConfig.cs) class.|
|[CompositeModel](./EasyMLCore/MLP/Model/Composite/CompositeModel.cs)|Implements the composite model, which is a cluster of several trained MLP Model(s), the results of which are weighted. CompositeModel's Build method builds specified inner MLP models one by one. The CompositeModel finally works by letting all inner MLP models perform the calculation and the final result is a weighted average (bagging). Average is weighted according to the confidence metrics achieved by the models in the build process. Build method requires instance of the [CompositeModelConfig](./EasyMLCore/MLP/Model/Composite/CompositeModelConfig.cs) class.|


### TimeSeries (namespace EasyMLCore.TimeSeries)
Contains implementation of [Reservoir](./EasyMLCore/TimeSeries/Preprocessing/Reservoir.cs) and the [Reservoir Computer](./EasyMLCore/TimeSeries/ResComp.cs) to solve ML tasks where input is univariate or multivariate time series.
<br />
#### Reservoir
Reservoir is a neural preprocessor consisting of hidden recurrent network as is schematically shown in the following figure. Hidden neurons have usually TanH activation.
Reservoir is implemented as a classic ESN, but with one unique essential feature on top, which is the ability of an Hidden neuron to spike in relation to the dynamics of changes in its activation.
Here implemented Reservoir is therefore an ESN lightly combined with a LSM.

![Reservoir](./EasyMLCore/Docs/Reservoir.png)

*Reservoir configuration*
<br />
Reservoir has its own [Config class](./EasyMLCore/TimeSeries/Preprocessing/ReservoirConfig.cs). It is necessary to specify several parameters in the configuration (a default value is available for most of them).
Reservoir's Config has two parts: [Input config](./EasyMLCore/TimeSeries/Preprocessing/ReservoirInputConfig.cs) and [Hidden layer config](./EasyMLCore/TimeSeries/Preprocessing/ReservoirHiddenLayerConfig.cs).

*Reservoir's output*
<br />
Each Reservoir's Hidden neuron provides a set of predictors.

|Predictor type|Description|
|--|--|
|Activation|Use it for Regression tasks. It is simply the current activation value of the hidden neuron.|
|Squared Activation|Use it for Regression tasks (together with Activation or alone). It is the current activation value of Hidden neuron squared, but with the preserved sign.|
|Spikes Fading Trace|Use it for Categorical and Binary tasks.The predictor is constructed as a gradually decaying trace of the history of spikes emitted by the Hidden neuron. The predictor is the result of my research and is my main contribution to Reservoir Computing.|

The output of the Reservoir is all the predictors collected from all the hidden neurons as well as the original input to the Reservoir.
In order to further work with the output, it is divided into sections: Activations, SquaredActivations, SpikesFadingTraces and ResInput.

#### Reservoir Computer
The Reservoir Computer is shown schematically in the following figure. 

![Reservoir Computer](./EasyMLCore/Docs/ResComp.png)

The mutual relationship of the components will be described in more detail within the description of the individual components.

|Main content|Description|
|--|--|
|[Reservoir](./EasyMLCore/TimeSeries/Preprocessing/Reservoir.cs)|...|
|[ResCompTask](./EasyMLCore/TimeSeries/ResCompTask.cs)|...|
|[ResComp](./EasyMLCore/TimeSeries/ResComp.cs)|...|

The detailed description is being worked on and will be added gradually and as soon as possible.

## EasyMLEduApp (namespace EasyMLEduApp)
Contains a small console application (.net 6, C#10), where is shown how to work with EasyMLCore. Application has no startup parameters and walking through examples is solved as the menu.
Examples are divided into two main parts. The first one shows usage of MLP models (here is recommended to start) and the second shows usage of Reservoir Computer.
Application uses datasets stored in Data sub-folder. Datasets are in csv format and each dataset has associated text file describing it (just for information). Application writes serialization data to Temp sub-folder.

#### Contact
Questions, ideas, suggestions for improvement and other constructive comments are welcome at my email address oldrich.kozelsky@email.cz