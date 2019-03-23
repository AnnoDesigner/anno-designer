using System.Runtime.Serialization;
using System.Windows;

namespace AnnoDesigner.Presets
{
    /// <summary>
    /// Contains information for one building type, deserialized from presets.json.
    /// </summary>
    [DataContract]
    public class BuildingInfo
    {
        // technical information
        //[DataMember(Name = "GUID")]
        //public int Guid;
        //[DataMember(Name = ".ifo")]
        //public int IfoFile;

        // main
        [DataMember]
        public SerializableDictionary<int> BuildBlocker;
        [DataMember]
        public string Identifier;
        [DataMember]
        public string IconFileName;
        [DataMember]
        public int InfluenceRadius;

        [DataMember]
        public SerializableDictionary<string> Localization;

        //[DataMember]
        //public SerializableDictionary<int> BuildCost;
        
        // grouping
        [DataMember]
        public string Faction;
        [DataMember]
        public string Group;
        [DataMember]
        public string Template;

        // production
        //[DataMember(Name = "Production.Product.GUID")]
        //public int ProductGUID;
        //[DataMember(Name = "Production.Product.Name")]
        //public string ProductName;
        //[DataMember(Name = "Production.Product.Eng1")]
        //public string ProductEng1;

        public AnnoObject ToAnnoObject()
        {
            return new AnnoObject
            {
                Label = Localization == null ? Identifier : Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage)],
                Icon = IconFileName,
                Radius = InfluenceRadius,
                Size = BuildBlocker == null ? new Size() : new Size(BuildBlocker["x"], BuildBlocker["z"])
                //BuildCosts = BuildCost
            };
        }

        public string GetOrderParameter()
        {
            return Localization == null ? Identifier : Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage)];
        }
    }
}
