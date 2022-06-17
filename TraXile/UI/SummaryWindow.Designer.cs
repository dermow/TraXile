namespace TraXile.UI
{
    partial class SummaryWindow
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelDuaration = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelAvgDuration = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.45705F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 76.69173F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23.30827F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(457, 133);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelAvgDuration);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.labelDuaration);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.labelCount);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(451, 96);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Summary";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Noto Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "Activity count:";
            // 
            // labelCount
            // 
            this.labelCount.AutoSize = true;
            this.labelCount.Font = new System.Drawing.Font("Noto Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCount.Location = new System.Drawing.Point(156, 31);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(14, 14);
            this.labelCount.TabIndex = 1;
            this.labelCount.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Noto Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 14);
            this.label3.TabIndex = 2;
            this.label3.Text = "Total duration:";
            // 
            // labelDuaration
            // 
            this.labelDuaration.AutoSize = true;
            this.labelDuaration.Font = new System.Drawing.Font("Noto Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDuaration.Location = new System.Drawing.Point(156, 49);
            this.labelDuaration.Name = "labelDuaration";
            this.labelDuaration.Size = new System.Drawing.Size(14, 14);
            this.labelDuaration.TabIndex = 3;
            this.labelDuaration.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Noto Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(9, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 14);
            this.label5.TabIndex = 4;
            this.label5.Text = "Avg. duration:";
            // 
            // labelAvgDuration
            // 
            this.labelAvgDuration.AutoSize = true;
            this.labelAvgDuration.Font = new System.Drawing.Font("Noto Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAvgDuration.Location = new System.Drawing.Point(156, 66);
            this.labelAvgDuration.Name = "labelAvgDuration";
            this.labelAvgDuration.Size = new System.Drawing.Size(14, 14);
            this.labelAvgDuration.TabIndex = 5;
            this.labelAvgDuration.Text = "0";
            // 
            // SummaryWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 133);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SummaryWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "SummaryWindow";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelAvgDuration;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelDuaration;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Label label1;
    }
}