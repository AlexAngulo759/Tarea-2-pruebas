using System;
using System.Collections.Generic;

namespace Proyecto_Grafos.Models 
{
    public class Dictionary<K, V>
    {
        private class KeyValuePair
        {
            public K Key { get; set; }
            public V Value { get; set; }

            public KeyValuePair(K key, V value)
            {
                Key = key;
                Value = value;
            }
        }

        private LinkedList<KeyValuePair>[] _buckets;
        private int _capacity;
        private int _count;

        public Dictionary() 
        {
            _capacity = 10;
            _buckets = new LinkedList<KeyValuePair>[_capacity];
            _count = 0;
        }

        private int GetHash(K key)
        {
            int hash = key.GetHashCode() & 0x7FFFFFFF;
            return hash % _capacity;
        }

        public void Add(K key, V value)
        {
            int index = GetHash(key);

            if (_buckets[index] == null)
            {
                _buckets[index] = new LinkedList<KeyValuePair>();
            }

            for (int i = 0; i < _buckets[index].Count; i++)
            {
                if (_buckets[index].Get(i).Key.Equals(key))
                {
                    _buckets[index].Get(i).Value = value;
                    return;
                }
            }

            _buckets[index].Add(new KeyValuePair(key, value));
            _count++;
        }

        public V Get(K key)
        {
            int index = GetHash(key);

            if (_buckets[index] != null)
            {
                for (int i = 0; i < _buckets[index].Count; i++)
                {
                    KeyValuePair pair = _buckets[index].Get(i);
                    if (pair.Key.Equals(key))
                    {
                        return pair.Value;
                    }
                }
            }

            throw new KeyNotFoundException("Key does not exist in the dictionary");
        }

        public bool ContainsKey(K key)
        {
            int index = GetHash(key);

            if (_buckets[index] != null)
            {
                for (int i = 0; i < _buckets[index].Count; i++)
                {
                    if (_buckets[index].Get(i).Key.Equals(key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public LinkedList<K> Keys()
        {
            LinkedList<K> keys = new LinkedList<K>();

            for (int i = 0; i < _capacity; i++)
            {
                if (_buckets[i] != null)
                {
                    for (int j = 0; j < _buckets[i].Count; j++)
                    {
                        keys.Add(_buckets[i].Get(j).Key);
                    }
                }
            }

            return keys;
        }

        public V this[K key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }
    }
}