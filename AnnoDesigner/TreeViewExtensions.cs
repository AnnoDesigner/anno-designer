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
    }
}
