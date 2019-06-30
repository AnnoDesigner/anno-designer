using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Layout.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using FandomParser.Core.Presets.Models;
using FandomTemplateExporter;
using FandomTemplateExporter.Export;
using NLog;

namespace AnnoDesigner.Export
{
    public class FandomExporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string TEMPLATE_START = "{{";
        private const string TEMPLATE_END = "}}";
        private const string TEMPLATE_LINE_START = "|";
        private const string TEMPLATE_ENTRY_DELIMITER = " = ";
        private const string TEMPLATE_SIZE_ENTRY_DELIMITER = " x ";
        private const string MAPPING_NOT_FOUND = "##Please adjust value## ";

        private const string HEADER_PRODUCTION_LAYOUT = "Production layout";
        private const string HEADER_ICON = "Icon";
        private const string HEADER_LAYOUT_NAME = "Name of the layout";
        private const string HEADER_LAYOUT_IMAGE = "Layout image";
        private const string HEADER_LAYOUT_DESCRIPTION = "Layout description";
        private const string HEADER_SIZE = "Size";
        private const string HEADER_TILES = "Tiles";
        private const string HEADER_AUTHOR = "Author";
        private const string HEADER_SOURCE = "Source";
        private const string HEADER_BUILDING = "Building ";
        private const string HEADER_BUILDING_AMOUNT = " Amount";
        private const string HEADER_BUILDING_TYPE = " Type";
        private const string HEADER_CREDITS = "Credits";
        private const string HEADER_TIMBER = "Timber";
        private const string HEADER_BRICKS = "Bricks";
        private const string HEADER_STEEL_BEAMS = "Steel beams";
        private const string HEADER_WINDOWS = "Windows";
        private const string HEADER_REINFORCED_CONCRETE = "Reinforced concrete";
        private const string HEADER_BALANCE = "Balance";
        private const string HEADER_FARMERS_WORKFORCE = "Farmers workforce";
        private const string HEADER_WORKERS_WORKFORCE = "Workers workforce";
        private const string HEADER_ARTISANS_WORKFORCE = "Artisans workforce";
        private const string HEADER_ENGINEERS_WORKFORCE = "Engineers workforce";
        private const string HEADER_JORNALEROS_WORKFORCE = "Jornaleros workforce";
        private const string HEADER_OBREROS_WORKFORCE = "Obreros workforce";
        private const string HEADER_ATTRACTIVENESS = "Attractiveness";
        private const string HEADER_INFLUENCE = "Influence";
        private const string HEADER_PRODUCTION = "Production ";
        private const string HEADER_PRODUCTION_PER_MINUTE = " per minute";
        private const string HEADER_PRODUCTION_TYPE = " type";

        private FandomNameMappingPresets _fandomNameMappingPresets;
        private StatisticsCalculationHelper _statisticsCalculationHelper;

        //example: Potato Field - (72)
        private static readonly Regex regexFieldCount = new Regex(@"(?<begin> - \()\s*(?<value>\d*(?:[\.\,]\d*)?)\s*(?<end>\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FandomExporter()
        {
            _statisticsCalculationHelper = new StatisticsCalculationHelper();
        }

        private FandomNameMappingPresets FandomeNamePresets
        {
            get
            {
                if (_fandomNameMappingPresets == null)
                {
                    var applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    _fandomNameMappingPresets = SerializationHelper.LoadFromFile<FandomNameMappingPresets>(Path.Combine(applicationDirectory, Constants.FandomNameMappingFile));
                }

                return _fandomNameMappingPresets;
            }
        }

        public string StartExport(string layoutName, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets, bool exportUnsupportedTags)
        {
            //https://anno1800.fandom.com/wiki/Template:Production_layout
            //https://anno1800.fandom.com/wiki/Template:Production_layout/doc
            //https://anno1800.fandom.com/wiki/Category:Template_documentation

            //https://anno1800.fandom.com/wiki/Testing_page

            //TODO warn user when layout contains more than 15 building types because template only supports 1-15
            //template only supports 1-8 for "Production x Type" and "Production x per minute"
            //TODO warn user (or exit) when layout contains buildings other than Anno 1800

            var calculatedStatistics = _statisticsCalculationHelper.CalculateStatistics(placedObjects);

            var exportString = new StringBuilder(900);//best guess on minimal layout

            exportString.Append(TEMPLATE_START)
                .AppendLine(HEADER_PRODUCTION_LAYOUT)
                .Append(TEMPLATE_LINE_START).Append(HEADER_ICON).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_LAYOUT_NAME).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(layoutName)
                .Append(TEMPLATE_LINE_START).Append(HEADER_LAYOUT_IMAGE).AppendLine(TEMPLATE_ENTRY_DELIMITER);

            //add buildings
            exportString = addBuildingInfo(exportString, placedObjects, buildingPresets, wikiBuildingInfoPresets);

            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_LAYOUT_DESCRIPTION).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_SIZE).Append(TEMPLATE_ENTRY_DELIMITER).Append(calculatedStatistics.UsedAreaX).Append(TEMPLATE_SIZE_ENTRY_DELIMITER).AppendLine(calculatedStatistics.UsedAreaY.ToString())
                .Append(TEMPLATE_LINE_START).Append(HEADER_TILES).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(calculatedStatistics.UsedTiles.ToString())
                .Append(TEMPLATE_LINE_START).Append(HEADER_AUTHOR).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_SOURCE).AppendLine(TEMPLATE_ENTRY_DELIMITER);

            exportString = addProductionInfo(exportString, placedObjects, buildingPresets, wikiBuildingInfoPresets);
            exportString = addConstructionInfo(exportString, placedObjects, buildingPresets, wikiBuildingInfoPresets);
            exportString = addBalance(exportString, placedObjects, buildingPresets, wikiBuildingInfoPresets);
            exportString = addWorkforceInfo(exportString, placedObjects, buildingPresets, wikiBuildingInfoPresets);
            exportString = addInfluence(exportString, placedObjects, buildingPresets, wikiBuildingInfoPresets);
            exportString = addAttractiveness(exportString, placedObjects, buildingPresets, wikiBuildingInfoPresets);

            if (exportUnsupportedTags)
            {
                exportString = addUnsupportedEntries(exportString);
            }

            exportString.Append(TEMPLATE_END);

            return exportString.ToString();
        }

        private StringBuilder addBuildingInfo(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            var buildingCounter = 0;

            var groupedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase))
                .GroupBy(x => x.Identifier)
                .OrderBy(x => x.Count());

            foreach (var curGroup in groupedBuildings)
            {
                ++buildingCounter;

                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_BUILDING)
                    .Append(buildingCounter)
                    .Append(HEADER_BUILDING_TYPE)
                    .Append(TEMPLATE_ENTRY_DELIMITER)
                    .AppendLine(getBuildingName(curGroup.Key, buildingPresets, wikiBuildingInfoPresets));

                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_BUILDING)
                    .Append(buildingCounter)
                    .Append(HEADER_BUILDING_AMOUNT)
                    .Append(TEMPLATE_ENTRY_DELIMITER)
                    .AppendLine(curGroup.Count().ToString());
            }

            return exportString;
        }

        private string getBuildingName(string buildingIdentifier, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            var result = MAPPING_NOT_FOUND + buildingIdentifier;

            var foundFandomNameMapping = FandomeNamePresets.Names.FirstOrDefault(x => x.Identifiers.Contains(buildingIdentifier));
            if (foundFandomNameMapping != null)
            {
                result = foundFandomNameMapping.FandomName;
            }
            else
            {
                //try to get by wiki info
                var foundWikiBuildingInfo = getWikiBuildingInfo(buildingIdentifier, buildingPresets, wikiBuildingInfoPresets);
                if (!string.IsNullOrWhiteSpace(foundWikiBuildingInfo?.Icon))
                {
                    result = Path.GetFileNameWithoutExtension(foundWikiBuildingInfo.Icon);
                }
            }

            return result;
        }

        private StringBuilder addProductionInfo(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            var buildingCounter = 0;

            var supportedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase)).ToList();

            //1. get all buildings that need input
            //2. gat all buildings that have an endproduct
            //3. foreach building with input -> remove it from list of buildings with an endproduct
            //4. foreach building with an endproduct -> add it to the template

            //1. get all buildings that need input
            var buildingsWithInputProduct = new Dictionary<AnnoObject, List<InputProduct>>();
            foreach (var curBuilding in supportedBuildings)
            {
                var foundWikiBuildingInfo = getWikiBuildingInfo(curBuilding.Identifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null ||
                    foundWikiBuildingInfo.ProductionInfos == null ||
                    foundWikiBuildingInfo.ProductionInfos.InputProducts.Count == 0)
                {
                    //no Production building or no info found -> skip
                    continue;
                }

                foreach (var curInput in foundWikiBuildingInfo.ProductionInfos.InputProducts)
                {
                    if (!buildingsWithInputProduct.ContainsKey(curBuilding))
                    {
                        buildingsWithInputProduct.Add(curBuilding, new List<InputProduct> { curInput });
                    }
                    else
                    {
                        buildingsWithInputProduct[curBuilding].Add(curInput);
                    }
                }
            }

            //2. gat all buildings that have an endproduct
            var buildingsWithEndProduct = new Dictionary<AnnoObject, EndProduct>();
            foreach (var curBuilding in supportedBuildings)
            {
                var foundWikiBuildingInfo = getWikiBuildingInfo(curBuilding.Identifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null ||
                    foundWikiBuildingInfo.ProductionInfos == null ||
                    foundWikiBuildingInfo.ProductionInfos.EndProduct == null)
                {
                    //no Production building or no info found -> skip
                    continue;
                }

                buildingsWithEndProduct.Add(curBuilding, foundWikiBuildingInfo.ProductionInfos.EndProduct);
            }

            //3. foreach building with input -> remove it from list of buildings with an endproduct
            foreach (var curBuildingWithInputProduct in buildingsWithInputProduct)
            {
                foreach (var curInputProduct in curBuildingWithInputProduct.Value)
                {
                    var foundBuildingWithSameEndProduct = buildingsWithEndProduct.FirstOrDefault(x => string.Equals(x.Value.Icon, curInputProduct.Icon, StringComparison.OrdinalIgnoreCase)).Key;
                    if (foundBuildingWithSameEndProduct != null)
                    {
                        buildingsWithEndProduct.Remove(foundBuildingWithSameEndProduct);
                    }
                }
            }

            //4. foreach building with an endproduct -> add it to the template
            var groupedBuildingsWithEndProduct = buildingsWithEndProduct.GroupBy(x => x.Key.Identifier).ToList();
            foreach (var curGroup in groupedBuildingsWithEndProduct)
            {
                var buildingIdentifier = curGroup.Key;
                var buildingCount = curGroup.Count();

                var foundWikiBuildingInfo = getWikiBuildingInfo(buildingIdentifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null)
                {
                    //no Production building or no info found -> skip
                    continue;
                }

                if (buildingCount == 1 && foundWikiBuildingInfo.ProductionInfos.EndProduct.Amount <= 1)
                {
                    continue;
                }

                //add info about end product
                ++buildingCounter;

                var productionAmount = foundWikiBuildingInfo.ProductionInfos.EndProduct.Amount * buildingCount;
                var endProductIcon = Path.GetFileNameWithoutExtension(foundWikiBuildingInfo.ProductionInfos.EndProduct.Icon);

                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_PRODUCTION)
                    .Append(buildingCounter)
                    .Append(HEADER_PRODUCTION_TYPE)
                    .Append(TEMPLATE_ENTRY_DELIMITER)
                    .AppendLine(endProductIcon);

                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_PRODUCTION)
                    .Append(buildingCounter)
                    .Append(HEADER_PRODUCTION_PER_MINUTE)
                    .Append(TEMPLATE_ENTRY_DELIMITER)
                    .AppendLine(productionAmount.ToString("0.##"));

            }

            return exportString;
        }

        private StringBuilder addBalance(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            double balance = 0;

            var groupedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase)).GroupBy(x => x.Identifier);
            foreach (var curGroup in groupedBuildings)
            {
                var buildingIdentifier = curGroup.Key;
                var buildingCount = curGroup.Count();

                var foundWikiBuildingInfo = getWikiBuildingInfo(buildingIdentifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null || foundWikiBuildingInfo.MaintenanceInfos.Count == 0)
                {
                    //no info found -> skip
                    continue;
                }

                //get balance
                var foundBalanceEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.Equals("Balance", StringComparison.OrdinalIgnoreCase));
                if (foundBalanceEntry != null)
                {
                    balance = balance + (foundBalanceEntry.Value * buildingCount);
                }
            }

            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_BALANCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(balance != 0 ? balance.ToString("0") : string.Empty);

            return exportString;
        }

        private StringBuilder addConstructionInfo(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            double credits = 0;
            double timber = 0;
            double bricks = 0;
            double steelBeams = 0;
            double windows = 0;
            double reinforcedConcrete = 0;

            var groupedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase)).GroupBy(x => x.Identifier);
            foreach (var curGroup in groupedBuildings)
            {
                var buildingIdentifier = curGroup.Key;
                var buildingCount = curGroup.Count();

                var foundWikiBuildingInfo = getWikiBuildingInfo(buildingIdentifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null || foundWikiBuildingInfo.ConstructionInfos.Count == 0)
                {
                    //no info found -> skip
                    logger.Trace($"found no wiki info for identifier: {buildingIdentifier}");
                    continue;
                }

                //get credits
                var foundCreditsEntry = foundWikiBuildingInfo.ConstructionInfos.FirstOrDefault(x => x.Unit.Name.Equals("Credits", StringComparison.OrdinalIgnoreCase));
                if (foundCreditsEntry != null)
                {
                    credits += (foundCreditsEntry.Value * buildingCount);
                }

                //get timber
                var foundTimberEntry = foundWikiBuildingInfo.ConstructionInfos.FirstOrDefault(x => x.Unit.Name.Equals("Timber", StringComparison.OrdinalIgnoreCase));
                if (foundTimberEntry != null)
                {
                    timber += (foundTimberEntry.Value * buildingCount);
                }

                //get bricks
                var foundBricksEntry = foundWikiBuildingInfo.ConstructionInfos.FirstOrDefault(x => x.Unit.Name.Equals("Bricks", StringComparison.OrdinalIgnoreCase));
                if (foundBricksEntry != null)
                {
                    bricks += (foundBricksEntry.Value * buildingCount);
                }

                //get steel beams
                var foundSteelBeamsEntry = foundWikiBuildingInfo.ConstructionInfos.FirstOrDefault(x => x.Unit.Name.Equals("Steel Beams", StringComparison.OrdinalIgnoreCase));
                if (foundSteelBeamsEntry != null)
                {
                    steelBeams += (foundSteelBeamsEntry.Value * buildingCount);
                }

                //get windows
                var foundWindowsEntry = foundWikiBuildingInfo.ConstructionInfos.FirstOrDefault(x => x.Unit.Name.Equals("Windows", StringComparison.OrdinalIgnoreCase));
                if (foundWindowsEntry != null)
                {
                    windows += (foundWindowsEntry.Value * buildingCount);
                }

                //get reinforced concrete
                var foundReinforcedConcreteEntry = foundWikiBuildingInfo.ConstructionInfos.FirstOrDefault(x => x.Unit.Name.Equals("Reinforced Concrete", StringComparison.OrdinalIgnoreCase));
                if (foundReinforcedConcreteEntry != null)
                {
                    reinforcedConcrete += (foundReinforcedConcreteEntry.Value * buildingCount);
                }
            }

            //add info to template
            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_CREDITS).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(credits > 0 ? credits.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_TIMBER).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(timber > 0 ? timber.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_BRICKS).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(bricks > 0 ? bricks.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_STEEL_BEAMS).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(steelBeams > 0 ? steelBeams.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_WINDOWS).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(windows > 0 ? windows.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_REINFORCED_CONCRETE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(reinforcedConcrete > 0 ? reinforcedConcrete.ToString("0") : string.Empty);

            return exportString;
        }

        private StringBuilder addWorkforceInfo(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            double farmers = 0;
            double workers = 0;
            double artisans = 0;
            double engineers = 0;
            double jornaleros = 0;
            double obreros = 0;

            var groupedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase)).GroupBy(x => x.Identifier);
            foreach (var curGroup in groupedBuildings)
            {
                var buildingIdentifier = curGroup.Key;
                var buildingCount = curGroup.Count();

                var foundWikiBuildingInfo = getWikiBuildingInfo(buildingIdentifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null || foundWikiBuildingInfo.MaintenanceInfos.Count == 0)
                {
                    //no info found -> skip
                    logger.Trace($"found no wiki info for identifier: {buildingIdentifier}");
                    continue;
                }

                //get farmers
                var foundFarmersEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.Equals("Workforce Farmers", StringComparison.OrdinalIgnoreCase));
                if (foundFarmersEntry != null)
                {
                    farmers += (foundFarmersEntry.Value * buildingCount);
                }

                //get workers
                var foundWorkersEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.Equals("Workforce Workers", StringComparison.OrdinalIgnoreCase));
                if (foundWorkersEntry != null)
                {
                    workers += (foundWorkersEntry.Value * buildingCount);
                }

                //get artisans
                var foundArtisansEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.Equals("Workforce Artisans", StringComparison.OrdinalIgnoreCase));
                if (foundArtisansEntry != null)
                {
                    artisans += (foundArtisansEntry.Value * buildingCount);
                }

                //get engineers
                var foundEngineersEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.Equals("Workforce Engineers", StringComparison.OrdinalIgnoreCase));
                if (foundEngineersEntry != null)
                {
                    engineers += (foundEngineersEntry.Value * buildingCount);
                }

                //get jornaleros
                var foundJornalerosEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.Equals("Workforce Jornaleros", StringComparison.OrdinalIgnoreCase));
                if (foundJornalerosEntry != null)
                {
                    jornaleros += (foundJornalerosEntry.Value * buildingCount);
                }

                //get obreros
                var foundObrerosEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.Equals("Workforce Obreros", StringComparison.OrdinalIgnoreCase));
                if (foundObrerosEntry != null)
                {
                    obreros += (foundObrerosEntry.Value * buildingCount);
                }
            }

            //add info to template
            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_FARMERS_WORKFORCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(farmers != 0 ? farmers.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_WORKERS_WORKFORCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(workers != 0 ? workers.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_ARTISANS_WORKFORCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(artisans != 0 ? artisans.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_ENGINEERS_WORKFORCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(engineers != 0 ? engineers.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_JORNALEROS_WORKFORCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(jornaleros != 0 ? jornaleros.ToString("0") : string.Empty)
                .Append(TEMPLATE_LINE_START).Append(HEADER_OBREROS_WORKFORCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(obreros != 0 ? obreros.ToString("0") : string.Empty);

            return exportString;
        }

        private StringBuilder addInfluence(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            double influence = 0;

            var groupedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase)).GroupBy(x => x.Identifier);
            foreach (var curGroup in groupedBuildings)
            {
                var buildingIdentifier = curGroup.Key;
                var buildingCount = curGroup.Count();

                var foundWikiBuildingInfo = getWikiBuildingInfo(buildingIdentifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null || foundWikiBuildingInfo.MaintenanceInfos.Count == 0)
                {
                    //no info found -> skip
                    logger.Trace($"found no wiki info for identifier: {buildingIdentifier}");
                    continue;
                }

                //get influence
                var foundInfluenceEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.StartsWith("Influence", StringComparison.OrdinalIgnoreCase));
                if (foundInfluenceEntry != null)
                {
                    influence = influence + (foundInfluenceEntry.Value * buildingCount);
                }
            }

            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_INFLUENCE).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(influence != 0 ? influence.ToString("0") : string.Empty);

            return exportString;
        }

        private StringBuilder addAttractiveness(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            double attractiveness = 0;

            var groupedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase)).GroupBy(x => x.Identifier);
            foreach (var curGroup in groupedBuildings)
            {
                var buildingIdentifier = curGroup.Key;
                var buildingCount = curGroup.Count();

                var foundWikiBuildingInfo = getWikiBuildingInfo(buildingIdentifier, buildingPresets, wikiBuildingInfoPresets);
                if (foundWikiBuildingInfo == null || foundWikiBuildingInfo.MaintenanceInfos.Count == 0)
                {
                    //no info found -> skip
                    logger.Trace($"found no wiki info for identifier: {buildingIdentifier}");
                    continue;
                }

                //get attractiveness
                var foundAttractivenessEntry = foundWikiBuildingInfo.MaintenanceInfos.FirstOrDefault(x => x.Unit.Name.StartsWith("Attractiveness", StringComparison.OrdinalIgnoreCase));
                if (foundAttractivenessEntry != null)
                {
                    attractiveness = attractiveness + (foundAttractivenessEntry.Value * buildingCount);
                }
            }

            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_ATTRACTIVENESS).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(attractiveness != 0 ? attractiveness.ToString("0") : string.Empty);

            return exportString;
        }

        private WikiBuildingInfo getWikiBuildingInfo(string buildingIdentifier, BuildingPresets buildingPresets, WikiBuildingInfoPresets wikiBuildingInfoPresets)
        {
            WikiBuildingInfo result = null;

            var foundPresetBuilding = buildingPresets.Buildings.FirstOrDefault(x => x.Identifier.Equals(buildingIdentifier, StringComparison.OrdinalIgnoreCase));
            var buildingName = foundPresetBuilding?.Localization["eng"];
            var buildingFaction = foundPresetBuilding?.Faction;
            var buildingRegion = WorldRegion.OldWorld;
            if (buildingFaction?.Contains("Obreros") == true || buildingFaction?.Contains("Jornaleros") == true)
            {
                buildingRegion = WorldRegion.NewWorld;
            }

            if (string.IsNullOrWhiteSpace(buildingName))
            {
                //TODO error?
                logger.Warn($"found no building name for identifier: {buildingIdentifier}");
                return result;
            }

            //try to find by name
            result = wikiBuildingInfoPresets.Infos.FirstOrDefault(x => x.Name.Equals(buildingName, StringComparison.OrdinalIgnoreCase) && x.Region == buildingRegion);
            if (result == null)
            {
                //Is it a farm with field info? (e.g. "Potato Farm - (72)")
                var matchFieldCount = regexFieldCount.Match(buildingName);
                if (matchFieldCount.Success)
                {
                    //strip field info and search again
                    var strippedBuildingName = buildingName.Replace(matchFieldCount.Value, string.Empty).Trim();
                    result = wikiBuildingInfoPresets.Infos.FirstOrDefault(x => x.Name.Equals(strippedBuildingName, StringComparison.OrdinalIgnoreCase) && x.Region == buildingRegion);
                }
            }

            return result;
        }

        private StringBuilder addUnsupportedEntries(StringBuilder exportString)
        {
            return exportString;
        }
    }
}
