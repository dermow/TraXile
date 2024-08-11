
namespace TraXile
{
    partial class FileSelectScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileSelectScreen));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panelAutoSelect = new System.Windows.Forms.Panel();
            this.labelDiscoverStatus = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            this.panelAutoSelect.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(731, 88);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(518, 219);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(204, 19);
            this.button1.TabIndex = 1;
            this.button1.Text = "Select Client.txt manually";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.panelAutoSelect, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(737, 437);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // panelAutoSelect
            // 
            this.panelAutoSelect.Controls.Add(this.groupBox1);
            this.panelAutoSelect.Controls.Add(this.button2);
            this.panelAutoSelect.Controls.Add(this.button1);
            this.panelAutoSelect.Controls.Add(this.labelDiscoverStatus);
            this.panelAutoSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAutoSelect.Location = new System.Drawing.Point(3, 97);
            this.panelAutoSelect.Name = "panelAutoSelect";
            this.panelAutoSelect.Size = new System.Drawing.Size(731, 317);
            this.panelAutoSelect.TabIndex = 1;
            // 
            // labelDiscoverStatus
            // 
            this.labelDiscoverStatus.AutoSize = true;
            this.labelDiscoverStatus.ForeColor = System.Drawing.SystemColors.Control;
            this.labelDiscoverStatus.Location = new System.Drawing.Point(14, 11);
            this.labelDiscoverStatus.Name = "labelDiscoverStatus";
            this.labelDiscoverStatus.Size = new System.Drawing.Size(589, 13);
            this.labelDiscoverStatus.TabIndex = 0;
            this.labelDiscoverStatus.Text = "TraXile needs the path to your client log file to work. Select one of the discove" +
    "red installations, or choose the path manually.";
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(17, 219);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(160, 34);
            this.button2.TabIndex = 2;
            this.button2.Text = "Start using selected file";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Location = new System.Drawing.Point(17, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(705, 178);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Installations";
            // 
            // FileSelectScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(737, 437);
            this.Controls.Add(this.tableLayoutPanel4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FileSelectScreen";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.panelAutoSelect.ResumeLayout(false);
            this.panelAutoSelect.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panelAutoSelect;
        private System.Windows.Forms.Label labelDiscoverStatus;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}