using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FandomParser.Core;
using FandomParser.Core.Models;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Models;
using NLog;

namespace InfoboxParser.Parser
{
    internal class ParserOldAndNewWorld : IParser
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;
        private readonly ISpecialBuildingNameHelper _specialBuildingNameHelper;
        private readonly IRegionHelper _regionHelper;
        private readonly List<string> possibleRegions;

        //TODO support edge cases in regex like "|Input 1 Amount Electricity (OW) = 1.79769313486232E+308"

        //|Building Icon      = Icon palace module.png        
        private static readonly Regex regexBuildingIcon = new Regex(@"\|Building Icon\s*=\s*(?<icon>(\w*\s*['`´]*)+([.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        //|Building Type (OW)     = Institution
        //|Building Type (NW)     = Institution
        private static readonly Regex regexBuildingType = new Regex(@"\|Building Type\s*\((?<region>\w{2})\s*\)\s*=\s*(?<typeName>(\w*\s*)+([.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        //|Produces Amount (OW)   = 1
        private static readonly Regex regexProducesAmount = new Regex(@"\|Produces Amount\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Produces Amount Electricity (OW)   = 1
        private static readonly Regex regexProducesAmountElectricity = new Regex(@"\|Produces Amount Electricity\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Produces Icon (OW)     = Bricks.png
        private static readonly Regex regexProducesIcon = new Regex(@"\|Produces Icon\s*\((?<region>\w{2})\s*\)\s*=\s*(?<fileName>(\w*\s*)+([.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        //|Input 1 Amount (OW) = 2
        private static readonly Regex regexInputAmount = new Regex(@"\|Input\s*(?<counter>[1-9]+)\s*Amount\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Input 1 Amount Electricity (OW) = 4
        private static readonly Regex regexInputAmountElectricity = new Regex(@"\|Input\s*(?<counter>[1-9]+)\s*Amount Electricity\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Input 1 Icon (OW) = Potato.png
        private static readonly Regex regexInputIcon = new Regex(@"\|Input\s*(?<counter>[1-9]+)\s*Icon\s*\((?<region>\w{2})\s*\)\s*=\s*(?<fileName>(\w*\s*)+([.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        //|Supplies 1 Type (OW) = Farmers
        private static readonly Regex regexSupplyType = new Regex(@"\|Supplies\s*(?<counter>[1-9]+)\s*Type\s*\((?<region>\w{2})\s*\)\s*=\s*(?<typeName>(\w*\s*)+([.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Supplies 1 Amount (OW) = 2
        private static readonly Regex regexSupplyAmount = new Regex(@"\|Supplies\s*(?<counter>[1-9]+)\s*Amount\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Supplies 1 Amount Electricity (OW) = 4
        private static readonly Regex regexSupplyAmountElectricity = new Regex(@"\|Supplies\s*(?<counter>[1-9]+)\s*Amount Electricity\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        //|Unlock Condition 1 Type (OW) = Farmers
        private static readonly Regex regexUnlockConditionType = new Regex(@"\|Unlock Condition\s*(?<counter>[1-9]+)\s*Type\s*\((?<region>\w{2})\s*\)\s*=\s*(?<typeName>(\w*\s*)+([.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Unlock Condition 1 Amount (OW) = 100
        private static readonly Regex regexUnlockConditionAmount = new Regex(@"\|Unlock Condition\s*(?<counter>[1-9]+)\s*Amount\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        //|Credits (OW) = 15000
        private static readonly Regex regexConstructionCredits = new Regex(@"\|Credits\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Timber (OW) = 12
        private static readonly Regex regexConstructionTimber = new Regex(@"\|Timber\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Bricks (OW) = 3
        private static readonly Regex regexConstructionBricks = new Regex(@"\|Bricks\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Steel Beams (OW) = 8
        private static readonly Regex regexConstructionSteelBeams = new Regex(@"\|Steel Beams\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Windows (OW) = 2
        private static readonly Regex regexConstructionWindows = new Regex(@"\|Windows\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Concrete (OW) = 15
        private static readonly Regex regexConstructionConcrete = new Regex(@"\|Concrete\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Weapons (OW) = 20
        private static readonly Regex regexConstructionWeapons = new Regex(@"\|Weapons\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        //|Advanced Weapons (OW) = 25
        private static readonly Regex regexConstructionAdvancedWeapons = new Regex(@"\|Advanced Weapons\s*\((?<region>\w{2})\s*\)\s*=\s*(?<value>[0-9]*([.,][0-9]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        private CultureInfo cultureForParsing;

        public ParserOldAndNewWorld(ICommons commons, ISpecialBuildingNameHelper specialBuildingNameHelperToUse, IRegionHelper regionHelperToUse)
        {
            _commons = commons;
            _specialBuildingNameHelper = specialBuildingNameHelperToUse;
            _regionHelper = regionHelperToUse;

            possibleRegions = new List<string> { "OW", "NW" };

            //all numbers in the wiki are entered with comma (,) e.g. "42,21", so we need to use a specific culture for parsing (https://anno1800.fandom.com/wiki/Cannery)
            cultureForParsing = new CultureInfo("de-DE");
        }

        public List<IInfobox> GetInfobox(string wikiText)
        {
            if (string.IsNullOrWhiteSpace(wikiText))
            {
                return null;
            }

            var result = new List<IInfobox>();

            //same for both
            var buildingName = getBuildingName(wikiText);
            if (string.IsNullOrWhiteSpace(buildingName))
            {
            }

            //parse wikitext 2 times. first time with parameter "OW", second time with parameter "NW"
            foreach (var curRegion in possibleRegions)
            {
                var buildingType = getBuildingType(wikiText, curRegion);
                if (buildingType == BuildingType.Unknown)
                {

                }

                var productionInfo = getProductionInfo(wikiText, curRegion);
                if (productionInfo == null && buildingType == BuildingType.Production)
                {

                }

                var buildingIcon = getBuildingIcon(wikiText);
                if (string.IsNullOrWhiteSpace(buildingIcon))
                {
                }

                var supplyInfo = getSupplyInfo(wikiText, curRegion);
                var unlockInfo = getUnlockInfo(wikiText, curRegion);
                var constructionInfo = getConstructionInfo(wikiText, curRegion);

                buildingName = _specialBuildingNameHelper.CheckSpecialBuildingName(buildingName);

                var region = _regionHelper.GetRegion(curRegion);

                var parsedInfobox = new Infobox
                {
                    Name = buildingName,
                    Icon = buildingIcon,
                    Type = buildingType,
                    ProductionInfos = productionInfo,
                    SupplyInfos = supplyInfo,
                    UnlockInfos = unlockInfo,
                    Region = region,
                    ConstructionInfos = constructionInfo
                };

                result.Add(parsedInfobox);
            }

            return result;
        }

        private string getBuildingName(string infobox)
        {
            var result = string.Empty;

            if (!infobox.Contains("|Title"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    if (curLine.StartsWith("|Title", StringComparison.OrdinalIgnoreCase))
                    {
                        result = curLine.Replace("|Title", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();
                        break;
                    }
                }
            }

            return result;
        }

        private BuildingType getBuildingType(string infobox, string regionToParse)
        {
            var result = BuildingType.Unknown;

            //short circuit infoboxes without building type info
            if (!infobox.Contains("|Building Type"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    if (curLine.StartsWith("|Building Type", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchBuildingType = regexBuildingType.Match(curLine);
                        if (matchBuildingType.Success)
                        {
                            var matchedRegion = matchBuildingType.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            //handle entry with no value e.g. "|Building Type (OW)     = "
                            var matchedTypeName = matchBuildingType.Groups["typeName"].Value;
                            if (string.IsNullOrWhiteSpace(matchedTypeName))
                            {
                                continue;
                            }

                            matchedTypeName = matchedTypeName.Replace(" ", string.Empty).Trim();

                            if (Enum.TryParse(matchedTypeName, ignoreCase: true, out BuildingType parsedBuildingType))
                            {
                                result = parsedBuildingType;
                            }

                            break;
                        }

                        break;
                    }
                }
            }

            return result;
        }

        private ProductionInfo getProductionInfo(string infobox, string regionToParse)
        {
            ProductionInfo result = null;

            //short circuit infoboxes without production info
            if (!infobox.Contains("|Produces Amount"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    if (curLine.StartsWith("|Produces", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchAmount = regexProducesAmount.Match(curLine);
                        if (matchAmount.Success)
                        {
                            var matchedRegion = matchAmount.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            //handle entry with no value e.g. "|Input 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedProductionAmount))
                            {
                                throw new Exception("could not find value for input");
                            }

                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            if (result.EndProduct == null)
                            {
                                result.EndProduct = new EndProduct();
                            }

                            result.EndProduct.Amount = parsedProductionAmount;

                            continue;
                        }

                        var matchAmountElectricity = regexProducesAmountElectricity.Match(curLine);
                        if (matchAmountElectricity.Success)
                        {
                            var matchedRegion = matchAmountElectricity.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            //handle entry with no value e.g. "|Input 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedProductionAmountElectricity))
                            {
                                throw new Exception("could not find value for input");
                            }

                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            if (result.EndProduct == null)
                            {
                                result.EndProduct = new EndProduct();
                            }

                            result.EndProduct.AmountElectricity = parsedProductionAmountElectricity;

                            continue;
                        }

                        var matchIcon = regexProducesIcon.Match(curLine);
                        if (matchIcon.Success)
                        {
                            var matchedRegion = matchIcon.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            //handle entry with no value e.g. "|Input 1 Icon = "
                            var matchedFileName = matchIcon.Groups["fileName"].Value;
                            if (string.IsNullOrWhiteSpace(matchedFileName))
                            {
                                continue;
                            }

                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            if (result.EndProduct == null)
                            {
                                result.EndProduct = new EndProduct();
                            }

                            result.EndProduct.Icon = matchedFileName;

                            continue;
                        }
                    }
                    else if (curLine.StartsWith("|Input ", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchAmount = regexInputAmount.Match(curLine);
                        if (matchAmount.Success)
                        {
                            var matchedRegion = matchAmount.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var inputValue))
                            {
                                throw new Exception("could not find value for input");
                            }

                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            var foundInputProduct = result.InputProducts.FirstOrDefault(x => x.Order == counter);
                            if (foundInputProduct == null)
                            {
                                foundInputProduct = new InputProduct
                                {
                                    Order = counter
                                };

                                result.InputProducts.Add(foundInputProduct);
                            }

                            foundInputProduct.Amount = inputValue;

                            continue;
                        }

                        var matchAmountElectricity = regexInputAmountElectricity.Match(curLine);
                        if (matchAmountElectricity.Success)
                        {
                            var matchedRegion = matchAmountElectricity.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchAmountElectricity.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var inputValue))
                            {
                                throw new Exception("could not find value for input");
                            }

                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            var foundInputProduct = result.InputProducts.FirstOrDefault(x => x.Order == counter);
                            if (foundInputProduct == null)
                            {
                                foundInputProduct = new InputProduct
                                {
                                    Order = counter
                                };

                                result.InputProducts.Add(foundInputProduct);
                            }

                            foundInputProduct.AmountElectricity = inputValue;

                            continue;
                        }

                        var matchIcon = regexInputIcon.Match(curLine);
                        if (matchIcon.Success)
                        {
                            var matchedRegion = matchIcon.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchIcon.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 1 Icon = "
                            var matchedFileName = matchIcon.Groups["fileName"].Value;
                            if (string.IsNullOrWhiteSpace(matchedFileName))
                            {
                                continue;
                            }

                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            var foundInputProduct = result.InputProducts.FirstOrDefault(x => x.Order == counter);
                            if (foundInputProduct == null)
                            {
                                foundInputProduct = new InputProduct
                                {
                                    Order = counter
                                };

                                result.InputProducts.Add(foundInputProduct);
                            }

                            foundInputProduct.Icon = matchedFileName;

                            continue;
                        }
                    }
                }
            }

            if (result != null)
            {
                //order by number from infobox
                result.InputProducts = result.InputProducts.OrderBy(x => x.Order).ToList();
            }

            return result;
        }

        private SupplyInfo getSupplyInfo(string infobox, string regionToParse)
        {
            SupplyInfo result = null;

            //short circuit infoboxes without supply info
            if (!infobox.Contains("|Supplies "))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    if (curLine.StartsWith("|Supplies ", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchAmount = regexSupplyAmount.Match(curLine);
                        if (matchAmount.Success)
                        {
                            var matchedRegion = matchAmount.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var supplyValue))
                            {
                                throw new Exception("could not find value for input");
                            }

                            if (result == null)
                            {
                                result = new SupplyInfo();
                            }

                            var foundSupplyEntry = result.SupplyEntries.FirstOrDefault(x => x.Order == counter);
                            if (foundSupplyEntry == null)
                            {
                                foundSupplyEntry = new SupplyEntry
                                {
                                    Order = counter
                                };

                                result.SupplyEntries.Add(foundSupplyEntry);
                            }

                            foundSupplyEntry.Amount = supplyValue;

                            continue;
                        }

                        var matchAmountElectricity = regexSupplyAmountElectricity.Match(curLine);
                        if (matchAmountElectricity.Success)
                        {
                            var matchedRegion = matchAmountElectricity.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchAmountElectricity.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var supplyValue))
                            {
                                throw new Exception("could not find value for input");
                            }

                            if (result == null)
                            {
                                result = new SupplyInfo();
                            }

                            var foundSupplyEntry = result.SupplyEntries.FirstOrDefault(x => x.Order == counter);
                            if (foundSupplyEntry == null)
                            {
                                foundSupplyEntry = new SupplyEntry
                                {
                                    Order = counter
                                };

                                result.SupplyEntries.Add(foundSupplyEntry);
                            }

                            foundSupplyEntry.AmountElectricity = supplyValue;

                            continue;
                        }

                        var matchType = regexSupplyType.Match(curLine);
                        if (matchType.Success)
                        {
                            var matchedRegion = matchType.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchType.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 1 Type = "
                            var matchedTypeName = matchType.Groups["typeName"].Value.Trim();
                            if (string.IsNullOrWhiteSpace(matchedTypeName))
                            {
                                continue;
                            }

                            if (result == null)
                            {
                                result = new SupplyInfo();
                            }

                            var foundSupplyEntry = result.SupplyEntries.FirstOrDefault(x => x.Order == counter);
                            if (foundSupplyEntry == null)
                            {
                                foundSupplyEntry = new SupplyEntry
                                {
                                    Order = counter
                                };

                                result.SupplyEntries.Add(foundSupplyEntry);
                            }

                            foundSupplyEntry.Type = matchedTypeName;

                            continue;
                        }
                    }
                }
            }

            if (result != null)
            {
                //order by number from infobox
                result.SupplyEntries = result.SupplyEntries.OrderBy(x => x.Order).ToList();
            }

            return result;
        }

        private UnlockInfo getUnlockInfo(string infobox, string regionToParse)
        {
            UnlockInfo result = null;

            //short circuit infoboxes without supply info
            if (!infobox.Contains("|Unlock "))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    if (curLine.StartsWith("|Unlock Condition ", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchAmount = regexUnlockConditionAmount.Match(curLine);
                        if (matchAmount.Success)
                        {
                            var matchedRegion = matchAmount.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Unlock Condition 1 Amount = "
                            var matchedValue = matchAmount.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var conditionValue))
                            {
                                throw new Exception("could not find value for input");
                            }

                            if (result == null)
                            {
                                result = new UnlockInfo();
                            }

                            var foundUnlockCondition = result.UnlockConditions.FirstOrDefault(x => x.Order == counter);
                            if (foundUnlockCondition == null)
                            {
                                foundUnlockCondition = new UnlockCondition
                                {
                                    Order = counter
                                };

                                result.UnlockConditions.Add(foundUnlockCondition);
                            }

                            foundUnlockCondition.Amount = conditionValue;

                            continue;
                        }

                        var matchType = regexUnlockConditionType.Match(curLine);
                        if (matchType.Success)
                        {
                            var matchedRegion = matchType.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            var matchedCounter = matchType.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Unlock Condition 1 Type = "
                            var matchedTypeName = matchType.Groups["typeName"].Value.Trim();
                            if (string.IsNullOrWhiteSpace(matchedTypeName))
                            {
                                continue;
                            }

                            if (result == null)
                            {
                                result = new UnlockInfo();
                            }

                            var foundUnlockCondition = result.UnlockConditions.FirstOrDefault(x => x.Order == counter);
                            if (foundUnlockCondition == null)
                            {
                                foundUnlockCondition = new UnlockCondition
                                {
                                    Order = counter
                                };

                                result.UnlockConditions.Add(foundUnlockCondition);
                            }

                            foundUnlockCondition.Type = matchedTypeName;

                            continue;
                        }
                    }
                }
            }

            if (result != null)
            {
                //order by number from infobox
                result.UnlockConditions = result.UnlockConditions.OrderBy(x => x.Order).ToList();
            }

            return result;
        }

        private string getBuildingIcon(string infobox)
        {
            var result = string.Empty;

            //short circuit infoboxes without building icon info
            if (!infobox.Contains("|Building Icon"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    var matchAmount = regexBuildingIcon.Match(curLine);
                    if (matchAmount.Success)
                    {
                        result = matchAmount.Groups["icon"].Value;
                    }
                }
            }

            return result;
        }

        private List<ConstructionInfo> getConstructionInfo(string infobox, string regionToParse)
        {
            List<ConstructionInfo> result = new List<ConstructionInfo>();

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    var matchCredits = regexConstructionCredits.Match(curLine);
                    if (matchCredits.Success)
                    {
                        var matchedRegion = matchCredits.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchCredits.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Credits: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Credits"
                            }
                        });
                        continue;
                    }

                    var matchTimber = regexConstructionTimber.Match(curLine);
                    if (matchTimber.Success)
                    {
                        var matchedRegion = matchTimber.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchTimber.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Timber: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Timber"
                            }
                        });
                        continue;
                    }

                    var matchBricks = regexConstructionBricks.Match(curLine);
                    if (matchBricks.Success)
                    {
                        var matchedRegion = matchBricks.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchBricks.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Bricks: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Bricks"
                            }
                        });
                        continue;
                    }

                    var matchSteelBeams = regexConstructionSteelBeams.Match(curLine);
                    if (matchSteelBeams.Success)
                    {
                        var matchedRegion = matchSteelBeams.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchSteelBeams.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Steel Beams: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Steel Beams"
                            }
                        });
                        continue;
                    }

                    var matchWindows = regexConstructionWindows.Match(curLine);
                    if (matchWindows.Success)
                    {
                        var matchedRegion = matchWindows.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchWindows.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Windows: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Windows"
                            }
                        });
                        continue;
                    }

                    var matchConcrete = regexConstructionConcrete.Match(curLine);
                    if (matchConcrete.Success)
                    {
                        var matchedRegion = matchConcrete.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchConcrete.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Concrete: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Reinforced Concrete"
                            }
                        });
                        continue;
                    }

                    var matchWeapons = regexConstructionWeapons.Match(curLine);
                    if (matchWeapons.Success)
                    {
                        var matchedRegion = matchWeapons.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchWeapons.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Weapons: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Weapons"
                            }
                        });
                        continue;
                    }

                    var matchAdvancedWeapons = regexConstructionAdvancedWeapons.Match(curLine);
                    if (matchAdvancedWeapons.Success)
                    {
                        var matchedRegion = matchAdvancedWeapons.Groups["region"].Value;
                        if (!regionToParse.Equals(matchedRegion))
                        {
                            continue;
                        }

                        var foundValue = matchAdvancedWeapons.Groups["value"].Value;
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            continue;
                        }

                        var couldParse = double.TryParse(foundValue, NumberStyles.Number, cultureForParsing, out var parsedValue);
                        if (!couldParse)
                        {
                            logger.Warn($"could not parse Advanced Weapons: \"{foundValue}\"");
                            continue;
                        }

                        result.Add(new ConstructionInfo
                        {
                            Value = parsedValue,
                            Unit = new CostUnit
                            {
                                Type = CostUnitType.InfoIcon,
                                Name = "Advanced Weapons"
                            }
                        });
                        continue;
                    }
                }
            }

            return result;
        }
    }
}
