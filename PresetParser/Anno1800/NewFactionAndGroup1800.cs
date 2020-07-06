﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Extensions;

namespace PresetParser.Anno1800
{
    /// <summary>
    /// in this file are made the following lists
    /// ChangeBuildingTo<1>_<2>_1800 
    /// <1> can be : AW (All Worlds) - NW1 (New World - (1) Farmers) - NW2 (New World - (2) Workers) - NW3 (New World - (3) Artisans)
    ///              NW4 (New World - (4) Engineers) - NW5 (New World - (5) Investors)  
    ///              OW1 (Old World - (7) Jornaleros) - OW2 (Old World - (8) Obreros)
    ///              AT1 (Arctic - (10) Explorers) - AT2 (Arctic - (11) Technicians)
    /// <2> wil be the Group under <1>, like Production, Public, etc
    /// 
    /// NOTE : New world and Old World are reversed in this code, in game it seams that the start world is Old World
    ///        So, Games Old World is NWx and Games New World is OWx in this code !!!!!
    ///
    /// </summary>
    public static class NewFactionAndGroup1800
    {
        // Public Buildings
        //private static readonly List<string> ChangeBuildingsToAW_Public_1800 = new List<string> { "" };
        private static readonly List<string> ChangeBuildingsToNW1_Public_1800 = new List<string> { "Logistic_01 (Marketplace)", "Service_01 (Pub)", "Institution_02 (Fire Department)", };
        private static readonly List<string> ChangeBuildingsToNW2_Public_1800 = new List<string> { "Institution_01 (Police)", "Service_04 (Church)", "Service_02 (School)" };
        private static readonly List<string> ChangeBuildingsToNW3_Public_1800 = new List<string> { "Institution_03 (Hospital)", "Service_05 (Cabaret)", "Service_07 (University)", };
        private static readonly List<string> ChangeBuildingsToNW4_Public_1800 = new List<string> { "Service_03 (Bank)", "Electricity_02 (Oil Power Plant)" };
        private static readonly List<string> ChangeBuildingsToNW5_Public_1800 = new List<string> { "Service_09 (Club House)" };
        private static readonly List<string> ChangeBuildingsToOW1_Public_1800 = new List<string> { "Institution_colony01_02 (Fire Department)", "Institution_colony01_01 (Police)", "Service_colony01_01 (Marketplace)", "Service_colony01_02 (Chapel)" };
        private static readonly List<string> ChangeBuildingsToOW2_Public_1800 = new List<string> { "Institution_colony01_03 (Hospital)", "Service_colony01_03 (Boxing Arena)" };
        private static readonly List<string> ChangeBuildingsToAT1_Public_1800 = new List<string> { "Service_arctic_01 (Canteen)", "Institution_arctic_01 (Ranger Station)" };
        private static readonly List<string> ChangeBuildingsToAT2_Public_1800 = new List<string> { "Service_arctic_02 (Post Office)" };

        // Production Buildings
        // private static readonly List<string> ChangeBuildingsToAW_Productions_1800 = new List<string> { "" };
        private static readonly List<string> ChangeBuildingsToNW1_Productions_1800 = new List<string> { "Coastal_01 (Fish Coast Building)", "Processing_04 (Weavery)", "Food_06 (Schnapps Maker)", "Factory_03 (Timber Factory)", "Agriculture_05 (Timber Yard)" };
        private static readonly List<string> ChangeBuildingsToNW2_Productions_1800 = new List<string> { "Factory_09 (Sailcloth Factory)", "Heavy_01 (Beams Heavy Industry)", "Heavy_04 (Weapons Heavy Industry)", "Heavy_02 (Steel Heavy Industry)", "Heavy_03 (Coal Heavy Industry)", "Processing_01 (Tallow Processing)", "Food_07 (Sausage Maker)", "Processing_02 (Flour Processing)", "Factory_02 (Soap Factory)", "Processing_03 (Malt Processing)", "Food_02 (Beer Maker)", "Factory_04 (Brick Factory)", "Food_01 (Bread Maker)" };
        private static readonly List<string> ChangeBuildingsToNW3_Productions_1800 = new List<string> { "Food_03 (Goulash Factory)", "Food_05 (Canned Food Factory)", "Processing_06 (Glass Processing)", "Factory_07 (Window Factory)", "Agriculture_09 (Hunter's Cabin)", "Factory_05 (Fur Coat Workshop)", "Workshop_03 (Sewing Machines Factory)" };
        private static readonly List<string> ChangeBuildingsToNW4_Productions_1800 = new List<string> { "Factory_06 (Light Bulb Factory)", "Processing_08 (Carbon Filament Processing)", "Workshop_02 (Pocket Watch Workshop)", "Workshop_05 (Gold Workshop)", "Heavy_07 (Steam Motors Heavy Industry)", "Workshop_01 (High-Wheeler Workshop)", "Heavy_06 (Advanced Weapons Heavy Industry)", "Processing_05 (Dynamite Processing)", "Coastal_02 (Niter Coast Building)", "Workshop_07 (Glasses Workshop)", "Heavy_09 (Brass Heavy Industry)", "Heavy_10 (Oil Heavy Industry)", "Factory_01 (Concrete Factory)", "Heavy_10_field (Oil Pump)", "Moderate_fuel_station_01 (FuelStation)" };
        private static readonly List<string> ChangeBuildingsToNW5_Productions_1800 = new List<string> { "Heavy_08 (Steam Carriages Heavy Industry)", "Factory_10 (Chassis Factory)", "Workshop_04 (Phonographs Workshop)", "Processing_07 (Inlay Processing)", "Workshop_06 (Jewelry Workshop)", "Food_08 (Champagne Maker)" };
        private static readonly List<string> ChangeBuildingsToOW1_Productions_1800 = new List<string> { "Processing_colony01_02 (Poncho Maker)", "Coastal_colony01_01 (Pearls Coast Building)", "Food_colony01_04 (Fried Banana Maker)", "Coastal_colony01_02 (Fish Coast Building)", "Factory_colony01_02 (Sailcloth Factory)", "Factory_colony01_01 (Timber Factory)", "Agriculture_colony01_06 (Timber Yard)", "Factory_colony01_03 (Cotton Cloth Processing)", "Food_colony01_01 (Rum Maker)" };
        private static readonly List<string> ChangeBuildingsToOW2_Productions_1800 = new List<string> { "Food_colony01_02 (Chocolate Maker)", "Workshop_colony01_01 (Cigars Workshop)", "Factory_colony01_07 (Bombin Maker)", "Factory_colony01_06 (Felt Maker)", "Food_colony01_03 (Coffee Maker)", "Food_colony01_05 (Burrito Maker)", "Processing_colony01_01 (Sugar Processing)", "Processing_colony01_03 (Inlay Processing)", "Heavy_colony01_01 (Oil Heavy Industry)", "Heavy_colony01_01_field (Oil Pump)", "Colony01_fuel_station_01 (FuelStation)" };
        private static readonly List<string> ChangeBuildingsToAT1_Productions_1800 = new List<string> { "Agriculture_arctic_01 (Timber Yard)", "Factory_arctic_01 (Timber Factory)", "Agriculture_arctic_02 (Caribou Hunter)", "Factory_arctic_02 (Sleeping Bags Factory)", "Heavy_arctic_01 (Coal Heavy Industry)", "Coastal_arctic_01 (Whale Coast Building)", "Coastal_arctic_02 (Seal Hunter)", "Factory_arctic_03 (Oil Lamp Factory)", "Food_arctic_01 (Pemmican)" };
        private static readonly List<string> ChangeBuildingsToAT2_Productions_1800 = new List<string> { "Agriculture_arctic_04 (Bear Hunter)", "Factory_arctic_04 (Parka Factory)", "Agriculture_arctic_06 (Normal Fur Hunter)", "Factory_arctic_05 (Sled Frame Factory)", "Factory_arctic_06 (Husky Sled Factory)", "Mining_arctic_01 (Gas Mine)", "Mining_arctic_02 (Gold Mine)", "Mining_arctic_01_pump (Gas Pump)", "Monument_arctic_01_00" };

        // Farm Buildings
        //private static readonly List<string> ChangeBuildingsToAW_FarmBuilding_1800 = new List<string> { "" };
        private static readonly List<string> ChangeBuildingsToNW1_FarmBuilding_1800 = new List<string> { "Agriculture_04 (Potato Farm)", "Agriculture_06 (Sheep Farm)", };
        private static readonly List<string> ChangeBuildingsToNW2_FarmBuilding_1800 = new List<string> { "Agriculture_08 (Pig Farm)", "Agriculture_03 (Hop Farm)", "Agriculture_01 (Grain Farm)" };
        private static readonly List<string> ChangeBuildingsToNW3_FarmBuilding_1800 = new List<string> { "Agriculture_11 (Bell Pepper Farm)", "Agriculture_02 (Cattle Farm)" };
        //private static readonly List<string> ChangeBuildingsToNW4_FarmBuilding_1800 = new List<string> { "" };
        private static readonly List<string> ChangeBuildingsToNW5_FarmBuilding_1800 = new List<string> { "Agriculture_10 (Vineyard)" };
        private static readonly List<string> ChangeBuildingsToOW1_FarmBuilding_1800 = new List<string> { "Agriculture_colony01_11 (Alpaca Farm)", "Agriculture_colony01_08 (Banana Farm)", "Agriculture_colony01_03 (Cotton Farm)", "Agriculture_colony01_01 (Sugar Cane Farm)", "Agriculture_colony01_05 (Caoutchouc Farm)" };
        private static readonly List<string> ChangeBuildingsToOW2_FarmBuilding_1800 = new List<string> { "Agriculture_colony01_09 (Cattle Farm)", "Agriculture_colony01_04 (Cocoa Farm)", "Agriculture_colony01_02 (Tobacco Farm)", "Agriculture_colony01_07 (Coffee Beans Farm)", "Agriculture_colony01_10 (Corn Farm)" };
        private static readonly List<string> ChangeBuildingsToAT1_FarmBuilding_1800 = new List<string> { "Agriculture_arctic_03 (Goose Farm)" };
        private static readonly List<string> ChangeBuildingsToAT2_FarmBuilding_1800 = new List<string> { "Agriculture_arctic_05 (Husky Farm)" };

        // Special Buildings
        private static readonly List<string> ChangeBuildingsToAW_SpecialBuilding_1800 = new List<string> { "Guild_house", "Town hall" };

        //Ornamentals 
        //private static readonly List<string> ChangeBuildingsToAW_Ornamentals_1800 = new List<string> { "Culture_preorder_statue", "Uplay_ornament_2x1_lion_statue", "Uplay_ornament_2x2_pillar_chess_park", "Uplay_ornament_3x2_large_fountain" };

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
            //if (identifierName.IsPartOf(ChangeBuildingsToAW_Public_1800)) { factionName = "All Worlds"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW1_Public_1800)) { factionName = "(01) Farmers"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW2_Public_1800)) { factionName = "(02) Workers"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW3_Public_1800)) { factionName = "(03) Artisans"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW4_Public_1800)) { factionName = "(04) Engineers"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW5_Public_1800)) { factionName = "(05) Investors"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToOW1_Public_1800)) { factionName = "(07) Jornaleros"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToOW2_Public_1800)) { factionName = "(08) Obreros"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToAT1_Public_1800)) { factionName = "(10) Explorers"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToAT2_Public_1800)) { factionName = "(11) Technicians"; groupName = "Public Buildings"; }

            //Production buildings
            //if (identifierName.IsPartOf(ChangeBuildingsToAW_Productions_1800)) { factionName = "All Worlds"; groupName = "Public Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW1_Productions_1800)) { factionName = "(01) Farmers"; groupName = "Production Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW2_Productions_1800)) { factionName = "(02) Workers"; groupName = "Production Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW3_Productions_1800)) { factionName = "(03) Artisans"; groupName = "Production Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW4_Productions_1800)) { factionName = "(04) Engineers"; groupName = "Production Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW5_Productions_1800)) { factionName = "(05) Investors"; groupName = "Production Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToOW1_Productions_1800)) { factionName = "(07) Jornaleros"; groupName = "Production Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToOW2_Productions_1800)) { factionName = "(08) Obreros"; groupName = "Production Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToAT1_Productions_1800)) { factionName = "(10) Explorers"; groupName = "Production Buildings"; templateName = "FactoryBuilding7"; }
            if (identifierName.IsPartOf(ChangeBuildingsToAT2_Productions_1800)) { factionName = "(11) Technicians"; groupName = "Production Buildings"; templateName = "FactoryBuilding7"; }

            //Farm buildings
            //if (identifierName.IsPartOf(ChangeBuildingsToAW_FarmBuilding_1800)) { factionName = "All Worlds"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW1_FarmBuilding_1800)) { factionName = "(01) Farmers"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW2_FarmBuilding_1800)) { factionName = "(02) Workers"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW3_FarmBuilding_1800)) { factionName = "(03) Artisans"; groupName = "Farm Buildings"; }
            //if (identifierName.IsPartOf(ChangeBuildingsToNW4_FarmBuilding_1800)) { factionName = "(04) Engineers"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToNW5_FarmBuilding_1800)) { factionName = "(05) Investors"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToOW1_FarmBuilding_1800)) { factionName = "(07) Jornaleros"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToOW2_FarmBuilding_1800)) { factionName = "(08) Obreros"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToAT1_FarmBuilding_1800)) { factionName = "(10) Explorers"; groupName = "Farm Buildings"; }
            if (identifierName.IsPartOf(ChangeBuildingsToAT2_FarmBuilding_1800)) { factionName = "(11) Technicians"; groupName = "Farm Buildings"; }

            //Special Buildings
            if (identifierName.IsPartOf(ChangeBuildingsToAW_SpecialBuilding_1800)) { factionName = "All Worlds"; groupName = "Special Buildings"; }
            //Ornamentals 
            //if (identifierName.IsPartOf(ChangeBuildingsToAW_Ornamentals_1800)) { factionName = "All Worlds"; }

            return (factionName, groupName, templateName);
        }
    }
}
