using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterCard.SDK
{


    public class OAuthParameters
    {
        protected SortedList<string, SortedSet<string>> baseParameters;

        public OAuthParameters()
        {
            baseParameters = new SortedList<string, SortedSet<string>>(new QueryParameterComparer());
        }

        public SortedList<string, SortedSet<string>> getBaseParameters()
        {
            return baseParameters;
        }

        public void addParameter(String key, String value)
        {
            put(key, value);
        }

        public string get(string key)
        {
            SortedSet<string> s;
            this.baseParameters.TryGetValue(key, out s);
            return s == null ? Connector.EMPTY_STRING : s.Min;
        }

        public void put(String key, String value)
        {
            SortedSet<string> temp;
            if (this.baseParameters.ContainsKey(key))
            {
                this.baseParameters.TryGetValue(key, out temp);
                temp.Add(value);

                this.baseParameters.Add(key, temp);

            }
            else
            {
                temp = new SortedSet<string>(new QueryParameterComparer());
                temp.Add(value);
                this.baseParameters.Add(key, temp);
            }
        }

        public void putAll(SortedList<string, SortedSet<string>> Oparams)
        {
            foreach (KeyValuePair<string, SortedSet<string>> kvpList in Oparams)
            {
                this.baseParameters.Add(kvpList.Key, kvpList.Value);
            }

        }

        public void remove(string key, string value)
        {
            SortedSet<string> temp;
            if (this.baseParameters.ContainsKey(key) && value != null)
            {
                this.baseParameters.TryGetValue(key, out temp);
                this.baseParameters.Remove(key);
                if (temp.Contains(value))
                {
                    temp.Remove(value);
                    this.baseParameters.Add(key, temp);
                }
            }
            else if (this.baseParameters.ContainsKey(key) && value == null)
            {
                this.baseParameters.Remove(key);
            }
        }


    }
}