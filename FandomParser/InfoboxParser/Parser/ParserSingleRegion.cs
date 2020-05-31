using System;
using System.Collections.Generic;
using System.Drawing;
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
    internal class ParserSingleRegion : IParser
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;
        private readonly ITitleParserSingle _titleParserSingle;

        //TODO support edge cases in regex like "|Input 1 Amount Electricity = 1.79769313486232E+308"

        //|Building Icon      = Icon palace module.png
        private static readonly Regex regexBuildingIcon = new Regex(@"(?<begin>\|Building Icon)\s*(?<equalSign>[=])\s*(?<icon>(?:\w*\s*['`´]*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Building Size = 3x13
        private static readonly Regex regexBuildingSize = new Regex(@"(?<begin>\|Building Size)\s*(?<equalSign>[=])\s*(?<value>\d*\s*(?:[x]\s*\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Input 1 Amount = 2
        private static readonly Regex regexInputAmount = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Amount)\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Input 1 Amount Electricity = 4        
        private static readonly Regex regexInputAmountElectricity = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Amount Electricity)\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Input 1 Icon = Potato.png
        private static readonly Regex regexInputIcon = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Icon)\s*(?<equalSign>[=])\s*(?<fileName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Supplies 1 Type = Farmers
        private static readonly Regex regexSupplyType = new Regex(@"(?<begin>\|Supplies)\s*(?<counter>\d+)\s*(?<end>Type)\s*(?<equalSign>[=])\s*(?<typeName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Supplies 1 Amount = 2
        private static readonly Regex regexSupplyAmount = new Regex(@"(?<begin>\|Supplies)\s*(?<counter>\d+)\s*(?<end>Amount)\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Supplies 1 Amount Electricity = 4
        private static readonly Regex regexSupplyAmountElectricity = new Regex(@"(?<begin>\|Supplies)\s*(?<counter>\d+)\s*(?<end>Amount Electricity)\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Unlock Condition 1 Type = Farmers
        private static readonly Regex regexUnlockConditionType = new Regex(@"(?<begin>\|Unlock Condition)\s*(?<counter>\d+)\s*(?<end>Type)\s*(?<equalSign>[=])\s*(?<typeName>(?:\w*\s*)+(?:[\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Unlock Condition 1 Amount = 100
        private static readonly Regex regexUnlockConditionAmount = new Regex(@"(?<begin>\|Unlock Condition)\s*(?<counter>\d+)\s*(?<end>Amount)\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //|Credits = 15000
        private static readonly Regex regexConstructionCredits = new Regex(@"(?<begin>\|Credits)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Timber = 12
        private static readonly Regex regexConstructionTimber = new Regex(@"(?<begin>\|Timber)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Bricks = 3
        private static readonly Regex regexConstructionBricks = new Regex(@"(?<begin>\|Bricks)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Steel Beams = 8
        private static readonly Regex regexConstructionSteelBeams = new Regex(@"(?<begin>\|Steel Beams)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Windows = 2
        private static readonly Regex regexConstructionWindows = new Regex(@"(?<begin>\|Windows)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Concrete = 15
        private static readonly Regex regexConstructionConcrete = new Regex(@"(?<begin>\|Concrete)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Weapons = 20
        private static readonly Regex regexConstructionWeapons = new Regex(@"(?<begin>\|Weapons)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //|Advanced Weapons = 25
        private static readonly Regex regexConstructionAdvancedWeapons = new Regex(@"(?<begin>\|Advanced Weapons)\s*(?<equalSign>[=])\s*(?<value>[-+]*\d*(?:[.,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private CultureInfo cultureForParsing;

        public ParserSingleRegion(ICommons commons, ITitleParserSingle titleParserSingleToUse)
        {
            _commons = commons;
            _titleParserSingle = titleParserSingleToUse;

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

            var buildingName = _titleParserSingle.GetBuildingTitle(wikiText);
            if (string.IsNullOrWhiteSpace(buildingName))
            {
            }

            var buildingIcon = getBuildingIcon(wikiText);
            if (string.IsNullOrWhiteSpace(buildingIcon))
            {
            }

            var buildingType = getBuildingType(wikiText);
            if (buildingType == BuildingType.Unknown)
            {
            }

            var productionInfo = getProductionInfo(wikiText);
            if (productionInfo == null && buildingType == BuildingType.Production)
            {
            }

            var supplyInfo = getSupplyInfo(wikiText);
            var unlockInfo = getUnlockInfo(wikiText);
            var buildingSize = getBuildingSize(wikiText);
            var constructionInfo = getConstructionInfo(wikiText);

            var parsedInfobox = new Infobox
            {
                Name = buildingName,
                Icon = buildingIcon,
                Type = buildingType,
                ProductionInfos = productionInfo,
                SupplyInfos = supplyInfo,
                UnlockInfos = unlockInfo,
                BuildingSize = buildingSize,
                ConstructionInfos = constructionInfo
                //Region
            };

            result.Add(parsedInfobox);

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
                        break;
                    }
                }
            }

            return result;
        }

        private Size getBuildingSize(string infobox)
        {
            var result = Size.Empty;

            //short circuit infoboxes without building size info
            if (!infobox.Contains("|Building Size"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    var matchAmount = regexBuildingSize.Match(curLine);
                    if (matchAmount.Success)
                    {
                        var foundValue = matchAmount.Groups["value"].Value;
                        var splittedSize = foundValue.Split('x');
                        if (splittedSize.Length != 2)
                        {
                            return result;
                        }

                        var couldParseX = int.TryParse(splittedSize[0], out int x);
                        var couldParseY = int.TryParse(splittedSize[1], out int y);
                        if (!couldParseX || !couldParseY)
                        {
                            logger.Warn($"could not parse Size: \"{foundValue}\"");
                        }

                        result = new Size(x, y);
                        break;
                    }
                }
            }

            return result;
        }

        private BuildingType getBuildingType(string infobox)
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
                        var buildingType = curLine.Replace("|Building Type (OW)", string.Empty)
                            .Replace("|Building Type (NW)", string.Empty)
                            .Replace("|Building Type", string.Empty)
                            .Replace("=", string.Empty)
                            .Replace(" ", string.Empty)
                            .Trim();

                        if (Enum.TryParse(buildingType, ignoreCase: true, out BuildingType parsedBuildingType))
                        {
                            result = parsedBuildingType;
                        }

                        break;
                    }
                }
            }

            return result;
        }

        private ProductionInfo getProductionInfo(string infobox)
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

                    if (curLine.StartsWith("|Produces Amount Electricity", StringComparison.OrdinalIgnoreCase))
                    {
                        var productionAmountElectricity = curLine.Replace("|Produces Amount Electricity", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        if (double.TryParse(productionAmountElectricity, NumberStyles.Number, cultureForParsing, out var parsedProductionAmountElectricity))
                        {
                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            if (result.EndProduct == null)
                            {
                                result.EndProduct = new EndProduct();
                            }

                            result.EndProduct.AmountElectricity = parsedProductionAmountElectricity;
                        }
                    }
                    else if (curLine.StartsWith("|Produces Amount", StringComparison.OrdinalIgnoreCase))
                    {
                        var productionAmount = curLine.Replace("|Produces Amount", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        if (double.TryParse(productionAmount, NumberStyles.Number, cultureForParsing, out var parsedProductionAmount))
                        {
                            if (result == null)
                            {
                                result = new ProductionInfo();
                            }

                            if (result.EndProduct == null)
                            {
                                result.EndProduct = new EndProduct();
                            }

                            result.EndProduct.Amount = parsedProductionAmount;
                        }
                    }
                    else if (curLine.StartsWith("|Produces Icon", StringComparison.OrdinalIgnoreCase))
                    {
                        var icon = curLine.Replace("|Produces Icon", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        if (string.IsNullOrWhiteSpace(icon))
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

                        result.EndProduct.Icon = icon;
                    }
                    else if (curLine.StartsWith("|Input ", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchAmount = regexInputAmount.Match(curLine);
                        if (matchAmount.Success)
                        {
                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out var inputValue))
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
                            var matchedCounter = matchAmountElectricity.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out var inputValue))
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

        private SupplyInfo getSupplyInfo(string infobox)
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
                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out var supplyValue))
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
                            var matchedCounter = matchAmountElectricity.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out var supplyValue))
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

        private UnlockInfo getUnlockInfo(string infobox)
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
                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out var counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Unlock Condition 1 Amount = "
                            var matchedValue = matchAmount.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out var conditionValue))
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

        private List<ConstructionInfo> getConstructionInfo(string infobox)
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
