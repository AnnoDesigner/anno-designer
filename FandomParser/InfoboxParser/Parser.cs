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

namespace InfoboxParser
{
    internal class Parser
    {
        private readonly ICommons _commons;

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

        //var culture = CultureInfo.InvariantCulture;
        private CultureInfo cultureForParsing;

        public Parser(ICommons commons)
        {
            _commons = commons;
            //all numbers in the wiki are entered with "," e.g. "42,21", so we need to use a specific culture
            cultureForParsing = new CultureInfo("de-DE");
        }

        public List<IInfobox> GetInfobox(string wikiText)
        {
            if (string.IsNullOrWhiteSpace(wikiText))
            {
                return null;
            }

            var result = new List<IInfobox>();

            var buildingName = getBuildingName(wikiText);
            if (string.IsNullOrWhiteSpace(buildingName))
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

            var specialBuildingNameHelper = new SpecialBuildingNameHelper();
            buildingName = specialBuildingNameHelper.CheckSpecialBuildingName(buildingName);

            var parsedInfobox = new Infobox
            {
                Name = buildingName,
                Type = buildingType,
                ProductionInfos = productionInfo,
                SupplyInfos = supplyInfo,
                UnlockInfos = unlockInfo,
                //Region
            };

            result.Add(parsedInfobox);

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

        private ProductionInfo getProductionInfo(string infobox)
        {
            ProductionInfo result = null;

            //short circuit infoboxes without production info
            if (!infobox.Contains("|Produces Amount"))
            {
                return result;
            }

            result = new ProductionInfo();
            result.EndProduct = new EndProduct();

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

                        if (double.TryParse(productionAmountElectricity, NumberStyles.Number, cultureForParsing, out double parsedProductionAmountElectricity))
                        {
                            result.EndProduct.AmountElectricity = parsedProductionAmountElectricity;
                        }
                    }
                    else if (curLine.StartsWith("|Produces Amount", StringComparison.OrdinalIgnoreCase))
                    {
                        var productionAmount = curLine.Replace("|Produces Amount", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        if (double.TryParse(productionAmount, NumberStyles.Number, cultureForParsing, out double parsedProductionAmount))
                        {
                            result.EndProduct.Amount = parsedProductionAmount;
                        }
                    }
                    else if (curLine.StartsWith("|Produces Icon", StringComparison.OrdinalIgnoreCase))
                    {
                        var icon = curLine.Replace("|Produces Icon", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        result.EndProduct.Icon = icon;
                    }
                    else if (curLine.StartsWith("|Input ", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchAmount = regexInputAmount.Match(curLine);
                        if (matchAmount.Success)
                        {
                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out double inputValue))
                            {
                                throw new Exception("could not find value for input");
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
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out double inputValue))
                            {
                                throw new Exception("could not find value for input");
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
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 1 Icon = "
                            var matchedFileName = matchIcon.Groups["fileName"].Value;
                            if (string.IsNullOrWhiteSpace(matchedFileName))
                            {
                                continue;
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

            //order by number from infobox
            result.InputProducts = result.InputProducts.OrderBy(x => x.Order).ToList();

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

            result = new SupplyInfo();

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
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out double supplyValue))
                            {
                                throw new Exception("could not find value for input");
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
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out double supplyValue))
                            {
                                throw new Exception("could not find value for input");
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
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Supplies 1 Type = "
                            var matchedTypeName = matchType.Groups["typeName"].Value;
                            if (string.IsNullOrWhiteSpace(matchedTypeName))
                            {
                                continue;
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

            //order by number from infobox
            result.SupplyEntries = result.SupplyEntries.OrderBy(x => x.Order).ToList();

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

            result = new UnlockInfo();

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
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Unlock Condition 1 Amount = "
                            var matchedValue = matchAmount.Groups["value"].Value;
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Number, cultureForParsing, out double conditionValue))
                            {
                                throw new Exception("could not find value for input");
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
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Unlock Condition 1 Type = "
                            var matchedTypeName = matchType.Groups["typeName"].Value;
                            if (string.IsNullOrWhiteSpace(matchedTypeName))
                            {
                                continue;
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

            //order by number from infobox
            result.UnlockConditions = result.UnlockConditions.OrderBy(x => x.Order).ToList();

            return result;
        }
    }
}
