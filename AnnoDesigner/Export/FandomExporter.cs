using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Presets;
using AnnoDesigner.viewmodel;

namespace AnnoDesigner.Export
{
    public class FandomExporter
    {
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
        //currently no support
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

        private FandomNameMappingPresets FandomeNamePresets
        {
            get { return _fandomNameMappingPresets ?? (_fandomNameMappingPresets = DataIO.LoadFromFile<FandomNameMappingPresets>(Path.Combine(App.ApplicationPath, Constants.FandomNameMappingFile))); }
        }

        public void StartExport(StatisticsViewModel statisticsViewModel, List<AnnoObject> placedObjects, BuildingPresets buildingPresets, bool exportUnsupportedTags)
        {
            //https://anno1800.fandom.com/wiki/Template:Production_layout
            //https://anno1800.fandom.com/wiki/Template:Production_layout/doc
            //https://anno1800.fandom.com/wiki/Category:Template_documentation
            //TODO warn user when layout contains more than 15 building types because template only supports 1-15
            //TODO warn user (or exit) when layout contains buildings other than Anno 1800

            var exportString = new StringBuilder(900);//best guess on minimal layout

            exportString.Append(TEMPLATE_START)
                .AppendLine(HEADER_PRODUCTION_LAYOUT)
                .Append(TEMPLATE_LINE_START).Append(HEADER_ICON).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_LAYOUT_NAME).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_LAYOUT_IMAGE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_LAYOUT_DESCRIPTION).AppendLine(TEMPLATE_ENTRY_DELIMITER);

            //add buildings
            exportString = addBuildingInfo(exportString, placedObjects, buildingPresets);

            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_SIZE).Append(TEMPLATE_ENTRY_DELIMITER).Append(statisticsViewModel.UsedAreaX).Append(TEMPLATE_SIZE_ENTRY_DELIMITER).AppendLine(statisticsViewModel.UsedAreaY.ToString())
                .Append(TEMPLATE_LINE_START).Append(HEADER_TILES).Append(TEMPLATE_ENTRY_DELIMITER).AppendLine(statisticsViewModel.UsedTiles.ToString())
                .Append(TEMPLATE_LINE_START).Append(HEADER_AUTHOR).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_SOURCE).AppendLine(TEMPLATE_ENTRY_DELIMITER);

            if (exportUnsupportedTags)
            {
                exportString = addUnsupportedEntries(exportString);
            }

            exportString.Append(TEMPLATE_END);

            var temp = exportString.ToString();
            File.WriteAllText("fandom_export.txt", temp);
        }

        private StringBuilder addBuildingInfo(StringBuilder exportString, List<AnnoObject> placedObjects, BuildingPresets buildingPresets)
        {
            var buildingCounter = 0;

            var groupedBuildings = placedObjects.Where(x => !x.Road && !string.IsNullOrWhiteSpace(x.Identifier) && !x.Identifier.Equals("Unknown Object", StringComparison.OrdinalIgnoreCase)).GroupBy(x => x.Identifier);

            foreach (var curGroup in groupedBuildings)
            {
                ++buildingCounter;

                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_BUILDING)
                    .Append(buildingCounter)
                    .Append(HEADER_BUILDING_TYPE)
                    .Append(TEMPLATE_ENTRY_DELIMITER)
                    .AppendLine(getBuildingName(curGroup.Key));

                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_BUILDING)
                    .Append(buildingCounter)
                    .Append(HEADER_BUILDING_AMOUNT)
                    .Append(TEMPLATE_ENTRY_DELIMITER)
                    .AppendLine(curGroup.Count().ToString());
            }

            return exportString;
        }

        private string getBuildingName(string buildingName)
        {
            var result = MAPPING_NOT_FOUND + buildingName;

            var foundFandomNameMapping = FandomeNamePresets.Names.FirstOrDefault(x => x.Identifiers.Contains(buildingName));
            if (foundFandomNameMapping != null)
            {
                result = foundFandomNameMapping.FandomName;
            }

            return result;
        }

        private StringBuilder addUnsupportedEntries(StringBuilder exportString)
        {
            exportString.Append(TEMPLATE_LINE_START).Append(HEADER_CREDITS).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_TIMBER).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_BRICKS).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_STEEL_BEAMS).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_WINDOWS).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_REINFORCED_CONCRETE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_BALANCE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_FARMERS_WORKFORCE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_WORKERS_WORKFORCE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_ARTISANS_WORKFORCE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_ENGINEERS_WORKFORCE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_JORNALEROS_WORKFORCE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_OBREROS_WORKFORCE).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_ATTRACTIVENESS).AppendLine(TEMPLATE_ENTRY_DELIMITER)
                .Append(TEMPLATE_LINE_START).Append(HEADER_INFLUENCE).AppendLine(TEMPLATE_ENTRY_DELIMITER);

            //https://anno1800.fandom.com/wiki/Template:Production_layout
            //just add 3 entries
            for (int i = 1; i <= 4; i++)
            {
                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_PRODUCTION)
                    .Append(i)
                    .Append(HEADER_PRODUCTION_PER_MINUTE)
                    .AppendLine(TEMPLATE_ENTRY_DELIMITER);

                exportString.Append(TEMPLATE_LINE_START)
                    .Append(HEADER_PRODUCTION)
                    .Append(i)
                    .Append(HEADER_PRODUCTION_TYPE)
                    .AppendLine(TEMPLATE_ENTRY_DELIMITER);
            }

            return exportString;
        }
    }
}
