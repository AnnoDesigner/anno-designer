using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AnnoDesigner
{
    public static class TreeViewExtensions
    {
        public static List<bool> GetTreeViewState(this TreeView t)
        {
            var nodeList = new List<bool>();
            foreach (var item in t.ItemContainerGenerator.Items
                .Where(_ => _ as TreeViewItem != null)
                .Cast<TreeViewItem>())
            {
                BuildExpandedList(item, nodeList);
            }
            return nodeList;
        } 

        private static List<bool> BuildExpandedList(TreeViewItem t, List<bool> nodeList)
        {

            if (!t.IsExpanded)
            {
                nodeList.Add(false);
                return nodeList;
            }
            else
            {
                nodeList.Add(true);
                foreach (var item in t.ItemContainerGenerator.Items
                    .Where(_ => _ as TreeViewItem != null)
                    .Cast<TreeViewItem>())
                {
                    BuildExpandedList(item, nodeList);
                }
            }
            return nodeList;
        }

        public static void SetTreeViewState(this TreeView t, List<bool> nodeStateList)
        {
            int currentIndex = -1;
            foreach (var item in t.ItemContainerGenerator.Items
                    .Where(_ => _ as TreeViewItem != null)
                    .Cast<TreeViewItem>())
            {
               currentIndex = SetState(item, nodeStateList, currentIndex);
            }
        }

        private static int SetState(TreeViewItem t, List<bool> nodeStateList, int currentIndex)
        {
            if (currentIndex == 20)
            {

            }
            if (t == null || !t.HasItems)
            {
                return currentIndex;
            }
            else if (!nodeStateList[currentIndex + 1])
            {
                currentIndex++;
                return currentIndex;
            }
            else
            {
                currentIndex++;
                t.IsExpanded = nodeStateList[currentIndex];
                var list = t.ItemContainerGenerator.Items
                    .Where(_ => _ as TreeViewItem != null)
                    .Cast<TreeViewItem>();
                foreach (TreeViewItem item in list)
                {
                    currentIndex = SetState(item, nodeStateList, currentIndex);
                }
            }
            return currentIndex;
        }

        /// <summary>
        /// Expands all ancestors for the given item.
        /// </summary>
        /// <param name="item">The item to expand</param>
        public static void ExpandAncestors(this TreeViewItem item)
        {
            item.IsExpanded = true;
            if (item.Parent is TreeViewItem treeViewItem)
            {
                ExpandAncestors(treeViewItem);
            }
        }

        /// <summary>
        /// Expands all ancestors for the given item. Returns a Dictionary that represents the previous expansion state.
        /// </summary>
        /// <param name="item">The item to expand</param>
        public static List<KeyValuePair<TreeViewItem, bool>> ExpandAncestors(this TreeViewItem item, List<KeyValuePair<TreeViewItem, bool>> previousExpansionState)
        {
            if (previousExpansionState == null)
            {
                previousExpansionState = new List<KeyValuePair<TreeViewItem, bool>>();
            }
            previousExpansionState.Add(new KeyValuePair<TreeViewItem, bool>(item, item.IsExpanded));
            item.IsExpanded = true;
            if (item.Parent is TreeViewItem treeViewItem)
            {
                return ExpandAncestors(treeViewItem, previousExpansionState);
            }
            return previousExpansionState;
        }
    }
}
