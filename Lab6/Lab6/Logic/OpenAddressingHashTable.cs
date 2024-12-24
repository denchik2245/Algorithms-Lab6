using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class OpenAddressingHashTable<TKey, TValue>
    {
        public struct Entry
        {
            public TKey Key;
            public TValue Value;
            public bool IsOccupied;
            public bool WasDeleted; // Флаг для отметки удаленных элементов
        }

        public Entry[] table;
        private int size;
        private int count;
        private double loadFactor = 0.75;

        public OpenAddressingHashTable(int size)
        {
            this.size = size;
            this.table = new Entry[size];
            this.count = 0;
        }

        private int Hash(TKey key)
        {
            double A = 0.6180339887; // Константа для умножения
            int keyValue = key is int ? (int)(object)key : key.GetHashCode(); // Преобразуем ключ в целое число или используем GetHashCode
            double fraction = (keyValue * A) % 1; // Вычисляем дробную часть
            return (int)(size * fraction); // Вычисляем индекс
        }

        // Линейное исследование
        private int LinearProbeHashFunction(int hash, int i)
        {
            return (hash + i) % size;
        }

        // Квадратичное исследование
        private int QuadraticProbeHashFunction(int hash, int i)
        {
            return (hash + i * i) % size;
        }

        // Двойное хеширование
        private int DoubleHashHashFunction(TKey key, int i)
        {
            int hash1 = Hash(key);
            int hash2 = 1 + (Hash(key) % (size - 2)); // Второе хеширование (простое число меньше размера таблицы)
            return (hash1 + i * hash2) % size;
        }

        // Шаговое исследование
        private int StepProbeHashFunction(int hash, int i, TKey key)
        {
            int step = 1 + (Hash(key) % (size - 1)); // Шаг для обхода
            return (hash + i * step) % size;
        }

        // Экспоненциальное исследование
        private int ExponentialProbeHashFunction(int hash, int i)
        {
            return (hash + (1 << i)) % size; // 1 << i это 2^i
        }

        private void Resize()
        {
            int newSize = GetPrime(size * 2); // Получаем новое простое число, примерно в 2 раза больше
            Entry[] newTable = new Entry[newSize];
            Entry[] oldTable = table; // Сохраняем ссылку на старую таблицу
            table = newTable;
            size = newSize;
            count = 0;

            foreach (var entry in oldTable)
            {
                if (entry.IsOccupied && !entry.WasDeleted)
                {
                    Insert(entry.Key, entry.Value, ProbeType.Linear); // Переносим элементы в новую таблицу
                }
            }
        }

        private int GetPrime(int min)
        {
            for (int i = min; ; i++)
            {
                if (IsPrime(i))
                {
                    return i;
                }
            }
        }

        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number <= 3) return true;
            if (number % 2 == 0 || number % 3 == 0) return false;
            for (int i = 5; i * i <= number; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
            }
            return true;
        }

        public void Insert(TKey key, TValue value, ProbeType probeType)
        {
            if ((double)count / size >= loadFactor)
            {
                Resize();
            }

            int hash = Hash(key);
            int i = 0;
            int index;

            while (true)
            {
                index = GetProbeIndex(hash, key, i, probeType);

                if (!table[index].IsOccupied || table[index].WasDeleted)
                {
                    table[index].Key = key;
                    table[index].Value = value;
                    table[index].IsOccupied = true;
                    table[index].WasDeleted = false;
                    count++;
                    return;
                }

                if (table[index].Key.Equals(key))
                {
                    throw new ArgumentException("Элемент с таким ключом уже существует.");
                }

                i++;
                if (i >= size)
                {
                    throw new InvalidOperationException("Не удалось найти свободное место.");
                }
            }
        }

        public bool Search(TKey key, ProbeType probeType, out TValue value)
        {
            int hash = Hash(key);
            int i = 0;
            int index;

            while (true)
            {
                switch (probeType)
                {
                    case ProbeType.Linear:
                        index = LinearProbeHashFunction(hash, i);
                        break;
                    case ProbeType.Quadratic:
                        index = QuadraticProbeHashFunction(hash, i);
                        break;
                    case ProbeType.Double:
                        index = DoubleHashHashFunction(key, i);
                        break;
                    default:
                        throw new ArgumentException("Неверный тип исследования.");
                }

                if (!table[index].IsOccupied)
                {
                    value = default(TValue);
                    return false;
                }

                if (table[index].Key.Equals(key) && !table[index].WasDeleted)
                {
                    value = table[index].Value;
                    return true;
                }
                i++;
                if (i >= size)
                {
                    value = default(TValue);
                    return false;
                }
            }
        }

        public bool Delete(TKey key, ProbeType probeType)
        {
            int hash = Hash(key);
            int i = 0;
            int index;

            while (true)
            {
                switch (probeType)
                {
                    case ProbeType.Linear:
                        index = LinearProbeHashFunction(hash, i);
                        break;
                    case ProbeType.Quadratic:
                        index = QuadraticProbeHashFunction(hash, i);
                        break;
                    case ProbeType.Double:
                        index = DoubleHashHashFunction(key, i);
                        break;
                    default:
                        throw new ArgumentException("Неверный тип исследования.");
                }

                if (!table[index].IsOccupied)
                {
                    return false;
                }

                if (table[index].Key.Equals(key) && !table[index].WasDeleted)
                {
                    table[index].WasDeleted = true; // Помечаем как удаленный
                    count--;
                    return true;
                }
                i++;
                if (i >= size)
                {
                    return false;
                }
            }
        }

        private int GetProbeIndex(int hash, TKey key, int i, ProbeType probeType)
        {
            return probeType switch
            {
                ProbeType.Linear => LinearProbeHashFunction(hash, i),
                ProbeType.Quadratic => QuadraticProbeHashFunction(hash, i),
                ProbeType.Double => DoubleHashHashFunction(key, i),
                ProbeType.Exponential => ExponentialProbeHashFunction(hash, i),
                ProbeType.Step => StepProbeHashFunction(hash, i, key),
                _ => throw new ArgumentException("Неверный тип исследования.")
            };
        }

        public enum ProbeType
        {
            Linear,
            Quadratic,
            Double,
            Exponential,
            Step
        }
    }
}
