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

            // Привязываем контекстное меню к treeView
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

            TreeNode root = new TreeNode(compName);
            root.Tag = "root";
            AddSpecNodes(root, spec);
            treeViewSpec.Nodes.Add(root);
            treeViewSpec.ExpandAll();
        }

        private void AddSpecNodes(TreeNode parent, List<SpecificationItem> items)
        {
            foreach (var item in items)
            {
                TreeNode node = new TreeNode($"{item.Name} ({item.Quantity} шт.)");
                node.Tag = item.Name;
                parent.Nodes.Add(node);

                if (item.Children != null && item.Children.Count > 0)
                {
                    AddSpecNodes(node, item.Children);
                }
            }
        }

        private void treeViewSpec_MouseDown(object sender, MouseEventArgs e)
        {
            // Определяем, на каком узле произошел клик
            selectedNode = treeViewSpec.GetNodeAt(e.X, e.Y);

            if (selectedNode != null)
            {
                treeViewSpec.SelectedNode = selectedNode;
            }
        }

        private void treeViewSpec_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            selectedNode = e.Node;

            // Обработка клика по квадратику
            if (e.X < 25) // Область квадратика
            {
                if (e.Node.Nodes.Count > 0)
                {
                    if (e.Node.IsExpanded)
                        e.Node.Collapse();
                    else
                        e.Node.Expand();
                }
            }
        }

        private void treeViewSpec_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Рисуем фон
            if (e.Node == selectedNode)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 220, 220)), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.White), e.Bounds);
            }

            // Рисуем квадратик для узлов с детьми
            if (e.Node.Nodes.Count > 0)
            {
                Rectangle rect = new Rectangle(e.Bounds.X - 18, e.Bounds.Y + 2, 14, 14);

                // Белый фон для квадратика
                e.Graphics.FillRectangle(new SolidBrush(Color.White), rect);

                // Черная рамка
                using (Pen pen = new Pen(Color.Black))
                {
                    e.Graphics.DrawRectangle(pen, rect);

                    // Горизонтальная линия (минус)
                    e.Graphics.DrawLine(pen, rect.X + 2, rect.Y + 7, rect.X + 11, rect.Y + 7);

                    // Вертикальная линия (плюс) - только если узел свернут
                    if (!e.Node.IsExpanded)
                    {
                        e.Graphics.DrawLine(pen, rect.X + 7, rect.Y + 2, rect.X + 7, rect.Y + 11);
                    }
                }
            }

            // Рисуем текст узла со смещением
            int textOffset = (e.Node.Nodes.Count > 0) ? 20 : 5;
            Rectangle textBounds = new Rectangle(e.Bounds.X + textOffset, e.Bounds.Y,
                                                  e.Bounds.Width - textOffset, e.Bounds.Height);

            TextRenderer.DrawText(e.Graphics, e.Node.Text, e.Node.TreeView.Font,
                                  textBounds, Color.Black, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
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

                MessageBox.Show($"Редактирование '{partName}'", "Информация",
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