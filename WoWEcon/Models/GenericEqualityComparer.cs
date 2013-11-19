using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WoWEcon.Models
{
    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        public GenericEqualityComparer(Func<T, T, Boolean> equals, Func<T, int> getHashCode)
        {
            _equals = equals;
            _hash = getHashCode;
        }
        private readonly Func<T, T, Boolean> _equals;
        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        private readonly Func<T, int> _hash;
        public int GetHashCode(T obj)
        {
            return _hash(obj);
        }
    }
}