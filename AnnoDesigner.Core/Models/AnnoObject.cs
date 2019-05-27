using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;

namespace AnnoDesigner.Core.Models
{
    /// <summary>
    /// Object that contains all information needed to fully describe a building on the grid
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{" + nameof(Identifier) + "}")]
    public class AnnoObject
    {
        #region ctor

        /// <summary>
        /// Empty constructor needed for deserialization
        /// </summary>
        public AnnoObject()
        {
        }

        /// <summary>
        /// Copy constructor used to place independent objects on the grid
        /// </summary>
        /// <param name="obj"></param>
        public AnnoObject(AnnoObject obj)
        {
            Size = obj.Size;
            Color = obj.Color;
            Position = obj.Position;
            Label = obj.Label;
            Identifier = obj.Identifier;
            Template = obj.Template;
            //Localization = obj.Localization;
            Icon = obj.Icon;
            Radius = obj.Radius;
            InfluenceRange = obj.InfluenceRange;
            PavedStreet = obj.PavedStreet;
            Borderless = obj.Borderless;
            Road = obj.Road;
            // note: this is not really a copy, just a reference, but it is not supposed to change anyway
            //BuildCosts = obj.BuildCosts;
        }

        #endregion

        /// <summary>
        /// The identifier of the object.
        /// </summary>
        [DataMember(Order = 0)]
        public string Identifier { get; set; }

        /// <summary>
        /// The label of the object.
        /// </summary>
        [DataMember(Order = 1)]
        public string Label { get; set; }

        /// <summary>
        /// Position in grid units
        /// </summary>
        [DataMember(Order = 2)]
        public Point Position { get; set; }

        /// <summary>
        /// Size in grid units
        /// </summary>
        [DataMember(Order = 3)]
        public Size Size { get; set; }

        /// <summary>
        /// Filename for an icon
        /// </summary>
        [DataMember(Order = 4)]
        public string Icon { get; set; }

        /// <summary>
        /// ObjName string
        /// </summary>
        [DataMember(Order = 5)]
        public string Template { get; set; }

        /// <summary>
        /// Color used to fill this object
        /// </summary>
        [DataMember(Order = 6)]
        public SerializableColor Color { get; set; }

        /// <summary>
        /// Indicates whether the border should be omitted.
        /// </summary>
        [DataMember(Order = 7)]
        public bool Borderless { get; set; }

        /// <summary>
        /// Indicates whether the object is treated as a road tile.
        /// </summary>
        [DataMember(Order = 8)]
        public bool Road { get; set; }

        //[DataMember]
        //public SerializableDictionary<int> BuildCosts { get; set; }

        /// <summary>
        /// Influence radius in grid units
        /// </summary>
        [DataMember(Order = 9)]
        public double Radius { get; set; }

        /// <summary>
        /// Influence range in grid units
        /// </summary>
        [DataMember(Order = 10)]
        public double InfluenceRange { get; set; }

        /// <summary>
        /// If PavedStreet is selected
        /// </summary>
        [DataMember(Order = 11)]
        public bool PavedStreet { get; set; }

        ///// <summary>
        ///// Localization string
        ///// </summary>
        //[DataMember]
        //public string[] Localization { get; set; }

        //[DataMember]
        //public SerializableDictionary<int> BuildCosts { get; set; }
    }
}