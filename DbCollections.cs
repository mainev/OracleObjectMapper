using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAS.DatabaseConnections.Oracle
{
    public class DbCollections<T> where T : IDbObject, new()
    {

        private string dataSource;
        private string tableName;

        public DbCollections(string dataSource)
        {
            this.dataSource = dataSource;
            this.tableName = GetTableName(typeof(T));
        }

        /// <summary>
        /// Returns a list of <typeparamref name="T"/> objects.
        /// </summary>
        public List<T> Get()
        {
            using (OracleConnection oracleConnection = new OracleConnection(this.dataSource))
            {
                string sql = "SELECT * FROM " + this.tableName;
                DbMapper<T> dbMapper = new DbMapper<T>();

                OracleCommand cmd = oracleConnection.CreateCommand();
                oracleConnection.Open();
                cmd.CommandText = sql;
                OracleDataReader dr = cmd.ExecuteReader();
                List<T> objectList = dbMapper.GetDataObjects(dr);
                oracleConnection.Close();

                return objectList;
            }
        }

        public T Add(T item)
        {
            using (OracleConnection oracleConnection = new OracleConnection(this.dataSource))
            {
                oracleConnection.Open();
                OracleCommand cmd = oracleConnection.CreateCommand();
                cmd.CommandText = new DbMapper<T>().CreateInsertSQL(item, this.tableName);
                int rowsUpdated = cmd.ExecuteNonQuery();
                oracleConnection.Close();
            }
            return item;
        }

        public T Update(T item)
        {
            using (OracleConnection oracleConnection = new OracleConnection(this.dataSource))
            {
                oracleConnection.Open();
                OracleCommand cmd = oracleConnection.CreateCommand();
                cmd.CommandText = new DbMapper<T>().CreateUpdateSQL(item, this.tableName);
                int rowsUpdated = cmd.ExecuteNonQuery();
                oracleConnection.Close();
            }
            return item;
        }

        private string GetTableName(System.Type type)
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(type);
            foreach (System.Attribute attr in attrs)
            {
                if (attr is DbObject)
                {
                    DbObject db = (DbObject)attr;
                    return db.table;
                }
            }
            throw new Exception(type.FullName + " is not a valid implementation of DbObject");
        }

    }
}
