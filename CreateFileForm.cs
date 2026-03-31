using System;
using System.Windows.Forms;

namespace SpecificationApp
{
    public partial class CreateFileForm : Form
    {
        public string FileName { get; private set; }
        public int DataLen { get; private set; }
        public string SpecFileName { get; private set; }

        // Конструктор - инициализирует форму для создания нового файла
        public CreateFileForm()
        {
            InitializeComponent();
        }

        // Обрабатывает нажатие OK: проверяет введенные данные, сохраняет их и закрывает форму
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show("Введите имя файла", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtDataLen.Text, out int dataLen) || dataLen <= 0)
            {
                MessageBox.Show("Введите корректную длину имени компонента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FileName = txtFileName.Text;
            DataLen = dataLen;
            SpecFileName = string.IsNullOrWhiteSpace(txtSpecFileName.Text) ? null : txtSpecFileName.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        // Обрабатывает нажатие Cancel: закрывает форму с отменой
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}