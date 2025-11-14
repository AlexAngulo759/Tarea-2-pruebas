using System;

namespace Proyecto_Grafos.Models
{
    public class LinkedList<T>
    {
        private Node<T> _head;
        private int _count;

        public LinkedList()
        {
            _head = null;
            _count = 0;
        }

        public void Add(T data)
        {
            Node<T> newNode = new Node<T>(data);

            if (_head == null)
            {
                _head = newNode;
            }
            else
            {
                Node<T> current = _head;
                while (current.Next != null)
                {
                    current = current.Next;
                }
                current.Next = newNode;
            }
            _count++;
        }

        public T Get(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException("Index out of range");

            Node<T> current = _head;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }
            return current.Data;
        }
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException("Index out of range");

            if (index == 0)
            {
                _head = _head.Next;
            }
            else
            {
                Node<T> current = _head;
                for (int i = 0; i < index - 1; i++)
                {
                    current = current.Next;
                }
                current.Next = current.Next.Next;
            }
            _count--;
        }

        public int Count
        {
            get { return _count; }
        }
        public T[] ToArray()
        {
            T[] array = new T[_count];
            Node<T> current = _head;
            int index = 0;

            while (current != null)
            {
                array[index] = current.Data;
                current = current.Next;
                index++;
            }
            return array;
        }
    }
}