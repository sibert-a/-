using System;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class AddPartForm : Form
    {
        private FileManager fileManager;
        private string componentName;

        // Конструктор - инициализирует форму, сохраняет менеджер и имя компонента, загружает список деталей
        public AddPartForm(FileManager manager, string compName)
        {
            InitializeComponent();
            fileManager = manager;
            componentName = compName;
            LoadParts();
        }

        // Загружает в выпадающий список все компоненты, кроме текущего
        private void LoadParts()
        {
            cmbPart.Items.Clear();
            var components = fileManager.GetAllComponents();
            foreach (var comp in components)
            {
                if (comp.Name != componentName)
                {
                    cmbPart.Items.Add(comp.Name);
                }
            }
        }

        // Обрабатывает нажатие OK: проверяет выбор, добавляет деталь к компоненту, закрывает форму
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cmbPart.SelectedItem == null)
            {
                MessageBox.Show("Выберите комплектующее", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string partName = cmbPart.SelectedItem.ToString();
                short quantity = (short)nudQuantity.Value;
                fileManager.InputPart(componentName, partName, quantity);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обрабатывает нажатие Cancel: закрывает форму с отменой
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}