using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class SpecificationForm : Form
    {
        private FileManager fileManager;
        private string currentComponent;
        private ContextMenuStrip contextMenu;
        private TreeNode selectedNode;

        public SpecificationForm(FileManager manager)
        {
            InitializeComponent();
            fileManager = manager;
            LoadComponents();
            this.Load += SpecificationForm_Load;
            CreateContextMenu();

            // Настройка TreeView для гарантированного отображения значков
            treeViewSpec.ShowLines = true;
            treeViewSpec.ShowPlusMinus = true;
            treeViewSpec.ShowRootLines = true;
        }

        private void SpecificationForm_Load(object sender, EventArgs e)
        {
            LoadComponents();
        }

        private void CreateContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = Color.White;
            contextMenu.ForeColor = Color.Black;

            ToolStripMenuItem addItem = new ToolStripMenuItem("Добавить");
            addItem.Click += AddItem_Click;
            addItem.ForeColor = Color.Black;

            ToolStripMenuItem editItem = new ToolStripMenuItem("Изменить");
            editItem.Click += EditItem_Click;
            editItem.ForeColor = Color.Black;

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Удалить");
            deleteItem.Click += DeleteItem_Click;
            deleteItem.ForeColor = Color.Black;

            contextMenu.Items.Add(addItem);
            contextMenu.Items.Add(editItem);
            contextMenu.Items.Add(deleteItem);

            treeViewSpec.ContextMenuStrip = contextMenu;
        }

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
                if (comp.Type != "Деталь")
                {
                    cmbComponent.Items.Add(comp.Name);
                }
            }

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
            }
        }

        private void LoadSpecification(string compName)
        {
            treeViewSpec.Nodes.Clear();
            var spec = fileManager.GetSpecification(compName);

            // Создаем корневой узел (изделие)
            TreeNode root = new TreeNode(compName);
            root.Tag = "root";

            // Добавляем все дочерние узлы
            bool hasChildren = AddSpecNodes(root, spec);

            // Добавляем корневой узел в дерево
            treeViewSpec.Nodes.Add(root);

            // Разворачиваем все узлы, чтобы увидеть значки
            root.ExpandAll();

            // Принудительно обновляем дерево
            treeViewSpec.Refresh();

            // Отладочная информация
            System.Diagnostics.Debug.WriteLine($"Загружена спецификация для {compName}");
            System.Diagnostics.Debug.WriteLine($"Корневой узел имеет детей: {root.Nodes.Count > 0}");

            // Проверяем каждый узел
            CheckNodes(root);
        }

        private bool AddSpecNodes(TreeNode parent, List<SpecificationItem> items)
        {
            if (items == null || items.Count == 0)
                return false;

            foreach (var item in items)
            {
                // Создаем узел для текущего элемента
                TreeNode node = new TreeNode($"{item.Name} ({item.Quantity} шт.)");
                node.Tag = item.Name;

                // Рекурсивно добавляем дочерние элементы
                bool hasGrandChildren = false;
                if (item.Children != null && item.Children.Count > 0)
                {
                    hasGrandChildren = AddSpecNodes(node, item.Children);
                }

                // Добавляем узел к родителю
                parent.Nodes.Add(node);

                // Отладочная информация
                System.Diagnostics.Debug.WriteLine($"Добавлен узел: {item.Name}, детей: {node.Nodes.Count}");
            }

            return parent.Nodes.Count > 0;
        }

        private void CheckNodes(TreeNode node)
        {
            System.Diagnostics.Debug.WriteLine($"Узел '{node.Text}' имеет детей: {node.Nodes.Count}");
            foreach (TreeNode child in node.Nodes)
            {
                CheckNodes(child);
            }
        }

        private void treeViewSpec_MouseDown(object sender, MouseEventArgs e)
        {
            selectedNode = treeViewSpec.GetNodeAt(e.X, e.Y);
            if (selectedNode != null)
            {
                treeViewSpec.SelectedNode = selectedNode;
            }
        }

        private void treeViewSpec_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            selectedNode = e.Node;
        }

        private void AddItem_Click(object sender, EventArgs e)
        {
            if (selectedNode != null && !string.IsNullOrEmpty(currentComponent))
            {
                string parentName;
                if (selectedNode.Tag?.ToString() == "root")
                    parentName = currentComponent;
                else
                    parentName = selectedNode.Tag?.ToString();

                using (var addForm = new AddPartForm(fileManager, currentComponent))
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadSpecification(currentComponent);
                    }
                }
            }
        }

        private void EditItem_Click(object sender, EventArgs e)
        {
            if (selectedNode != null && selectedNode.Parent != null)
            {
                string nodeText = selectedNode.Text;
                string partName = nodeText.Substring(0, nodeText.LastIndexOf('(')).Trim();

                MessageBox.Show($"Редактирование количества '{partName}' будет реализовано позже", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void DeleteItem_Click(object sender, EventArgs e)
        {
            if (selectedNode != null && selectedNode.Parent != null)
            {
                string nodeText = selectedNode.Text;
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