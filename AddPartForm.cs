using System;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class AddPartForm : Form
    {
        private FileManager fileManager;
        private string componentName;

        public AddPartForm(FileManager manager, string compName)
        {
            InitializeComponent();
            fileManager = manager;
            componentName = compName;
            LoadParts();
        }

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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}