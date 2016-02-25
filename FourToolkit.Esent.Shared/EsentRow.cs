using System.Collections.Generic;
using System.Linq;

namespace FourToolkit.Esent
{
    public class EsentRow : List<EsentCell>
    {
        public EsentCell this[string key]
        {
            get { return this.FirstOrDefault(c => c.ColumnName == key); }
        }
    }
}
