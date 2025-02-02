﻿// -----------------------------------------------------------------------
// <copyright file="DataStoreView.Designer.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace UserInterface.Views
{
    /// <summary>
    /// A data store view
    /// </summary>
    public partial class DataStoreView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// List of table names.
        /// </summary>
        private System.Windows.Forms.ListView listBox1;

        /// <summary>
        /// Top panel
        /// </summary>
        private System.Windows.Forms.Panel panel1;

        /// <summary>
        /// A splitter
        /// </summary>
        private System.Windows.Forms.Splitter splitter1;

        /// <summary>
        /// An output grid view.
        /// </summary>
        private GridView gridView;

        /// <summary>
        /// The main tab control.
        /// </summary>
        private System.Windows.Forms.TabControl tabControl1;

        /// <summary>
        /// First tab page
        /// </summary>
        private System.Windows.Forms.TabPage tabPage1;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox1 = new System.Windows.Forms.ListView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.gridView = new GridView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBox1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listBox1.HideSelection = false;
            this.listBox1.Location = new System.Drawing.Point(4, 52);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4);
            this.listBox1.MultiSelect = false;
            this.listBox1.Name = "listBox1";
            this.listBox1.ShowGroups = false;
            this.listBox1.Size = new System.Drawing.Size(604, 118);
            this.listBox1.TabIndex = 1;
            this.listBox1.UseCompatibleStateImageBehavior = false;
            this.listBox1.View = System.Windows.Forms.View.List;
            this.listBox1.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.OnTableSelectedInGrid);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(604, 48);
            this.panel1.TabIndex = 3;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(17, 12);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(181, 21);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "Auto export to text files?";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.OnAutoExportCheckedChanged);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(4, 170);
            this.splitter1.Margin = new System.Windows.Forms.Padding(4);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(604, 4);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // gridView
            // 
            this.gridView.AutoFilterOn = false;
            this.gridView.DataSource = null;
            this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridView.GetCurrentCell = null;
            this.gridView.Location = new System.Drawing.Point(4, 174);
            this.gridView.Margin = new System.Windows.Forms.Padding(5);
            this.gridView.Name = "gridView";
            this.gridView.NumericFormat = null;
            this.gridView.ReadOnly = false;
            this.gridView.RowCount = 0;
            this.gridView.Size = new System.Drawing.Size(604, 394);
            this.gridView.TabIndex = 5;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(620, 601);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.gridView);
            this.tabPage1.Controls.Add(this.splitter1);
            this.tabPage1.Controls.Add(this.listBox1);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(612, 572);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Output";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // DataStoreView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DataStoreView";
            this.Size = new System.Drawing.Size(620, 601);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
    }
}
