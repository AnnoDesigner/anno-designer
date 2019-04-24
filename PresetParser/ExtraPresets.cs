using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Localization;
using AnnoDesigner.Presets;

namespace PresetParser
{
    public class ExtraPresets
    {
        public static IList<ExtraPreset> ExtraPresetList1404 = new List<ExtraPreset>();
        public static IList<ExtraPreset> ExtraPresetList2070 = new List<ExtraPreset>();
        //public static IList<ExtraPreset> ExtraPresetList2205 = new List<ExtraPreset>();
        public static IList<ExtraPreset> ExtraPresetList1800 = new List<ExtraPreset>();
        public static bool getList(string annoVersion)
        {
            if (annoVersion == Program.ANNO_VERSION_1404)
            { }
            if (annoVersion == Program.ANNO_VERSION_2070)
            { }
            /* As the is no extra preset information to set, this block is commented out
            if (annoVersion == Program.ANNO_VERSION_2205)
            { }*/
            if (annoVersion == Program.ANNO_VERSION_1800)
            {
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "5", BuildBlockerZ = "5", Faction = "(2) Workers", Group = null, Header = "(A7) Anno 1800", IconFileName = "A7_warehouse.png", Identifier = "Logistic_02 (Warehouse I)", InfluenceRadius = 0, InFluenceRange = 0, Template = "Warehouse", LocaEng = "Small Warehouse", LocaGer = "Lagerhaus", LocaPol = "Mały magazyn", LocaRus = "Маленький склад" });
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "5", BuildBlockerZ = "5", Faction = "(3) Artisans", Group = null, Header = "(A7) Anno 1800", IconFileName = "A7_warehouse.png", Identifier = "Logistic_02 (Warehouse I)", InfluenceRadius = 0, InFluenceRange = 0, Template = "Warehouse", LocaEng = "Small Warehouse", LocaGer = "Lagerhaus", LocaPol = "Mały magazyn", LocaRus = "Маленький склад" });
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "5", BuildBlockerZ = "5", Faction = "(4) Engineers", Group = null, Header = "(A7) Anno 1800", IconFileName = "A7_warehouse.png", Identifier = "Logistic_02 (Warehouse I)", InfluenceRadius = 0, InFluenceRange = 0, Template = "Warehouse", LocaEng = "Small Warehouse", LocaGer = "Lagerhaus", LocaPol = "Mały magazyn", LocaRus = "Маленький склад" });
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "5", BuildBlockerZ = "5", Faction = "(5) Investors", Group = null, Header = "(A7) Anno 1800", IconFileName = "A7_warehouse.png", Identifier = "Logistic_02 (Warehouse I)", InfluenceRadius = 0, InFluenceRange = 0, Template = "Warehouse", LocaEng = "Small Warehouse", LocaGer = "Lagerhaus", LocaPol = "Mały magazyn", LocaRus = "Маленький склад" });
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "5", BuildBlockerZ = "5", Faction = "(7) Jornaleros", Group = null, Header = "(A7) Anno 1800", IconFileName = "A7_warehouse.png", Identifier = "Logistic_02 (Warehouse I)", InfluenceRadius = 0, InFluenceRange = 0, Template = "Warehouse", LocaEng = "Small Warehouse", LocaGer = "Lagerhaus", LocaPol = "Mały magazyn", LocaRus = "Маленький склад" });
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "5", BuildBlockerZ = "5", Faction = "(8) Obreros", Group = null, Header = "(A7) Anno 1800", IconFileName = "A7_warehouse.png", Identifier = "Logistic_02 (Warehouse I)", InfluenceRadius = 0, InFluenceRange = 0, Template = "Warehouse", LocaEng = "Small Warehouse", LocaGer = "Lagerhaus", LocaPol = "Mały magazyn", LocaRus = "Маленький склад" });
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "6", BuildBlockerZ = "4", Faction = "Attractiveness", Group = "Modules", Header = "(A7) Anno 1800", IconFileName = "A7_Zoo module.png", Identifier = "Culture_01_module", InfluenceRadius = 0, InFluenceRange = 0, Template = "CultureModule", LocaEng = "Zoo Module (6x4)", LocaGer = "Zoo-Modul (6x4)", LocaPol = "Moduł ZOO (6x4)", LocaRus = "Модуль зоопарка (6x4)" });
                ExtraPresetList1800.Add( new ExtraPreset { BuildBlockerX = "5", BuildBlockerZ = "4", Faction = "Attractiveness", Group = "Modules", Header = "(A7) Anno 1800", IconFileName = "A7_Museum module.png", Identifier = "Culture_02_module", InfluenceRadius = 0, InFluenceRange = 0, Template = "CultureModule", LocaEng = "Museum Module (5x4)", LocaGer = "Museumsmodul (5x4)", LocaPol = "Moduł Muzeum (5x4)", LocaRus = "Модуль музея (5x4)" });
                return true;
            }
            return false;
        }
        public class ExtraPreset
        {
            public string BuildBlockerX { get; set; }
            public string BuildBlockerZ { get; set; }
            public string Faction { get; set; }
            public string Group { get; set; }
            public string Header { get; set; }
            public string IconFileName { get; set; }
            public string Identifier { get; set; }
            public int InfluenceRadius { get; set; }
            public int InFluenceRange { get; set; }
            public string LocaEng { get; set; }
            public string LocaGer { get; set; }
            public string LocaPol { get; set; }
            public string LocaRus { get; set; }
            public string Template { get; set; }
        }
    }
}
