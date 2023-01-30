using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Extensions;
using PresetParser.Extensions;

namespace PresetParser.Anno1800
{
    /// <summary>
    /// in this file are made the following lists
    /// ChangeBuildingTo<1>_<2>_1800 
    /// <1> can be : OW (All Worlds) - OW1 (New World - (1) Farmers) - OW2 (New World - (2) Workers) - OW3 (New World - (3) Artisans)
    ///              OW4 (New World - (4) Engineers  - OW5 (New World - (5) Investors) - OW6 (New World (13) Scholars)  
    ///              NW1 (Old World - (7) Jornaleros  - NW2 (Old World - (8) Obreros  - NW3 (Old World - (21) Artista)
    ///              AT1 (Arctic - (10) Explorers)  - AT2 (Arctic - (11) Technicians)
    ///              AF1 (Africa - (14) Shepherds)  - AF2 (Africa - (15) Elders) 
    ///              Tou ((17) Tourists) - HL1 ((18) High Live)
    /// <2> wil be the Group under <1>, like Production, Public, etc
    ///
    /// Changed the mistake OW/NW (23-10-2020) it is as in game now OW = Old World and NW = New World
    /// 
    /// </summary>
    public static class NewFactionAndGroup1800
    {
        // Public Buildings
        //private static readonly List<string> ChangeBuildingsToAW_Public_1800 = new List<string> { "" }; // Example
        private static readonly List<string> ChangeBuildingsToOW1_Public_1800 = new List<string> { "Logistic_01 (Marketplace)", "Service_01 (Pub)", "Institution_02 (Fire Department)", };
        private static readonly List<string> ChangeBuildingsToOW2_Public_1800 = new List<string> { "Institution_01 (Police)", "Service_04 (Church)", "Service_02 (School)" };
        private static readonly List<string> ChangeBuildingsToOW3_Public_1800 = new List<string> { "Institution_03 (Hospital)", "Service_05 (Cabaret)", "Service_07 (University)", };
        private static readonly List<string> ChangeBuildingsToOW4_Public_1800 = new List<string> { "Service_03 (Bank)", "Electricity_02 (Oil Power Plant)" };
        private static readonly List<string> ChangeBuildingsToOW5_Public_1800 = new List<string> { "Service_09 (Club House)" };
        private static readonly List<string> ChangeBuildingsToOW6_Public_1800 = new List<string> { "ResearchCenter_01", "Service_moderate_LoL_01 (Radio Station)" };
        private static readonly List<string> ChangeBuildingsToNW1_Public_1800 = new List<string> { "Institution_colony01_02 (Fire Department)", "Institution_colony01_01 (Police)", "Service_colony01_01 (Marketplace)", "Service_colony01_02 (Chapel)" };
        private static readonly List<string> ChangeBuildingsToNW2_Public_1800 = new List<string> { "Institution_colony01_03 (Hospital)", "Service_colony01_03 (Boxing Arena)" };
        private static readonly List<string> ChangeBuildingsToNW3_Public_1800 = new List<string> { "Colony01 electricity_02 (Oil Power Plant)", "DLC12 Beach", "DLC12 Samba School", "DLC12 Cinema" };
        private static readonly List<string> ChangeBuildingsToAT1_Public_1800 = new List<string> { "Service_arctic_01 (Canteen)", "Institution_arctic_01 (Ranger Station)" };
        private static readonly List<string> ChangeBuildingsToAT2_Public_1800 = new List<string> { "Service_arctic_02 (Post Office)"};
        private static readonly List<string> ChangeBuildingsToAF1_Public_1800 = new List<string> { "Service_colony02_01 (Bazaar)", "Service_colony02_02 (Music Plaza)", "Institution_colony02_01 (Fire Station)" };
        private static readonly List<string> ChangeBuildingsToAF2_Public_1800 = new List<string> { "Institution_colony02_02 (Police)", "Institution_colony02_03 (Hospital)", "Service_colony02_03 (Monastery)" };

        // Production Buildings
        // private static readonly List<string> ChangeBuildingsToAW_Productions_1800 = new List<string> { "" }; // Example
        private static readonly List<string> ChangeBuildingsToOW1_Productions_1800 = new List<string> { "Coastal_01 (Fish Coast Building)", "Processing_04 (Weavery)", "Food_06 (Schnapps Maker)", "Factory_03 (Timber Factory)", "Agriculture_05 (Timber Yard)" };
        private static readonly List<string> ChangeBuildingsToOW2_Productions_1800 = new List<string> { "Factory_09 (Sailcloth Factory)", "Heavy_01 (Beams Heavy Industry)", "Heavy_04 (Weapons Heavy Industry)", "Heavy_02 (Steel Heavy Industry)", "Heavy_03 (Coal Heavy Industry)", "Processing_01 (Tallow Processing)", "Food_07 (Sausage Maker)", "Processing_02 (Flour Processing)", "Factory_02 (Soap Factory)", "Processing_03 (Malt Processing)", "Food_02 (Beer Maker)", "Factory_04 (Brick Factory)", "Food_01 (Bread Maker)" };
        private static readonly List<string> ChangeBuildingsToOW3_Productions_1800 = new List<string> { "Food_03 (Goulash Factory)", "Food_05 (Canned Food Factory)", "Processing_06 (Glass Processing)", "Factory_07 (Window Factory)", "Agriculture_09 (Hunter's Cabin)", "Factory_05 (Fur Coat Workshop)", "Workshop_03 (Sewing Machines Factory)" };
        private static readonly List<string> ChangeBuildingsToOW4_Productions_1800 = new List<string> { "Factory_06 (Light Bulb Factory)", "Processing_08 (Carbon Filament Processing)", "Workshop_02 (Pocket Watch Workshop)", "Workshop_05 (Gold Workshop)", "Heavy_07 (Steam Motors Heavy Industry)", "Workshop_01 (High-Wheeler Workshop)", "Heavy_06 (Advanced Weapons Heavy Industry)", "Processing_05 (Dynamite Processing)", "Coastal_02 (Niter Coast Building)", "Workshop_07 (Glasses Workshop)", "Heavy_09 (Brass Heavy Industry)", "Heavy_10 (Oil Heavy Industry)", "Factory_01 (Concrete Factory)", "Heavy_10_field (Oil Pump)", "Moderate_fuel_station_01 (FuelStation)" };
        private static readonly List<string> ChangeBuildingsToOW5_Productions_1800 = new List<string> { "Heavy_08 (Steam Carriages Heavy Industry)", "Factory_10 (Chassis Factory)", "Workshop_04 (Phonographs Workshop)", "Processing_07 (Inlay Processing)", "Workshop_06 (Jewelry Workshop)", "Food_08 (Champagne Maker)" };
        private static readonly List<string> ChangeBuildingsToOW6_Productions_1800 = new List<string> { "Final_moderate_LoL_01(Leather Shoes)", "Final_moderate_LoL_02 (Suits)", "Final_moderate_LoL_03 (Telephones)", "Final_moderate_LoL_01 (Leather Shoes)" };
        private static readonly List<string> ChangeBuildingsToNW1_Productions_1800 = new List<string> { "Processing_colony01_02 (Poncho Maker)", "Coastal_colony01_01 (Pearls Coast Building)", "Food_colony01_04 (Fried Banana Maker)", "Coastal_colony01_02 (Fish Coast Building)", "Factory_colony01_02 (Sailcloth Factory)", "Factory_colony01_01 (Timber Factory)", "Agriculture_colony01_06 (Timber Yard)", "Factory_colony01_03 (Cotton Cloth Processing)", "Food_colony01_01 (Rum Maker)" };
        private static readonly List<string> ChangeBuildingsToNW2_Productions_1800 = new List<string> { "Food_colony01_02 (Chocolate Maker)", "Workshop_colony01_01 (Cigars Workshop)", "Factory_colony01_07 (Bombin Maker)", "Factory_colony01_06 (Felt Maker)", "Food_colony01_03 (Coffee Maker)", "Food_colony01_05 (Burrito Maker)", "Processing_colony01_01 (Sugar Processing)", "Processing_colony01_03 (Inlay Processing)", "Heavy_colony01_01 (Oil Heavy Industry)", "Heavy_colony01_01_field (Oil Pump)", "Colony01_fuel_station_01 (FuelStation)", "Factory_colony01_05 (Brick Factory)" };
        private static readonly List<string> ChangeBuildingsToNW3_Productions_1800 = new List<string> { "Coastal_colony01_05 (Calamari Fishery)", "DLC12 food_colony01_08 (NotTequilla)", "DLC12 food_colony01_06 (Jalea Maker)", "DLC12 Colony01 Steelworks", "DLC12 Colony01 Sewing Machine Factory", "DLC12 workshop_colony01_04 (Soccer Ball Maker)", "DLC12 food_colony01_07 (Ice Cream Factory)", "DLC12 Lab Fire Extinguishers", "DLC12 Lab Cuffs", "DLC12 Lab Medicine", "DLC12 workshop_colony01_03 (Perfume Mixer)", "DLC12 Lab Pigments", "DLC12 workshop_colony01_05 (Costume Maker)", "DLC12 Cable Factory", "DLC12 Motor Factory", "DLC12 Ventilator Factory NEW", "Multifactory_SA_Chemical_Film", "DLC12 Scooter Factory" };
        private static readonly List<string> ChangeBuildingsToAT1_Productions_1800 = new List<string> { "Agriculture_arctic_01 (Timber Yard)", "Factory_arctic_01 (Timber Factory)", "Agriculture_arctic_02 (Caribou Hunter)", "Factory_arctic_02 (Sleeping Bags Factory)", "Heavy_arctic_01 (Coal Heavy Industry)", "Coastal_arctic_01 (Whale Coast Building)", "Coastal_arctic_02 (Seal Hunter)", "Factory_arctic_03 (Oil Lamp Factory)", "Food_arctic_01 (Pemmican)" };
        private static readonly List<string> ChangeBuildingsToAT2_Productions_1800 = new List<string> { "Agriculture_arctic_04 (Bear Hunter)", "Factory_arctic_04 (Parka Factory)", "Agriculture_arctic_06 (Normal Fur Hunter)", "Factory_arctic_05 (Sled Frame Factory)", "Factory_arctic_06 (Husky Sled Factory)", "Mining_arctic_01 (Gas Mine)", "Mining_arctic_02 (Gold Mine)", "Mining_arctic_01_pump (Gas Pump)", "Monument_arctic_01_00" };
        private static readonly List<string> ChangeBuildingsToAF1_Productions_1800 = new List<string> { "Agriculture_colony02_01 (Wanza Woodcutter)", "Intermediate_colony02_01 (Linen Mill)", "Final_colony02_02 (Weaver)", "Coastal_colony02_01 (Salt Coast Building)", "Final_colony02_01 (Meat Dry-House)", "Final_colony02_03 (Tea Maker)" };
        private static readonly List<string> ChangeBuildingsToAF2_Productions_1800 = new List<string> { "Final_colony02_05 (Brick Dry-House)", "Final_colony02_10 (Ceramics Workshop)", "Final_colony02_04 (Tapestry Workshop)", "Intermediate_colony02_02 (Spiced Teff Mill)", "Coastal_colony02_02 (Seafood Fisher)", "Final_colony02_06 (Stew Kitchen)", "Final_colony02_07 (Pipe Maker)", "Africa_fuel_station_01 (FuelStation)", "Intermediate_colony02_03 (Candle Maker)", "Final_colony02_09 (Lanterns Maker)", "Final_colony02_08 (Scriptures Workshop)" };
        private static readonly List<string> ChangeBuildingsToTou_Productions_1800 = new List<string> { "Multifactory_Chemical_Lemonade", "Multifactory_Chemical_Shampoo", "Multifactory_Chemical_Souvenier" };
        // Farm Buildings
        //private static readonly List<string> ChangeBuildingsToAW_FarmBuilding_1800 = new List<string> { "" }; // Example
        private static readonly List<string> ChangeBuildingsToOW1_FarmBuilding_1800 = new List<string> { "Agriculture_04 (Potato Farm)", "Agriculture_06 (Sheep Farm)", };
        private static readonly List<string> ChangeBuildingsToOW2_FarmBuilding_1800 = new List<string> { "Agriculture_08 (Pig Farm)", "Agriculture_03 (Hop Farm)", "Agriculture_01 (Grain Farm)" };
        private static readonly List<string> ChangeBuildingsToOW3_FarmBuilding_1800 = new List<string> { "Agriculture_11 (Bell Pepper Farm)", "Agriculture_02 (Cattle Farm)" };
        private static readonly List<string> ChangeBuildingsToOW5_FarmBuilding_1800 = new List<string> { "Agriculture_10 (Vineyard)" };
        private static readonly List<string> ChangeBuildingsToNW1_FarmBuilding_1800 = new List<string> { "Agriculture_colony01_11 (Alpaca Farm)", "Agriculture_colony01_08 (Banana Farm)", "Agriculture_colony01_03 (Cotton Farm)", "Agriculture_colony01_01 (Sugar Cane Farm)", "Agriculture_colony01_05 (Caoutchouc Farm)" };
        private static readonly List<string> ChangeBuildingsToNW2_FarmBuilding_1800 = new List<string> { "Agriculture_colony01_09 (Cattle Farm)", "Agriculture_colony01_04 (Cocoa Farm)", "Agriculture_colony01_02 (Tobacco Farm)", "Agriculture_colony01_07 (Coffee Beans Farm)", "Agriculture_colony01_10 (Corn Farm)" };
        private static readonly List<string> ChangeBuildingsToNW3_FarmBuilding_1800 = new List<string> { "DLC12 agriculture_colony01_14 (Orchid Farm)", "DLC12 agriculture_colony01_13 (Herb Farm)", "DLC12 agriculture_colony01_12 (Nandu Farm)" };
        private static readonly List<string> ChangeBuildingsToAT1_FarmBuilding_1800 = new List<string> { "Agriculture_arctic_03 (Goose Farm)" };
        private static readonly List<string> ChangeBuildingsToAT2_FarmBuilding_1800 = new List<string> { "Agriculture_arctic_05 (Husky Farm)" };
        private static readonly List<string> ChangeBuildingsToAF1_FarmBuilding_1800 = new List<string> { "Animal_colony02_01 (Goat Farm)", "Agriculture_colony02_02 (Flax Farm)", "Animal_colony02_02 (Sanga Farm)", "Agriculture_colony02_03 (Hibiscus)" };
        private static readonly List<string> ChangeBuildingsToAF2_FarmBuilding_1800 = new List<string> { "Agriculture_colony02_04 (Teff Farm)", "Agriculture_colony02_05 (Indigo Farm)", "Agriculture_colony02_06 (Spice Grower)", "Agriculture_colony02_07 (Beekeeper)" };

        // River Buildigs (only Africa DLC 06)
        //private static readonly List<string> ChangeBuildingsToAF1_RiverBuilding_1800 = new List<string> { }; // Example
        private static readonly List<string> ChangeBuildingsToAF2_RiverBuilding_1800 = new List<string> { "River_colony02_01 (Clay Harvester)", "River_colony02_02 (Paper Mill)", "River_colony02_03 (Water Pump)" };

        // Special Buildings
        private static readonly List<string> ChangeBuildingsToAW_SpecialBuilding_1800 = new List<string> { "Guild_house", "Town hall" };

        //Ornamentals has his own file now, so check NewOrnamentsGroup1800.cs

        /// <summary>
        /// Retuns the faction and group for an identifier.
        /// </summary>
        /// <param name="identifierName">The given Buildingname, this will not changed</param>
        /// <param name="factionName">If Buildingmane is in one of the lists, factionName will be changed</param>
        /// <param name="groupName">If Buildingmane is in one of the lists, groupName will be changed</param>
        /// <param name="templateName">if buildingname is in ToAT1 or ToAT2 and is a Production Building, the Templatename will be changed</param>
        /// <returns></returns>
        public static (string Faction, string Group, string Template) GetNewFactionAndGroup1800(string identifierName, string factionName, string groupName, string templateName = "")
        {
            if (string.IsNullOrWhiteSpace(identifierName))
            {
                throw new ArgumentNullException(nameof(identifierName), "No identifier was given.");
            }

            //public buildings
            //if (identifierName.IsMatch(ChangeBuildingsToAW_Public_1800)) { factionName = "All Worlds"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW1_Public_1800)) { factionName = "(01) Farmers"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW2_Public_1800)) { factionName = "(02) Workers"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW3_Public_1800)) { factionName = "(03) Artisans"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW4_Public_1800)) { factionName = "(04) Engineers"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW5_Public_1800)) { factionName = "(05) Investors"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW6_Public_1800)) { factionName = "(13) Scholars"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW1_Public_1800)) { factionName = "(07) Jornaleros"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW2_Public_1800)) { factionName = "(08) Obreros"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW3_Public_1800)) { factionName = "(21) Artista"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAT1_Public_1800)) { factionName = "(10) Explorers"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAT2_Public_1800)) { factionName = "(11) Technicians"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAF1_Public_1800)) { factionName = "(14) Shepherds"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAF2_Public_1800)) { factionName = "(15) Elders"; groupName = "Public Buildings"; }

            //Production buildings
            //if (identifierName.IsMatch(ChangeBuildingsToAW_Productions_1800)) { factionName = "All Worlds"; groupName = "Public Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW1_Productions_1800)) { factionName = "(01) Farmers"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW2_Productions_1800)) { factionName = "(02) Workers"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW3_Productions_1800)) { factionName = "(03) Artisans"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW4_Productions_1800)) { factionName = "(04) Engineers"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW5_Productions_1800)) { factionName = "(05) Investors"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW6_Productions_1800)) { factionName = "(13) Scholars"; groupName = "Production Buildings"; } 
            if (identifierName.IsMatch(ChangeBuildingsToNW1_Productions_1800)) { factionName = "(07) Jornaleros"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW2_Productions_1800)) { factionName = "(08) Obreros"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW3_Productions_1800)) { factionName = "(21) Artista"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAT1_Productions_1800)) { factionName = "(10) Explorers"; groupName = "Production Buildings"; templateName = "FactoryBuilding7"; }
            if (identifierName.IsMatch(ChangeBuildingsToAT2_Productions_1800)) { factionName = "(11) Technicians"; groupName = "Production Buildings"; templateName = "FactoryBuilding7"; }
            if (identifierName.IsMatch(ChangeBuildingsToAF1_Productions_1800)) { factionName = "(14) Shepherds"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAF2_Productions_1800)) { factionName = "(15) Elders"; groupName = "Production Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToTou_Productions_1800)) { factionName = "(17) Tourists"; groupName = "Production Buildings"; }

            //Farm buildings
            //if (identifierName.IsMatch(ChangeBuildingsToAW_FarmBuilding_1800)) { factionName = "All Worlds"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW1_FarmBuilding_1800)) { factionName = "(01) Farmers"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW2_FarmBuilding_1800)) { factionName = "(02) Workers"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW3_FarmBuilding_1800)) { factionName = "(03) Artisans"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToOW5_FarmBuilding_1800)) { factionName = "(05) Investors"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW1_FarmBuilding_1800)) { factionName = "(07) Jornaleros"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW2_FarmBuilding_1800)) { factionName = "(08) Obreros"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToNW3_FarmBuilding_1800)) { factionName = "(21) Artista"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAT1_FarmBuilding_1800)) { factionName = "(10) Explorers"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAT2_FarmBuilding_1800)) { factionName = "(11) Technicians"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAF1_FarmBuilding_1800)) { factionName = "(14) Shepherds"; groupName = "Farm Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAF2_FarmBuilding_1800)) { factionName = "(15) Elders"; groupName = "Farm Buildings"; }

            //River Buildings (Only Africa DLC 06)
            //if (identifierName.IsMatch(ChangeBuildingsToAF1_RiverBuilding_1800)) { factionName = "(14) Shepherds"; groupName = "River Buildings"; }
            if (identifierName.IsMatch(ChangeBuildingsToAF2_RiverBuilding_1800)) { factionName = "(15) Elders"; groupName = "River Buildings"; }
            
            //Special Buildings
            if (identifierName.IsMatch(ChangeBuildingsToAW_SpecialBuilding_1800)) { factionName = "All Worlds"; groupName = "Special Buildings"; }
            //Ornamentals 
            //if (identifierName.IsMatch(ChangeBuildingsToAW_Ornamentals_1800)) { factionName = "All Worlds"; }

            return (factionName, groupName, templateName);
        }
    }
}
