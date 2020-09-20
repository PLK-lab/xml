using _JPK__XML_to_XSD_Validator.Properties;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace _JPK__XML_to_XSD_Validator
{
    public partial class Form1 : Form
    {
        // Zmienne
        private string SchemaFilePath = null, XmlFilePath;
        private static Form1 instance;
        private bool expanded = false;

        // Funkcje startowe
        public Form1()
        {
            instance = this;
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ReadConfig("config.txt");
            pictureBox1.BackColor = Color.Transparent;
            fastColoredTextBox1.Language = FastColoredTextBoxNS.Language.XML;
        }
        private void ReadConfig(string fileName)
        {
            string _schemaFilePath = null;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    _schemaFilePath = sr.ReadLine();
                    if (File.Exists(_schemaFilePath))
                    {
                        SchemaFilePath = _schemaFilePath;
                        instance.XsdStatusIcon.Image = Resources.okStatus;
                    }
                    else
                    {
                        instance.XsdStatusIcon.Image = Resources.errorStatus;
                    }
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void defaultSchema(object sender, EventArgs e)
        {
            if (SchemaFilePath != null)
            {
                File.WriteAllText("config.txt", SchemaFilePath);
                MessageBox.Show("Plik " + SchemaFilePath + "\nbędzie teraz używany jako domyślny schemat!");
            }
            else
            {
                MessageBox.Show("Ścieżka pliku nie może być pusta.", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Funkcje XML'owe
        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    instance.richTextBox1.Text = Environment.NewLine + "Błąd: " + e.Message;
                    break;

                case XmlSeverityType.Warning:
                    instance.richTextBox1.Text = Environment.NewLine + "Ostrzeżenie: " + e.Message;
                    break;
            }
        }

        private void populateTreeview()
        {
            try
            {
                treeView1.Nodes.Clear();
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(XmlFilePath);
                TreeNode tNode = new TreeNode();
                tNode.Nodes.Add(new TreeNode(xDoc.DocumentElement.Name));
                tNode = tNode.Nodes[0];
                addTreeNode(xDoc.DocumentElement, tNode);
                treeView1.Nodes.Insert(0, tNode);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void addTreeNode(XmlNode xmlNode, TreeNode treeNode)
        {    
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList xNodeList;
            if (xmlNode.HasChildNodes)
            {
                xNodeList = xmlNode.ChildNodes;
                for (int x = 0; x <= xNodeList.Count - 1; x++)
                {
                    xNode = xmlNode.ChildNodes[x];
                    treeNode.Nodes.Add(new TreeNode(xNode.Name));
                    tNode = treeNode.Nodes[x];
                    addTreeNode(xNode, tNode);
                }
            }
            else 
                treeNode.Text = xmlNode.OuterXml.Trim();
        }

        private void ValidXML()
        {
            if (SchemaFilePath != null & XmlFilePath != null)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.Schemas.Add("http://crd.gov.pl/wzor/2020/03/06/9196/", SchemaFilePath);
                    settings.ValidationType = ValidationType.Schema;
                    XmlReader reader = XmlReader.Create(new StringReader(fastColoredTextBox1.Text), settings);
                    XmlDocument document = new XmlDocument();
                    document.Load(reader);



                    ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);
                    // the following call to Validate succeeds.
                    document.Validate(eventHandler);
                    Cursor.Current = Cursors.Default;
                    richTextBox1.Text = "Brak błędów!";
                }

                catch (Exception ex)
                {
                    richTextBox1.Text = ex.Message;
                }
            }

            else if (SchemaFilePath == null)
            {
                MessageBox.Show($"Ścieżka pliku XSD nie może być pusta!", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            else if (XmlFilePath == null)
            {
                MessageBox.Show($"Ścieżka pliku XML nie może być pusta!", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateXML(XmlNode newNode, XmlNode oldNode, string xPath)
        {
            MessageBox.Show(currentXml);
            /*XDocument xdoc1 = XDocument.Parse(fastColoredTextBox1.Text);
            XElement old = xdoc1.XPathSelectElement(xPath);
            XDocument xdoc2 = XDocument.Parse(newNode.OuterXml);
            XElement new = xdoc2.Root;

            old.ReplaceWith(new);
            fastColoredTextBox1.Text = xdoc1.ToString();*/
        }

        public string GetXPath(XmlNode xmlNode)
        {
            string pathName = xmlNode.Name;
            XmlNode node = xmlNode;
            while (true)
            {
               if (node.ParentNode.Name != "#document")
                {
                    pathName = $"{node.ParentNode.Name}/{pathName}";
                }
                else
                {
                    return pathName;

                }
                node = node.ParentNode;
            }
        }

            // Funkcje przycisków
        private void zapiszToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Plik XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                fastColoredTextBox1.SaveToFile(sfd.FileName, Encoding.UTF8);
            }
        }
        private void sprawdz_Click(object sender, EventArgs e)
        {
            ValidXML();
        }
        private void XML_OFD(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Plik XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                XmlFilePath = ofd.FileName;
                fastColoredTextBox1.OpenFile(XmlFilePath, Encoding.UTF8);
                ReloadTreeView();
                XmlStatusIcon.Image = Resources.okStatus;
                Cursor.Current = Cursors.Default;
            }
        }
        private void rozwin_Click(object sender, EventArgs e)
        {
            if (!expanded)
            {
                treeView1.ExpandAll();
                button1.Text = "Zwiń";
                expanded = true;
            }
            else
            {
                treeView1.CollapseAll();
                button1.Text = "Rozwiń";
                expanded = false;
            }
        }
        private void reload_Click(object sender, EventArgs e)
        {
            ReloadTreeView();
            button3.Text = "Załaduj ponownie";
        }
        private void ReloadTreeView()
        {
            if (richTextBox1.Text == "Brak błędów!")
            {
                Cursor.Current = Cursors.WaitCursor;
                populateTreeview();
                Cursor.Current = Cursors.Default;
            }
            else if (String.IsNullOrEmpty(richTextBox1.Text))
            {
                ValidXML();
                ReloadTreeView();
            }
            else
            {
                MessageBox.Show("Plik XML wymaga korekcji, ponieważ nie jest zgodny z wybranym schematem!", "Drzewko nie zostanie utworzone", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            DataSet ds = new DataSet();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(XmlFilePath);
            
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        public string currentXml;
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                try
                {
                    currentXml = fastColoredTextBox1.Text;
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(fastColoredTextBox1.Text);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                    nsmgr.AddNamespace("tns", "http://crd.gov.pl/wzor/2020/03/06/9196/");
                    string path = treeView1.SelectedNode.FullPath.Replace(@"\", "/");
                    XmlNode node = document.SelectSingleNode(path, nsmgr);
                    Form2 form2 = new Form2(node, document);
                    form2.Show();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void zawijanieWierszówToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.WordWrap = !zawijanieWierszówToolStripMenuItem.Checked;
            zawijanieWierszówToolStripMenuItem.Checked = !zawijanieWierszówToolStripMenuItem.Checked;
        }
        private void XSD_OFD(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Plik XSD (*.xsd)|*.xsd|Wszystkie pliki (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SchemaFilePath = ofd.FileName;
                XsdStatusIcon.Image = Resources.okStatus;
            }
        }
    }
}


