using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDAX
{
    public class TabularGrouping<T,U> : IGrouping<T,U>
    {
        public T Key
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator<U> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
