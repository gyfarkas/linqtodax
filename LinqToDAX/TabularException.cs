using System;

namespace LinqToDAX
{
    [Serializable]
    public class TabularException : Exception
    {
        public TabularException(string message) : base(message)
        {
        }
    }
}