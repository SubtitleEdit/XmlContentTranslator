using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace XmlContentTranslator
{
    public partial class Main : Form
    {
        class ComboBoxItem
        {
            private string Text { get; set; }
            public string Value { get; private set; }

            public ComboBoxItem(string text, string value)
            {
                if (text.Length > 1)
                    text = text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        Hashtable _treeNodesHashtable = new Hashtable();
        Hashtable _listViewItemHashtable = new Hashtable();
        XmlDocument _originalDocument;
        string _secondLanguageFileName;
        bool _change;
        private Find _formFind;

        public Main()
        {
            InitializeComponent();
            toolStripStatusLabel1.Text = string.Empty;
            toolStripStatusLabel2.Text = string.Empty;

            FillComboWithLanguages(comboBoxFrom);
            FillComboWithLanguages(comboBoxTo);
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_change && listViewLanguageTags.Columns.Count == 3 &&
                MessageBox.Show("Changes will be lost. Continue?", "Continue", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            openFileDialog1.FileName = string.Empty;
            openFileDialog1.DefaultExt = ".xml";
            openFileDialog1.Filter = "Xml files|*.xml" + "|All files|*.*";
            openFileDialog1.Title = "Open language master file";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                MakeNew();
                if (OpenFirstFile(openFileDialog1.FileName))
                    OpenSecondFile();
            }
        }

        private bool OpenFirstFile(string fileName)
        {
            toolStripStatusLabel1.Text = "Opening " + fileName + "...";
            var doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
            }
            catch
            {
                MessageBox.Show("Not a valid xml file: " + fileName);
                return false;
            }

            return OpenFirstXmlDocument(doc);
        }

        private bool OpenFirstXmlDocument(XmlDocument doc)
        {
            listViewLanguageTags.Columns.Add("Tag", 150);
            TryGetLanguageNameAttribute(doc, comboBoxFrom);

            AddAttributes(doc.DocumentElement);
            if (doc.DocumentElement != null)
            {
                foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
                {
                    if (childNode.NodeType != XmlNodeType.Attribute)
                    {
                        var treeNode = new TreeNode(childNode.Name);
                        treeNode.Tag = childNode;
                        treeView1.Nodes.Add(treeNode);
                        if (childNode.ChildNodes.Count > 0 && !XmlUtils.IsTextNode(childNode))
                        {
                            ExpandNode(treeNode, childNode);
                        }
                        else
                        {
                            _treeNodesHashtable.Add(treeNode, childNode);
                            AddListViewItem(childNode);
                        }
                    }
                }
            }
            _originalDocument = doc;
            toolStripStatusLabel1.Text = "Done reading " + openFileDialog1.FileName;
            return true;
        }

        private void SetLanguage(ComboBox comboBox, XmlDocument doc)
        {
            int index = 0;
            comboBox.SelectedIndex = -1;
            int defaultIndex = -1;
            bool isDefaultSet = false;
            if (doc != null && doc.DocumentElement != null && doc.DocumentElement.SelectSingleNode("General/CultureName") != null)
            {
                string culture = doc.DocumentElement.SelectSingleNode("General/CultureName").InnerText;
                foreach (ComboBoxItem item in comboBox.Items)
                {
                    if (item.Value == culture)
                    {
                        comboBox.SelectedIndex = index;
                        return;
                    }
                    if (isDefaultSet == false && item.Value == "en")
                    {
                        defaultIndex = index;
                        isDefaultSet = true;
                    }
                    index++;
                }

                culture = culture.Substring(0, 2);
                index = 0;
                foreach (ComboBoxItem item in comboBox.Items)
                {
                    if (item.Value == culture)
                    {
                        comboBox.SelectedIndex = index;
                        return;
                    }
                    index++;
                }
            }
            if (defaultIndex >= 0)
                comboBox.SelectedIndex = defaultIndex;
        }

        private void OpenSecondFile()
        {
            _secondLanguageFileName = string.Empty;
            Text = "XML Content Translator - New";
            openFileDialog1.Title = "Open file to translate/correct";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenSecondFile(openFileDialog1.FileName);
            }
            else
            {
                listViewLanguageTags.Columns.Add("Language 2", 200);
                SetLanguage(comboBoxTo, null);
                CreateEmptyLanguage();
            }
            HighLightLinesWithSameText();
        }

        private void OpenSecondFile(string fileName)
        {
            toolStripStatusLabel1.Text = "Opening " + fileName + "...";
            _secondLanguageFileName = fileName;
            Text = "XML Content Translator - " + _secondLanguageFileName;

            Cursor = Cursors.WaitCursor;
            listViewLanguageTags.BeginUpdate();
            var doc = new XmlDocument();
            try
            {
                doc.Load(_secondLanguageFileName);
            }
            catch
            {
                MessageBox.Show("Not a valid xml file: " + _secondLanguageFileName);
            }

            TryGetLanguageNameAttribute(doc, comboBoxTo);

            AddAttributes(doc.DocumentElement);
            if (doc.DocumentElement != null)
            {
                foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
                {
                    if (childNode.ChildNodes.Count > 0 && !XmlUtils.IsTextNode(childNode))
                    {
                        ExpandNode(null, childNode);
                    }
                    else
                    {
                        AddListViewItem(childNode);
                        AddAttributes(doc.DocumentElement);
                    }
                }
            }

            CreateEmptyLanguage();

            listViewLanguageTags.EndUpdate();
            Cursor = Cursors.Default;
            toolStripStatusLabel1.Text = "Done reading " + _secondLanguageFileName;
        }

        private void TryGetLanguageNameAttribute(XmlDocument doc, ComboBox cb)
        {
            if (doc.DocumentElement != null && doc.DocumentElement.Attributes["Name"] != null)
            {
                listViewLanguageTags.Columns.Add(doc.DocumentElement.Attributes["Name"].InnerText, 200);
            }
            else if (doc.DocumentElement != null && doc.DocumentElement.Attributes["name"] != null)
            {
                listViewLanguageTags.Columns.Add(doc.DocumentElement.Attributes["name"].InnerText, 200);
            }
            else
            {
                string language = "Language1";
                if (cb.Name == "comboBoxTo")
                {
                    language = "Language2";
                }
                listViewLanguageTags.Columns.Add(language, 200);
            }
            SetLanguage(cb, doc);
        }

        private void CreateEmptyLanguage()
        {
            foreach (ListViewItem lvi in listViewLanguageTags.Items)
            {
                if (lvi.SubItems.Count == 2)
                {
                    lvi.SubItems.Add(string.Empty);
                }
            }
        }

        private void AddListViewItem(XmlNode node)
        {
            if (listViewLanguageTags.Columns.Count == 2)
            {
                if (node.NodeType != XmlNodeType.Comment && node.NodeType != XmlNodeType.CDATA)
                {

                    ListViewItem item;
                    if (node.NodeType == XmlNodeType.Attribute)
                        item = new ListViewItem("@" + node.Name);
                    else
                        item = new ListViewItem(node.Name);
                    item.Tag = node;

                    item.SubItems.Add(node.InnerText);
                    listViewLanguageTags.Items.Add(item);
                    _listViewItemHashtable.Add(XmlUtils.BuildNodePath(node), item); // fails on some attributes!!
                }
            }
            else if (listViewLanguageTags.Columns.Count == 3)
            {
                var item = _listViewItemHashtable[XmlUtils.BuildNodePath(node)] as ListViewItem;
                if (item != null)
                {
                    item.SubItems.Add(node.InnerText);
                }
            }
        }

        private void MakeNew()
        {
            _treeNodesHashtable = new Hashtable();
            _listViewItemHashtable = new Hashtable();
            treeView1.Nodes.Clear();
            listViewLanguageTags.Items.Clear();
            listViewLanguageTags.Clear();

            _secondLanguageFileName = string.Empty;
            Text = "XML Content Translator";

            _change = false;
        }

        private void ExpandNode(TreeNode parentNode, XmlNode node)
        {
            if (listViewLanguageTags.Columns.Count == 2)
            {
                AddAttributes(node);
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    var treeNode = new TreeNode(childNode.Name);
                    treeNode.Tag = childNode;
                    if (parentNode == null)
                        treeView1.Nodes.Add(treeNode);
                    else
                        parentNode.Nodes.Add(treeNode);
                    if (XmlUtils.IsParentElement(childNode))
                    {
                        ExpandNode(treeNode, childNode);
                    }
                    else
                    {
                        _treeNodesHashtable.Add(treeNode, childNode);
                        AddListViewItem(childNode);
                        AddAttributes(childNode);
                    }
                }
            }
            else if (listViewLanguageTags.Columns.Count == 3)
            {
                AddAttributes(node);
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (XmlUtils.IsParentElement(childNode))
                    {
                        ExpandNode(null, childNode);
                    }
                    else
                    {
                        AddListViewItem(childNode);
                        AddAttributes(childNode);
                    }
                }
            }
        }

        private void AddAttributes(XmlNode node)
        {
            if (node.Attributes == null || node.Attributes.Count == 0)
                return;
            foreach (XmlNode childNode in node.Attributes)
            {
                AddListViewItem(childNode);
            }
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_change && listViewLanguageTags.Columns.Count == 3 &&
                MessageBox.Show("Changes will be lost. Continue?", "Continue", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            _change = false;
            Close();
        }

        private void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = _treeNodesHashtable[e.Node] as XmlNode;
            if (node != null)
            {
                DeSelectListViewItems();

                var item = _listViewItemHashtable[XmlUtils.BuildNodePath(node)] as ListViewItem;
                if (item != null)
                {
                    item.Selected = true;
                    listViewLanguageTags.EnsureVisible(item.Index);
                }
            }
        }

        private void DeSelectListViewItems()
        {
            var selectedItems = new List<ListViewItem>();
            foreach (ListViewItem lvi in listViewLanguageTags.SelectedItems)
            {
                selectedItems.Add(lvi);
            }
            foreach (ListViewItem lvi in selectedItems)
            {
                lvi.Selected = false;
            }
        }

        private void ListViewLanguageTagsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewLanguageTags.SelectedItems.Count == 1 && listViewLanguageTags.SelectedItems[0].SubItems.Count > 2)
            {
                textBoxCurrentText.Enabled = true;
                textBoxCurrentText.Text = listViewLanguageTags.SelectedItems[0].SubItems[2].Text;

                var node = listViewLanguageTags.SelectedItems[0].Tag as XmlNode;
                if (node != null)
                    toolStripStatusLabel2.Text = string.Format("{0}     {1} / {2}", XmlUtils.BuildNodePath(node).Replace("#document/", ""), listViewLanguageTags.SelectedItems[0].Index + 1, listViewLanguageTags.Items.Count);
                else
                    toolStripStatusLabel2.Text = string.Format("{0} / {1}", listViewLanguageTags.SelectedItems[0].Index + 1, listViewLanguageTags.Items.Count);
            }
            else
            {
                textBoxCurrentText.Text = string.Empty;
                textBoxCurrentText.Enabled = false;
                toolStripStatusLabel2.Text = string.Format("{0} items selected", listViewLanguageTags.SelectedItems.Count);
            }
            HighLightLinesWithSameText();
        }

        private void TextBoxCurrentTextTextChanged(object sender, EventArgs e)
        {
            if (listViewLanguageTags.SelectedItems.Count == 1)
            {
                listViewLanguageTags.SelectedItems[0].SubItems[2].Text = textBoxCurrentText.Text;
            }

        }

        private void FillOriginalDocumentFromSecondLanguage()
        {
            FillAttributes(_originalDocument.DocumentElement);
            if (_originalDocument.DocumentElement != null)
            {
                foreach (XmlNode childNode in _originalDocument.DocumentElement.ChildNodes)
                {
                    if (childNode.ChildNodes.Count > 0 && !XmlUtils.IsTextNode(childNode))
                    {
                        FillOriginalDocumentExpandNode(childNode);
                    }
                    else
                    {
                        var item = _listViewItemHashtable[XmlUtils.BuildNodePath(childNode)] as ListViewItem;
                        if (item != null)
                        {
                            childNode.InnerText = item.SubItems[2].Text;
                        }
                        FillAttributes(_originalDocument.DocumentElement);
                    }
                }
            }
        }

        private void FillOriginalDocumentExpandNode(XmlNode node)
        {
            FillAttributes(node);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.ChildNodes.Count > 0 && !XmlUtils.IsTextNode(childNode))
                {
                    FillOriginalDocumentExpandNode(childNode);
                }
                else
                {
                    var item = _listViewItemHashtable[XmlUtils.BuildNodePath(childNode)] as ListViewItem;
                    if (item != null)
                    {
                        childNode.InnerText = item.SubItems[2].Text;
                    }
                    FillAttributes(childNode);
                }
            }
        }

        private void FillAttributes(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            foreach (XmlNode attribute in node.Attributes)
            {
                var item = _listViewItemHashtable[XmlUtils.BuildNodePath(attribute)] as ListViewItem;
                if (item != null)
                {
                    attribute.InnerText = item.SubItems[2].Text;
                }
            }
        }

        private void Form1KeyDown(object sender, KeyEventArgs e)
        {
            if (listViewLanguageTags.Items.Count == 0)
                return;

            if (e.Control && e.KeyCode == Keys.Down)
            {
                if (listViewLanguageTags.SelectedItems.Count == 0)
                    listViewLanguageTags.Items[0].Selected = true;

                int index = listViewLanguageTags.SelectedItems[0].Index + 1;
                if (index < listViewLanguageTags.Items.Count)
                {
                    DeSelectListViewItems();
                    listViewLanguageTags.Items[index].Selected = true;
                    listViewLanguageTags.EnsureVisible(index);
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                ActiveControl = textBoxCurrentText;
            }
            else if (e.Control && e.KeyCode == Keys.Up)
            {
                if (listViewLanguageTags.SelectedItems.Count == 0)
                    listViewLanguageTags.Items[0].Selected = true;

                int index = listViewLanguageTags.SelectedItems[0].Index - 1;
                if (index >= 0)
                {
                    DeSelectListViewItems();
                    listViewLanguageTags.Items[index].Selected = true;
                    listViewLanguageTags.EnsureVisible(index);
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
                ActiveControl = textBoxCurrentText;
            }
            else if (e.KeyCode == Keys.F6)
            {
                ButtonGoToNextBlankLineClick(null, null);
            }
            else if (e.KeyCode == Keys.F3 && !e.Control && !e.Alt)
            {
                if (_formFind == null || _formFind.SearchText.Length == 0)
                {
                    findToolStripMenuItem_Click(this, null);
                }
                else
                {
                    FindNext();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void GoogleTranslateSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            GoogleTranslateSelectedLines();
        }

        /// <summary>
        /// Translate Text using Google Translate API's
        /// Google URL - https://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="languagePair">2 letter Language Pair, delimited by "|".
        /// E.g. "ar|en" language pair means to translate from Arabic to English</param>
        /// <returns>Translated to String</returns>
        public static string TranslateTextViaScreenScraping(string input, string languagePair)
        {
            input = input.Replace(Environment.NewLine, "<br/>").Trim();
            input = input.Replace("'", "&apos;");

            //string url = String.Format("https://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", HttpUtility.UrlEncode(input), languagePair);
            string url = String.Format("https://translate.google.com/?hl=en&eotf=1&sl={0}&tl={1}&q={2}", languagePair.Substring(0, 2), languagePair.Substring(3), HttpUtility.UrlEncode(input));

            var webClient = new WebClient { Encoding = Encoding.Default };
            string result = webClient.DownloadString(url);
            int startIndex = result.IndexOf("<span id=result_box", StringComparison.Ordinal);
            var sb = new StringBuilder();
            if (startIndex > 0)
            {
                startIndex = result.IndexOf("<span title=", startIndex, StringComparison.Ordinal);
                while (startIndex > 0)
                {
                    startIndex = result.IndexOf(">", startIndex, StringComparison.Ordinal);
                    if (startIndex > 0)
                    {
                        startIndex++;
                        int endIndex = result.IndexOf("</span>", startIndex, StringComparison.Ordinal);
                        string translatedText = result.Substring(startIndex, endIndex - startIndex);
                        string test = HttpUtility.HtmlDecode(translatedText);
                        sb.Append(test);
                        startIndex = result.IndexOf("<span title=", startIndex, StringComparison.Ordinal);
                    }
                }
            }
            string res = sb.ToString();
            res = res.Replace("<BR/>", Environment.NewLine);
            res = res.Replace("<BR />", Environment.NewLine);
            res = res.Replace("< BR />", Environment.NewLine);
            res = res.Replace(" <br/>", Environment.NewLine);
            res = res.Replace("<br/>", Environment.NewLine);
            res = res.Replace("<br />", Environment.NewLine);
            return res.Trim();
        }

        private void GoogleTranslateSelectedLines()
        {
            if (string.IsNullOrEmpty(_secondLanguageFileName))
                return;

            if (comboBoxFrom.SelectedItem == null || comboBoxTo.SelectedItem == null)
            {
                MessageBox.Show("From/to language not selected");
                return;
            }

            int skipped = 0;
            int translated = 0;
            string oldText = string.Empty;
            string newText = string.Empty;

            if (listViewLanguageTags.SelectedItems.Count > 10)
            {
                toolStripStatusLabel1.Text = "Translating via Google Translate. Please wait...";
                Refresh();
            }

            Cursor = Cursors.WaitCursor;
            var sb = new StringBuilder();
            var res = new StringBuilder();
            var oldLines = new List<string>();
            foreach (ListViewItem item in listViewLanguageTags.SelectedItems)
            {
                oldText = item.SubItems[1].Text;
                oldLines.Add(oldText);
                var urlEncode = HttpUtility.UrlEncode(sb + newText);
                if (urlEncode != null && urlEncode.Length >= 1000)
                {
                    res.Append(TranslateTextViaScreenScraping(sb.ToString(), (comboBoxFrom.SelectedItem as ComboBoxItem).Value + "|" + (comboBoxTo.SelectedItem as ComboBoxItem).Value));
                    sb = new StringBuilder();
                }
                sb.Append("== " + oldText + " ");
            }
            res.Append(TranslateTextViaScreenScraping(sb.ToString(), (comboBoxFrom.SelectedItem as ComboBoxItem).Value + "|" + (comboBoxTo.SelectedItem as ComboBoxItem).Value));

            var lines = new List<string>();
            foreach (string s in res.ToString().Split(new string[] { "==" }, StringSplitOptions.None))
                lines.Add(s.Trim());
            lines.RemoveAt(0);

            if (listViewLanguageTags.SelectedItems.Count != lines.Count)
            {
                MessageBox.Show("Error getting/decoding translation from google!");
                Cursor = Cursors.Default;
                return;
            }

            int index = 0;
            foreach (ListViewItem item in listViewLanguageTags.SelectedItems)
            {
                string s = lines[index];
                string cleanText = s.Replace("</p>", string.Empty).Trim();
                cleanText = cleanText.Replace(" ...", "...");
                cleanText = cleanText.Replace("<br>", Environment.NewLine);
                cleanText = cleanText.Replace("<br/>", Environment.NewLine);
                cleanText = cleanText.Replace("<br />", Environment.NewLine);
                cleanText = cleanText.Replace(Environment.NewLine + " ", Environment.NewLine);
                newText = cleanText;

                oldText = oldLines[index];
                if (oldText.Contains("{0:"))
                {
                    newText = oldText;
                }
                else
                {
                    if (!oldText.Contains(" / "))
                        newText = newText.Replace(" / ", "/");

                    if (!oldText.Contains(" ..."))
                        newText = newText.Replace(" ...", "...");

                    if (!oldText.Contains("& "))
                        newText = newText.Replace("& ", "&");

                    if (!oldText.Contains("# "))
                        newText = newText.Replace("# ", "#");

                    if (!oldText.Contains("@ "))
                        newText = newText.Replace("@ ", "@");

                    if (oldText.Contains("{0}"))
                    {
                        for (int i = 0; i < 50; i++)
                            newText = newText.Replace("(" + i + ")", "{" + i + "}");
                    }
                    translated++;
                }
                item.SubItems[2].Text = newText;
                _change = true;
                index++;
            }


            Cursor = Cursors.Default;
            if (translated == 1 && skipped == 0)
            {
                toolStripStatusLabel1.Text = "One line translated: '" + StringUtils.Max50(oldText) + "' => '" + StringUtils.Max50(newText) + "'";
            }
            else
            {
                if (translated == 1)
                    toolStripStatusLabel1.Text = "One line translated";
                else
                    toolStripStatusLabel1.Text = translated + " lines translated";
                if (skipped > 0)
                    toolStripStatusLabel1.Text += ", " + skipped + " line(s) skipped";
            }
            ListViewLanguageTagsSelectedIndexChanged(null, null);
        }

        private void translateViaGoogleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoogleTranslateSelectedLines();
        }

        private void setValueFromMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_secondLanguageFileName))
                return;

            int transfered = 0;
            string oldText = string.Empty;
            string newText = string.Empty;
            foreach (ListViewItem item in listViewLanguageTags.SelectedItems)
            {
                oldText = item.SubItems[2].Text;
                newText = item.SubItems[1].Text;
                transfered++;
                item.SubItems[2].Text = newText;
                _change = true;
            }
            if (transfered == 1)
                toolStripStatusLabel1.Text = "One line transfered from master: '" + oldText + "' => '" + newText + "'";
            else
                toolStripStatusLabel1.Text = transfered + " line(s) transfered from master";
            ListViewLanguageTagsSelectedIndexChanged(null, null);
        }

        public static void FillComboWithLanguages(ComboBox comboBox)
        {
            comboBox.Items.Add(new ComboBoxItem("AFRIKAANS", "af"));
            comboBox.Items.Add(new ComboBoxItem("ALBANIAN", "sq"));
            comboBox.Items.Add(new ComboBoxItem("AMHARIC", "am"));
            comboBox.Items.Add(new ComboBoxItem("ARABIC", "ar"));
            comboBox.Items.Add(new ComboBoxItem("ARMENIAN", "hy"));
            comboBox.Items.Add(new ComboBoxItem("AZERBAIJANI", "az"));
            comboBox.Items.Add(new ComboBoxItem("BASQUE", "eu"));
            comboBox.Items.Add(new ComboBoxItem("BELARUSIAN", "be"));
            comboBox.Items.Add(new ComboBoxItem("BENGALI", "bn"));
            comboBox.Items.Add(new ComboBoxItem("BIHARI", "bh"));
            comboBox.Items.Add(new ComboBoxItem("BULGARIAN", "bg"));
            comboBox.Items.Add(new ComboBoxItem("BURMESE", "my"));
            comboBox.Items.Add(new ComboBoxItem("CATALAN", "ca"));
            comboBox.Items.Add(new ComboBoxItem("CHEROKEE", "chr"));
            comboBox.Items.Add(new ComboBoxItem("CHINESE", "zh"));
            comboBox.Items.Add(new ComboBoxItem("CHINESE_SIMPLIFIED", "zh-CN"));
            comboBox.Items.Add(new ComboBoxItem("CHINESE_TRADITIONAL", "zh-TW"));
            comboBox.Items.Add(new ComboBoxItem("CROATIAN", "hr"));
            comboBox.Items.Add(new ComboBoxItem("CZECH", "cs"));
            comboBox.Items.Add(new ComboBoxItem("DANISH", "da"));
            comboBox.Items.Add(new ComboBoxItem("DHIVEHI", "dv"));
            comboBox.Items.Add(new ComboBoxItem("DUTCH", "nl"));
            comboBox.Items.Add(new ComboBoxItem("ENGLISH", "en"));
            comboBox.Items.Add(new ComboBoxItem("ESPERANTO", "eo"));
            comboBox.Items.Add(new ComboBoxItem("ESTONIAN", "et"));
            comboBox.Items.Add(new ComboBoxItem("FILIPINO", "tl"));
            comboBox.Items.Add(new ComboBoxItem("FINNISH", "fi"));
            comboBox.Items.Add(new ComboBoxItem("FRENCH", "fr"));
            comboBox.Items.Add(new ComboBoxItem("GALICIAN", "gl"));
            comboBox.Items.Add(new ComboBoxItem("GEORGIAN", "ka"));
            comboBox.Items.Add(new ComboBoxItem("GERMAN", "de"));
            comboBox.Items.Add(new ComboBoxItem("GREEK", "el"));
            comboBox.Items.Add(new ComboBoxItem("GUARANI", "gn"));
            comboBox.Items.Add(new ComboBoxItem("GUJARATI", "gu"));
            comboBox.Items.Add(new ComboBoxItem("HEBREW", "iw"));
            comboBox.Items.Add(new ComboBoxItem("HINDI", "hi"));
            comboBox.Items.Add(new ComboBoxItem("HUNGARIAN", "hu"));
            comboBox.Items.Add(new ComboBoxItem("ICELANDIC", "is"));
            comboBox.Items.Add(new ComboBoxItem("IRISH", "ga"));
            comboBox.Items.Add(new ComboBoxItem("INDONESIAN", "id"));
            comboBox.Items.Add(new ComboBoxItem("INUKTITUT", "iu"));
            comboBox.Items.Add(new ComboBoxItem("ITALIAN", "it"));
            comboBox.Items.Add(new ComboBoxItem("JAPANESE", "ja"));
            comboBox.Items.Add(new ComboBoxItem("KANNADA", "kn"));
            comboBox.Items.Add(new ComboBoxItem("KAZAKH", "kk"));
            comboBox.Items.Add(new ComboBoxItem("KHMER", "km"));
            comboBox.Items.Add(new ComboBoxItem("KOREAN", "ko"));
            comboBox.Items.Add(new ComboBoxItem("KURDISH", "ku"));
            comboBox.Items.Add(new ComboBoxItem("KYRGYZ", "ky"));
            comboBox.Items.Add(new ComboBoxItem("LAOTHIAN", "lo"));
            comboBox.Items.Add(new ComboBoxItem("LATVIAN", "lv"));
            comboBox.Items.Add(new ComboBoxItem("LITHUANIAN", "lt"));
            comboBox.Items.Add(new ComboBoxItem("MACEDONIAN", "mk"));
            comboBox.Items.Add(new ComboBoxItem("MALAY", "ms"));
            comboBox.Items.Add(new ComboBoxItem("MALAYALAM", "ml"));
            comboBox.Items.Add(new ComboBoxItem("MALTESE", "mt"));
            comboBox.Items.Add(new ComboBoxItem("MARATHI", "mr"));
            comboBox.Items.Add(new ComboBoxItem("MONGOLIAN", "mn"));
            comboBox.Items.Add(new ComboBoxItem("NEPALI", "ne"));
            comboBox.Items.Add(new ComboBoxItem("NORWEGIAN", "no"));
            comboBox.Items.Add(new ComboBoxItem("ORIYA", "or"));
            comboBox.Items.Add(new ComboBoxItem("PASHTO", "ps"));
            comboBox.Items.Add(new ComboBoxItem("PERSIAN", "fa"));
            comboBox.Items.Add(new ComboBoxItem("POLISH", "pl"));
            comboBox.Items.Add(new ComboBoxItem("PORTUGUESE", "pt-PT"));
            comboBox.Items.Add(new ComboBoxItem("PUNJABI", "pa"));
            comboBox.Items.Add(new ComboBoxItem("ROMANIAN", "ro"));
            comboBox.Items.Add(new ComboBoxItem("RUSSIAN", "ru"));
            comboBox.Items.Add(new ComboBoxItem("SANSKRIT", "sa"));
            comboBox.Items.Add(new ComboBoxItem("SERBIAN", "sr"));
            comboBox.Items.Add(new ComboBoxItem("SINDHI", "sd"));
            comboBox.Items.Add(new ComboBoxItem("SINHALESE", "si"));
            comboBox.Items.Add(new ComboBoxItem("SLOVAK", "sk"));
            comboBox.Items.Add(new ComboBoxItem("SLOVENIAN", "sl"));
            comboBox.Items.Add(new ComboBoxItem("SPANISH", "es"));
            comboBox.Items.Add(new ComboBoxItem("SWAHILI", "sw"));
            comboBox.Items.Add(new ComboBoxItem("SWEDISH", "sv"));
            comboBox.Items.Add(new ComboBoxItem("TAJIK", "tg"));
            comboBox.Items.Add(new ComboBoxItem("TAMIL", "ta"));
            comboBox.Items.Add(new ComboBoxItem("TAGALOG", "tl"));
            comboBox.Items.Add(new ComboBoxItem("TELUGU", "te"));
            comboBox.Items.Add(new ComboBoxItem("THAI", "th"));
            comboBox.Items.Add(new ComboBoxItem("TIBETAN", "bo"));
            comboBox.Items.Add(new ComboBoxItem("TURKISH", "tr"));
            comboBox.Items.Add(new ComboBoxItem("UKRAINIAN", "uk"));
            comboBox.Items.Add(new ComboBoxItem("URDU", "ur"));
            comboBox.Items.Add(new ComboBoxItem("UZBEK", "uz"));
            comboBox.Items.Add(new ComboBoxItem("UIGHUR", "ug"));
            comboBox.Items.Add(new ComboBoxItem("VIETNAMESE", "vi"));
            comboBox.Items.Add(new ComboBoxItem("WELSH", "cy"));
            comboBox.Items.Add(new ComboBoxItem("YIDDISH", "yi"));
        }

        private void ToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (_change && listViewLanguageTags.Columns.Count == 3 &&
                MessageBox.Show("Changes will be lost. Continue?", "Continue", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            MakeNew();
            toolStripStatusLabel1.Text = "New";
        }

        private void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_originalDocument == null)
                return;

            saveFileDialog1.Title = "Save language file as...";
            saveFileDialog1.DefaultExt = ".xml";
            saveFileDialog1.Filter = "Xml files|*.xml" + "|All files|*.*";
            saveFileDialog1.Title = "Save as language master file";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _secondLanguageFileName = saveFileDialog1.FileName;
                FillOriginalDocumentFromSecondLanguage();
                _originalDocument.Save(saveFileDialog1.FileName);
                _change = false;
                toolStripStatusLabel1.Text = "File saved as " + _secondLanguageFileName;
            }
        }

        private void TextBoxCurrentTextKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control && !e.Alt)
                _change = true;
        }

        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_secondLanguageFileName))
            {
                SaveAsToolStripMenuItemClick(null, null);
            }
            else
            {
                FillOriginalDocumentFromSecondLanguage();

                var settings = new XmlWriterSettings { Indent = true };
                using (var writer = XmlWriter.Create(_secondLanguageFileName, settings))
                {
                    _originalDocument.Save(writer);
                }
                _change = false;
                toolStripStatusLabel1.Text = "File saved - " + _secondLanguageFileName;
            }
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_change && listViewLanguageTags.Columns.Count == 3 &&
                MessageBox.Show("Changes will be lost. Continue?", "Continue", MessageBoxButtons.YesNo) == DialogResult.No)
                e.Cancel = true;
        }

        private void ListViewLanguageTagsDragEnter(object sender, DragEventArgs e)
        { // make sure they're actually dropping files (not text or anything else)
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void ListViewLanguageTagsDragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (!string.IsNullOrEmpty(_secondLanguageFileName))
            {
                MessageBox.Show("Two files already loaded");
                return;
            }

            if (files.Length == 1)
            {

                string fileName = files[0];
                var fi = new FileInfo(fileName);
                if (fi.Length < 1024 * 1024 * 20) // max 20 mb
                {
                    if (treeView1.Nodes.Count == 0)
                        OpenFirstFile(fileName);
                    else
                        OpenSecondFile(fileName);
                }
                else
                {
                    MessageBox.Show(fileName + " is too large (max 20 mb)");
                }
            }
            else
            {
                MessageBox.Show("Only file drop supported");
            }

        }

        private void Form1Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (File.Exists(args[1]))
                    OpenFirstFile(args[1]);
                if (args.Length > 2 && File.Exists(args[2]))
                    OpenSecondFile(args[2]);
            }

        }

        private void ButtonGoToNextBlankLineClick(object sender, EventArgs e)
        {
            int index = 0;
            if (listViewLanguageTags.SelectedItems.Count > 0)
                index = listViewLanguageTags.SelectedItems[0].Index + 1;

            for (; index < listViewLanguageTags.Items.Count; index++)
            {
                if (listViewLanguageTags.Items[index].SubItems.Count > 1 && string.IsNullOrEmpty(listViewLanguageTags.Items[index].SubItems[2].Text))
                {
                    foreach (ListViewItem item in listViewLanguageTags.SelectedItems)
                    {
                        item.Selected = false;
                    }

                    listViewLanguageTags.Items[index].Selected = true;
                    listViewLanguageTags.Items[index].EnsureVisible();
                    return;
                }
            }
        }

        private void HighLightLinesWithSameText()
        {
            foreach (ListViewItem item in listViewLanguageTags.Items)
            {
                if (item.SubItems.Count == 3)
                {
                    if (item.SubItems[1].Text.Trim() == item.SubItems[2].Text.Trim())
                    {
                        item.BackColor = Color.LightYellow;
                        item.UseItemStyleForSubItems = true;
                    }
                    else if (item.SubItems[2].Text.Trim().Length == 0)
                    {
                        item.BackColor = Color.LightPink;
                        item.UseItemStyleForSubItems = true;
                    }
                    else
                    {
                        item.BackColor = listViewLanguageTags.BackColor;
                        item.UseItemStyleForSubItems = true;
                    }
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.DefaultExt = ".xml";
            openFileDialog1.Filter = "Xml files|*.xml" + "|All files|*.*";
            openFileDialog1.Title = "Open language master file";

            var doc = new XmlDocument();
            try
            {
                const string url = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/LanguageMaster.xml";
                var wc = new WebClient();
                var xml = wc.DownloadString(url);
                MakeNew();
                doc.LoadXml(xml);
                OpenFirstXmlDocument(doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return;
            }
            OpenSecondFile();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_formFind == null)
            {
                _formFind = new Find();
            }
            if (_formFind.ShowDialog(this) == DialogResult.OK)
            {
                FindNext();
            }
        }

        private void FindNext()
        {
            if (_formFind.SearchText.Length == 0)
                return;

            int index = 0;
            if (listViewLanguageTags.SelectedItems.Count > 0)
            {
                index = listViewLanguageTags.SelectedItems[0].Index + 1;
            }
            while (index < listViewLanguageTags.Items.Count)
            {
                if (_formFind.SearchTags)
                {
                    if (listViewLanguageTags.Items[index].Text.ToLower().Contains(_formFind.SearchText.ToLower()))
                    {
                        SelectOnlyThis(index);
                        return;
                    }
                }
                else
                {
                    if (listViewLanguageTags.Items[index].SubItems[0].Text.ToLower().Contains(_formFind.SearchText) ||
                        listViewLanguageTags.Items[index].SubItems[1].Text.ToLower().Contains(_formFind.SearchText))
                    {
                        SelectOnlyThis(index);
                        return;
                    }
                }
                index++;
            }
        }

        private void SelectOnlyThis(int index)
        {
            foreach (ListViewItem selectedItem in listViewLanguageTags.SelectedItems)
            {
                selectedItem.Selected = false;
            }
            listViewLanguageTags.Items[index].Selected = true;
            listViewLanguageTags.Items[index].EnsureVisible();
            listViewLanguageTags.Items[index].Focused = true;
        }

        private void listViewLanguageTags_DoubleClick(object sender, EventArgs e)
        {
            if (listViewLanguageTags.SelectedItems.Count != 1)
            {
                return;
            }

            var node = listViewLanguageTags.SelectedItems[0].Tag as XmlNode;
            if (node == null)
            {
                return;
            }

            foreach (TreeNode treeNode in treeView1.Nodes)
            {
                if (treeNode.Tag == node)
                {
                    treeView1.SelectedNode = treeNode;
                    return;
                }
                foreach (TreeNode subTreeNode in treeNode.Nodes)
                {
                    if (subTreeNode.Tag == node)
                    {
                        treeView1.SelectedNode = subTreeNode;
                        return;
                    }
                }
            }
        }

    }
}