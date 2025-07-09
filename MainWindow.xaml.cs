using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlComparer
{
    public partial class MainWindow : Window
    {
        private string leftFilePath, rightFilePath;

        public MainWindow()
        {
            InitializeComponent();
            BtnLoadLeft.Click += BtnLoadLeft_Click;
            BtnLoadRight.Click += BtnLoadRight_Click;
            BtnCompare.Click += BtnCompare_Click;
        }

        private void BtnLoadLeft_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                leftFilePath = dlg.FileName;
                TxtFiles.Text = $"Left: {System.IO.Path.GetFileName(leftFilePath)} | Right: {System.IO.Path.GetFileName(rightFilePath)}";
                LeftTreeHeader.Text = System.IO.Path.GetFileName(leftFilePath);
            }
        }

        private void BtnLoadRight_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                rightFilePath = dlg.FileName;
                TxtFiles.Text = $"Left: {System.IO.Path.GetFileName(leftFilePath)} | Right: {System.IO.Path.GetFileName(rightFilePath)}";
                RightTreeHeader.Text = System.IO.Path.GetFileName(rightFilePath);
            }
        }

        private void BtnCompare_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(leftFilePath) || string.IsNullOrEmpty(rightFilePath))
            {
                MessageBox.Show("Please upload both XML files.");
                return;
            }

            string node = TxtXPath.Text.Trim();
            if (string.IsNullOrWhiteSpace(node))

            {
                MessageBox.Show("Please enter a valid node.");
                return;
            }

            XElement leftRoot = XElement.Load(leftFilePath);
            XElement rightRoot = XElement.Load(rightFilePath);

            XElement leftNode = leftRoot;
            XElement rightNode = rightRoot;

            if (TxtXPath.IsEnabled)
            {
                leftNode = leftRoot.XPathSelectElement($"./{node}");
                rightNode = rightRoot.XPathSelectElement($"./{node}");
            }


            TreeLeft.Items.Clear();
            TreeRight.Items.Clear();

            if (leftNode == null || rightNode == null)
            {
                MessageBox.Show("Could not find node(s) using the provided XPath.");
                return;
            }

            TreeViewItem leftTree = new TreeViewItem { Header = leftNode.Name.LocalName };
            TreeViewItem rightTree = new TreeViewItem { Header = rightNode.Name.LocalName };
            TreeLeft.Items.Add(leftTree);
            TreeRight.Items.Add(rightTree);

            CompareElements(leftNode, rightNode, leftTree, rightTree);
            leftTree.IsExpanded = true;
            rightTree.IsExpanded = true;
        }

        private void CanFilterByNode_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.IsChecked.HasValue)
            {
                TxtXPath.IsEnabled = checkBox.IsChecked.Value;
            }
        }

        private void CompareElements(XElement left, XElement right, TreeViewItem leftNode, TreeViewItem rightNode)
        {
            var leftChildren = left?.Elements().ToList() ?? new List<XElement>();
            var rightChildren = right?.Elements().ToList() ?? new List<XElement>();

            int max = Math.Max(leftChildren.Count, rightChildren.Count);
            AddMissingNodeOnRight(leftChildren, rightChildren, max);
            if(ShowOnlyDifferences())
            {
                AddItemsToTreeOnlyDifferences(leftNode, rightNode, leftChildren, rightChildren, max);
                return;
            }
            AddItemsToTree(leftNode, rightNode, leftChildren, rightChildren, max);
        }

        private void AddItemsToTree(TreeViewItem leftNode, TreeViewItem rightNode, List<XElement> leftChildren, List<XElement> rightChildren, int max)
        {
            for (int i = 0; i < max; i++)
            {
                XElement l = i < leftChildren.Count ? leftChildren[i] : null;
                XElement r = i < rightChildren.Count ? rightChildren[i] : null;

                string lh = l == null ? "<missing>" : $"{l.Name.LocalName}: {l.Value}";
                string rh = r == null ? "<missing>" : $"{r.Name.LocalName}: {r.Value}";

                var lItem = new TreeViewItem { Header = lh };
                var rItem = new TreeViewItem { Header = rh };

                IfDifferentThenHighlight(l, r, lItem, rItem);

                leftNode.Items.Add(lItem);
                rightNode.Items.Add(rItem);

                IfChildrenCompareAgain(l, r, lItem, rItem);

                lItem.IsExpanded = true;
                rItem.IsExpanded = true;
            }
        }

        private void AddItemsToTreeOnlyDifferences(TreeViewItem leftNode, TreeViewItem rightNode, List<XElement> leftChildren, List<XElement> rightChildren, int max)
        {
            for (int i = 0; i < max; i++)
            {
                XElement l = i < leftChildren.Count ? leftChildren[i] : null;
                XElement r = i < rightChildren.Count ? rightChildren[i] : null;

                string lh = l == null ? "<missing>" : $"{l.Name.LocalName}: {l.Value}";
                string rh = r == null ? "<missing>" : $"{r.Name.LocalName}: {r.Value}";

                var lItem = new TreeViewItem { Header = lh };
                var rItem = new TreeViewItem { Header = rh };

                IfDifferentThenHighlightAndAddItems(leftNode,rightNode, l, r, lItem, rItem);

                IfChildrenCompareAgain(l, r, lItem, rItem);

                lItem.IsExpanded = true;
                rItem.IsExpanded = true;
            }
        }

        private void IfChildrenCompareAgain(XElement l, XElement r, TreeViewItem lItem, TreeViewItem rItem)
        {
            bool hasLeftChildren = l != null && l.HasElements;
            bool hasRightChildren = r != null && r.HasElements;

            if (hasLeftChildren || hasRightChildren)
            {
                CompareElements(l, r, lItem, rItem);
            }
        }

        private static void IfDifferentThenHighlight(XElement l, XElement r, TreeViewItem lItem, TreeViewItem rItem)
        {
            bool isDifferent = false;

            if (l == null || r == null)
            {
                isDifferent = true;
            }
            else if (l.Name != r.Name || l.Value.Trim() != r.Value.Trim())
            {
                isDifferent = true;
            }

            if (isDifferent)
            {
                lItem.Background = Brushes.Orange;
                rItem.Background = Brushes.Red;
            }
        }

        private static void IfDifferentThenHighlightAndAddItems(TreeViewItem leftNode, TreeViewItem rightNode,XElement l, XElement r, TreeViewItem lItem, TreeViewItem rItem)
        {
            bool isDifferent = false;

            if (l == null || r == null)
            {
                isDifferent = true;
            }
            else if (l.Name != r.Name || l.Value.Trim() != r.Value.Trim())
            {
                isDifferent = true;
            }

            if (isDifferent)
            {
                lItem.Background = Brushes.Orange;
                rItem.Background = Brushes.Red;
                leftNode.Items.Add(lItem);
                rightNode.Items.Add(rItem);
            }
        }

        private static void AddMissingNodeOnRight(List<XElement> leftChildren, List<XElement> rightChildren, int max)
        {
            for (int i = 0; i < max; i++)
            {
                if (rightChildren.Count > i && leftChildren[i].Name == rightChildren[i].Name)
                    continue;

                XElement xElement = new XElement("missing");
                rightChildren.Insert(i, xElement);
            }
        }

        private bool ShowOnlyDifferences()
        {
            return ShowDifferencesOnly != null && ShowDifferencesOnly.IsChecked.HasValue && ShowDifferencesOnly.IsChecked.Value;
        }
    }
}