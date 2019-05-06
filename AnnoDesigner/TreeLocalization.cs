using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnnoDesigner.TreeLocalization
{
    public static class TreeLocalization
    {
        public static Dictionary<string, Dictionary<string, string>> Translations;

        static TreeLocalization()
        {
            //This dictionary initialisation can be find on (:
            //https://docs.google.com/spreadsheets/d/1CjECty43mkkm1waO4yhQl1rzZ-ZltrBgj00aq-WJX4w/edit?usp=sharing 
            //Steps to format:
            //Run CreateDictionary Script
            //Copy Output
            //Replace the escaped characters (\t\r\n) with the actual characters from within an editor of your choice
            Translations = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "eng", new Dictionary<string, string>() {
                        {"SpaceModule", " Module"}, //for the extra 2205 fourthlevel Module List
                        {"Military" , "Military" },
                        {"Camps" , "Camps" },
                        {"Production" , "Production" },
                        {"Ornament" , "Ornament" },
                        {"ManorialPalace" , "Manorial Palace" },
                        {"PlayerBuildings" , "Player Buildings" },
                        {"Harbor" , "Harbour" },
                        {"Harbour" , "Harbour" },
                        {"Residence" , "Residence" },
                        {"Animalfarm" , "Animalfarm" },
                        {"Factory" , "Factory" },
                        {"Farm" , "Farm" },
                        {"FarmFields" , "Farm Fields" },
                        {"Plantation" , "Plantation" },
                        {"Resource" , "Resource" },
                        {"Public" , "Public Buildings" },
                        {"PublicBuildings" , "Public Buildings" },
                        {"Demand" , "Demand" },
                        {"Marine" , "Marine" },
                        {"Special" , "Special" },
                        {"Trade" , "Trade" },
                    }
                    },
                {
                    "ger", new Dictionary<string, string>() {
                        {"SpaceModule", " Modul"}, //for the extra 2205 fourthlevel Module List
                        {"Military" , "Militär" },
                        {"Camps" , "Lager" },
                        {"Production" , "Produktion" },
                        {"Ornament" , "Verzierung" },
                        {"ManorialPalace" , "Herrschaftspalast" },
                        {"PlayerBuildings" , "Spielergebäude" },
                        {"Harbor" , "Hafen" },
                        {"Harbour" , "Hafen" },
                        {"Residence" , "Wohnsitz" },
                        {"Animalfarm" , "Tierfarm" },
                        {"Factory" , "Fabrik" },
                        {"Farm" , "Bauernhof" },
                        {"FarmFields" , "Farm-Felder" },
                        {"Plantation" , "Pflanzung" },
                        {"Resource" , "Ressource" },
                        {"Public" , "Öffentliche Gebäude" },
                        {"PublicBuiildings" , "Öffentliche Gebäude" },
                        {"Demand" , "Nachfrage" },
                        {"Marine" , "Marine" },
                        {"Special" , "Spezial" },
                        {"Trade" , "Handel" },
                    }
                    },
                {
                    "pol", new Dictionary<string, string>() {
                        {"SpaceModule", " Moduł"}, //for the extra 2205 fourthlevel Module List
                        {"Military" , "Wojsko" },
                        {"Camps" , "Obozy" },
                        {"Production" , "Produkcja" },
                        {"Ornament" , "Zdobienie" },
                        {"ManorialPalace" , "Pałac Manoriałów" },
                        {"PlayerBuildings" , "Budynki dla graczy" },
                        {"Harbour" , "Port" },
                        {"Harbor" , "Port" },
                        {"Residence" , "Rezydencja" },
                        {"Animalfarm" , "Gospodarstwo hodowlane" },
                        {"Factory" , "Fabryka" },
                        {"Farm" , "Gospodarstwo" },
                        {"FarmFields" , "Pola uprawne" },
                        {"Plantation" , "Plantacja" },
                        {"Resource" , "Zasoby" },
                        {"Public" , "Budynki publiczne" },
                        {"PublicBuildings" , "Budynki publiczne" },
                        {"Demand" , "Popyt" },
                        {"Marine" , "Żegluga morska" },
                        {"Special" , "Specjalne" },
                        {"Trade" , "Handel" },
                    }
                    },
                {
                    "rus", new Dictionary<string, string>() {
                        {"SpaceModule", " Модуль"}, //for the extra 2205 fourthlevel Module List
                        {"Military" , "Военные" },
                        {"Camps" , "Лагеря" },
                        {"Production" , "Производство" },
                        {"Ornament" , "Орнамент" },
                        {"ManorialPalace" , "Поместный дворец" },
                        {"PlayerBuildings" , "Здания для игроков" },
                        {"Harbour" , "гавань" },
                        {"Harbor" , "гавань" },
                        {"Residence" , "Резиденция" },
                        {"Animalfarm" , "Анимальфарм" },
                        {"Factory" , "Завод" },
                        {"Farm" , "Ферма" },
                        {"FarmFields" , "Фермерские поля" },
                        {"Plantation" , "Плантация" },
                        {"Resource" , "Ресурс" },
                        {"Public" , "Общественные здания" },
                        {"PublicBuildings" , "Общественные здания" },
                        {"Demand" , "Спрос" },
                        {"Marine" , "Морпех" },
                        {"Special" , "Специально" },
                        {"Trade" , "Торговля" },
                    }
                    },
            };
        }
    }
}