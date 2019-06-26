using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AnnoDesigner.model;

namespace AnnoDesigner.Localization
{
    public class TreeLocalization : ILocalizationHelper
    {
        public static Dictionary<string, Dictionary<string, string>> Translations;

        static TreeLocalization()
        {
            //This dictionary initialisation can be find on :
            //https://docs.google.com/spreadsheets/d/1CjECty43mkkm1waO4yhQl1rzZ-ZltrBgj00aq-WJX4w/edit?usp=sharing 
            //Steps to format:
            //Run CreateDictionary Script
            //Copy Output
            //Replace the escaped characters (\t\r\n) with the actual characters from within an editor of your choice
            Translations = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "eng", new Dictionary<string, string>() {
                        { "RoadTile" , "Road tile" },
                        { "BorderlessRoadTile" , "Borderless road tile" },
                        { "Military" , "Military" },
                        { "Camps" , "Camps" },
                        { "Production" , "Production" },
                        { "Ornament" , "Ornaments" },
                        { "ManorialPalace" , "Manorial Palace" },
                        { "PlayerBuildings" , "Player Buildings" },
                        { "Harbour" , "Harbour" },
                        { "Harbor" , "Harbour" },
                        { "Residence" , "Residence" },
                        { "Animalfarm" , "Animalfarm" },
                        { "Factory" , "Factory" },
                        { "Farm" , "Farm" },
                        { "FarmFields" , "Farm Fields" },
                        { "Plantation" , "Plantation" },
                        { "Resource" , "Resource" },
                        { "Public" , "Public Buildings" },
                        { "PublicBuildings" , "Public Buildings" },
                        { "Demand" , "Demand" },
                        { "Marine" , "Marine" },
                        { "Special" , "Special" },
                        { "Trade" , "Trade" },
                        { "(1)Ecos" , "(1) Ecos" },
                        { "(2)Tycoons" , "(2) Tycoons" },
                        { "(3)Techs" , "(3) Techs" },
                        { "Others" , "Others" },
                        { "Energy" , "Energy" },
                        { "BlackSmokers(DeepSea)" , "Black Smokers (Deep Sea)" },
                        { "BlackSmokers(Normal)" , "Black Smokers (Normal)" },
                        { "(1)Earth" , "(1) Earth" },
                        { "(2)Arctic" , "(2) Arctic" },
                        { "(3)Moon" , "(3) Moon" },
                        { "(4)Tundra" , "(4) Tundra" },
                        { "(5)Orbit" , "(5) Orbit" },
                        { "Facilities" , "Facilities" },
                        { "Ornaments" , "Ornaments" },
                        { "GlobalModules" , "Global Modules" },
                        { "Laboratories" , "Laboratories" },
                        { "Modules" , "Modules" },
                        { "Agriculture" , "Agriculture" },
                        { "Biotech" , "Biotech" },
                        { "Chemical" , "Chemical" },
                        { "Electronics" , "Electronics" },
                        { "Food" , "Food" },
                        { "Heavy" , "Heavy" },
                        { "Mining" , "Mining" },
                        { "TundraModules" , "Tundra Modules" },
                        { "IndustryModules" , "Industry Modules" },
                        { "CentralTechUnitReactor" , "Central Tech Unit Reactor" },
                        { "(1)Farmers" , "(1) Farmers" },
                        { "(2)Workers" , "(2) Workers" },
                        { "(3)Artisans" , "(3) Artisans" },
                        { "(4)Engineers" , "(4) Engineers" },
                        { "(5)Investors" , "(5) Investors" },
                        { "(6)OldWorldFields" , "(6) Old World Fields" },
                        { "(7)Jornaleros" , "(7) Jornaleros" },
                        { "(8)Obreros" , "(8) Obreros" },
                        { "(9)NewWorldFields" , "(9) New World Fields" },
                        { "AllWorlds" , "All Worlds" },
                        { "Attractiveness" , "Attractiveness" },
                        { "Electricity" , "Electricity" },
                        { "FarmBuildings" , "Farm Buildings" },
                        { "ProductionBuildings" , "Production Buildings" },
                        { "Depots" , "Depots" },
                        { "Logistics" , "Logistics" },
                        { "Shipyards" , "Shipyards" },
                        { "SpecialBuildings" , "Special Buildings" },
                        { "MiningBuildings" , "Mining Buildings" },
                        { "OrnamentalBuilding" , "Ornaments" },
                    }
                    },
                {
                    "ger", new Dictionary<string, string>() {
                        { "RoadTile" , "Straßenkachel" },
                        { "BorderlessRoadTile" , "Straßenkachel (randlos)" },
                        { "Military" , "Militär" },
                        { "Camps" , "Lager" },
                        { "Production" , "Produktion" },
                        { "Ornament" , "Ornamente" },
                        { "ManorialPalace" , "Herrschaftspalast" },
                        { "PlayerBuildings" , "Spielergebäude" },
                        { "Harbour" , "Hafen" },
                        { "Harbor" , "Hafen" },
                        { "Residence" , "Wohnsitz" },
                        { "Animalfarm" , "Tierfarm" },
                        { "Factory" , "Fabrik" },
                        { "Farm" , "Bauernhof" },
                        { "FarmFields" , "Farm-Felder" },
                        { "Plantation" , "Pflanzung" },
                        { "Resource" , "Ressource" },
                        { "Public" , "Öffentliche Gebäude" },
                        { "PublicBuildings" , "Öffentliche Gebäude" },
                        { "Demand" , "Nachfrage" },
                        { "Marine" , "Marine" },
                        { "Special" , "Spezial" },
                        { "Trade" , "Handel" },
                        { "(1)Ecos" , "(1) Ecos" },
                        { "(2)Tycoons" , "(2) Tycoons" },
                        { "(3)Techs" , "(3) Techniker" },
                        { "Others" , "Andere" },
                        { "Energy" , "Energie" },
                        { "BlackSmokers(DeepSea)" , "Schwarze Raucher (Tiefsee)" },
                        { "BlackSmokers(Normal)" , "Schwarze Raucher (Normal)" },
                        { "(1)Earth" , "(1) Erde" },
                        { "(2)Arctic" , "(2) Arktis" },
                        { "(3)Moon" , "(3) Moon" },
                        { "(4)Tundra" , "(4) Tundra" },
                        { "(5)Orbit" , "(5) Umlaufbahn" },
                        { "Facilities" , "Einrichtungen" },
                        { "Ornaments" , "Ornamente" },
                        { "GlobalModules" , "Globale Module" },
                        { "Laboratories" , "Laboratorien" },
                        { "Modules" , "Module" },
                        { "Agriculture" , "Landwirtschaft" },
                        { "Biotech" , "Biotechnologie" },
                        { "Chemical" , "Chemikalie" },
                        { "Electronics" , "Elektronik" },
                        { "Food" , "Lebensmittel" },
                        { "Heavy" , "Schwer" },
                        { "Mining" , "Bergbau" },
                        { "TundraModules" , "Tundra-Module" },
                        { "IndustryModules" , "Industriemodule" },
                        { "CentralTechUnitReactor" , "Zentraler Tech-Unit-Reaktor" },
                        { "(1)Farmers" , "(1) Bauern" },
                        { "(2)Workers" , "(2) Arbeiter" },
                        { "(3)Artisans" , "(3) Handwerker" },
                        { "(4)Engineers" , "(4) Ingenieure" },
                        { "(5)Investors" , "(5) Investoren" },
                        { "(6)OldWorldFields" , "(6) Felder der Alten Welt" },
                        { "(7)Jornaleros" , "(7) Jornaleros" },
                        { "(8)Obreros" , "(8) Obreros" },
                        { "(9)NewWorldFields" , "(9) Felder der Neuen Welt" },
                        { "AllWorlds" , "Alle Welten" },
                        { "Attractiveness" , "Attraktivität" },
                        { "Electricity" , "Elektrizität" },
                        { "FarmBuildings" , "Bauernhofgebäude" },
                        { "ProductionBuildings" , "Produktionsgebäude" },
                        { "Depots" , "Depots" },
                        { "Logistics" , "Logistik" },
                        { "Shipyards" , "Schiffswerften" },
                        { "SpecialBuildings" , "Spezielle Gebäude" },
                        { "MiningBuildings" , "Bergbau Gebäude" },
                        { "OrnamentalBuilding" , "Ornamente" }
                    }
                    },
                 {
                    "fra", new Dictionary<string, string>() {
                        { "RoadTile" , "Route" },
                        { "BorderlessRoadTile" , "Route sans bordure" },
                        { "Military" , "Militaire" },
                        { "Camps" , "Camps" },
                        { "Production" , "Production" },
                        { "Ornament" , "Ornements" },
                        { "ManorialPalace" , "Palais" },
                        { "PlayerBuildings" , "Bâtiments du joueur" },
                        { "Harbour" , "Port" },
                        { "Harbor" , "Port" },
                        { "Residence" , "Résidence" },
                        { "Animalfarm" , "Ferme animalière" },
                        { "Factory" , "Usinek" },
                        { "Farm" , "Ferme animalière" },
                        { "FarmFields" , "Champs agricole" },
                        { "Plantation" , "Plantation" },
                        { "Resource" , "Ressource" },
                        { "Public" , "Bâtiments publics" },
                        { "PublicBuildings" , "Bâtiments publics" },
                        { "Demand" , "Demande" },
                        { "Marine" , "Marine" },
                        { "Special" , "Spécial" },
                        { "Trade" , "Échange" },
                        { "(1)Ecos" , "(1) Écos" },
                        { "(2)Tycoons" , "(2) Entrepreneurs" },
                        { "(3)Techs" , "(3) Techniciens" },
                        { "Others" , "Autres" },
                        { "Energy" , "Énergie" },
                        { "BlackSmokers(DeepSea)" , "Fumeurs noirs (Grands fonds marins)" },
                        { "BlackSmokers(Normal)" , "Fumeurs noirs (Normal)" },
                        { "(1)Earth" , "(1) Terre" },
                        { "(2)Arctic" , "(2) Arctique" },
                        { "(3)Moon" , "(3) Lune" },
                        { "(4)Tundra" , "(4) Toundra" },
                        { "(5)Orbit" , "(5) Orbite" },
                        { "Facilities" , "Installations" },
                        { "Ornaments" , "Ornements" },
                        { "GlobalModules" , "Modules global" },
                        { "Laboratories" , "Laboratoires" },
                        { "Modules" , "Modules" },
                        { "Agriculture" , "Agriculture" },
                        { "Biotech" , "Biotechnologie" },
                        { "Chemical" , "Chimique" },
                        { "Electronics" , "Électronique" },
                        { "Food" , "Alimentation" },
                        { "Heavy" , "Lourd" },
                        { "Mining" , "Exploitation minière" },
                        { "TundraModules" , "Modules de la Toundra" },
                        { "IndustryModules" , "Modules industriel" },
                        { "CentralTechUnitReactor" , "Réacteur de l'unité de technologie" },
                        { "(1)Farmers" , "(1) Fermiers" },
                        { "(2)Workers" , "(2) Ouvriers" },
                        { "(3)Artisans" , "(3) Artisans" },
                        { "(4)Engineers" , "(4) Ingénieurs" },
                        { "(5)Investors" , "(5) Investisseurs" },
                        { "(6)OldWorldFields" , "(6) L'ancien monde" },
                        { "(7)Jornaleros" , "(7) Jornaleros" },
                        { "(8)Obreros" , "(8) Obreros" },
                        { "(9)NewWorldFields" , "(9) Nouveau monde" },
                        { "AllWorlds" , "Tous les mondes" },
                        { "Attractiveness" , "Attractivité" },
                        { "Electricity" , "Électricité" },
                        { "FarmBuildings" , "Bâtiments agricoles" },
                        { "ProductionBuildings" , "Bâtiments de production" },
                        { "Depots" , "Dépôts" },
                        { "Logistics" , "Logistique" },
                        { "Shipyards" , "Chantiers navals" },
                        { "SpecialBuildings" , "Bâtiments spéciaux" },
                        { "MiningBuildings" , "Bâtiments miniers" },
                        { "OrnamentalBuilding" , "Ornements" }
                    }
                    },
                {
                    "pol", new Dictionary<string, string>() {
                        { "RoadTile" , "Płytka drogowa" },
                        { "BorderlessRoadTile" , "Płytka drogowa bez granic" },
                        { "Military" , "Wojsko" },
                        { "Camps" , "Obozy" },
                        { "Production" , "Produkcja" },
                        { "Ornament" , "Ornamenty" },
                        { "ManorialPalace" , "Pałac Dworski" },
                        { "PlayerBuildings" , "Budynki graczy" },
                        { "Harbour" , "Port" },
                        { "Harbor" , "Port" },
                        { "Residence" , "Rezydencja" },
                        { "Animalfarm" , "Hodowla" },
                        { "Factory" , "Fabryka" },
                        { "Farm" , "Farma" },
                        { "FarmFields" , "Pola uprawne" },
                        { "Plantation" , "Plantacja" },
                        { "Resource" , "Zasoby" },
                        { "Public" , "Budynki publiczne" },
                        { "PublicBuildings" , "Budynki publiczne" },
                        { "Demand" , "Popyt" },
                        { "Marine" , "Żegluga morska" },
                        { "Special" , "Specjalne" },
                        { "Trade" , "Handel" },
                        { "(1)Ecos" , "(1) Ekosi" },
                        { "(2)Tycoons" , "(2) Fabryci" },
                        { "(3)Techs" , "(3) Technosi" },
                        { "Others" , "Inne" },
                        { "Energy" , "Energia" },
                        { "BlackSmokers(DeepSea)" , "Kominy hydrotermalne (Błękitna Głębia)" },
                        { "BlackSmokers(Normal)" , "Kominy hydrotermalne (normalnie)" },
                        { "(1)Earth" , "(1) Ziemia" },
                        { "(2)Arctic" , "(2) Arktyka" },
                        { "(3)Moon" , "(3) Księżyc" },
                        { "(4)Tundra" , "(4) Tundra" },
                        { "(5)Orbit" , "(5) Orbita" },
                        { "Facilities" , "Udogodnienia" },
                        { "Ornaments" , "Ornamenty" },
                        { "GlobalModules" , "Moduły globalne" },
                        { "Laboratories" , "Laboratoria" },
                        { "Modules" , "Moduły" },
                        { "Agriculture" , "Rolnictwo" },
                        { "Biotech" , "Biotechnologia" },
                        { "Chemical" , "Chemikalia" },
                        { "Electronics" , "Elektronika" },
                        { "Food" , "Żywność" },
                        { "Heavy" , "Ciężki" },
                        { "Mining" , "Górnictwo" },
                        { "TundraModules" , "Moduły Tundra" },
                        { "IndustryModules" , "Moduły branżowe" },
                        { "CentralTechUnitReactor" , "Reaktor centralnej jednostki technicznej" },
                        { "(1)Farmers" , "(1) Farmerzy" },
                        { "(2)Workers" , "(2) Robotnicy" },
                        { "(3)Artisans" , "(3) Rzemieślnicy" },
                        { "(4)Engineers" , "(4) Inżynierowie" },
                        { "(5)Investors" , "(5) Inwestorzy" },
                        { "(6)OldWorldFields" , "(6) Pola Starego Świata" },
                        { "(7)Jornaleros" , "(7) Jornaleros" },
                        { "(8)Obreros" , "(8) Obreros" },
                        { "(9)NewWorldFields" , "(9) Pola Nowego Świata" },
                        { "AllWorlds" , "Wszystkie światy" },
                        { "Attractiveness" , "Atrakcyjność" },
                        { "Electricity" , "Energia elektryczna" },
                        { "FarmBuildings" , "Budynki rolnicze" },
                        { "ProductionBuildings" , "Budynki produkcyjne" },
                        { "Depots" , "Magazyny" },
                        { "Logistics" , "Logistyka" },
                        { "Shipyards" , "Stocznie" },
                        { "SpecialBuildings" , "Budynki specjalne" },
                        { "MiningBuildings" , "Budynki górnicze" },
                        { "OrnamentalBuilding" , "Ornamenty" }
                    }
                    },
                {
                    "rus", new Dictionary<string, string>() {
                        { "RoadTile" , "Плитка для дорог" },
                        { "BorderlessRoadTile" , "Плитка для безграничных дорог" },
                        { "Military" , "Военные" },
                        { "Camps" , "Лагеря" },
                        { "Production" , "Производство" },
                        { "Ornament" , "Орнаменты" },
                        { "ManorialPalace" , "Поместный дворец" },
                        { "PlayerBuildings" , "Здания для игроков" },
                        { "Harbour" , "гавань" },
                        { "Harbor" , "гавань" },
                        { "Residence" , "Резиденция" },
                        { "Animalfarm" , "Анимальфарм" },
                        { "Factory" , "Завод" },
                        { "Farm" , "Ферма" },
                        { "FarmFields" , "Фермерские поля" },
                        { "Plantation" , "Плантация" },
                        { "Resource" , "Ресурс" },
                        { "Public" , "Общественные здания" },
                        { "PublicBuildings" , "Общественные здания" },
                        { "Demand" , "Спрос" },
                        { "Marine" , "Морпех" },
                        { "Special" , "Специально" },
                        { "Trade" , "Торговля" },
                        { "(1)Ecos" , "(1) Экос" },
                        { "(2)Tycoons" , "(2) Талисманы" },
                        { "(3)Techs" , "(3) Техники" },
                        { "Others" , "Другие" },
                        { "Energy" , "Энергетика" },
                        { "BlackSmokers(DeepSea)" , "Чернокожие курильщики (Глубоководье)" },
                        { "BlackSmokers(Normal)" , "Чернокожие курильщики (нормальные)" },
                        { "(1)Earth" , "(1) Земля" },
                        { "(2)Arctic" , "(2) Арктика" },
                        { "(3)Moon" , "(3) Луна" },
                        { "(4)Tundra" , "(4) Тундра" },
                        { "(5)Orbit" , "(5) Орбита" },
                        { "Facilities" , "Объекты" },
                        { "Ornaments" , "Орнаменты" },
                        { "GlobalModules" , "Глобальные модули" },
                        { "Laboratories" , "Лаборатории" },
                        { "Modules" , "Модули" },
                        { "Agriculture" , "Сельское хозяйство" },
                        { "Biotech" , "Биотехника" },
                        { "Chemical" , "Химикаты" },
                        { "Electronics" , "Электроника" },
                        { "Food" , "Продукты" },
                        { "Heavy" , "Тяжелый" },
                        { "Mining" , "Горное дело" },
                        { "TundraModules" , "Модули тундры" },
                        { "IndustryModules" , "Модули промышленности" },
                        { "CentralTechUnitReactor" , "Реактор центрального технологического блока" },
                        { "(1)Farmers" , "(1) Фермеры" },
                        { "(2)Workers" , "(2) Рабочие" },
                        { "(3)Artisans" , "(3) Ремесленники" },
                        { "(4)Engineers" , "(4) Инженеры" },
                        { "(5)Investors" , "(5) Инвесторы" },
                        { "(6)OldWorldFields" , "(6) Поля Старого Света" },
                        { "(7)Jornaleros" , "(7) Хорналерос" },
                        { "(8)Obreros" , "(8) Обрерос" },
                        { "(9)NewWorldFields" , "(9) Новые Поля Мира" },
                        { "AllWorlds" , "Все миры" },
                        { "Attractiveness" , "Привлекательность" },
                        { "Electricity" , "Электричество" },
                        { "FarmBuildings" , "Фермерские здания" },
                        { "ProductionBuildings" , "Производственные здания" },
                        { "Depots" , "Аптеки" },
                        { "Logistics" , "Логистика" },
                        { "Shipyards" , "Судоремонтные заводы" },
                        { "SpecialBuildings" , "Специальные здания" },
                        { "MiningBuildings" , "Горнодобывающие здания" },
                        {"OrnamentalBuilding" , "Орнаменты" }
                    }
                    },
            };
        }

        public string GetLocalization(string valueToTranslate)
        {
            return GetLocalization(valueToTranslate, null);
        }

        public string GetLocalization(string valueToTranslate, string language = null)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                language = Localization.GetLanguageCodeFromName(AnnoDesigner.MainWindow.SelectedLanguage);
            }

            if (!Localization.LanguageCodeMap.ContainsKey(language))
            {
                language = "eng";
            }

            try
            {
                if (Translations[language].TryGetValue(valueToTranslate.Replace(" ", string.Empty), out string foundLocalization))
                {
                    return foundLocalization;
                }
                else
                {
                    Debug.WriteLine($"try to set localization to english for: : \"{valueToTranslate}\"");
                    if (Translations["eng"].TryGetValue(valueToTranslate.Replace(" ", string.Empty), out string engLocalization))
                    {
                        return engLocalization;
                    }
                    else
                    {
                        Debug.WriteLine($"found no localization (\"eng\") and ({language}) for : \"{valueToTranslate}\"");
                        return valueToTranslate;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"error getting localization ({language}) for: \"{valueToTranslate}\"{Environment.NewLine}{ex}");
                return valueToTranslate;
            }
        }
    }
}