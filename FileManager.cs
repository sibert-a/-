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

            Close();

            if (string.IsNullOrEmpty(specFileName))
            {
                specFileName = Path.GetFileNameWithoutExtension(fileName) + ".prs";
            }
            else if (!specFileName.EndsWith(".prs"))
            {
                specFileName += ".prs";
            }

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

            // Заголовок файла спецификаций: первые 8 байт
            specW.Write(-1); // firstSpecPtr - указатель на первую запись
            specW.Write(FIRST_SIZE + FREE_SIZE); // freeSpecPtr - указатель на свободную область

            specW.Flush();

            currentCompFile = fileName;
            currentSpecFile = specFileName;
            currentDataLen = dataLen;
        }

        public void Open(string fileName)
        {
            if (!fileName.EndsWith(".prd"))
                fileName += ".prd";

            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Файл {fileName} не существует");

            Close();

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

            if (File.Exists(currentSpecFile))
            {
                specFs = new FileStream(currentSpecFile, FileMode.Open, FileAccess.ReadWrite);
                specR = new BinaryReader(specFs, Encoding.Default);
                specW = new BinaryWriter(specFs, Encoding.Default);
            }

            currentCompFile = fileName;
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

            // Словарь для хранения информации о том, на какие компоненты ссылаются
            Dictionary<int, bool> isReferenced = new Dictionary<int, bool>();

            // Сначала собираем информацию о ссылках из файла спецификаций
            if (specFs != null && specFs.Length > FIRST_SIZE + FREE_SIZE)
            {
                try
                {
                    specFs.Seek(0, SeekOrigin.End);
                    long specEnd = specFs.Position;

                    // Пропускаем заголовок файла спецификаций (первые 8 байт)
                    int pos = FIRST_SIZE + FREE_SIZE;
                    while (pos < specEnd)
                    {
                        // Проверяем, достаточно ли места для чтения записи
                        if (pos + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > specEnd)
                            break;

                        specFs.Seek(pos, SeekOrigin.Begin);
                        sbyte delFlag = specR.ReadSByte();
                        int compRef = specR.ReadInt32();
                        short qty = specR.ReadInt16();
                        int nextPtr = specR.ReadInt32();

                        if (delFlag == 0 && compRef != -1 && compRef != 0)
                        {
                            isReferenced[compRef] = true;
                        }

                        pos += DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при чтении спецификаций: {ex.Message}");
                }
            }

            // Читаем компоненты
            compFs.Seek(0, SeekOrigin.End);
            long endPos = compFs.Position;

            int compPos = SIG_SIZE + LEN_SIZE + FIRST_SIZE + FREE_SIZE + SPEC_NAME_SIZE;
            while (compPos < endPos)
            {
                // Проверяем, достаточно ли места для чтения записи
                if (compPos + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen > endPos)
                    break;

                compFs.Seek(compPos, SeekOrigin.Begin);
                sbyte delFlag = compR.ReadSByte();
                int specAreaPtr = compR.ReadInt32();
                int nextPtr = compR.ReadInt32();
                byte[] nameBytes = compR.ReadBytes(currentDataLen);
                string name = Encoding.Default.GetString(nameBytes).TrimEnd();

                if (delFlag == 0 && !string.IsNullOrWhiteSpace(name))
                {
                    string type;
                    if (specAreaPtr == -1)
                    {
                        type = "Деталь";
                    }
                    else
                    {
                        if (isReferenced.ContainsKey(compPos))
                            type = "Узел";
                        else
                            type = "Изделие";
                    }
                    result.Add(new ComponentInfo { Name = name, Type = type });
                }

                compPos += DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen;
            }

            return result;
        }

        public void InputComponent(string name, string type)
        {
            if (compFs == null)
                throw new Exception("Файлы не открыты");

            if (FindComponentByName(name) != -1)
                throw new Exception($"Компонент с именем '{name}' уже существует");

            if (type == "Изделие" && HasAnyProduct())
                throw new Exception("Изделие может быть только одно");

            // Получаем указатель на свободное место
            compFs.Seek(SIG_SIZE + LEN_SIZE + FIRST_SIZE, SeekOrigin.Begin);
            int freePtr = compR.ReadInt32();

            // Если свободного места нет, добавляем в конец
            if (freePtr == -1)
            {
                compFs.Seek(0, SeekOrigin.End);
                freePtr = (int)compFs.Position;
            }

            // Резервируем место в файле спецификаций
            int specAreaPtr = -1;
            if (type != "Деталь")
            {
                if (specFs == null)
                    throw new Exception("Файл спецификаций не открыт");

                specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);
                int freeSpecPtr = specR.ReadInt32();
                specAreaPtr = freeSpecPtr;

                // Проверяем, нужно ли расширить файл
                long neededSize = specAreaPtr + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE;
                if (neededSize > specFs.Length)
                {
                    specFs.SetLength(neededSize);
                }

                specFs.Seek(specAreaPtr, SeekOrigin.Begin);
                specW.Write((sbyte)0); // delFlag для области (не используется)
                specW.Write(-1); // firstRecordPtr - важно: -1 означает "нет записей"
                specW.Write(-1); // nextSpecAreaPtr (не используется)

                int newFreeSpecPtr = specAreaPtr + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE;
                specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);
                specW.Write(newFreeSpecPtr);
                specW.Flush();
            }

            // Записываем компонент
            compFs.Seek(freePtr, SeekOrigin.Begin);
            compW.Write((sbyte)0); // delFlag
            compW.Write(specAreaPtr); // specAreaPtr
            compW.Write(-1); // nextPtr

            byte[] nameBytes = Encoding.Default.GetBytes(name);

            // Дополняем пробелами до нужной длины
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

            // Обновляем указатель на свободное место
            int newFreePtr = freePtr + DEL_SIZE + SPEC_PTR_SIZE + NEXT_SIZE + currentDataLen;
            compFs.Seek(SIG_SIZE + LEN_SIZE + FIRST_SIZE, SeekOrigin.Begin);
            compW.Write(newFreePtr);
            compW.Flush();

            // Добавляем в алфавитный порядок
            AddInAlphabeticalOrder(freePtr, name);
        }

        public void InputPart(string compName, string partName, short quantity)
        {
            if (compFs == null || specFs == null)
                throw new Exception("Файлы не открыты");

            int compPtr = FindComponentByName(compName);
            if (compPtr == -1)
                throw new Exception($"Компонент '{compName}' не найден");

            // Получаем указатель на область спецификации компонента
            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int specAreaPtr = compR.ReadInt32();

            if (specAreaPtr == -1)
                throw new Exception($"Компонент '{compName}' является деталью и не может иметь спецификацию");

            // Проверяем корректность указателя
            if (specAreaPtr < FIRST_SIZE + FREE_SIZE || specAreaPtr >= specFs.Length)
                throw new Exception($"Компонент '{compName}' имеет некорректную область спецификации");

            int partPtr = FindComponentByName(partName);
            if (partPtr == -1)
                throw new Exception($"Комплектующее '{partName}' не найдено");

            if (compPtr == partPtr)
                throw new Exception("Компонент не может быть комплектующим для самого себя");

            // Переходим в область спецификации компонента
            specFs.Seek(specAreaPtr + DEL_SIZE, SeekOrigin.Begin);

            // Проверяем, можно ли прочитать firstRecordPtr
            if (specFs.Position + 4 > specFs.Length)
                throw new Exception("Ошибка чтения firstRecordPtr");

            int firstRecordPtr = specR.ReadInt32();

            // Проверяем, нет ли уже такой записи
            int current = firstRecordPtr;
            while (current != -1 && current != 0)
            {
                // Проверяем корректность указателя
                if (current < FIRST_SIZE + FREE_SIZE || current >= specFs.Length)
                    break;

                specFs.Seek(current, SeekOrigin.Begin);

                // Проверяем, можно ли прочитать запись
                if (specFs.Position + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > specFs.Length)
                    break;

                sbyte recDelFlag = specR.ReadSByte();
                int compRef = specR.ReadInt32();
                short qty = specR.ReadInt16();
                int next = specR.ReadInt32();

                if (recDelFlag == 0 && compRef == partPtr)
                    throw new Exception($"Комплектующее '{partName}' уже есть в спецификации");

                current = next;
            }

            // Получаем указатель на свободное место в файле спецификаций
            specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);

            // Проверяем, можно ли прочитать freeSpecPtr
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

            if (firstRecordPtr == -1 || firstRecordPtr == 0)
            {
                // Это первая запись в спецификации
                specFs.Seek(specAreaPtr + DEL_SIZE, SeekOrigin.Begin);
                specW.Write(recordPtr);
                specW.Flush();
            }
            else
            {
                // Ищем последнюю запись
                int last = firstRecordPtr;
                int maxIterations = 1000;
                int iterations = 0;

                while (iterations < maxIterations)
                {
                    iterations++;

                    // Проверяем корректность указателя
                    if (last < FIRST_SIZE + FREE_SIZE || last >= specFs.Length)
                        break;

                    specFs.Seek(last + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE, SeekOrigin.Begin);

                    // Проверяем, можно ли прочитать next
                    if (specFs.Position + 4 > specFs.Length)
                        break;

                    int next = specR.ReadInt32();

                    if (next == -1 || next == 0)
                    {
                        // Нашли последнюю запись
                        specFs.Seek(last + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE, SeekOrigin.Begin);
                        specW.Write(recordPtr);
                        specW.Flush();
                        break;
                    }
                    last = next;
                }
            }

            // Записываем новую запись спецификации
            specFs.Seek(recordPtr, SeekOrigin.Begin);
            specW.Write((sbyte)0); // delFlag
            specW.Write(partPtr); // compPtr
            specW.Write(quantity); // qty
            specW.Write(-1); // nextPtr
            specW.Flush();

            // Обновляем указатель на свободную область
            int newFreeSpecPtr = recordPtr + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE;
            specFs.Seek(FIRST_SIZE, SeekOrigin.Begin);
            specW.Write(newFreeSpecPtr);
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

            // Помечаем все записи в его спецификации как удаленные
            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int specAreaPtr = compR.ReadInt32();

            if (specAreaPtr != -1 && specFs != null && specAreaPtr >= FIRST_SIZE + FREE_SIZE && specAreaPtr < specFs.Length)
            {
                specFs.Seek(specAreaPtr + DEL_SIZE, SeekOrigin.Begin);

                if (specFs.Position + 4 <= specFs.Length)
                {
                    int firstRecordPtr = compR.ReadInt32();

                    int current = firstRecordPtr;
                    while (current != -1 && current != 0 && current >= FIRST_SIZE + FREE_SIZE && current < specFs.Length)
                    {
                        specFs.Seek(current, SeekOrigin.Begin);

                        if (specFs.Position + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE <= specFs.Length)
                        {
                            specW.Write((sbyte)-1);
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
            int specAreaPtr = compR.ReadInt32();

            if (specAreaPtr == -1)
                throw new Exception($"Компонент '{compName}' является деталью и не имеет спецификации");

            int partPtr = FindComponentByName(partName);
            if (partPtr == -1)
                throw new Exception($"Комплектующее '{partName}' не найдено");

            specFs.Seek(specAreaPtr + DEL_SIZE, SeekOrigin.Begin);

            if (specFs.Position + 4 > specFs.Length)
                return;

            int firstRecordPtr = compR.ReadInt32();

            int current = firstRecordPtr;
            while (current != -1 && current != 0 && current >= FIRST_SIZE + FREE_SIZE && current < specFs.Length)
            {
                specFs.Seek(current, SeekOrigin.Begin);

                if (specFs.Position + DEL_SIZE + SPEC_PTR_SIZE + QTY_SIZE + NEXT_SIZE > specFs.Length)
                    break;

                sbyte delFlag = specR.ReadSByte();
                int compRef = specR.ReadInt32();
                short qty = specR.ReadInt16();
                int next = specR.ReadInt32();

                if (delFlag == 0 && compRef == partPtr)
                {
                    specFs.Seek(current, SeekOrigin.Begin);
                    specW.Write((sbyte)-1);
                    specW.Flush();
                    return;
                }

                current = next;
            }

            throw new Exception($"Комплектующее '{partName}' не найдено в спецификации '{compName}'");
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

            // Получаем указатель на область спецификации
            compFs.Seek(compPtr + DEL_SIZE, SeekOrigin.Begin);
            int specAreaPtr = compR.ReadInt32();

            // Восстанавливаем все записи в спецификации
            if (specAreaPtr != -1 && specAreaPtr != 0 && specFs != null && specAreaPtr >= FIRST_SIZE + FREE_SIZE && specAreaPtr < specFs.Length)
            {
                specFs.Seek(specAreaPtr + DEL_SIZE, SeekOrigin.Begin);

                if (specFs.Position + 4 <= specFs.Length)
                {
                    int firstRecordPtr = compR.ReadInt32();

                    int current = firstRecordPtr;
                    while (current != -1 && current != 0 && current >= FIRST_SIZE + FREE_SIZE && current < specFs.Length)
                    {
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
            int specAreaPtr = compR.ReadInt32();

            if (specAreaPtr == -1 || specFs == null)
                return result;

            // Проверяем, что указатель корректен
            if (specAreaPtr < FIRST_SIZE + FREE_SIZE || specAreaPtr >= specFs.Length)
                return result;

            specFs.Seek(specAreaPtr + DEL_SIZE, SeekOrigin.Begin);

            // Проверяем, можем ли прочитать
            if (specFs.Position + 4 > specFs.Length)
                return result;

            int firstRecordPtr = compR.ReadInt32();

            if (firstRecordPtr == -1 || firstRecordPtr == 0)
                return result;

            return GetSpecRecords(firstRecordPtr);
        }

        private List<SpecificationItem> GetSpecRecords(int recordPtr)
        {
            var result = new List<SpecificationItem>();

            if (recordPtr == -1 || recordPtr == 0 || specFs == null)
                return result;

            int current = recordPtr;
            while (current != -1 && current != 0 && current >= FIRST_SIZE + FREE_SIZE && current < specFs.Length)
            {
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
                            int compSpecArea = compR.ReadInt32();
                            int compNext = compR.ReadInt32();
                            byte[] nameBytes = compR.ReadBytes(currentDataLen);
                            string name = Encoding.Default.GetString(nameBytes).TrimEnd();

                            var item = new SpecificationItem
                            {
                                Name = name,
                                Quantity = qty
                            };

                            if (compSpecArea != -1 && compSpecArea != 0 &&
                                compSpecArea >= FIRST_SIZE + FREE_SIZE && compSpecArea < specFs.Length)
                            {
                                specFs.Seek(compSpecArea + DEL_SIZE, SeekOrigin.Begin);
                                if (specFs.Position + 4 <= specFs.Length)
                                {
                                    int subFirstPtr = compR.ReadInt32();
                                    if (subFirstPtr != -1 && subFirstPtr != 0)
                                    {
                                        item.Children = GetSpecRecords(subFirstPtr);
                                    }
                                }
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