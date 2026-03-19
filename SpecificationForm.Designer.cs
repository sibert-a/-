namespace SpecificationApp
{
    partial class SpecificationForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TreeView treeViewSpec;
        private System.Windows.Forms.ComboBox cmbComponent;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.treeViewSpec = new System.Windows.Forms.TreeView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFind = new System.Windows.Forms.Button();
            this.cmbComponent = new System.Windows.Forms.ComboBox();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();

            // treeViewSpec
            this.treeViewSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewSpec.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.treeViewSpec.FullRowSelect = true;
            this.treeViewSpec.HideSelection = false;
            this.treeViewSpec.Location = new System.Drawing.Point(0, 50);
            this.treeViewSpec.Name = "treeViewSpec";
            this.treeViewSpec.Size = new System.Drawing.Size(600, 400);
            this.treeViewSpec.TabIndex = 1;

            // ВАЖНО! Эти свойства включают отображение значков
            this.treeViewSpec.ShowLines = true;
            this.treeViewSpec.ShowPlusMinus = true;
            this.treeViewSpec.ShowRootLines = true;

            this.treeViewSpec.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewSpec_NodeMouseClick);
            this.treeViewSpec.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewSpec_MouseDown);

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.btnFind);
            this.panelTop.Controls.Add(this.cmbComponent);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(600, 50);
            this.panelTop.TabIndex = 0;

            // label1
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 15);
            this.label1.TabIndex = 2;

            // cmbComponent - СДВИНУТ ЛЕВЕЕ И РАСШИРЕН
            this.cmbComponent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbComponent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbComponent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbComponent.Location = new System.Drawing.Point(30, 15); // Было 58, стало 45
            this.cmbComponent.Name = "cmbComponent";
            this.cmbComponent.Size = new System.Drawing.Size(250, 23); // Было 200, стало 250
            this.cmbComponent.TabIndex = 0;

            // btnFind - СДВИНУТ ИЗ-ЗА РАСШИРЕНИЯ КОМБОБОКСА
            this.btnFind.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnFind.FlatAppearance.BorderSize = 0;
            this.btnFind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFind.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnFind.Location = new System.Drawing.Point(280, 13); // Было 265, стало 300
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(80, 25);
            this.btnFind.TabIndex = 1;
            this.btnFind.Text = "Найти";
            this.btnFind.UseVisualStyleBackColor = false;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);

            // SpecificationForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Controls.Add(this.treeViewSpec);
            this.Controls.Add(this.panelTop);
            this.MinimumSize = new System.Drawing.Size(616, 489);
            this.Name = "SpecificationForm";
            this.Text = "Спецификация";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}