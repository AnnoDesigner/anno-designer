using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;

namespace AnnoDesigner.Presets
{
    /// <summary>
    /// Contains information for one building type, deserialized from presets.json.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Identifier}")]
    public class BuildingInfo
    {
        // technical information
        //[DataMember(Name = "GUID")]
        //public int Guid { get; set; }
        //[DataMember(Name = ".ifo")]
        //public int IfoFile { get; set; }
        
        [DataMember]
        public SerializableDictionary<int> BuildBlocker { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public string IconFileName { get; set; }

        [DataMember]
        public int InfluenceRadius { get; set; }

        [DataMember]
        public int InfluenceRange { get; set; }

        [DataMember]
        public SerializableDictionary<string> Localization { get; set; }

        //[DataMember]
        //public SerializableDictionary<int> BuildCost { get; set; }

        // grouping
        [DataMember]
        public string Header { get; set; }

        [DataMember]
        public string Faction { get; set; }

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public string Template { get; set; }

        // production
        //[DataMember(Name = "Production.Product.GUID")]
        //public int ProductGUID { get; set; }
        //[DataMember(Name = "Production.Product.Name")]
        //public string ProductName { get; set; }
        //[DataMember(Name = "Production.Product.Eng1")]
        //public string ProductEng1 { get; set; }

        public AnnoObject ToAnnoObject()
        {
            return new AnnoObject
            {
                Label = (Localization == null ? Identifier : Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage)]),
                Icon = IconFileName,
                Radius = InfluenceRadius,
                InfluenceRange = InfluenceRange,
                Identifier = Identifier,
                Size = BuildBlocker == null ? new Size() : new Size(BuildBlocker["x"], BuildBlocker["z"]),
                Template = Template
                //BuildCosts = BuildCost
            };
        }

        public string GetOrderParameter()
        {
            return Localization == null ? Identifier : Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage)];
        }
    }
}
