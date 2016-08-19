using System;
using System.Collections.Generic;

namespace GraphTest.BankModel.Classes
{
    sealed class EventList<T> : List<T>
    {
        public delegate void MethodContainer(T item);
        public event MethodContainer OnAdd;
        public event MethodContainer OnRemove;

        public new void Add(T item)
        {
            OnAdd(item);
            base.Add(item);
        }

        public new void AddRange(List<T> itemList) //TODO see RemoveAll
        {
            foreach (var item in itemList)
            {
                OnAdd(item);
                base.Add(item);
            }
        }

        public new void Remove(T item)
        {
            OnRemove(item);
            base.Remove(item);
        }

        public new void RemoveAll(Predicate<T> match)
        {
            var itemsToRemove = FindAll(match);
            foreach (var item in itemsToRemove)
                Remove(item);
        }
        
    }
}
