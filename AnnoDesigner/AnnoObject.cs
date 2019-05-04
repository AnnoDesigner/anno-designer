using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;

namespace AnnoDesigner
{
    /// <summary>
    /// Object that contains all information needed to fully describe a building on the grid
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Identifier}")]
    public class AnnoObject
    {
        /// <summary>
        /// Size in grid units
        /// </summary>
        [DataMember]
        public Size Size { get; set; }

        /// <summary>
        /// Color used to fill this object
        /// </summary>
        [DataMember]
        public SerializableColor Color { get; set; }

        /// <summary>
        /// Position in grid units
        /// </summary>
        [DataMember]
        public Point Position { get; set; }

        /// <summary>
        /// Filename for an icon
        /// </summary>
        [DataMember]
        public string Icon { get; set; }

        /// <summary>
        /// Label string
        /// </summary>
        [DataMember]
        public string Label { get; set; }

        /// <summary>
        /// ObjName string
        /// </summary>
        [DataMember]
        public string Identifier { get; set; }

        /* /// <summary>
        /// Localization string
        /// </summary>
        [DataMember]
        public string[] Localization { get; set; }*/

        /// <summary>
        /// Influence radius in grid units
        /// </summary>
        [DataMember]
        public double Radius { get; set; }

        /// <summary>
        /// Influence range in grid units
        /// </summary>
        [DataMember]
        public double InfluenceRange { get; set; }

        /// <summary>
        /// Indicates whether the border should be omitted.
        /// </summary>
        [DataMember]
        public bool Borderless { get; set; }

        /// <summary>
        /// Indicates whether the object is treated as a road tile.
        /// </summary>
        [DataMember]
        public bool Road { get; set; }

        /// <summary>
        /// ObjName string
        /// </summary>
        [DataMember]
        public string Template;

        //[DataMember]
        //public SerializableDictionary<int> BuildCosts { get; set; }

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
            Borderless = obj.Borderless;
            Road = obj.Road;
            // note: this is not really a copy, just a reference, but it is not supposed to change anyway
            //BuildCosts = obj.BuildCosts;
        }
    }
}