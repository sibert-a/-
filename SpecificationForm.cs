using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class SpecificationForm : Form
    {
        private FileManager fileManager;
        private string currentComponent;

        public SpecificationForm(FileManager manager)
        {
            InitializeComponent();
            fileManager = manager;
            LoadComponents();

            // Подписываемся на событие загрузки формы
            this.Load += SpecificationForm_Load;
        }

        private void SpecificationForm_Load(object sender, EventArgs e)
        {
            // Обновляем список компонентов при загрузке формы
            LoadComponents();
        }

        // Добавьте метод для обновления списка (можно вызывать из других мест)
        public void RefreshComponents()
        {
            LoadComponents();
        }

        private void LoadComponents()
        {
            cmbComponent.Items.Clear();
            var components = fileManager.GetAllComponents();

            foreach (var comp in components)
            {
                // Добавляем все компоненты, кроме деталей (или можно все)
                if (comp.Type != "Деталь")
                {
                    cmbComponent.Items.Add(comp.Name);
                }
            }

            // Если есть элементы, выбираем первый
            if (cmbComponent.Items.Count > 0)
            {
                cmbComponent.SelectedIndex = 0;
            }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (cmbComponent.SelectedItem != null)
            {
                currentComponent = cmbComponent.SelectedItem.ToString();
                LoadSpecification(currentComponent);
                panelButtons.Visible = true;
            }
        }

        private void LoadSpecification(string compName)
        {
            treeViewSpec.Nodes.Clear();
            var spec = fileManager.GetSpecification(compName);

            TreeNode root = new TreeNode(compName);
            AddSpecNodes(root, spec);
            treeViewSpec.Nodes.Add(root);
            treeViewSpec.ExpandAll();
        }

        private void AddSpecNodes(TreeNode parent, List<SpecificationItem> items)
        {
            foreach (var item in items)
            {
                TreeNode node = new TreeNode($"{item.Name} ({item.Quantity} шт.)");
                parent.Nodes.Add(node);

                if (item.Children != null && item.Children.Count > 0)
                {
                    AddSpecNodes(node, item.Children);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentComponent))
            {
                using (var addForm = new AddPartForm(fileManager, currentComponent))
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadSpecification(currentComponent);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (treeViewSpec.SelectedNode != null && treeViewSpec.SelectedNode.Parent != null)
            {
                string nodeText = treeViewSpec.SelectedNode.Text;
                string partName = nodeText.Substring(0, nodeText.LastIndexOf('(')).Trim();

                if (MessageBox.Show($"Удалить '{partName}' из спецификации?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        fileManager.DeletePart(currentComponent, partName);
                        LoadSpecification(currentComponent);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}