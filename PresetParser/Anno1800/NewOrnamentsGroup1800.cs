using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Extensions;

namespace PresetParser.Anno1800
{
    /// <summary>
    /// in this file are made the following lists
    /// ChangeOrnamentTo<1>_1800 
    /// <1> can be : ParkPaths
    ///              ParkFences
    ///              ParkVegetation
    ///              ParkFountains
    ///              ParkStatues
    ///              ParkDecorations
    ///              CityPaths
    ///              CityFences
    ///              CityStatues
    ///              CityDecorations
    ///              SpecialOrnaments
    ///              ChristmasDecorations
    ///              WorldsFairReward
    ///              Gardens 
    ///              AgriculturalOrnaments
    ///              AgriculturalFences
    ///              IndustrialOnrnaments
    ///              IndustrialFences
    ///              AmusementPark
    ///              City Lights
    ///              <reservered>EnbesaOrnaments</reservered>
    ///              <reservered>DocklandsOrnaments</reservered>
    /// </summary>
    public static class NewOrnamentsGroup1800
    {
        //private static readonly List<string> ChangeOrnamentToParkPaths_1800 = new List<string> {  "" };
        private static readonly List<string> ChangeOrnamentToParkPaths_1800 = new List<string> { "Park_1x1_pathstraight", "Park_1x1_pathend", "Park_1x1_pathangle", "Park_1x1_pathcrossing", "Park_1x1_pathwall" };
        private static readonly List<string> ChangeOrnamentToParkFences_1800 = new List<string> { "Park_1x1_hedgestraight", "Park_1x1_hedgeend", "Park_1x1_hedgeangle", "Park_1x1_hedgecrossing", "Park_1x1_hedgewall", "Culture_1x1_fencestraight", "Culture_1x1_fenceend",
            "Culture_1x1_fenceangle", "Culture_1x1_fencecrossing", "Culture_1x1_fencewall", "Park_1x1_hedgegate", "Culture_1x1_fencegate" };
        private static readonly List<string> ChangeOrnamentToParkVegetation_1800 = new List<string> { "Park_1x1_grass", "Park_1x1_bush", "Park_1x1_smalltree", "Park_1x1_pine", "Park_1x1_poplar" , "Park_1x1_bigtree", "Park_1x1_poplarforest", "Park_1x1_tropicalforest",
            "Park_1x1_philodendron", "Park_1x1_ferns", "Park_1x1_floweringshrub", "Park_1x1_smallpalmtree", "Park_1x1_palmtree", "Park_1x1_shrub", "Park_1x1_growncypress"};
        private static readonly List<string> ChangeOrnamentToParkFountains_1800 = new List<string> { "Uplay_ornament_3x2_large_fountain", "Park_2x2_fountain", "Park_3x3_fountain" };
        private static readonly List<string> ChangeOrnamentToParkStatues_1800 = new List<string> { "Sunken Treasure Ornament 01", "Sunken Treasure Ornament 02", "Sunken Treasure Ornament 03", "Uplay_ornament_2x1_lion_statue", "Culture_preorder_statue", "Park_2x2_statue", "Park_2x2_horsestatue" };
        private static readonly List<string> ChangeOrnamentToParkDecorations_1800 = new List<string> { "Park_1x1_benches", "Uplay_ornament_2x2_pillar_chess_park", "Park_2x2_garden", "Park_1x1_stand", "Park_2x2_gazebo", "Park_3x3_gazebo" };
        private static readonly List<string> ChangeOrnamentToCityPaths_1800 = new List<string> { "Palace Ornament02 Set01 hedge pointy", "Palace Ornament01 Set01 banner", "Palace Ornament03 Set01 hedge round", "Palace Ornament05 Set01 fountain big", "Palace Ornament04 Set01 fountain small",
            "Palace Ornament03 Set02 angle", "Palace Ornament04 Set02 crossing", "Palace Ornament02 Set02 end", "Palace Ornament05 Set02 junction", "Palace Ornament01 Set02 straight", "Palace Ornament06 Set02 straight variation",};
        private static readonly List<string> ChangeOrnamentToCityFences_1800 = new List<string> { "Culture_prop_system_1x1_03", "Culture_prop_system_1x1_04", "Culture_prop_system_1x1_05", "Culture_prop_system_1x1_06", "Culture_prop_system_1x1_07", "Culture_prop_system_1x1_08",
            "Culture_prop_system_1x1_09", "Culture_prop_system_1x1_11", "Culture_prop_system_1x1_12", "Culture_prop_system_1x1_13", "Culture_prop_system_1x1_14", "Culture_1x1_hedgegate", "Culture_1x1_hedgestraight",
            "Park_1x1_fencegate"};
        private static readonly List<string> ChangeOrnamentToCityStatues_1800 = new List<string> { "Park_1x1_statue", "City_prop_system_2x2_03", "Culture_prop_system_1x1_10", "Park_2x2_manstatue", "Culture_1x1_statue" };
        private static readonly List<string> ChangeOrnamentToCityDecorations_1800 = new List<string> { "PropagandaTower Players Version", "PropagandaFlag Players Version", "Botanica Ornament 01", "Botanica Ornament 02", "Botanica Ornament 03", "Culture_1x1_benches", "Culture_1x1_stand",
            "Palace Ornament06 Set01 statue", "Palace Ornament01 Set02 promenade", "Park_1x1_flowerbed", "Park_1x1_grownbush", "Park_1x1_elmtree", "Park_1x1_tremblingaspen", "Park_1x1_appletree", "Park_1x1_wateringplace", "Park_1x1_well", "Park_1x1_temperateforest" };
        private static readonly List<string> ChangeOrnamentToSpecialOrnaments_1800 = new List<string> { "Event_ornament_halloween_2019", "Event_ornament_christmas_2019", "Event_ornament_onemio", "Twitchdrops_ornament_billboard_annoholic", "Twitchdrops_ornament_billboard_anno_union",
            "Twitchdrops_ornament_billboard_anarchy","Twitchdrops_ornament_billboard_sunken_treasures","Twitchdrops_ornament_botanical_garden","Twitchdrops_ornament_flag_banner_annoholic","Twitchdrops_ornament_flag_banner_anno_union","Twitchdrops_ornament_billboard_the_passage",
            "Twitchdrops_ornament_morris_column_annoholic","Twitchdrops_ornament_flag_seat_of_power","Twitchdrops_ornament_billboard_seat_of_power","Twitchdrops_ornament_flag_bright_harvest","Twitchdrops_ornament_billboard_bright_harvest","Twitchdrops_ornament_flag_land_of_lions",
            "Twitchdrops_ornament_billboard_land_of_lions","Season 2 - Fountain Elephant","Season 2 - Statue Tractor","Season 2 - Pillar", "Season 3 - Kiosk", "Season 3 - Anchor", "Season 3 - Skyscraper", "Twitchdrops_ornament_Flag_Dock", "Twitchdrops_ornament_Lamp_Dock",
            "Twitchdrops_ornament_Flag_Tourist", "Twitchdrops_ornament_Lamp_Tourist", "Twitchdrops_ornament_Flag_HighL", "Twitchdrops_ornament_Lamp_HighL", "Twitchdrops_ornament_Flag_DLC11", "Twitchdrops_ornament_billboard_DLC11", "Twitchdrops_ornament_Flag_DLC12",
            "Twitchdrops_ornament_billboard_DLC12","Twitchdrops_ornament_Flag_DLC10", "Twitchdrops_ornament_billboard_DLC10", "Season 4 - Blimp", "Season 4 - Gate", "Season 4 - Fountain"};
        private static readonly List<string> ChangeOrnamentToChristmasDecorations_1800 = new List<string> { "Xmas City Tree Small 01", "Xmas City Tree Big 01","Xmas Parksystem Ornament Straight", "Xmas Parksystem Ornament Corner", "Xmas Parksystem Ornament End", "Xmas City Snowman Ornament 01",
            "Xmas City Lightpost Ornament 01","Xmas Citysystem Ornament T","Xmas Citysystem Ornament Straight","Xmas Citysystem Ornament Gap","Xmas Citysystem Ornament Cross","Xmas Citysystem Ornament End","Xmas Citysystem Ornament Corner","Xmas Parksystem Ornament Gap","Xmas Market 1",
            "Xmas Parksystem Ornament Cross","Xmas Parksystem Ornament T","Xmas Lightpost Ornament 02","Xmas Market 2","XMas Market 3","Xmas Carousel","Xmas presents","Xmas Santa Chair"};
        private static readonly List<string> ChangeOrnamentToWFRewards_1800 = new List<string> { "City_prop_system_1x1_02", "City_prop_system_1x1_03", "City_prop_system_2x2_02", "City_prop_system_2x2_04", "City_prop_system_3x3_02", "City_prop_system_3x3_03", "Culture_prop_system_1x1_02",
            "Culture_1x1_basinbridge","Culture_1x1_secondground"};
        private static readonly List<string> ChangeOrnamentToGardens_1800 = new List<string> { "Light pink flower field - roses", "Blue flower field - gentian", "Labyrinth", "Pink flower field - hibiscus", "Purple blue flower field - iris", "White flower field - blue heart lily",
            "Orange flower field - plumeria aussi orange", "Red white flower field - red white petunia", "Trees alley", "Sculpted trees", "Yellow flower field - miracle daisy" };
        private static readonly List<string> ChangeOrnamentToAgriculturalOrnaments_1800 = new List<string> { "BH Ornament04 Flatbed Wagon", "BH Ornament05 Scarecrow", "BH Ornament06 LogPile", "BH Ornament07 Outhouse", "BH Ornament08 Signpost", "BH Ornament09 HayBalePile", "BH Ornament10 Swing",
            "BH Ornament23 Clothes Line" };
        private static readonly List<string> ChangeOrnamentToAgriculturalFences_1800 = new List<string> { "BH Ornament03 Fence Straight", "BH Ornament03 Fence End", "BH Ornament03 Fence Cross", "BH Ornament03 Fence T-Cross", "BH Ornament03 Fence Corner", "BH Ornament03 Fence Gate" };
        private static readonly List<string> ChangeOrnamentToIndustrialOrnaments_1800 = new List<string> { "BH Ornament24 Empty Groundplane", "BH Ornament11 Pipes", "BH Ornament12 Barrel Pile", "BH Ornament13 WoddenBoxes", "BH Ornament14 Tanks", "BH Ornament15 Water Tower",
            "BH Ornament17 Shed", "BH Ornament18 Pile Iron Bars", "BH Ornament19 Pile Boxes and Barrels", "BH Ornament20 Heap", "BH Ornament21 Large Boxes", "BH Ornament22 Gangway" };
        private static readonly List<string> ChangeOrnamentToIndustrialFences_1800 = new List<string> { "BH Ornament01 Wall Straight", "BH Ornament01 Wall End", "BH Ornament01 Wall Cross", "BH Ornament01 Wall T-Cross", "BH Ornament01 Wall Corner", "BH Ornament01 Wall Gate",
            "BH Ornament01 Wall Gate 02", "BH Ornament02 Wall Straight Large", "BH Ornament02 Wall End Large", "BH Ornament02 Wall Cross Large", "BH Ornament02 Wall T-Cross Large", "BH Ornament02 Wall Corner Large", "BH Ornament02 Wall Gate Large", "BH Ornament02 Wall Large" };
        private static readonly List<string> ChangeOrnamentToAmusementPark_1800 = new List<string> { "AmusementPark FerrisWheel", "AmusementPark RollerCoaster", "AmusementPark PayBooth", "AmusementPark Icecream", "AmusementPark CottonCandy", "AmusementPark TinCanGame", "AmusementPark ShootingGame",
            "AmusementPark Consumable01", "AmusementPark Consumable02", "AmusementPark Strongman", "AmusementPark PictureWall", "AmusementPark OrganPlayer", "AmusementPark BalloonSeller", "AmusementPark Painter", "AmusementPark GateBig", "AmusementPark BarTable", "AmusementPark DrinkStand",
            "AmusementPark FoodStand" };

        /// <summary>
        /// Retuns the faction and group for an identifier.
        /// </summary>
        /// <param name="identifierName">The given objectname, this will not changed</param>
        /// <param name="factionName">If objectname is in one of the lists, factionName will be changed</param>
        /// <param name="groupName">If objectname is in one of the lists, groupName will be changed</param>
        /// <param name="templateName">Objects Color Assignement for color.json</param>
        /// <returns></returns>
        public static (string Faction, string Group, string Template) GetNewOrnamentsGroup1800(string identifierName, string factionName, string groupName, string templateName)
        {
            if (string.IsNullOrWhiteSpace(identifierName))
            {
                throw new ArgumentNullException(nameof(identifierName), "No identifier was given.");
            }

            //New Ornaments Groups
            if (identifierName.IsPartOf(ChangeOrnamentToParkPaths_1800)) { factionName = "Ornaments"; groupName = "01 Park Paths"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkFences_1800)) { factionName = "Ornaments"; groupName = "02 Park Fences"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkVegetation_1800)) { factionName = "Ornaments"; groupName = "03 Park Vegetation"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkFountains_1800)) { factionName = "Ornaments"; groupName = "04 Park Fountains"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkStatues_1800)) { factionName = "Ornaments"; groupName = "05 Park Statues"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkDecorations_1800)) { factionName = "Ornaments"; groupName = "06 Park Decorations"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityPaths_1800)) { factionName = "Ornaments"; groupName = "07 City Paths"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityFences_1800)) { factionName = "Ornaments"; groupName = "08 City Fences"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityStatues_1800)) { factionName = "Ornaments"; groupName = "09 City Statues"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityDecorations_1800)) { factionName = "Ornaments"; groupName = "10 City Decorations"; }
            if (identifierName.IsPartOf(ChangeOrnamentToSpecialOrnaments_1800)) { factionName = "Ornaments"; groupName = "11 Special Ornaments"; }
            if (identifierName.IsPartOf(ChangeOrnamentToChristmasDecorations_1800)) { factionName = "Ornaments"; groupName = "12 Christmas Decorations"; }
            if (identifierName.IsPartOf(ChangeOrnamentToWFRewards_1800)) { factionName = "Ornaments"; groupName = "13 World's Fair Rewards"; }
            if (identifierName.IsPartOf(ChangeOrnamentToGardens_1800)) { factionName = "Ornaments"; groupName = "14 Gardens"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToAgriculturalOrnaments_1800)) { factionName = "Ornaments"; groupName = "15 Agricultural Ornaments"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToAgriculturalFences_1800)) { factionName = "Ornaments"; groupName = "16 Agricultural Fences"; templateName = "OrnamentalBuilding_Park"; }
            if (identifierName.IsPartOf(ChangeOrnamentToIndustrialOrnaments_1800)) { factionName = "Ornaments"; groupName = "17 Industrial Ornaments"; templateName = "OrnamentalBuilding_Industrial"; }
            if (identifierName.IsPartOf(ChangeOrnamentToIndustrialFences_1800)) { factionName = "Ornaments"; groupName = "18 IndustrialFences"; templateName = "OrnamentalBuilding_Industrial"; }
            if (identifierName.IsPartOf(ChangeOrnamentToAmusementPark_1800)) { factionName = "Ornaments"; groupName = "19 Amusement Park"; }
            return (factionName, groupName, templateName);
        }
    }
}
