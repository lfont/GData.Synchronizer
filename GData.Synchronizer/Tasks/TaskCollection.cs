using System;
using System.Collections;
using System.Collections.Generic;

namespace GData.Synchronizer.Tasks
{
    public class TaskCollection<TItem> : ICollection<IGDataTask<TItem>>
    {
        private readonly List<IGDataTask<TItem>> _tasks = new List<IGDataTask<TItem>>();
        private string _unicityToken;

        public string UnicityToken
        {
            get { return _unicityToken; }
            set
            {
                if (_unicityToken != value)
                {
                    string oldValue = _unicityToken;
                    _unicityToken = value;
                    RebuildIds(oldValue);
                }
            }
        }

        public int Count
        {
            get { return _tasks.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        private void RebuildIds(string oldToken)
        {
            foreach (IGDataTask<TItem> task in _tasks)
            {
                task.Id = task.Id.Replace(oldToken, UnicityToken);
            }
        }

        public void Add(IGDataTask<TItem> item)
        {
            item.Id = UnicityToken + (String.IsNullOrEmpty(item.Id) ? String.Empty : "_" + item.Id) + "_" + Count;
            _tasks.Add(item);
        }

        public void Clear()
        {
            _tasks.Clear();
        }

        public bool Contains(IGDataTask<TItem> item)
        {
            return _tasks.Contains(item);
        }

        public void CopyTo(IGDataTask<TItem>[] array, int arrayIndex)
        {
            _tasks.CopyTo(array, arrayIndex);
        }

        public bool Remove(IGDataTask<TItem> item)
        {
            return _tasks.Remove(item);
        }

        public IEnumerator<IGDataTask<TItem>> GetEnumerator()
        {
            return _tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tasks.GetEnumerator();
        }
    }
}
