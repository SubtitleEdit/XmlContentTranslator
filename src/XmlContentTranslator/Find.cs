using System;
using System.Windows.Forms;

namespace XmlContentTranslator
{
    public partial class Find : Form
    {
        public bool SearchTags { get; set; }
        public string SearchText { get; set; }

        public Find()
        {
            InitializeComponent();
            radioButtonText.Checked = true;
        }

        private void Find_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                buttonFind_Click(sender, e);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonFind_Click(object sender, EventArgs e)
        {
            SearchTags = radioButtonTags.Checked;
            SearchText = textBox1.Text;
            DialogResult = DialogResult.OK;
        }

        private void Find_Shown(object sender, EventArgs e)
        {
            textBox1.Focus();
            textBox1.SelectAll();
        }
    }
}
