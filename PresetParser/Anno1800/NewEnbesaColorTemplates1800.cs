using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Extensions;

namespace PresetParser.Anno1800
{
    /// <summary>
    /// This process will set the right Template name to the Enbesa Park Ornamets the green color, all other no listed will be standard yellow/gold color
   
    public static class NewEnbesaColorTemplates1800
    {
        private static readonly List<string> ChangeTemplateNameToOrnamentalBuilding_Park_1800 = new List<string> { "AfriFlowerBed_Single_Tree", "AfriFlowerBed_straight_noTree", "AfriFlowerBed_T", "AfriFlowerBed_Corner", "AfriFlowerBed_X", 
            "AfriFlowerBed_End", "AfriPark_Bush01", "AfriPark_Tree02", "AfriPark_Grass", "AfriFlowerBed_Single_NoTree", "AfriPark_Bush02","AfriFlowerBed_straight_Tree","AfriPark_Tree01"};
        
        /// <summary>
        /// Retuns the faction and group for an identifier.
        /// </summary>
        /// <param name="identifierName">The given objectname, this will not changed</param>
        /// <param name="factionName">If objectname is in one of the lists, factionName will be changed</param>
        /// <param name="groupName">If objectname is in one of the lists, groupName will be changed</param>
        /// <param name="templateName">Objects Color Assignement for color.json</param>
        /// <returns></returns>
        public static (string Faction, string Group, string Template) GetNewOrnamentsGroup1800(string identifierName, string factionName, string groupName, string templateName)
        {
            if (string.IsNullOrWhiteSpace(identifierName))
            {
                throw new ArgumentNullException(nameof(identifierName), "No identifier was given.");
            }

            //New Ornaments Groups
            if (identifierName.IsPartOf(ChangeTemplateNameToOrnamentalBuilding_Park_1800)) { templateName = "OrnamentalBuilding_Park"; }
            return (factionName, groupName, templateName);
        }
    }
}
