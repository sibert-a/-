using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class ComponentsForm : Form
    {
        private FileManager fileManager;

        public ComponentsForm(FileManager manager)
        {
            InitializeComponent();
            fileManager = manager;
            LoadComponents();
        }

        private void LoadComponents()
        {
            listViewComponents.Items.Clear();
            var components = fileManager.GetAllComponents();

            foreach (var comp in components)
            {
                ListViewItem item = new ListViewItem(comp.Name);
                item.SubItems.Add(comp.Type);
                listViewComponents.Items.Add(item);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            panelEdit.Visible = true;
            txtName.Clear();
            cmbType.SelectedIndex = -1;
            txtName.Focus();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listViewComponents.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewComponents.SelectedItems[0];
                txtName.Text = item.SubItems[0].Text;
                cmbType.Text = item.SubItems[1].Text;
                panelEdit.Visible = true;
                txtName.Focus();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listViewComponents.SelectedItems.Count > 0)
            {
                string name = listViewComponents.SelectedItems[0].SubItems[0].Text;
                if (MessageBox.Show($"Удалить компонент '{name}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        fileManager.DeleteComponent(name);
                        LoadComponents();
                        MessageBox.Show("Компонент помечен на удаление", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (listViewComponents.SelectedItems.Count > 0)
            {
                string name = listViewComponents.SelectedItems[0].SubItems[0].Text;
                try
                {
                    fileManager.RestoreComponent(name);
                    LoadComponents();
                    MessageBox.Show("Компонент восстановлен", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string name = txtName.Text.Trim();
                string type = cmbType.SelectedItem.ToString();

                fileManager.InputComponent(name, type);

                LoadComponents();
                panelEdit.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listViewComponents_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = listViewComponents.SelectedItems.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnRestore.Enabled = hasSelection;
        }
    }
}