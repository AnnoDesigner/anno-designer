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
        /// <summary>
        /// Saves the state of the TreeView to a List&lt;bool&gt;. Call <c>SetTreeViewState()</c> to restore.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Restores the state of a TreeView from a List&lt;bool&gt; generated with GetTreeViewState().
        /// </summary>
        /// <param name="t"></param>
        /// <param name="nodeStateList"></param>
        public static void SetTreeViewState(this TreeView t, List<bool> nodeStateList)
        {
            if (nodeStateList.Count == 0) return;
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
        /// Expands all ancestors for this <c>TreeViewItem</c>.
        /// </summary>
        /// <param name="item">The <c>TreeViewItem</c> to expand</param>
        public static void ExpandAncestors(this TreeViewItem item)
        {
            if (item.Parent is TreeViewItem treeViewItem)
            {
                //Make sure to only expand **ancestors** - this is the reason we need a separate function
                //it would better if this could be rewritten to use a single recursive function,
                ExpandAncestorsToRoot(treeViewItem);
            }
        }

        /// <summary>
        /// Expands all ancestors for this <c>TreeViewItem</c>.
        /// </summary>
        /// <param name="item"></param>
        private static void ExpandAncestorsToRoot(TreeViewItem item)
        {
            item.IsExpanded = true;
            if (item.Parent is TreeViewItem treeViewItem)
            {
                ExpandAncestors(treeViewItem);
            }
        }

        /// <summary>
        /// Expands all ancestors for the this <c>TreeViewItem</c>. Returns a Dictionary that represents the previous expansion state.
        /// </summary>
        /// <param name="item">The item to expand</param>
        public static List<KeyValuePair<TreeViewItem, bool>> ExpandAncestors(this TreeViewItem item, List<KeyValuePair<TreeViewItem, bool>> previousExpansionState)
        {
            if (previousExpansionState == null)
            {
                previousExpansionState = new List<KeyValuePair<TreeViewItem, bool>>();
            }
            //Make sure to only expand **ancestors** - this is the reason we need a separate function
            //it would better if this could be rewritten to use a single recursive function,
            if (item.Parent is TreeViewItem treeViewItem)
            {
                return ExpandAncestorsToRoot(treeViewItem, previousExpansionState);
            }
            return previousExpansionState;
        }

        /// <summary>
        /// Expands all ancestors for the this <c>TreeViewItem</c>. Returns a Dictionary that represents the previous expansion state.
        /// </summary>
        /// <param name="item">The item to expand</param>
        public static List<KeyValuePair<TreeViewItem, bool>> ExpandAncestorsToRoot(this TreeViewItem item, List<KeyValuePair<TreeViewItem, bool>> previousExpansionState)
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
