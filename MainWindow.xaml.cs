using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlComparerWPF_NET6
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
            }
        }

        private void BtnLoadRight_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                rightFilePath = dlg.FileName;
                TxtFiles.Text = $"Left: {System.IO.Path.GetFileName(leftFilePath)} | Right: {System.IO.Path.GetFileName(rightFilePath)}";
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

            XElement leftNode = leftRoot.XPathSelectElement($"./{node}");
            XElement rightNode = rightRoot.XPathSelectElement($"./{node}");

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
        }

        private void CompareElements(XElement left, XElement right, TreeViewItem leftNode, TreeViewItem rightNode)
        {
            var leftChildren = left.Elements().ToList();
            var rightChildren = right.Elements().ToList();
            int max = Math.Max(leftChildren.Count, rightChildren.Count);

            for (int i = 0; i < max; i++)
            {
                XElement l = i < leftChildren.Count ? leftChildren[i] : null;
                XElement r = i < rightChildren.Count ? rightChildren[i] : null;

                string lh = l == null ? "<missing>" : l.Name.LocalName + ": " + l.Value;
                string rh = r == null ? "<missing>" : r.Name.LocalName + ": " + r.Value;

                var lItem = new TreeViewItem { Header = lh };
                var rItem = new TreeViewItem { Header = rh };

                if (l == null || r == null || l.Name != r.Name || l.Value != r.Value)
                {
                    lItem.Background = Brushes.Orange;
                    rItem.Background = Brushes.Red;
                }

                leftNode.Items.Add(lItem);
                rightNode.Items.Add(rItem);

                if (l != null && r != null && l.HasElements && r.HasElements)
                    CompareElements(l, r, lItem, rItem);
            }
        }
    }
}