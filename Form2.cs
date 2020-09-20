using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace _JPK__XML_to_XSD_Validator
{
    public partial class Form2 : Form
    {
        private DataSet dataSet = new DataSet();
        private XmlNode node;
        private XmlDocument xDoc;

        public Form2(XmlNode _node, XmlDocument _xDoc)
        {
            InitializeComponent();
            xDoc = _xDoc;
            node = _node;
            this.Text = node.Name;
            CreateGridView(node.OuterXml);
        }

        private void CreateGridView(string outerXml)
        {
            try
            {
                dataSet.ReadXml(XmlReader.Create(new StringReader(outerXml)));
                dataGridView1.DataSource = dataSet.Tables[0];
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                this.Hide();
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            var msg = MessageBox.Show("Czy chcesz zapisać zmiany?", "", MessageBoxButtons.YesNoCancel);
            if(msg == DialogResult.Yes)
            {
                Form1 form1 = new Form1();

                XmlDocument new_xDoc = new XmlDocument();
                new_xDoc.LoadXml(dataSet.GetXml());
                XmlNode newNode = new_xDoc.DocumentElement;

                var nsmgr = new XmlNamespaceManager(xDoc.NameTable);
                nsmgr.AddNamespace("tns", "http://crd.gov.pl/wzor/2020/03/06/9196/");
                XmlNode oldNode = xDoc.DocumentElement.SelectSingleNode(form1.GetXPath(node), nsmgr);
                string xpath = form1.GetXPath(node);
                form1.UpdateXML(newNode, oldNode, xpath);
            }
            else if(msg == DialogResult.No)
            {

            }
            else if (msg == DialogResult.Cancel)
            {
                
            }
        }
    }
}
