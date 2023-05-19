using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMLCore.MLP.Model
{
    /// <summary>
    /// Holds diagnostic data related to model.
    /// </summary>
    [Serializable]
    public class ModelDiagnosticData : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Name of diagnosed model.
        /// </summary>
        public string ModelName { get; }

        /// <summary>
        /// Model's error statistics on test samples.
        /// </summary>
        public ModelErrStat TestErrStat { get; }

        /// <summary>
        /// Holds diagnostic data related to model's sub-models (if there are any).
        /// </summary>
        public List<ModelDiagnosticData> SubModelsDiagData { get; }

        /// <summary>
        /// Indexes in SubModelsDiagData of better sub models.
        /// </summary>
        public List<int> BetterSubModelsIndexes { get; }

        /// <summary>
        /// Indicates finalized diagnostics data.
        /// </summary>
        public bool Finalized { get; private set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="modelName">Name of diagnosed model.</param>
        /// <param name="testErrStat">Model's error statistics on test samples.</param>
        public ModelDiagnosticData(string modelName,
                                   ModelErrStat testErrStat
                                   )
        {
            ModelName = modelName;
            TestErrStat = testErrStat;
            SubModelsDiagData = new List<ModelDiagnosticData>();
            BetterSubModelsIndexes = new List<int>();
            Finalized = false;
            return;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance.</param>
        public ModelDiagnosticData(ModelDiagnosticData source)
        {
            ModelName = source.ModelName;
            TestErrStat = new ModelErrStat(source.TestErrStat);
            SubModelsDiagData = new List<ModelDiagnosticData>(source.SubModelsDiagData.Count);
            foreach(ModelDiagnosticData subModelDiagData in source.SubModelsDiagData)
            {
                SubModelsDiagData.Add(new ModelDiagnosticData(subModelDiagData));
            }
            BetterSubModelsIndexes = new List<int>(source.BetterSubModelsIndexes);
            Finalized = source.Finalized;
            return;
        }

        //Properties
        /// <summary>
        /// Number of sub-models having better error statistics than the model.
        /// </summary>
        public int NumOfBetterSubModels { get { return BetterSubModelsIndexes.Count; } }

        //Methods
        /// <summary>
        /// Adds next sub-model diagnostics data.
        /// </summary>
        /// <param name="data">Sub-model's diagnostics data.</param>
        public void AddSubModelDiagData(ModelDiagnosticData data)
        {
            if(data == this)
            {
                throw new ArgumentException("Cannot add instance of self.", nameof(data));
            }
            SubModelsDiagData.Add(data);
            return;
        }

        /// <summary>
        /// Finalizes diagnostics data.
        /// </summary>
        public void SetFinalized()
        {
            if(!Finalized)
            {
                int idx = 0;
                foreach (ModelDiagnosticData diagData in SubModelsDiagData)
                {
                    if (TestErrStat.IsBetter(diagData.TestErrStat))
                    {
                        BetterSubModelsIndexes.Add(idx);
                    }
                    diagData.SetFinalized();
                    ++idx;
                }
                Finalized = true;
            }
            return;
        }

        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        public ModelDiagnosticData DeepClone()
        {
            return new ModelDiagnosticData(this);
        }

        /// <summary>
        /// Gets formatted informative text containing diagnostics information.
        /// </summary>
        /// <param name="detail">Specifies whether to report all details if available.</param>
        /// <param name="margin">Specifies left margin.</param>
        /// <returns>Formatted informative text containing diagnostics information.</returns>
        public string GetInfoText(bool detail = false, int margin = 0)
        {
            if(!Finalized)
            {
                throw new ApplicationException("Diagnostics data not finalized.");
            }
            //Build the diagnostics text message
            string marginStr = new string(' ', margin);
            StringBuilder infoText = new StringBuilder($"{marginStr}Diagnostics data of model: [{ModelName}]:{Environment.NewLine}");
            infoText.Append(TestErrStat.GetReportText(detail, margin + 4));
            if (NumOfBetterSubModels > 0)
            {
                infoText.Append($"{marginStr}    Better sub-models {NumOfBetterSubModels.ToString(CultureInfo.InvariantCulture)}:{Environment.NewLine}");
                foreach(int betterSubModelIdx in  BetterSubModelsIndexes)
                {
                    infoText.Append(SubModelsDiagData[betterSubModelIdx].GetInfoText(detail, margin + 8));
                }
            }
            if (SubModelsDiagData.Count > 0)
            {
                infoText.Append($"{marginStr}    All sub-models {SubModelsDiagData.Count.ToString(CultureInfo.InvariantCulture)}:{Environment.NewLine}");
                foreach (ModelDiagnosticData diagnosticData in SubModelsDiagData)
                {
                    infoText.Append(diagnosticData.GetInfoText(detail, margin + 8));
                }
            }
            return infoText.ToString();
        }

    }//ModelDiagnosticData
}//Namespace
