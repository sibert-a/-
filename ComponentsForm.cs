using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class ComponentsForm : Form
    {
        private FileManager fileManager;
        private bool isEditMode = false;
        private string editingName = "";

        public ComponentsForm(FileManager manager)
        {
            InitializeComponent();
            fileManager = manager;
            LoadComponents();

            // Устанавливаем цвет выделения для ListView
            listViewComponents.OwnerDraw = true;
            listViewComponents.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            listViewComponents.DrawItem += (s, e) => e.DrawDefault = true;
            listViewComponents.DrawSubItem += (s, e) =>
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(220, 220, 220)), e.Bounds);
                    e.DrawText(System.Windows.Forms.TextFormatFlags.Left);
                }
                else
                {
                    e.DrawDefault = true;
                }
            };

            // Изначально все кнопки кроме Добавить неактивны
            UpdateButtonStates();
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
            isEditMode = false;
            panelBottom.Visible = true;
            txtName.Clear();
            cmbType.SelectedIndex = -1;
            txtName.Focus();

            // Активируем кнопки Сохранить и Отмена
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnSave.ForeColor = System.Drawing.Color.Black;
            btnCancel.ForeColor = System.Drawing.Color.Black;

            // Делаем остальные кнопки неактивными
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;

            // Обновляем цвета
            btnAdd.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listViewComponents.SelectedItems.Count > 0)
            {
                isEditMode = true;
                ListViewItem item = listViewComponents.SelectedItems[0];
                editingName = item.SubItems[0].Text;
                txtName.Text = editingName;
                cmbType.Text = item.SubItems[1].Text;

                panelBottom.Visible = true;
                txtName.Focus();

                // Активируем кнопки Сохранить и Отмена
                btnSave.Enabled = true;
                btnCancel.Enabled = true;
                btnSave.ForeColor = System.Drawing.Color.Black;
                btnCancel.ForeColor = System.Drawing.Color.Black;

                // Делаем остальные кнопки неактивными
                btnAdd.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;

                // Обновляем цвета
                btnAdd.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
                btnEdit.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
                btnDelete.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
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

                if (isEditMode)
                {
                    // Для редактирования нужно сначала удалить старый, потом добавить новый
                    // Или использовать специальный метод редактирования, если он есть в FileManager
                    MessageBox.Show("Редактирование будет доступно в следующей версии", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    fileManager.InputComponent(name, type);
                }

                LoadComponents();
                CancelEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelEdit();
        }

        private void CancelEdit()
        {
            panelBottom.Visible = false;

            // Деактивируем кнопки Сохранить и Отмена
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            btnSave.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
            btnCancel.ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);

            // Восстанавливаем активность кнопки Добавить
            btnAdd.Enabled = true;
            btnAdd.ForeColor = System.Drawing.Color.Black;

            // Обновляем состояние остальных кнопок
            UpdateButtonStates();
        }

        private void listViewComponents_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Обновляем состояние кнопок только если не в режиме редактирования
            if (!panelBottom.Visible)
            {
                UpdateButtonStates();
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = listViewComponents.SelectedItems.Count > 0;

            // Кнопка Добавить всегда активна (если не в режиме редактирования)
            btnAdd.Enabled = !panelBottom.Visible;
            btnAdd.ForeColor = btnAdd.Enabled ? System.Drawing.Color.Black : System.Drawing.Color.FromArgb(128, 128, 128);

            // Кнопки Изменить и Удалить активны только если есть выделение и не в режиме редактирования
            btnEdit.Enabled = hasSelection && !panelBottom.Visible;
            btnDelete.Enabled = hasSelection && !panelBottom.Visible;

            // Обновляем цвета кнопок
            btnEdit.ForeColor = btnEdit.Enabled ? System.Drawing.Color.Black : System.Drawing.Color.FromArgb(128, 128, 128);
            btnDelete.ForeColor = btnDelete.Enabled ? System.Drawing.Color.Black : System.Drawing.Color.FromArgb(128, 128, 128);

            // Кнопки Сохранить и Отмена управляются отдельно в режиме редактирования
        }
    }
}