using System.Text;

namespace Logic
{
    public class SeparateChainingHashTable<K, V>
    {
        public LinkedList<KeyValuePair<K, V>>[] table;
        private readonly int tableSize;
        public Func<K, int> hashFunction;

        public SeparateChainingHashTable(int size, Func<K, int> hashFunction)
        {
            tableSize = size;
            this.hashFunction = hashFunction;
            table = new LinkedList<KeyValuePair<K, V>>[tableSize];

            for (int i = 0; i < tableSize; i++)
                table[i] = new LinkedList<KeyValuePair<K, V>>();
        }

        /// Метод вычисления хэша путем деления.
        public static int DivisionHashFunction<T>(T key, int tableSize)
        {
            int keyValue = key is int ? (int)(object)key : key.GetHashCode();
            return Math.Abs(keyValue % tableSize);
        }

        /// Метод вычисления хэша путем умножения.
        public static int MultiplicationHashFunction<T>(T key, int tableSize)
        {
            double A = 0.6180339887;
            int keyValue = key is int ? (int)(object)key : key.GetHashCode();
            double fraction = (keyValue * A) % 1;
            return (int)(tableSize * fraction);
        }

        public static int FNV1aHashFunction<T>(T key, int tableSize)
        {
            const uint FNV_PRIME = 16777619;
            const uint FNV_OFFSET_BASIS = 2166136261;

            uint hash = FNV_OFFSET_BASIS;

            byte[] data;

            if (key is int)
            {
                data = BitConverter.GetBytes((int)(object)key);
            }
            else if (key is string)
            {
                data = Encoding.UTF8.GetBytes(key as string);
            }
            else
            {
                throw new ArgumentException("Тип ключа не поддерживается для FNV1a.");
            }

            foreach (byte b in data)
            {
                hash ^= b;
                hash *= FNV_PRIME;
            }

            return (int)(hash % tableSize);
        }

        /// Добавить пару ключ-значение в хэш-таблицу.
        /// Если ключ уже существует, обновить значение.
        public void Add(K key, V value)
        {
            int hash = hashFunction(key);
            var cell = table[hash];

            foreach (var pair in cell)
            {
                if (pair.Key.Equals(key))
                {
                    cell.Remove(pair);
                    cell.AddLast(new KeyValuePair<K, V>(key, value));
                    return;
                }
            }
            
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
    }
}