using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Logic;

namespace Lab6
{
    public partial class MainWindow : Window
    {
        private SeparateChainingHashTable<int, string> currentHashTable;
        private int tableSize = 1000;

        public MainWindow()
        {
            InitializeComponent();

            CollisionMethodComboBox.SelectedIndex = 0;

            UpdateHashingMethods();
            HashingMethodComboBox.SelectedIndex = 0;

            UpdateCurrentHashTable();
        }

        private void CollisionMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHashingMethods();

            // Если выбран режим автоматической генерации, заполняем таблицу.
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

            // Если выбран режим автоматической генерации, заполняем таблицу.
            if (AutoGenerateCheckBox.IsChecked == true)
            {
                GenerateAndFillTable();
            }
        }

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
            }
            else if (CollisionMethodComboBox.SelectedIndex == 1)
            {
                MessageBox.Show("Методы открытой адресации пока не реализованы.");
                currentHashTable = null;
            }

            UpdateOutput();
        }

        private void GenerateAndFillButton_Click(object sender, RoutedEventArgs e)
        {
            // Ручная генерация таблицы
            GenerateAndFillTable();
        }

        private void GenerateAndFillTable()
        {
            if (currentHashTable == null)
            {
                MessageBox.Show("Выберите метод хеширования.");
                return;
            }

            var random = new Random();
            for (int i = 0; i < 100000; i++)
            {
                int key = random.Next(0, tableSize * 10); // Уменьшенный диапазон
                string value = $"Value_{i}";

                try
                {
                    currentHashTable.Add(key, value);
                }
                catch (Exception ex)
                {
                    // Логируем ошибки, чтобы понять проблему
                    Console.WriteLine($"Ошибка при добавлении ключа {key}: {ex.Message}");
                }
            }

            UpdateOutput();
            MessageBox.Show("Хэш-таблица заполнена 100,000 элементами.");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentHashTable == null)
            {
                MessageBox.Show("Выберите метод хеширования.");
                return;
            }

            if (int.TryParse(KeyTextBox.Text, out int key) && !string.IsNullOrEmpty(ValueTextBox.Text))
            {
                currentHashTable.Add(key, ValueTextBox.Text);
                UpdateOutput();
            }
            else
            {
                MessageBox.Show("Неверный ввод. Пожалуйста, введите числовой ключ и значение.");
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentHashTable == null)
            {
                MessageBox.Show("Выберите метод хеширования.");
                return;
            }

            if (int.TryParse(KeyTextBox.Text, out int key))
            {
                try
                {
                    currentHashTable.Remove(key);
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

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentHashTable == null)
            {
                MessageBox.Show("Выберите метод хеширования.");
                return;
            }

            if (int.TryParse(KeyTextBox.Text, out int key))
            {
                try
                {
                    string value = currentHashTable.Get(key);
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

        // Класс для представления данных в DataGrid
        public class HashTableItem
        {
            public int CellIndex { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private void UpdateOutput()
        {
            if (currentHashTable != null)
            {
                ObservableCollection<HashTableItem> items = new ObservableCollection<HashTableItem>();

                for (int i = 0; i < tableSize; i++)
                {
                    if (currentHashTable.table[i] != null)
                    {
                        foreach (var pair in currentHashTable.table[i])
                        {
                            items.Add(new HashTableItem { CellIndex = i, Key = pair.Key.ToString(), Value = pair.Value });
                        }
                    }
                    else
                    {
                        items.Add(new HashTableItem { CellIndex = i, Key = "", Value = "пусто" });
                    }
                }
                OutputDataGrid.ItemsSource = items;
            }
            else
            {
                OutputDataGrid.ItemsSource = null; // Очищаем DataGrid
            }
        }
    }
}