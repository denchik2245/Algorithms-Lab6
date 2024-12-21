using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class SeparateChainingHashTable<K, V>
    {
        public LinkedList<KeyValuePair<K, V>>[] table; // Массив связных списков
        private readonly int tableSize; // Размер таблицы
        public Func<K, int> hashFunction; // Хэш-функция

        public SeparateChainingHashTable(int size, Func<K, int> hashFunction)
        {
            tableSize = size;
            this.hashFunction = hashFunction;
            table = new LinkedList<KeyValuePair<K, V>>[tableSize];

            for (int i = 0; i < tableSize; i++)
                table[i] = new LinkedList<KeyValuePair<K, V>>(); // Инициализация связных списков
        }

        /// Метод вычисления хэша путем деления.
        public static int DivisionHashFunction<T>(T key, int tableSize)
        {
            int keyValue = Convert.ToInt32(key); // Преобразуем ключ в целое число
            return Math.Abs(keyValue % tableSize); // Вычисляем индекс как остаток от деления
        }

        /// Метод вычисления хэша путем умножения.
        public static int MultiplicationHashFunction<T>(T key, int tableSize)
        {
            double A = 0.6180339887; // Константа для умножения
            int keyValue = Convert.ToInt32(key); // Преобразуем ключ в целое число
            double fraction = (keyValue * A) % 1; // Вычисляем дробную часть
            return (int)(tableSize * fraction); // Вычисляем индекс
        }

        public static int FNV1aHashFunction<T>(T key, int tableSize)
        {
            const uint FNV_PRIME = 16777619;
            const uint FNV_OFFSET_BASIS = 2166136261;

            uint hash = FNV_OFFSET_BASIS;

            byte[] data;

            if (typeof(T) == typeof(int))
            {
                data = BitConverter.GetBytes(Convert.ToInt32(key)); // Преобразование T в int, затем в byte[]
            }
            else if (typeof(T) == typeof(string))
            {
                data = System.Text.Encoding.UTF8.GetBytes(key as string);
            }
            else
            {
                // Обработка других типов T по необходимости.
                throw new ArgumentException("Тип ключа не поддерживается для FNV1a.");
            }

            foreach (byte b in data)
            {
                hash ^= b;
                hash *= FNV_PRIME;
            }

            return (int)(hash % tableSize);
        }


        private static byte[] GetBytes<T>(T key)
        {
            // Преобразуем ключ в строку
            string keyString = key.ToString();

            // Преобразуем строку в байтовый массив
            return Encoding.UTF8.GetBytes(keyString);
        }

        /// Добавить пару ключ-значение в хэш-таблицу.
        /// Если ключ уже существует, обновить значение.
        public void Add(K key, V value)
        {
            int hash = hashFunction(key); // Вычисляем хэш
            var cell = table[hash];

            foreach (var pair in cell)
            {
                if (pair.Key.Equals(key))
                {
                    // Обновляем значение, если ключ уже существует
                    cell.Remove(pair);
                    cell.AddLast(new KeyValuePair<K, V>(key, value));
                    return;
                }
            }

            // Если ключа нет, добавляем новую пару в конец списка
            cell.AddLast(new KeyValuePair<K, V>(key, value));
        }

        /// Получить значение по ключу.
        public V Get(K key)
        {
            int hash = hashFunction(key);
            var chain = table[hash];

            foreach (var pair in chain)
            {
                if (pair.Key.Equals(key))
                {
                    return pair.Value;
                }
            }

            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        /// Удалить элемент по ключу.
        public void Remove(K key)
        {
            int hash = hashFunction(key);
            var chain = table[hash];

            foreach (var pair in chain)
            {
                if (pair.Key.Equals(key))
                {
                    chain.Remove(pair);
                    return;
                }
            }

            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        /// Получить строковое представление всех элементов хэш-таблицы.
        //public string Display()
        //{
        //    var sb = new StringBuilder();
        //    int lineWidth = 120; // Максимальная длина строки

        //    for (int i = 0; i < tableSize; i++)
        //    {
        //        int maxKeyLength = table[i].Any() ? table[i].Max(p => p.Key.ToString().Length) : 0;
        //        int maxValueLength = table[i].Any() ? table[i].Max(p => p.Value.ToString().Length) : 0;

        //        sb.Append(string.Format("Ячейка {0,3}: ", i));

        //        if (table[i].Any())
        //        {
        //            int currentLineLength = sb.Length; // Текущая длина строки
        //            foreach (var pair in table[i])
        //            {
        //                string item = string.Format("[{0,-" + maxKeyLength + "} | {1,-" + maxValueLength + "}]", pair.Key, pair.Value);
        //                if (currentLineLength + item.Length + 2 > lineWidth) // +2 для ", "
        //                {
        //                    sb.AppendLine();
        //                    sb.Append("          "); // Отступ для продолжения строки
        //                    currentLineLength = 10; // Длина отступа
        //                }
        //                else if (currentLineLength > 10) // Добавляем запятую только если это не первый элемент в строке
        //                {
        //                    sb.Append(", ");
        //                    currentLineLength += 2;
        //                }

        //                sb.Append(item);
        //                currentLineLength += item.Length;
        //            }
        //        }
        //        else
        //        {
        //            sb.Append("[ ]");
        //        }

        //        sb.AppendLine();
        //    }

        //    return sb.ToString().TrimEnd();
        //}
    }
}