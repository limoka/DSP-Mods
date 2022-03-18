using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BlueprintTweaks
{
    [Serializable]
    public class FixedSizeStack<T> : ICollection, IReadOnlyCollection<T>
    {
        private T[] _data;
        private int _pointer;
        private int _count;
        private long _version;

        public FixedSizeStack(int size)
        {
            _data = new T[size];
            _pointer = _data.GetLowerBound(0);
            _version = 0;
        }

        private void _IncrementPointer()
        {
            if (_pointer++ == _data.GetUpperBound(0))
            {
                _pointer = _data.GetLowerBound(0);
            }
        }

        private void _DecrementPointer()
        {
            if (_pointer-- == _data.GetLowerBound(0))
            {
                _pointer = _data.GetUpperBound(0);
            }
        }

        public bool Contains(T item) => _data.Contains(item);
        
        public void CopyTo(Array array, int index)
        {
            if (array.Rank != 1)
                throw new ArgumentException($"Array with dimension of {array.Rank} is not supported!");
            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("Array with non zero lower bound is not supported!");
            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException($"{index} is out of range. Index has to be greater than zero and less than {array.Length}");
            if (array.Length - index < _count)
                throw new ArgumentException("Invalid off length");
            try
            {
                Array.Copy(_data, 0, array, index, _count);
                Array.Reverse(array, index, _count);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException($"Invalid array type! Expected type {typeof(T)}.");
            }
        }

        public int Count => _count;
        
        
        private object _syncRoot;

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        public bool IsSynchronized => false;
        public int Size => _data.Length;
        public void Clear() => _data = new T[Size];

        public T this[int index]
        {
            get
            {
                var i = _pointer - index;
                if (i < 0)
                {
                    i += Size;
                }

                return _data[i];
            }
        }

        public T Pop()
        {
            var item = _data[_pointer];
            _data[_pointer] = default(T);
            _DecrementPointer();
            _version++;
            if (_count > _data.GetLowerBound(0))
            {
                _count--;
            }

            return item;
        }

        public void Push(T item)
        {
            _IncrementPointer();
            _data[_pointer] = item;
            _version++;
            if (_count <= _data.GetUpperBound(0))
            {
                _count++;
            }
        }

        public T Peek() => _data[_pointer];

        [Serializable]
        private struct Enumerator : IEnumerator<T>
        {
            private FixedSizeStack<T> _stack;
            private int _index;
            private long _version;
            private T _currentElement;

            internal Enumerator(FixedSizeStack<T> stack)
            {
                _stack = stack;
                _version = _stack._version;
                _index = -2;
                _currentElement = default(T);
            }

            public void Dispose() => _index = -1;

            public bool MoveNext()
            {
                if (_version != _stack._version)
                    throw new InvalidOperationException("Version Conflict");
                if (_index == -2)
                {
                    _index = 0;
                    var flag = _index >= 0;
                    if (flag)
                        _currentElement = _stack[_index];
                    return flag;
                }

                if (_index == -1)
                    return false;
                var num = _index + 1;
                _index = num;
                var flag1 = num < _stack.Count();
                _currentElement = !flag1 ? default(T) : _stack[_index];
                return flag1;
            }

            public T Current
            {
                get
                {
                    if (_index == -2)
                        throw new InvalidOperationException("Enumeration Not Started");
                    if (_index == -1)
                        throw new InvalidOperationException("Enumeration Ended");
                    return _currentElement;
                }
            }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                if (_version != _stack._version)
                    throw new InvalidOperationException("Version Conflict");
                _index = -2;
                _currentElement = default(T);
            }
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}