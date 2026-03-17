using System;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class Form1 : Form
    {
        private FileManager fileManager;
        private ComponentsForm componentsForm;
        private SpecificationForm specificationForm;

        public Form1()
        {
            InitializeComponent();
            fileManager = new FileManager();
            обновитьСостояниеМеню(false);
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var createForm = new CreateFileForm())
            {
                if (createForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        fileManager.Create(createForm.FileName, createForm.DataLen, createForm.SpecFileName);
                        обновитьСостояниеМеню(true);
                        MessageBox.Show("Файлы успешно созданы", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании файлов: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PRD files (*.prd)|*.prd|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        fileManager.Open(openFileDialog.FileName);
                        обновитьСостояниеМеню(true);
                        MessageBox.Show("Файлы успешно открыты", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файлов: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileManager.Close();
            Application.Exit();
        }

        private void компонентыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (componentsForm == null || componentsForm.IsDisposed)
            {
                componentsForm = new ComponentsForm(fileManager);
                componentsForm.MdiParent = this;
                componentsForm.FormClosed += (s, args) => componentsForm = null;
                componentsForm.Show();
            }
            else
            {
                componentsForm.Activate();
            }
        }

        private void спецификацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (specificationForm == null || specificationForm.IsDisposed)
            {
                specificationForm = new SpecificationForm(fileManager);
                specificationForm.MdiParent = this;
                specificationForm.FormClosed += (s, args) => specificationForm = null;
                specificationForm.Show();
            }
            else
            {
                specificationForm.Activate();
            }
        }

        private void обновитьСостояниеМеню(bool файлыОткрыты)
        {
            компонентыToolStripMenuItem.Enabled = файлыОткрыты;
            спецификацияToolStripMenuItem.Enabled = файлыОткрыты;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            fileManager.Close();
        }
    }
}