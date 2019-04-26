using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CAS.DatabaseConnections.Oracle
{
    public class DbMapper<T> where T : IDbObject, new()
    {
        private List<String> dbFields = new List<String>();

        public DbMapper()
        {
        }

        private void setDbFields(OracleDataReader reader)
        {
            int i = 0;
            while (i < reader.FieldCount)
            {
                dbFields.Add(reader.GetName(i).ToLower());
                i++;
            }
        }

        private T GetInstance()
        {
            return new T();
        }

        public string CreateUpdateSQL(T item, string tbl)
        {

            List<string> values = new List<string>();
            int id = 0;
            var dbObject = this.GetInstance();
            Type dbObjectType = typeof(T);
            var objectFields = dbObjectType.GetProperties().ToList();
            //var objectFieldNames = dbObjectType.GetProperties().Select(field => field.Name).ToList();
            //var fieldNames = dbObjectType.GetProperties()
            //                .Select(field => field.Name)
            //                .ToList();

            objectFields.ForEach(field =>
            {
                //FieldInfo fi = dbObjectType.GetField(field, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                //PropertyInfo fi = dbObjectType.GetProperty(field);

                string value = "";
                if (String.Equals("ID".ToLower(), field.Name.ToLower()))
                {
                    id = Convert.ToInt32(field.GetValue(item));
                }
                else
                {
                    switch (field.PropertyType.Name)
                    {
                        case "Boolean":
                            value = (string)((bool)field.GetValue(item) ? "1" : "0");
                            break;
                        case "String":
                            value = "\'" + (string)(field.GetValue(item)) + "\'";
                            break;
                        case "Int16":
                        case "Int32":
                            value = Convert.ToString(field.GetValue(item));
                            break;
                        default:
                            throw new NotImplementedException("Field " + field + " " + field.PropertyType.Name + " is not filtered");
                    }
                    value = String.IsNullOrEmpty(value) ? "null" : value;

                    string field_value = "";
                    field_value += field.Name + "=" + value;
                    values.Add(field_value);
                }
            });

            return String.Format("UPDATE {0} SET {1} WHERE ID = {2}", tbl, string.Join(",", values), id);


        }

        public string CreateInsertSQL(T item, string tbl)
        {

            List<string> values = new List<string>();

            var dbObject = this.GetInstance();
            Type dbObjectType = typeof(T);
            var objectFields = dbObjectType.GetProperties().ToList();
            var objectFieldNames = dbObjectType.GetProperties().Select(field => field.Name).ToList();

            objectFields.ForEach(field =>
            {
                string value = "";
                    switch (field.PropertyType.Name)
                    {
                        case "Boolean":
                            value = (string)((bool)field.GetValue(item) ? "1" : "0");
                            break;
                        case "String":
                            value = "\'" + (string)(field.GetValue(item)) + "\'";
                            break;
                        case "Int16":
                        case "Int32":
                            value = Convert.ToString(field.GetValue(item));
                            break;
                        default:
                            throw new NotImplementedException("Field " + field + " " + field.PropertyType.Name + " is not filtered");
                    }
                    value = String.IsNullOrEmpty(value) ? "null" : value;
                    values.Add(value);
            });

            return String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tbl, string.Join(",", objectFieldNames), string.Join(",", values));

        }

        public T GetDataObject()
        {
            var dbObject = this.GetInstance();

            return dbObject;
        }

        public List<T> GetDataObjects(OracleDataReader reader)
        {

            List<T> list = new List<T>();
            setDbFields(reader);

            while (reader.Read())
            {
                var dbObject = this.GetInstance();
                Type dbObjectType = typeof(T);

                dbFields.ForEach(field_name =>
                {
                    //FieldInfo fi = dbObjectType.GetField(field_name);
                    PropertyInfo fi = dbObjectType.GetProperty(field_name);

                    if (fi != null && reader[field_name] != System.DBNull.Value)
                    {
                        switch (fi.PropertyType.Name)
                        {
                            case "Boolean":
                                fi.SetValue(dbObject, Convert.ToBoolean(reader[field_name]));
                                break;
                            case "String":
                                fi.SetValue(dbObject, reader[field_name]);
                                break;
                            case "Int16":
                                fi.SetValue(dbObject, Convert.ToInt16(reader[field_name]));
                                break;
                            case "Int32":
                                fi.SetValue(dbObject, Convert.ToInt32(reader[field_name]));
                                break;
                            default:
                                throw new NotImplementedException("Field " + field_name + " " + fi.PropertyType.Name + " is not configured in DbMapper");

                        }
                    }
                });
                list.Add(dbObject);
            }

            return list;
        }
    }
}
