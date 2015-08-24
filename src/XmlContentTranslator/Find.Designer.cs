namespace XmlContentTranslator
{
    partial class Find
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonFind = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.radioButtonTags = new System.Windows.Forms.RadioButton();
            this.groupBoxSearchIn = new System.Windows.Forms.GroupBox();
            this.radioButtonText = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBoxSearchIn.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonFind
            // 
            this.buttonFind.Location = new System.Drawing.Point(244, 25);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(98, 23);
            this.buttonFind.TabIndex = 3;
            this.buttonFind.Text = "&Find";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find what:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 25);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(226, 20);
            this.textBox1.TabIndex = 1;
            // 
            // radioButtonTags
            // 
            this.radioButtonTags.AutoSize = true;
            this.radioButtonTags.Location = new System.Drawing.Point(19, 19);
            this.radioButtonTags.Name = "radioButtonTags";
            this.radioButtonTags.Size = new System.Drawing.Size(49, 17);
            this.radioButtonTags.TabIndex = 0;
            this.radioButtonTags.TabStop = true;
            this.radioButtonTags.Text = "Tags";
            this.radioButtonTags.UseVisualStyleBackColor = true;
            // 
            // groupBoxSearchIn
            // 
            this.groupBoxSearchIn.Controls.Add(this.radioButtonText);
            this.groupBoxSearchIn.Controls.Add(this.radioButtonTags);
            this.groupBoxSearchIn.Location = new System.Drawing.Point(15, 54);
            this.groupBoxSearchIn.Name = "groupBoxSearchIn";
            this.groupBoxSearchIn.Size = new System.Drawing.Size(223, 51);
            this.groupBoxSearchIn.TabIndex = 2;
            this.groupBoxSearchIn.TabStop = false;
            this.groupBoxSearchIn.Text = "Search in";
            // 
            // radioButtonText
            // 
            this.radioButtonText.AutoSize = true;
            this.radioButtonText.Location = new System.Drawing.Point(86, 19);
            this.radioButtonText.Name = "radioButtonText";
            this.radioButtonText.Size = new System.Drawing.Size(46, 17);
            this.radioButtonText.TabIndex = 1;
            this.radioButtonText.TabStop = true;
            this.radioButtonText.Text = "Text";
            this.radioButtonText.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(244, 54);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "C&ancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Find
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 125);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBoxSearchIn);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonFind);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Find";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Find";
            this.Shown += new System.EventHandler(this.Find_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Find_KeyDown);
            this.groupBoxSearchIn.ResumeLayout(false);
            this.groupBoxSearchIn.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.RadioButton radioButtonTags;
        private System.Windows.Forms.GroupBox groupBoxSearchIn;
        private System.Windows.Forms.RadioButton radioButtonText;
        private System.Windows.Forms.Button button1;
    }
}