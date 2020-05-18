using System;
using System.Collections.Generic;
using System.Text;
using InfoboxParser.Models;

namespace InfoboxParser
{
    public class SpecialBuildingNameHelper : ISpecialBuildingNameHelper
    {
        public string CheckSpecialBuildingName(string buildingName)
        {
            switch (buildingName)
            {
                case "Bombin Weaver":
                    buildingName = "Bomb­ín Weaver";
                    break;
                case "Caoutchouc":
                    buildingName = "Caoutchouc Plantation";
                    break;
                case "Fried Plaintain Kitchen":
                    buildingName = "Fried Plantain Kitchen";
                    break;
                case "World's Fair: Foundations":
                    buildingName = "World's Fair|World's Fair: Foundations";
                    break;
                case "Airship Hangar":
                    buildingName = "Airship Hangar|Airship Hangar: Foundations";
                    break;
                case "Explorer Residence":
                case "Explorer Shelter":
                    buildingName = "Explorer Residence|Explorer Shelter";
                    break;
                case "Deep Gold Mine":
                    buildingName = "Gold Mine|Deep Gold Mine";
                    break;
                case "Pristine Hunting Cabin":
                    buildingName = "Hunting Cabin|Pristine Hunting Cabin";
                    break;
                case "Lumberjack":
                    buildingName = "Lumberjack's Hut";
                    break;
                default:
                    break;
            }

            return buildingName;
        }

    }
}
