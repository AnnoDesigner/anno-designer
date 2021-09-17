﻿using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Presets.Models
{
    public interface IBuildingInfo
    {
        string Header { get; set; }
        string Faction { get; set; }
        string Group { get; set; }
        string Identifier { get; set; }
        string IconFileName { get; set; }
        SerializableDictionary<int> BuildBlocker { get; set; }
        string Template { get; set; }
        double InfluenceRange { get; set; }
        double  InfluenceRadius { get; set; }
        bool Borderless { get; set; }
        bool Road { get; set; }
        SerializableDictionary<string> Localization { get; set; }
        double BlockedAreaLength { get; set; }
        double BlockedAreaWidth { get; set; }
        GridDirection Direction { get; set; }
    }
}