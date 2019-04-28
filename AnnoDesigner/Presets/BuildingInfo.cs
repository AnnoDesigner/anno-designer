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
        public int InfluenceRange;

        [DataMember]
        public SerializableDictionary<string> Localization;

        //[DataMember]
        //public SerializableDictionary<int> BuildCost;
        
        // grouping
        [DataMember]
        public string Header;
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
                Label = (Localization == null ? Identifier : Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage)]),
                Icon = IconFileName,
                Radius = InfluenceRadius,
                InfluenceRange = InfluenceRange,
                Identifier = Identifier,
                Size = BuildBlocker == null ? new Size() : new Size(BuildBlocker["x"], BuildBlocker["z"])
                //BuildCosts = BuildCost
            };
        }

        public string GetOrderParameter()
        {
            return Localization == null ? Identifier : Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage)];
        }
    }

    /// <summary>
    /// Holds the type of influence that a building has .
    /// Note: As this needs to be localized, I can't just bind directly to these enum values.
    /// </summary>
    public enum BuildingInfluence
    {
        None,
        Radius,
        Distance,
        Both = Radius | Distance,
    }
}
