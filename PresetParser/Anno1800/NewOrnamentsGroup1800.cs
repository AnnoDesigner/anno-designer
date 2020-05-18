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
    /// </summary>
    public static class NewOrnamentsGroup1800
    {
        //private static readonly List<string> ChangeOrnamentToParkPaths_1800 = new List<string> {  "" };
        private static readonly List<string> ChangeOrnamentToParkPaths_1800 = new List<string> { "Park_1x1_pathstraight", "Park_1x1_pathend", "Park_1x1_pathangle", "Park_1x1_pathcrossing", "Park_1x1_pathwall" };
        private static readonly List<string> ChangeOrnamentToParkFences_1800 = new List<string> { "Park_1x1_hedgestraight", "Park_1x1_hedgeend", "Park_1x1_hedgeangle", "Park_1x1_hedgecrossing", "Park_1x1_hedgewall" };
        private static readonly List<string> ChangeOrnamentToParkVegetation_1800 = new List<string> { "Park_1x1_grass", "Park_1x1_bush", "Park_1x1_smalltree", "Park_1x1_pine", "Park_1x1_poplar" , "Park_1x1_bigtree" };
        private static readonly List<string> ChangeOrnamentToParkFountains_1800 = new List<string> { "Uplay_ornament_3x2_large_fountain", "Park_2x2_fountain", "Park_3x3_fountain" };
        private static readonly List<string> ChangeOrnamentToParkStatues_1800 = new List<string> {  "Sunken Treasure Ornament 01", "Sunken Treasure Ornament 02", "Sunken Treasure Ornament 03", "Uplay_ornament_2x1_lion_statue", "Culture_preorder_statue", "Park_2x2_statue" };
        private static readonly List<string> ChangeOrnamentToParkDecorations_1800 = new List<string> { "Park_1x1_benches", "Uplay_ornament_2x2_pillar_chess_park", "Park_2x2_garden" };
        private static readonly List<string> ChangeOrnamentToCityPaths_1800 = new List<string> { "" };
        private static readonly List<string> ChangeOrnamentToCityFences_1800 = new List<string> { "Culture_prop_system_1x1_03", "Culture_prop_system_1x1_04", "Culture_prop_system_1x1_05", "Culture_prop_system_1x1_06", "Culture_prop_system_1x1_07", "Culture_prop_system_1x1_08",
            "Culture_prop_system_1x1_09", "Culture_prop_system_1x1_11", "Culture_prop_system_1x1_12", "Culture_prop_system_1x1_13", "Culture_prop_system_1x1_14" };
        private static readonly List<string> ChangeOrnamentToCityStatues_1800 = new List<string> { "Park_1x1_statue", "City_prop_system_2x2_03", "Culture_prop_system_1x1_10" };
        private static readonly List<string> ChangeOrnamentToCityDecorations_1800 = new List<string> { "PropagandaTower Players Version", "PropagandaFlag Players Version", "Botanica Ornament 01", "Botanica Ornament 02", "Botanica Ornament 03" };
        private static readonly List<string> ChangeOrnamentToSpecialOrnaments_1800 = new List<string> { "Event_ornament_halloween_2019", "Event_ornament_christmas_2019", "Event_ornament_onemio", "Twitchdrops_ornament_billboard_annoholic", "Twitchdrops_ornament_billboard_anno_union",
            "Twitchdrops_ornament_billboard_anarchy","Twitchdrops_ornament_billboard_sunken_treasures","Twitchdrops_ornament_botanical_garden","Twitchdrops_ornament_flag_banner_annoholic","Twitchdrops_ornament_flag_banner_anno_union","Twitchdrops_ornament_billboard_the_passage",
            "Twitchdrops_ornament_morris_column_annoholic","Twitchdrops_ornament_flag_seat_of_power","Twitchdrops_ornament_billboard_seat_of_power","Twitchdrops_ornament_flag_bright_harvest","Twitchdrops_ornament_billboard_bright_harvest","Twitchdrops_ornament_flag_land_of_lions",
            "Twitchdrops_ornament_billboard_land_of_lions","Season 2 - Fountain Elephant","Season 2 - Statue Tractor","Season 2 - Pillar"};
        private static readonly List<string> ChangeOrnamentToChristmasDecorations_1800 = new List<string> { "Xmas City Tree Small 01", "Xmas City Tree Big 01","Xmas Parksystem Ornament Straight", "Xmas Parksystem Ornament Corner", "Xmas Parksystem Ornament End", "Xmas City Snowman Ornament 01",
            "Xmas City Lightpost Ornament 01","Xmas Citysystem Ornament T","Xmas Citysystem Ornament Straight","Xmas Citysystem Ornament Gap","Xmas Citysystem Ornament Cross","Xmas Citysystem Ornament End","Xmas Citysystem Ornament Corner","Xmas Parksystem Ornament Gap","Xmas Market 1",
            "Xmas Parksystem Ornament Cross","Xmas Parksystem Ornament T","Xmas Lightpost Ornament 02","Xmas Market 2","XMas Market 3","Xmas Carousel","Xmas presents","Xmas Santa Chair"};
        private static readonly List<string> ChangeOrnamentToWFRewards_1800 = new List<string> { "City_prop_system_1x1_02", "City_prop_system_1x1_03", "City_prop_system_2x2_02", "City_prop_system_2x2_04", "City_prop_system_3x3_02", "City_prop_system_3x3_03", "Culture_prop_system_1x1_02" };
        private static readonly List<string> ChangeOrnamentToGardens_1800 = new List<string> { "" };
        /// <summary>
        /// Retuns the faction and group for an identifier.
        /// </summary>
        /// <param name="identifierName">The given objectname, this will not changed</param>
        /// <param name="factionName">If objectname is in one of the lists, factionName will be changed</param>
        /// <param name="groupName">If objectname is in one of the lists, groupName will be changed</param>
        /// <returns></returns>
        public static string[] GetNewOrnamentsGroup1800(string identifierName, string factionName, string groupName )
        {
            if (string.IsNullOrWhiteSpace(identifierName))
            {
                throw new ArgumentNullException(nameof(identifierName), "No identifier was given.");
            }

            //New Ornaments Groups
            if (identifierName.IsPartOf(ChangeOrnamentToParkPaths_1800)) { factionName = "Ornaments"; groupName = "01 Park Paths"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkFences_1800)) { factionName = "Ornaments"; groupName = "02 Park Fences"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkVegetation_1800)) { factionName = "Ornaments"; groupName = "03 Park Vegetation"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkFountains_1800)) { factionName = "Ornaments"; groupName = "04 Park Fountains"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkStatues_1800)) { factionName = "Ornaments"; groupName = "05 Park Statues"; }
            if (identifierName.IsPartOf(ChangeOrnamentToParkDecorations_1800)) { factionName = "Ornaments"; groupName = "06 Park Decorations"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityPaths_1800)) { factionName = "Ornaments"; groupName = "07 City Paths"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityFences_1800)) { factionName = "Ornaments"; groupName = "08 City Fences"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityStatues_1800)) { factionName = "Ornaments"; groupName = "09 City Statues"; }
            if (identifierName.IsPartOf(ChangeOrnamentToCityDecorations_1800)) { factionName = "Ornaments"; groupName = "10 City Decorations"; }
            if (identifierName.IsPartOf(ChangeOrnamentToSpecialOrnaments_1800)) { factionName = "Ornaments"; groupName = "11 Special Ornaments"; }
            if (identifierName.IsPartOf(ChangeOrnamentToChristmasDecorations_1800)) { factionName = "Ornaments"; groupName = "12 Christmas Decorations"; }
            if (identifierName.IsPartOf(ChangeOrnamentToWFRewards_1800)) { factionName = "Ornaments"; groupName = "13 World's Fair Rewards"; }
            if (identifierName.IsPartOf(ChangeOrnamentToGardens_1800)) { factionName = "Ornaments"; groupName = "14 Gardens"; }  

            return new string[] { factionName, groupName };
        }
    }
}
