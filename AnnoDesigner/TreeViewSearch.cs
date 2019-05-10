using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace AnnoDesigner
{
    public class TreeViewSearch<T>
    {
        /// <summary>
        /// Initialises a TreeViewSearch object with a TreeView instance and a function to select the key from the objects the TreeView holds.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="keySelector"></param>
        public TreeViewSearch(TreeView t, Func<T, string> keySelector)
        {
            Instance = t;
            this.KeySelector = keySelector;
            IsCaseSensitive = true;
            MatchFullWordOnly = false;
        }

        /// <summary>
        /// The instance of the TreeView that this search is filtering. 
        /// </summary>
        public TreeView Instance { get; }

        /// <summary>
        /// Selects a string value from T in the TreeView, which is used when comparing strings.
        /// </summary>
        public Func<T, string> KeySelector { get; set; }

        /// <summary>
        /// Backing field for the IsCaseSensitive property.
        /// </summary>
        private bool _isCaseSensitive;

        /// <summary>
        /// Indicates whether the search should be case-insensitive.
        /// </summary>
        public bool IsCaseSensitive
        {
            get => _isCaseSensitive;
            set
            {
                _isCaseSensitive = value;
                CurrentComparison = value ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            }
        }

        /// <summary>
        /// Indicates whether or not a search term should exactly match the search item. 
        /// </summary>
        public bool MatchFullWordOnly { get; set; }

        /// <summary>
        /// Holds the current StringComparison value.
        /// </summary>
        private StringComparison CurrentComparison;

        /// <summary>
        /// Searches a TreeView for a specified term.
        /// </summary>
        /// <param name="token">The value to search for</param>
        public void Search(string token)
        {
            foreach (var node in Instance.Items)
            {
                if (node is T obj)
                {
                    var t = Instance.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                    if (Compare(obj, token))
                    {
                        t.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        t.Visibility = Visibility.Collapsed;
                    }
                }
                else if (node is TreeViewItem treeViewItem)
                {
                    if (treeViewItem.HasItems)
                    {
                        if (Search(treeViewItem, token, false))
                        {
                            treeViewItem.Visibility = Visibility.Visible;
                            treeViewItem.IsExpanded = true;
                        }
                        else
                        {
                            treeViewItem.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively traverses a TreeViewItem looking for the values retrieved using the current KeySelector that match the specified token.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <param name="foundMatch"></param>
        /// <returns></returns>
        private bool Search(TreeViewItem item, string token, bool foundMatch)
        {
            foreach (var node in item.Items)
            {
                if (node is TreeViewItem treeViewItem)
                {
                    if (Search(treeViewItem, token, false))
                    {
                        foundMatch = true;
                        treeViewItem.IsExpanded = true;
                        treeViewItem.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        treeViewItem.IsExpanded = false;
                        treeViewItem.Visibility = Visibility.Collapsed;
                    }
                }
                if (node is T obj)
                {
                    var t = GetItemContainer(item, obj);
                    if (Compare(obj, token))
                    {
                        foundMatch = true;
                        t.IsExpanded = true;
                        t.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        t.IsExpanded = false;
                        t.Visibility = Visibility.Collapsed;
                    }
                }
            }
            return foundMatch;
        }

        /// <summary>
        /// Compares an object and a token, using the current KeySelector function to extract the string value from the object.
        /// Takes into account the current options set on the TreeViewSearch instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool Compare(T obj, string token)
        {
            if (MatchFullWordOnly)
            {
                return string.Equals(token, KeySelector(obj), CurrentComparison);
            }
            else
            {
                return KeySelector(obj).Contains(token, CurrentComparison);
            }
        }

        

        /// <summary>
        /// Collapses all items in the tree view, and makes them visible.
        /// </summary>
        public void Reset()
        {
            foreach (var node in Instance.Items)
            {
                if (node is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsExpanded = false;
                    treeViewItem.Visibility = Visibility.Visible;
                    Reset(treeViewItem);
                }
                else if (node is T obj)
                {
                    var t = Instance.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                    t.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Recursively traverses a TreeViewItem, making all branches and leaves visible, collapsing any branches.
        /// </summary>
        /// <param name="item"></param>
        private void Reset(TreeViewItem item)
        {
            item.IsExpanded = false;
            item.Visibility = Visibility.Visible;
            foreach (var node in item.Items)
            {
                if (node is TreeViewItem treeViewItem)
                {
                    Reset(treeViewItem);
                }
                else if (node is T obj)
                {
                    var t = GetItemContainer(item, obj);
                    t.Visibility = Visibility.Visible;
                }
            }

        }

        /// <summary>
        /// Ensures that all ItemContainerGenerators have generated.
        /// Calling this method explicitly as early on as possible is recommended.
        /// </summary>
        /// <returns></returns>
        public void EnsureItemContainersGenerated()
        {
            foreach (var node in Instance.Items)
            {
                if (node is TreeViewItem treeViewItem)
                {
                    EnsureItemContainersGenerated(treeViewItem);
                }
            }
        }

        /// <summary>
        /// Recursively traverses the TreeView and calls GenerateItemContainer for each TreeViewItem
        /// </summary>
        /// <param name="node"</param>
        private void EnsureItemContainersGenerated(TreeViewItem node)
        {
            if (node.HasItems)
            {
                foreach (var item in node.Items)
                {
                    if (item is TreeViewItem treeViewItem)
                    {
                        if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.NotStarted)
                        {
                            GenerateItemContainers(treeViewItem);
                        }
                        if (treeViewItem.HasItems)
                        {
                            EnsureItemContainersGenerated(treeViewItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Forces the ItemContainer to generate.
        /// This is a bit ugly, but all ancestors need to be expanded when UpdateLayout() is called to force the ItemContainer to generate.
        /// </summary>
        /// <param name="treeViewItem"></param>
        private void GenerateItemContainers(TreeViewItem treeViewItem)
        {
            Debug.WriteLine("Generating ItemContainer for: " + treeViewItem.ToString());
            var expandedState = treeViewItem.IsExpanded;
            treeViewItem.IsExpanded = true;
            var previousStates = treeViewItem.ExpandAncestors(new List<KeyValuePair<TreeViewItem, bool>>());
            treeViewItem.UpdateLayout();
            treeViewItem.IsExpanded = expandedState;
            foreach (var item in previousStates)
            {
                item.Key.IsExpanded = item.Value;
            }
        }

        /// <summary>
        /// Retrieves the ItemContainer from an object. Ensures the container has been generated.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private TreeViewItem GetItemContainer(TreeViewItem item, object obj)
        {
            if (item.ItemContainerGenerator.Status == GeneratorStatus.NotStarted)
            {
                GenerateItemContainers(item);
            }
            return item.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
        }
    }
}
