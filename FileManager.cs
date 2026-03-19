using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpecificationApp
{
    public class ComponentInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class SpecificationItem
    {
        public string Name { get; set; }
        public short Quantity { get; set; }
        public List<SpecificationItem> Children { get; set; } = new List<SpecificationItem>();
    }

    public class FileManager
    {
        const int SIG_SIZE = 2;
        const int LEN_SIZE = 2;
        const int FIRST_SIZE = 4;
        const int FREE_SIZE = 4;
        const int SPEC_NAME_SIZE = 16;
        const int DEL_SIZE = 1;
        const int SPEC_PTR_SIZE = 4;
        const int NEXT_SIZE = 4;
        const int QTY_SIZE = 2;

        FileStream compFs;
        BinaryWriter compW;
        BinaryReader compR;
        FileStream specFs;
        BinaryWriter specW;
        BinaryReader specR;

        string currentCompFile;
        string currentSpecFile;
        int currentDataLen;

        public bool IsFileOpen => compFs != null;

        public void Create(string fileName, int dataLen, string specFileName = null)
        {
            if (!fileName.EndsWith(".prd"))
                fileName += ".prd";

            if (File.Exists(fileName))
            {
                try
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte b1 = br.ReadByte();
                        byte b2 = br.ReadByte();

                        if (b1 == 'P' && b2 == 'S')
                        {
                            // В Windows Forms используем MessageBox вместо консольного ввода
                            DialogResult result = MessageBox.Show(
                                $"Файл {fileName} существует. Перезаписать?",
                                "Подтверждение",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result != DialogResult.Yes)
                            {
                                throw new Exception("Операция отменена");
                            }
                        }
                        else
                        {
                            throw new Exception($"Файл существует, но сигнатура не соответствует 'PS'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка при проверке файла: {ex.Message}");
                }
            }

            Close();

            if (string.IsNullOrEmpty(specFileName))
            {
                specFileName = Path.GetFileNameWithoutExtension(fileName) + ".prs";
            }
            else if (!specFileName.EndsWith(".prs"))
            {
                specFileName += ".prs";
            }

            try
            {
                // Создаем файл компонентов
                compFs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
                compW = new BinaryWriter(compFs, Encoding.Default);
                compR = new BinaryReader(compFs, Encoding.Default);

                compW.Write((byte)'P');
                compW.Write((byte)'S');
                compW.Write((short)dataLen);
                compW.Write(-1); // firstPtr
                compW.Write(SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE + SPEC_NAME_SIZE); // freePtr

                byte[] specBytes = Encoding.Default.GetBytes(specFileName);
                compW.Write(specBytes);
                for (int i = specBytes.Length; i < SPEC_NAME_SIZE; i++)
                    compW.Write((byte)' ');

                compW.Flush();

                // Создаем файл спецификаций
                specFs = new FileStream(specFileName, FileMode.Create, FileAccess.ReadWrite);
                specW = new BinaryWriter(specFs, Encoding.Default);
                specR = new BinaryReader(specFs, Encoding.Default);

                // Заголовок файла спецификаций
                specW.Write(-1); // firstSpecPtr - указатель на изделие
                specW.Write(FIRST_SIZE + FREE_SIZE); // freeSpecPtr - указатель на свободную область

                specW.Flush();

                currentCompFile = fileName;
                currentSpecFile = specFileName;
                currentDataLen = dataLen;
            }
            catch (Exception ex)
            {
                Close();
                throw new Exception($"Ошибка при создании файлов: {ex.Message}");
            }
        }

        public void Open(string fileName)
        {
            if (!fileName.EndsWith(".prd"))
                fileName += ".prd";

            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Файл {fileName} не существует");

            Close();

            try
            {
                compFs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
                compR = new BinaryReader(compFs, Encoding.Default);
                compW = new BinaryWriter(compFs, Encoding.Default);

                compFs.Seek(0, SeekOrigin.Begin);
                byte sig1 = compR.ReadByte();
                byte sig2 = compR.ReadByte();

                if (sig1 != 'P' || sig2 != 'S')
                {
                    Close();
                    throw new InvalidDataException("Сигнатура файла не соответствует 'PS'");
                }

                currentDataLen = compR.ReadInt16();

                compFs.Seek(SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE, SeekOrigin.Begin);
                byte[] specNameBytes = compR.ReadBytes(SPEC_NAME_SIZE);
                currentSpecFile = Encoding.Default.GetString(specNameBytes).TrimEnd();

                if (!File.Exists(currentSpecFile))
                {
                    // В Windows Forms можно показать предупреждение, но не прерывать операцию
                    System.Diagnostics.Debug.WriteLine($"Предупреждение: файл спецификаций {currentSpecFile} не найден");
                    // Можно также показать MessageBox, но это может быть навязчиво
                    // MessageBox.Show($"Предупреждение: файл спецификаций {currentSpecFile} не найден", 
                    //    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    specFs = new FileStream(currentSpecFile, FileMode.Open, FileAccess.ReadWrite);
                    specR = new BinaryReader(specFs, Encoding.Default);
                    specW = new BinaryWriter(specFs, Encoding.Default);
                }

                currentCompFile = fileName;
            }
            catch (Exception ex)
            {
                Close();
                throw new Exception($"Ошибка при открытии файла: {ex.Message}");
            }
        }

        public void Close()
        {
            specR?.Close();
            specW?.Close();
            specFs?.Close();
            compR?.Close();
            compW?.Close();
            compFs?.Close();

            specR = null;
            specW = null;
            specFs = null;
            compR = null;
            compW = null;
            compFs = null;
        }

        public List<ComponentInfo> GetAllComponents()
        {
            var result = new List<ComponentInfo>();

            if (compFs == null) return result;

            // Получаем указатель на первый логический элемент из заголовка
            compFs.Seek(SIG_SIZE + LEN_SIZE, SeekOrigin.Begin);
            int firstLogicalPtr = compR.ReadInt32();

            if (firstLogicalPtr == -1)
            {
                return result; // Список пуст
            }

            // Получаем указатель на изделие из заголовка файла спецификаций
            int productPtr = -1;
            if (specFs != null)
            {
                specFs.Seek(0, SeekOrigin.Begin);
                productPtr = specR.ReadInt32();
            }

            // Проходим по логическому списку в алфавитном порядке
            int currentPtr = firstLogicalPtr;
            while (currentPtr != -1)
            {
                // Проверяем корректность указателя
                if (currentPtr < SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE + SPEC_NAME_SIZE ||
                    currentPtr >= compFs.Length)
                    break;

                compFs.Seek(currentPtr, SeekOrigin.Begin);
                sbyte delFlag = compR.ReadSByte();
                int specAreaPtr = compR.ReadInt32();
                int nextPtr = compR.ReadInt32();

                // Проверяем, достаточно ли места для чтения имени
                if (compFs.Position + currentDataLen > compFs.Length)
                    break;

                byte[] nameBytes = compR.ReadBytes(currentDataLen);
                string name = Encoding.Default.GetString(nameBytes).TrimEnd();

                // Добавляем только неудаленные записи
                if (delFlag == 0 && !string.IsNullOrWhiteSpace(name))
                {
                    string type;

                    if (specAreaPtr == -1)
                    {
                        type = "Деталь";
                    }
                    else
                    {
                        // Проверяем, является ли компонент изделием (указатель из заголовка spec файла)
                        if (specAreaPtr == productPtr)
                        {
                            type = "Изделие";
                        }
                        else
                        {
                            type = "Узел";
                        }
                    }

                    result.Add(new ComponentInfo { Name = name, Type = type });
                }

                currentPtr = nextPtr;
            }

            return result;
        }

        public void InputComponent(string name, string type)
        {
            if (compFs == null)
                throw new Exception("Файлы не открыты");

            if (type != "Изделие" && type != "Узел" && type != "Деталь")
            {
                throw new Exception("Ошибка: тип должен быть Изделие, Узел или Деталь");
            }

            if (FindComponentByName(name) != -1)
            {
                throw new Exception($"Ошибка: компонент с именем '{name}' уже существует");
            }

            // изделие может быть только одно
            if (type == "Изделие")
            {
                if (HasAnyProduct())
                {
                    throw new Exception("Ошибка: изделие может быть только одно");
                }
            }

            // Получаем указатель на свободное место в файле компонентов
            compFs.Seek(SIG_SIZE + LEN_SIZE + FIRST_SIZE, SeekOrigin.Begin);
            int freeCompFilePtr = compR.ReadInt32();

            // РЕЗЕРВИРУЕМ МЕСТО В ФАЙЛЕ СПЕЦИФИКАЦИЙ для этого компонента
            int specificationPtr = -1;
            if (type != "Деталь") // Для деталей не нужна спецификация
            {
                if (specFs == null)
                    throw new Exception("Файл спецификаций не открыт");

                specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);
                int freeSpecFilePtr = specR.ReadInt32();
                specificationPtr = freeSpecFilePtr;

                // Убеждаемся, что место для записи существует
                long neededSize = specificationPtr + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
                if (neededSize > specFs.Length)
                {
                    specFs.SetLength(neededSize);
                }

                // Первая запись спецификации для компонента
                specFs.Seek(specificationPtr, SeekOrigin.Begin);
                specW.Write((sbyte)0);      // флаг удаления
                specW.Write(-1);            // указатель на компонент в списке спецификаций
                specW.Write((short)1);       // кратность вхождения
                specW.Write(-1);            // указатель на следующую запись спецификации компонента

                // Обновляем указатель на свободную область в файле спецификаций
                int newFreeSpecPtr = specificationPtr + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
                specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);
                specW.Write(newFreeSpecPtr);

                if (type == "Изделие")
                {
                    specFs.Seek(0, SeekOrigin.Begin);
                    specW.Write(specificationPtr);
                }
                specW.Flush();
            }

            // Записываем компонент в файл компонентов
            compFs.Seek(freeCompFilePtr, SeekOrigin.Begin);
            compW.Write((sbyte)0);          // флаг удаления
            compW.Write(specificationPtr);  // указатель на запись в файле спецификаций
            compW.Write(-1);                // следующий логический элемент списка

            byte[] nameBytes = Encoding.Default.GetBytes(name);

            if (nameBytes.Length < currentDataLen)
            {
                byte[] paddedName = new byte[currentDataLen];
                Array.Copy(nameBytes, paddedName, nameBytes.Length);
                for (int i = nameBytes.Length; i < currentDataLen; i++)
                    paddedName[i] = (byte)' ';
                compW.Write(paddedName);
            }
            else
            {
                compW.Write(nameBytes, 0, currentDataLen);
            }

            compW.Flush();

            // Обновляем указатель на свободное место в файле компонентов
            int newFreeCompFilePtr = freeCompFilePtr + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen;
            compFs.Seek(SIG_SIZE + LEN_SIZE + FIRST_SIZE, SeekOrigin.Begin);
            compW.Write(newFreeCompFilePtr);
            compW.Flush();

            // Добавляем в алфавитный порядок
            AddInAlphabeticalOrder(freeCompFilePtr, name);
        }

        public void InputPart(string compName, string partName, short quantity)
        {
            if (compFs == null || specFs == null)
                throw new Exception("Файлы не открыты");

            int compPtr = FindComponentByName(compName);
            if (compPtr == -1)
                throw new Exception($"Компонент '{compName}' не найден");

            // Получаем указатель на первый элемент списка спецификации компонента
            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int specListPtr = compR.ReadInt32();

            if (specListPtr == -1)
                throw new Exception($"Компонент '{compName}' является деталью и не может иметь спецификацию");

            int partPtr = FindComponentByName(partName);
            if (partPtr == -1)
                throw new Exception($"Комплектующее '{partName}' не найдено");

            // Проверяем, не добавляем ли компонент сам в себя
            if (compPtr == partPtr)
                throw new Exception("Компонент не может быть комплектующим для самого себя");

            if (quantity <= 0)
                throw new Exception("Кратность должна быть положительным числом");

            // Проверяем, нет ли уже такой записи
            int current = specListPtr;
            while (current != -1)
            {
                // Проверяем корректность указателя
                if (current < FIRST_SIZE + FREE_SIZE || current >= specFs.Length)
                    break;

                specFs.Seek(current, SeekOrigin.Begin);

                if (specFs.Position + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > specFs.Length)
                    break;

                sbyte recordDelFlag = specR.ReadSByte();
                int componentPtr = specR.ReadInt32();
                specR.ReadInt16(); // кратность пропускаем
                int nextRecordPtr = specR.ReadInt32();

                if (recordDelFlag == 0 && componentPtr == partPtr)
                    throw new Exception($"Комплектующее '{partName}' уже есть в спецификации");

                current = nextRecordPtr;
            }

            // Читаем текущий указатель на свободную область
            specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);

            if (specFs.Position + 4 > specFs.Length)
                throw new Exception("Ошибка чтения freeSpecPtr");

            int freeSpecPtr = specR.ReadInt32();

            // Если свободного места нет, добавляем в конец
            if (freeSpecPtr == -1 || freeSpecPtr == 0 || freeSpecPtr >= specFs.Length)
            {
                specFs.Seek(0, SeekOrigin.End);
                freeSpecPtr = (int)specFs.Position;
            }

            int recordPtr = freeSpecPtr;

            // Убеждаемся, что место для записи существует
            long neededSize = recordPtr + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
            if (neededSize > specFs.Length)
            {
                specFs.SetLength(neededSize);
            }

            // Если список не пуст, находим последнюю запись и связываем её с новым блоком
            if (specListPtr != -1)
            {
                int lastRecordPtr = specListPtr;
                int maxIterations = 1000;
                int iterations = 0;

                while (iterations < maxIterations)
                {
                    iterations++;

                    // Проверяем корректность указателя
                    if (lastRecordPtr < FIRST_SIZE + FREE_SIZE || lastRecordPtr >= specFs.Length)
                        break;

                    specFs.Seek(lastRecordPtr + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE, SeekOrigin.Begin);

                    if (specFs.Position + 4 > specFs.Length)
                        break;

                    int nextRecordPtr = specR.ReadInt32();

                    if (nextRecordPtr == -1 || nextRecordPtr == 0)
                        break;

                    lastRecordPtr = nextRecordPtr;
                }

                // Записываем в последнюю запись указатель на новый блок
                specFs.Seek(lastRecordPtr + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE, SeekOrigin.Begin);
                specW.Write(recordPtr);
                specW.Flush();
            }
            else
            {
                // Список пуст: нужно обновить указатель на спецификацию в записи компонента
                compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
                compW.Write(recordPtr);
                compW.Flush();
            }

            // Записываем новую запись по адресу freeSpecPtr
            specFs.Seek(recordPtr, SeekOrigin.Begin);
            specW.Write((sbyte)0);   // delFlag
            specW.Write(partPtr);    // compPtr
            specW.Write(quantity);   // qty
            specW.Write(-1);         // nextPtr
            specW.Flush();

            // Обновляем указатель на свободную область в заголовке
            int newFreePtr = recordPtr + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
            specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);
            specW.Write(newFreePtr);
            specW.Flush();
        }

        public void DeleteComponent(string name)
        {
            int compPtr = FindComponentByName(name);
            if (compPtr == -1)
                throw new Exception($"Компонент '{name}' не найден");

            if (HasReferences(compPtr))
                throw new Exception($"На компонент '{name}' есть ссылки в спецификациях других компонентов");

            // Помечаем компонент как удаленный
            compFs.Seek(compPtr, SeekOrigin.Begin);
            compW.Write((sbyte)-1);
            compW.Flush();

            // Помечаем все записи в списке спецификации как удаленные
            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int specListPtr = compR.ReadInt32();

            // проверяем, является ли компонент деталью и доступность файла
            if (specListPtr == -1 && specFs == null)
                return;

            int current = specListPtr; // указатель на итерируемый элемент списка спецификации
            while (current != -1)
            {
                // Проверяем корректность указателя
                if (current < FIRST_SIZE + FREE_SIZE || current >= specFs.Length)
                    break;

                specFs.Seek(current, SeekOrigin.Begin);
                specW.Write((sbyte)-1);

                specFs.Seek(current + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE, SeekOrigin.Begin);

                if (specFs.Position + 4 <= specFs.Length)
                {
                    current = specR.ReadInt32();
                }
                else
                {
                    break;
                }
            }
        }

        public void DeletePart(string compName, string partName)
        {
            if (specFs == null)
                throw new Exception("Файл спецификаций не открыт");

            int compPtr = FindComponentByName(compName);
            if (compPtr == -1)
                throw new Exception($"Компонент '{compName}' не найден");

            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int firstRecordPtr = compR.ReadInt32();

            if (firstRecordPtr == -1)
                throw new Exception($"Компонент '{compName}' является деталью и не имеет спецификации");

            int partPtr = FindComponentByName(partName);
            if (partPtr == -1)
                throw new Exception($"Комплектующее '{partName}' не найдено");

            bool found = false;
            int current = firstRecordPtr;
            while (current != -1)
            {
                // Проверяем корректность указателя
                if (current < FIRST_SIZE + FREE_SIZE || current >= specFs.Length)
                    break;

                specFs.Seek(current, SeekOrigin.Begin);

                if (specFs.Position + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > specFs.Length)
                    break;

                sbyte delFlag = specR.ReadSByte();
                int partInComponentListPtr = specR.ReadInt32();
                short qty = specR.ReadInt16();
                int nextRecordPtr = specR.ReadInt32();

                if (delFlag == 0 && partInComponentListPtr == partPtr)
                {
                    specFs.Seek(current, SeekOrigin.Begin);
                    specW.Write((sbyte)-1);
                    specW.Flush();
                    found = true;
                    break;
                }

                current = nextRecordPtr;
            }

            if (!found)
            {
                throw new Exception($"Комплектующее '{partName}' не найдено в спецификации '{compName}'");
            }
        }

        public void RestoreComponent(string name)
        {
            int compPtr = FindComponentByName(name, true);
            if (compPtr == -1)
                throw new Exception($"Компонент '{name}' не найден");

            // Восстанавливаем компонент
            compFs.Seek(compPtr, SeekOrigin.Begin);
            compW.Write((sbyte)0);
            compW.Flush();

            // Получаем указатель на список спецификации
            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int firstRecordPtr = compR.ReadInt32();

            // Восстанавливаем все записи в спецификации
            if (firstRecordPtr != -1 && specFs != null)
            {
                int current = firstRecordPtr;
                while (current != -1)
                {
                    // Проверяем корректность указателя
                    if (current < FIRST_SIZE + FREE_SIZE || current >= specFs.Length)
                        break;

                    specFs.Seek(current, SeekOrigin.Begin);

                    if (specFs.Position + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE <= specFs.Length)
                    {
                        specW.Write((sbyte)0);
                    }

                    specFs.Seek(current + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE, SeekOrigin.Begin);

                    if (specFs.Position + 4 <= specFs.Length)
                    {
                        current = specR.ReadInt32();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Восстанавливаем алфавитный порядок
            RestoreAlphabeticalOrder();
        }

        public List<SpecificationItem> GetSpecification(string compName)
        {
            var result = new List<SpecificationItem>();

            int compPtr = FindComponentByName(compName);
            if (compPtr == -1)
                return result;

            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int specHeadPtr = compR.ReadInt32();

            if (specHeadPtr == -1 || specFs == null)
                return result;

            return GetSpecRecordsRecursive(specHeadPtr, 1);
        }

        private List<SpecificationItem> GetSpecRecordsRecursive(int recordPtr, int level)
        {
            var result = new List<SpecificationItem>();

            if (recordPtr == -1 || recordPtr == 0 || specFs == null)
                return result;

            int current = recordPtr;
            while (current != -1 && current != 0)
            {
                // Проверяем корректность указателя
                if (current < FIRST_SIZE + FREE_SIZE || current >= specFs.Length)
                    break;

                try
                {
                    specFs.Seek(current, SeekOrigin.Begin);

                    // Проверяем, можно ли прочитать запись полностью
                    if (specFs.Position + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > specFs.Length)
                        break;

                    sbyte delFlag = specR.ReadSByte();
                    int compPtr = specR.ReadInt32();
                    short qty = specR.ReadInt16();
                    int nextPtr = specR.ReadInt32();

                    if (delFlag == 0 && compPtr != -1 && compPtr != 0)
                    {
                        // Проверяем указатель на компонент
                        if (compPtr >= SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE + SPEC_NAME_SIZE &&
                            compPtr + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen <= compFs.Length)
                        {
                            compFs.Seek(compPtr, SeekOrigin.Begin);
                            sbyte compDel = compR.ReadSByte();
                            int compSpecHead = compR.ReadInt32();
                            int compNext = compR.ReadInt32();
                            byte[] nameBytes = compR.ReadBytes(currentDataLen);
                            string name = Encoding.Default.GetString(nameBytes).TrimEnd();

                            var item = new SpecificationItem
                            {
                                Name = name,
                                Quantity = qty
                            };

                            // Рекурсивно получаем дочерние элементы
                            if (compSpecHead != -1 && compSpecHead != 0 &&
                                compSpecHead >= FIRST_SIZE + FREE_SIZE && compSpecHead < specFs.Length)
                            {
                                item.Children = GetSpecRecordsRecursive(compSpecHead, level + 1);
                            }

                            result.Add(item);
                        }
                    }

                    current = nextPtr;
                }
                catch
                {
                    break;
                }
            }

            return result;
        }

        private int FindComponentByName(string name, bool includeDeleted = false)
        {
            if (compFs == null) return -1;

            compFs.Seek(0, SeekOrigin.End);
            long endPos = compFs.Position;

            int pos = SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE + SPEC_NAME_SIZE;
            while (pos < endPos)
            {
                try
                {
                    // Проверяем, не выходим ли за пределы файла
                    if (pos + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen > endPos)
                        break;

                    compFs.Seek(pos, SeekOrigin.Begin);
                    sbyte delFlag = compR.ReadSByte();
                    int specPtr = compR.ReadInt32();
                    int nextPtr = compR.ReadInt32();
                    byte[] nameBytes = compR.ReadBytes(currentDataLen);
                    string compName = Encoding.Default.GetString(nameBytes).TrimEnd();

                    if ((includeDeleted || delFlag == 0) && compName == name)
                        return pos;

                    pos += DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen;
                }
                catch
                {
                    break;
                }
            }

            return -1;
        }

        private bool HasReferences(int compPtr)
        {
            if (specFs == null || specFs.Length <= FIRST_SIZE + FREE_SIZE)
                return false;

            specFs.Seek(0, SeekOrigin.End);
            long endPos = specFs.Position;

            int pos = FIRST_SIZE + FREE_SIZE;
            while (pos < endPos)
            {
                try
                {
                    // Проверяем, достаточно ли места для чтения записи
                    if (pos + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > endPos)
                        break;

                    specFs.Seek(pos, SeekOrigin.Begin);
                    sbyte delFlag = specR.ReadSByte();
                    int compRef = specR.ReadInt32();
                    short qty = specR.ReadInt16();
                    int nextPtr = specR.ReadInt32();

                    if (delFlag == 0 && compRef == compPtr)
                        return true;

                    pos += DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
                }
                catch
                {
                    break;
                }
            }

            return false;
        }

        private bool HasAnyProduct()
        {
            if (compFs == null) return false;

            compFs.Seek(0, SeekOrigin.End);
            long endPos = compFs.Position;

            int pos = SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE + SPEC_NAME_SIZE;
            while (pos < endPos)
            {
                compFs.Seek(pos, SeekOrigin.Begin);
                sbyte delFlag = compR.ReadSByte();
                int specAreaPtr = compR.ReadInt32();

                if (delFlag == 0 && specAreaPtr != -1)
                {
                    if (!IsReferenced(pos))
                        return true;
                }

                pos += DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen;
            }

            return false;
        }

        private bool IsReferenced(int compPtr)
        {
            if (specFs == null || specFs.Length <= FIRST_SIZE + FREE_SIZE)
                return false;

            specFs.Seek(0, SeekOrigin.End);
            long specEnd = specFs.Position;

            int pos = FIRST_SIZE + FREE_SIZE;
            while (pos < specEnd)
            {
                try
                {
                    // Проверяем, достаточно ли места для чтения записи
                    if (pos + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > specEnd)
                        break;

                    specFs.Seek(pos, SeekOrigin.Begin);
                    sbyte delFlag = specR.ReadSByte();
                    int compRef = specR.ReadInt32();

                    if (delFlag == 0 && compRef == compPtr)
                        return true;

                    pos += DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
                }
                catch
                {
                    break;
                }
            }

            return false;
        }

        private void AddInAlphabeticalOrder(int newRecordPtr, string newName)
        {
            // Получаем указатель на первую запись
            compFs.Seek(SIG_SIZE + LEN_SIZE, SeekOrigin.Begin);
            int firstPtr = compR.ReadInt32();

            // Если список пуст
            if (firstPtr == -1)
            {
                compFs.Seek(SIG_SIZE + LEN_SIZE, SeekOrigin.Begin);
                compW.Write(newRecordPtr);
                compFs.Seek(newRecordPtr + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                compW.Write(-1);
                compW.Flush();
                return;
            }

            // Ищем место для вставки
            int current = firstPtr;
            int prev = -1;
            string currentName;

            while (current != -1)
            {
                try
                {
                    // Проверяем, не выходим ли за пределы
                    if (current + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen > compFs.Length)
                        break;

                    // Читаем имя текущей записи
                    compFs.Seek(current + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE, SeekOrigin.Begin);
                    byte[] nameBytes = compR.ReadBytes(currentDataLen);
                    currentName = Encoding.Default.GetString(nameBytes).TrimEnd();

                    // Сравниваем имена
                    if (string.Compare(newName, currentName, StringComparison.Ordinal) < 0)
                    {
                        // Вставляем перед current
                        if (prev == -1)
                        {
                            // Вставляем в начало
                            compFs.Seek(SIG_SIZE + LEN_SIZE, SeekOrigin.Begin);
                            compW.Write(newRecordPtr);
                            compFs.Seek(newRecordPtr + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                            compW.Write(current);
                        }
                        else
                        {
                            // Вставляем между prev и current
                            compFs.Seek(prev + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                            compW.Write(newRecordPtr);
                            compFs.Seek(newRecordPtr + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                            compW.Write(current);
                        }
                        compW.Flush();
                        return;
                    }

                    prev = current;

                    // Переходим к следующей записи
                    compFs.Seek(current + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);

                    if (compFs.Position + 4 > compFs.Length)
                        break;

                    current = compR.ReadInt32();
                }
                catch
                {
                    break;
                }
            }

            // Если дошли сюда - вставляем в конец
            if (prev != -1)
            {
                try
                {
                    compFs.Seek(prev + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                    compW.Write(newRecordPtr);
                    compFs.Seek(newRecordPtr + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                    compW.Write(-1);
                    compW.Flush();
                }
                catch
                {
                    // Игнорируем ошибки записи
                }
            }
        }

        private void RestoreAlphabeticalOrder()
        {
            List<Tuple<string, int>> records = new List<Tuple<string, int>>();

            compFs.Seek(0, SeekOrigin.End);
            long endPos = compFs.Position;

            int pos = SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE + SPEC_NAME_SIZE;
            while (pos < endPos)
            {
                try
                {
                    // Проверяем, не выходим ли за пределы
                    if (pos + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen > endPos)
                        break;

                    compFs.Seek(pos, SeekOrigin.Begin);
                    sbyte delFlag = compR.ReadSByte();
                    int specPtr = compR.ReadInt32();
                    int nextPtr = compR.ReadInt32();
                    byte[] nameBytes = compR.ReadBytes(currentDataLen);
                    string name = Encoding.Default.GetString(nameBytes).TrimEnd();

                    if (delFlag == 0 && !string.IsNullOrWhiteSpace(name))
                    {
                        records.Add(new Tuple<string, int>(name, pos));
                    }

                    pos += DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen;
                }
                catch
                {
                    break;
                }
            }

            records.Sort((a, b) => string.Compare(a.Item1, b.Item1));

            if (records.Count > 0)
            {
                try
                {
                    compFs.Seek(SIG_SIZE + LEN_SIZE, SeekOrigin.Begin);
                    compW.Write(records[0].Item2);

                    for (int i = 0; i < records.Count - 1; i++)
                    {
                        compFs.Seek(records[i].Item2 + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                        compW.Write(records[i + 1].Item2);
                    }

                    compFs.Seek(records[records.Count - 1].Item2 + DEL_SIZE + SPEC_PTR_SIZE, SeekOrigin.Begin);
                    compW.Write(-1);
                }
                catch
                {
                    // Игнорируем ошибки записи
                }
            }
            else
            {
                try
                {
                    compFs.Seek(SIG_SIZE + LEN_SIZE, SeekOrigin.Begin);
                    compW.Write(-1);
                }
                catch
                {
                    // Игнорируем ошибки записи
                }
            }

            compW?.Flush();
        }

    }
}