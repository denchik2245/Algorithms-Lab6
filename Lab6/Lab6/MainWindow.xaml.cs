using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Lab6.Logic;
using Logic;

namespace Lab6
{
    public partial class MainWindow : Window
    {
        private SeparateChainingHashTable<int, string> currentHashTable;
        private int tableSize = 1000;

        private OpenAddressingHashTable<int, string> openAddressingHashTable;
        private OpenAddressingHashTable<int, string>.ProbeType currentProbeType;

        public MainWindow()
        {
            InitializeComponent();

            CollisionMethodComboBox.SelectedIndex = 0;

            UpdateHashingMethods();
            HashingMethodComboBox.SelectedIndex = 0;

            UpdateCurrentHashTable();
        }
        
        public class HashTableItem
        {
            public int CellIndex { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public bool IsLongest { get; set; }
            public bool IsShortest { get; set; }
        }
        
        private void CollisionMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CollisionMethodComboBox.SelectedIndex == 0)
            {
                tableSize = 1000;
            }
            else if (CollisionMethodComboBox.SelectedIndex == 1)
            {
                tableSize = 10000;
            }
            else
            {
                tableSize = 1000;
            }
            
            UpdateHashingMethods();
            HashingMethodComboBox.SelectedIndex = 0;

            UpdateCurrentHashTable();
            
            if (AutoGenerateCheckBox.IsChecked == true)
            {
                GenerateAndFillTable();
            }
        }

        private void UpdateHashingMethods()
        {
            HashingMethodComboBox.Items.Clear();

            if (CollisionMethodComboBox.SelectedIndex == 0)
            {
                HashingMethodComboBox.Items.Add("Метод деления");
                HashingMethodComboBox.Items.Add("Метод умножения");
                HashingMethodComboBox.Items.Add("Метод FNV1a");
            }
            else if (CollisionMethodComboBox.SelectedIndex == 1)
            {
                HashingMethodComboBox.Items.Add("Линейное исследование");
                HashingMethodComboBox.Items.Add("Квадратичное исследование");
                HashingMethodComboBox.Items.Add("Двойное хеширование");
                HashingMethodComboBox.Items.Add("Свой собственный метод");
                HashingMethodComboBox.Items.Add("Свой собственный метод 2");
            }

            if (HashingMethodComboBox.Items.Count > 0)
            {
                HashingMethodComboBox.SelectedIndex = 0;
                UpdateCurrentHashTable();
            }
        }

        private void HashingMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentHashTable();
            if (AutoGenerateCheckBox.IsChecked == true)
            {
                GenerateAndFillTable();
            }
        }

        //Обновление таблицы
        private void UpdateCurrentHashTable()
        {
            if (CollisionMethodComboBox.SelectedIndex == 0)
            {
                Func<int, int> hashFunction = null;

                switch (HashingMethodComboBox.SelectedIndex)
                {
                    case 0:
                        hashFunction = key => SeparateChainingHashTable<int, string>.DivisionHashFunction(key, tableSize);
                        break;
                    case 1:
                        hashFunction = key => SeparateChainingHashTable<int, string>.MultiplicationHashFunction(key, tableSize);
                        break;
                    case 2:
                        hashFunction = key => SeparateChainingHashTable<int, string>.FNV1aHashFunction(key, tableSize);
                        break;
                }

                if (hashFunction != null)
                {
                    currentHashTable = new SeparateChainingHashTable<int, string>(tableSize, hashFunction);
                }

                // Очистка таблицы открытой адресации
                if (openAddressingHashTable != null)
                {
                    openAddressingHashTable = null;
                }
            }
            // Открытая адресация
            else if (CollisionMethodComboBox.SelectedIndex == 1)
            {
                switch (HashingMethodComboBox.SelectedIndex)
                {
                    case 0:
                        currentProbeType = OpenAddressingHashTable<int, string>.ProbeType.Linear;
                        break;
                    case 1:
                        currentProbeType = OpenAddressingHashTable<int, string>.ProbeType.Quadratic;
                        break;
                    case 2:
                        currentProbeType = OpenAddressingHashTable<int, string>.ProbeType.Double;
                        break;
                    case 3:
                        currentProbeType = OpenAddressingHashTable<int, string>.ProbeType.Step;
                        break;
                    case 4:
                        currentProbeType = OpenAddressingHashTable<int, string>.ProbeType.Step; // Пример
                        break;
                }

                openAddressingHashTable = new OpenAddressingHashTable<int, string>(tableSize, currentProbeType);
                currentHashTable = null;
            }

            UpdateOutput();
        }
        
        //Кнопка "Генерировать"
        private void GenerateAndFillButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateAndFillTable();
        }

        //Генерация чисел
        private void GenerateAndFillTable()
        {
            if (currentHashTable == null && openAddressingHashTable == null)
            {
                MessageBox.Show("Выберите метод хеширования.");
                return;
            }

            var random = new Random();
            int numberOfElements = (CollisionMethodComboBox.SelectedIndex == 1) ? 10000 : 1000;
            List<string> errorMessages = new List<string>();

            for (int i = 0; i < numberOfElements; i++)
            {
                int key = random.Next(0, tableSize * 100);
                string value = $"Value_{i}";

                try
                {
                    if (currentHashTable != null)
                    {
                        currentHashTable.Add(key, value);
                    }
                    else if (openAddressingHashTable != null)
                    {
                        openAddressingHashTable.Insert(key, value);
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Ошибка при добавлении ключа {key}: {ex.Message}");
                }
            }

            UpdateOutput();

            if (errorMessages.Count > 0)
            {
                string allErrors = string.Join("\n", errorMessages);
                MessageBox.Show($"Хэш-таблица заполнена {numberOfElements} элементами.\nОшибки при добавлении:\n{allErrors}");
            }
            else
            {
                MessageBox.Show($"Хэш-таблица заполнена {numberOfElements} элементами. Размер таблицы: {tableSize}");
            }
        }
        
        //Кнопка "Добавить"
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(KeyTextBox.Text, out int key) && !string.IsNullOrEmpty(ValueTextBox.Text))
            {
                try
                {
                    if (currentHashTable != null)
                    {
                        currentHashTable.Add(key, ValueTextBox.Text);
                    }
                    else if (openAddressingHashTable != null)
                    {
                        openAddressingHashTable.Insert(key, ValueTextBox.Text);
                    }

                    UpdateOutput();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении ключа {key}: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Неверный ввод. Пожалуйста, введите числовой ключ и значение.");
            }
        }
        
        //Кнопка "Удалить"
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(KeyTextBox.Text, out int key))
            {
                try
                {
                    if (currentHashTable != null)
                    {
                        currentHashTable.Remove(key);
                    }
                    else if (openAddressingHashTable != null)
                    {
                        if (!openAddressingHashTable.Delete(key))
                        {
                            throw new KeyNotFoundException();
                        }
                    }

                    UpdateOutput();
                }
                catch (KeyNotFoundException)
                {
                    MessageBox.Show("Ключ не найден.");
                }
            }
            else
            {
                MessageBox.Show("Неверный ключ. Пожалуйста, введите числовой ключ.");
            }
        }

        //Кнопка "Поиск"
        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(KeyTextBox.Text, out int key))
            {
                try
                {
                    string value = null;

                    if (currentHashTable != null)
                    {
                        value = currentHashTable.Get(key);
                    }
                    else if (openAddressingHashTable != null)
                    {
                        if (!openAddressingHashTable.Search(key, out value))
                        {
                            throw new KeyNotFoundException();
                        }
                    }

                    MessageBox.Show($"Ключ: {key}, Значение: {value}");
                }
                catch (KeyNotFoundException)
                {
                    MessageBox.Show("Ключ не найден.");
                }
            }
            else
            {
                MessageBox.Show("Неверный ключ. Пожалуйста, введите числовой ключ.");
            }
        }
        
        private void UpdateOutput()
        {
            ObservableCollection<HashTableItem> items = new ObservableCollection<HashTableItem>();

            if (currentHashTable != null)
            {
                int longestChain = currentHashTable.table.Max(chain => chain?.Count ?? 0);
                var nonEmptyChains = currentHashTable.table.Where(chain => chain != null && chain.Count > 0);
                int shortestChain = nonEmptyChains.Any() ? nonEmptyChains.Min(chain => chain.Count) : 0;

                var longestIndices = currentHashTable.table
                                        .Select((chain, index) => new { chain, index })
                                        .Where(x => x.chain != null && x.chain.Count == longestChain)
                                        .Select(x => x.index)
                                        .ToList();

                var shortestIndices = currentHashTable.table
                                        .Select((chain, index) => new { chain, index })
                                        .Where(x => x.chain != null && x.chain.Count == shortestChain)
                                        .Select(x => x.index)
                                        .ToList();

                for (int i = 0; i < tableSize; i++)
                {
                    if (currentHashTable.table[i] != null && currentHashTable.table[i].Count > 0)
                    {
                        foreach (var pair in currentHashTable.table[i])
                        {
                            items.Add(new HashTableItem
                            {
                                CellIndex = i,
                                Key = pair.Key.ToString(),
                                Value = pair.Value,
                                IsLongest = longestIndices.Contains(i),
                                IsShortest = shortestIndices.Contains(i)
                            });
                        }
                    }
                    else
                    {
                        items.Add(new HashTableItem
                        {
                            CellIndex = i,
                            Key = "",
                            Value = "пусто",
                            IsLongest = false,
                            IsShortest = false
                        });
                    }
                }

                double fillFactor = currentHashTable.table.Count(chain => chain?.Count > 0) / (double)tableSize;

                InfoTextBox.Text = $"Коэффициент заполнения: {fillFactor:F2}\n \n" +
                                   $"Самая длинная цепочка: {longestChain} (ячейки: {string.Join(", ", longestIndices)})\n \n" +
                                   $"Самая короткая цепочка: {shortestChain} (ячейки: {(shortestIndices.Count > 0 ? string.Join(", ", shortestIndices) : "нет")})";
            }
            else if (openAddressingHashTable != null)
            {
                for (int i = 0; i < tableSize; i++)
                {
                    var entry = openAddressingHashTable.table[i];
                    if (entry.IsOccupied && !entry.WasDeleted)
                    {
                        items.Add(new HashTableItem
                        {
                            CellIndex = i,
                            Key = entry.Key.ToString(),
                            Value = entry.Value,
                            IsLongest = false,
                            IsShortest = false
                        });
                    }
                    else
                    {
                        items.Add(new HashTableItem
                        {
                            CellIndex = i,
                            Key = "",
                            Value = "пусто",
                            IsLongest = false,
                            IsShortest = false
                        });
                    }
                }

                InfoTextBox.Text = "";
            }

            OutputDataGrid.ItemsSource = items;
        }
    }
}
