using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Presets.Models
{
    /// <summary>
    /// Contains information for one building type, deserialized from presets.json.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{" + nameof(Header) + "} - {" + nameof(Identifier) + "}")]
    public class BuildingInfo : IBuildingInfo
    {
        #region properties used for grouping

        /// <summary>
        /// The top level name (name of the game).
        /// </summary>
        [DataMember(Order = 0)]
        public string Header { get; set; }

        /// <summary>
        /// The name of the faction (residential tier).
        /// </summary>
        [DataMember(Order = 1)]
        public string Faction { get; set; }

        /// <summary>
        /// The associated group of buildings (e.g. public or production).
        /// </summary>
        [DataMember(Order = 2)]
        public string Group { get; set; }

        #endregion

        /// <summary>
        /// The main identifier for this building (from the Assets.xml).
        /// </summary>
        [DataMember(Order = 3)]
        public string Identifier { get; set; }

        /// <summary>
        /// The name of the iconof this building.
        /// </summary>
        [DataMember(Order = 4)]
        public string IconFileName { get; set; }

        /// <summary>
        /// The information of the required space of the building.
        /// </summary>
        [DataMember(Order = 5)]
        public SerializableDictionary<int> BuildBlocker { get; set; }

        /// <summary>
        /// The template used for this building (currently for some checkers).
        /// </summary>
        [DataMember(Order = 6)]
        public string Template { get; set; }

        /// <summary>
        /// The range of influence of this building.
        /// </summary>
        [DataMember(Order = 7)]
        public int InfluenceRange { get; set; }

        /// <summary>
        /// The radius of influence of this building.
        /// </summary>
        [DataMember(Order = 8)]
        public int InfluenceRadius { get; set; }

        /// <summary>
        /// Is this a road?
        /// </summary>
        [DataMember(Order = 9)]
        public bool Road { get; set; }

        /// <summary>
        /// Render this building borderless?
        /// </summary>
        [DataMember(Order = 10)]
        public bool Borderless { get; set; }

        /// <summary>
        /// The localized names of this building.
        /// </summary>
        [DataMember(Order = 99)]
        public SerializableDictionary<string> Localization { get; set; }

        // technical information
        //[DataMember(Name = "GUID")]
        //public int Guid { get; set; }
        //[DataMember(Name = ".ifo")]
        //public int IfoFile { get; set; }

        //[DataMember]
        //public SerializableDictionary<int> BuildCost { get; set; }

        // production
        //[DataMember(Name = "Production.Product.GUID")]
        //public int ProductGUID { get; set; }
        //[DataMember(Name = "Production.Product.Name")]
        //public string ProductName { get; set; }
        //[DataMember(Name = "Production.Product.Eng1")]
        //public string ProductEng1 { get; set; }
    }
}
