using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterCard.SDK
{
    public class SortedSet<T> : SortedList<T, byte>
    {
        public SortedSet(IComparer<T> comparer)
            : base(comparer)
        {
        }

        public T Min
        {
            get
            {
                if ((base.Count) >= 1)
                {
                    return base.Keys[0];
                }
                else
                {
                    return default(T);
                }
            }
        }

        public T Max
        {
            get
            {
                if ((base.Count) >= 1)
                {
                    return base.Keys[base.Keys.Count - 1];
                }
                else
                {
                    return default(T);
                }
            }
        }


        public bool Contains(T value)
        {
            return base.ContainsKey(value);
        }

        public void Add(T value)
        {
            base.Add(value, 0);
        }
    }
}
