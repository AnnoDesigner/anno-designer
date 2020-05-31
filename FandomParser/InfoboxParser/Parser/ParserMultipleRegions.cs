﻿using System;
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
    internal class ParserMultipleRegions : IParserMultipleRegions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;
        private readonly ISpecialBuildingNameHelper _specialBuildingNameHelper;
        private readonly IRegionHelper _regionHelper;

        //TODO support edge cases in regex like "|Input 1 Amount Electricity A = 1.79769313486232E+308"

        //|Title A      = Building
        private static readonly Regex regexBuildingName = new Regex(@"(?<begin>\|Title)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<buildingName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Building Icon      = Icon palace module.png
        private static readonly Regex regexBuildingIcon = new Regex(@"(?<begin>\|Building Icon)\s*(?<equalSign>[=])\s*(?<icon>(?:\w*\s*['`´]*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Tab A               = Old World
        private static readonly Regex regexRegionName = new Regex(@"(?<begin>\|Tab)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<regionName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Building Type A     = Institution
        private static readonly Regex regexBuildingType = new Regex(@"(?<begin>\|Building Type)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<typeName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Produces Amount A   = 1
        private static readonly Regex regexProducesAmount = new Regex(@"(?<begin>\|Produces Amount)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Produces Amount Electricity A   = 1
        private static readonly Regex regexProducesAmountElectricity = new Regex(@"(?<begin>\|Produces Amount Electricity)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Produces Icon A     = Bricks.png
        private static readonly Regex regexProducesIcon = new Regex(@"(?<begin>\|Produces Icon)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<fileName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Input 1 Amount A = 2
        private static readonly Regex regexInputAmount = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Amount)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Input 1 Amount Electricity A = 4
        private static readonly Regex regexInputAmountElectricity = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Amount Electricity)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Input 1 Icon A = Potato.png
        private static readonly Regex regexInputIcon = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Icon)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<fileName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Supplies 1 Type A = Farmers
        private static readonly Regex regexSupplyType = new Regex(@"(?<begin>\|Supplies)\s*(?<counter>\d+)\s*(?<end>Type)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<typeName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Supplies 1 Amount A = 2
        private static readonly Regex regexSupplyAmount = new Regex(@"(?<begin>\|Supplies)\s*(?<counter>\d+)\s*(?<end>Amount)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Supplies 1 Amount Electricity A = 4
        private static readonly Regex regexSupplyAmountElectricity = new Regex(@"(?<begin>\|Supplies)\s*(?<counter>\d+)\s*(?<end>Amount Electricity)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Unlock Condition 1 Type A = Farmers
        private static readonly Regex regexUnlockConditionType = new Regex(@"(?<begin>\|Unlock Condition)\s*(?<counter>\d+)\s*(?<end>Type)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<typeName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Unlock Condition 1 Amount A = 100
        private static readonly Regex regexUnlockConditionAmount = new Regex(@"(?<begin>\|Unlock Condition)\s*(?<counter>\d+)\s*(?<end>Amount)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Credits (OW) = 15000
        private static readonly Regex regexConstructionCredits = new Regex(@"(?<begin>\|Credits)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Timber (OW) = 12
        private static readonly Regex regexConstructionTimber = new Regex(@"(?<begin>\|Timber)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Bricks (OW) = 3
        private static readonly Regex regexConstructionBricks = new Regex(@"(?<begin>\|Bricks)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Steel Beams (OW) = 8
        private static readonly Regex regexConstructionSteelBeams = new Regex(@"(?<begin>\|Steel Beams)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Windows (OW) = 2
        private static readonly Regex regexConstructionWindows = new Regex(@"(?<begin>\|Windows)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Concrete (OW) = 15
        private static readonly Regex regexConstructionConcrete = new Regex(@"(?<begin>\|Concrete)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Weapons (OW) = 20
        private static readonly Regex regexConstructionWeapons = new Regex(@"(?<begin>\|Weapons)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Advanced Weapons (OW) = 25
        private static readonly Regex regexConstructionAdvancedWeapons = new Regex(@"(?<begin>\|Advanced Weapons)\s*(?<region>\w{1})\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private CultureInfo cultureForParsing;

        public ParserMultipleRegions(ICommons commonsToUse, ISpecialBuildingNameHelper specialBuildingNameHelperToUse, IRegionHelper regionHelperToUse)
        {
            _commons = commonsToUse;
            _specialBuildingNameHelper = specialBuildingNameHelperToUse;
            _regionHelper = regionHelperToUse;

            //all numbers in the wiki are entered with comma (,) e.g. "42,21", so we need to use a specific culture for parsing (https://anno1800.fandom.com/wiki/Cannery)
            cultureForParsing = new CultureInfo("de-DE");
        }

        public List<IInfobox> GetInfobox(string wikiText, List<string> possibleRegions)
        {
            if (string.IsNullOrWhiteSpace(wikiText))
            {
                return null;
            }

            var result = new List<IInfobox>();

            //Parse wikitext multiple times. Once for every possible region.
            foreach (var curRegion in possibleRegions)
            {
                var buildingName = getBuildingName(wikiText, curRegion);
                if (string.IsNullOrWhiteSpace(buildingName))
                {
                }

                var buildingIcon = getBuildingIcon(wikiText);
                if (string.IsNullOrWhiteSpace(buildingIcon))
                {
                }

                var buildingType = getBuildingType(wikiText, curRegion);
                if (buildingType == BuildingType.Unknown)
                {

                }

                var productionInfo = getProductionInfo(wikiText, curRegion);
                if (productionInfo == null && buildingType == BuildingType.Production)
                {

                }

                var supplyInfo = getSupplyInfo(wikiText, curRegion);
                var unlockInfo = getUnlockInfo(wikiText, curRegion);
                var constructionInfo = getConstructionInfo(wikiText, curRegion);

                buildingName = _specialBuildingNameHelper.CheckSpecialBuildingName(buildingName);

                var parsedRegion = getRegionName(wikiText, curRegion);
                var region = _regionHelper.GetRegion(parsedRegion);

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

        private string getBuildingName(string infobox, string regionToParse)
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
                        var matchBuildingName = regexBuildingName.Match(curLine);
                        if (matchBuildingName.Success)
                        {
                            var matchedRegion = matchBuildingName.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            //handle entry with no value e.g. "|Title A     = "
                            var parsedBuildingName = matchBuildingName.Groups["buildingName"].Value;
                            if (string.IsNullOrWhiteSpace(parsedBuildingName))
                            {
                                continue;
                            }

                            result = parsedBuildingName;


                            break;
                        }

                        break;
                    }
                }
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

        private string getRegionName(string infobox, string regionToParse)
        {
            var result = string.Empty;

            if (!infobox.Contains("|Tab"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    if (curLine.StartsWith("|Tab", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchRegionName = regexRegionName.Match(curLine);
                        if (matchRegionName.Success)
                        {
                            var matchedRegion = matchRegionName.Groups["region"].Value;
                            if (!regionToParse.Equals(matchedRegion))
                            {
                                continue;
                            }

                            //handle entry with no value e.g. "|Tab A     = "
                            var parsedRegionName = matchRegionName.Groups["regionName"].Value;
                            if (string.IsNullOrWhiteSpace(parsedRegionName))
                            {
                                continue;
                            }

                            result = parsedRegionName;


                            break;
                        }

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
