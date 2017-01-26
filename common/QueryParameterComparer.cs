using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterCard.SDK
{
    public class QueryParameterComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.Equals(y))
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            else
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
        }
    }
}