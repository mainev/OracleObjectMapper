using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAS.DatabaseConnections.Oracle
{
    public class DbObject : Attribute
    {
        public string table;
    }
}
