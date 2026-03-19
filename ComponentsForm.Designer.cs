namespace SpecificationApp
{
    partial class ComponentsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView listViewComponents;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

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
            this.listViewComponents = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();

            // listViewComponents
            this.listViewComponents.BackColor = System.Drawing.Color.White;
            this.listViewComponents.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.columnHeader1,
                this.columnHeader2});
            this.listViewComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewComponents.FullRowSelect = true;
            this.listViewComponents.GridLines = false;
            this.listViewComponents.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewComponents.HideSelection = false;
            this.listViewComponents.Location = new System.Drawing.Point(0, 40);
            this.listViewComponents.MultiSelect = false;
            this.listViewComponents.Name = "listViewComponents";
            this.listViewComponents.Size = new System.Drawing.Size(600, 410);
            this.listViewComponents.TabIndex = 0;
            this.listViewComponents.UseCompatibleStateImageBehavior = false;
            this.listViewComponents.View = System.Windows.Forms.View.Details;
            this.listViewComponents.SelectedIndexChanged += new System.EventHandler(this.listViewComponents_SelectedIndexChanged);

            // columnHeader1
            this.columnHeader1.Text = "Наименование";
            this.columnHeader1.Width = 300;

            // columnHeader2
            this.columnHeader2.Text = "Тип";
            this.columnHeader2.Width = 150;

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.btnAdd);
            this.panelTop.Controls.Add(this.btnEdit);
            this.panelTop.Controls.Add(this.btnDelete);
            this.panelTop.Controls.Add(this.btnSave);
            this.panelTop.Controls.Add(this.btnCancel);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(600, 40);
            this.panelTop.TabIndex = 1;

            // btnAdd
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnAdd.ForeColor = System.Drawing.Color.Black;
            this.btnAdd.Location = new System.Drawing.Point(5, 8);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(80, 25);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "Добавить";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // btnEdit
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnEdit.Enabled = false;
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnEdit.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
            this.btnEdit.Location = new System.Drawing.Point(90, 8);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(80, 25);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "Изменить";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            // btnDelete
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
            this.btnDelete.Location = new System.Drawing.Point(175, 8);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 25);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Удалить";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // btnSave
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnSave.Enabled = false;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSave.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
            this.btnSave.Location = new System.Drawing.Point(260, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 25);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnCancel.Enabled = false;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
            this.btnCancel.Location = new System.Drawing.Point(345, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 25);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // panelBottom
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBottom.Controls.Add(this.cmbType);
            this.panelBottom.Controls.Add(this.txtName);
            this.panelBottom.Controls.Add(this.label2);
            this.panelBottom.Controls.Add(this.label1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 450);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(600, 50);
            this.panelBottom.TabIndex = 2;
            this.panelBottom.Visible = false;

            // label1
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(10, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Наименование";

            // txtName
            this.txtName.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtName.Location = new System.Drawing.Point(105, 15);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(180, 21);
            this.txtName.TabIndex = 1;

            // label2
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(295, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Тип";

            // cmbType
            this.cmbType.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbType.Items.AddRange(new object[] {
                "Изделие",
                "Узел",
                "Деталь"});
            this.cmbType.Location = new System.Drawing.Point(330, 15);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(100, 23);
            this.cmbType.TabIndex = 3;

            // ComponentsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Controls.Add(this.listViewComponents);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "ComponentsForm";
            this.Text = "Список компонентов";
            this.panelTop.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}