using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace Tools.Extension
{
    /// <summary>
    /// JSON转换类
    /// </summary>
    public class ConvertJson
    {
        #region 私有方法

        /// <summary>
        /// 过滤特殊字符
        /// </summary>
        private static string String2Json(string s)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < s.Length; i++)
            {
                var c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 格式化字符型、日期型、布尔型
        /// </summary>
        private static string StringFormat(string str, Type type)
        {
            if (type == typeof(string))
            {
                str = String2Json(str);
                str = "\"" + str + "\"";
            }
            else if (type == typeof(DateTime))
            {
                str = "\"" + str + "\"";
            }
            else if (type == typeof(bool))
            {
                str = str.ToLower();
            }
            else if (type != typeof(string) && string.IsNullOrEmpty(str))
            {
                str = "\"" + str + "\"";
            }
            return str;
        }

        #endregion 私有方法

        #region List转换成Json

        /// <summary>
        /// List转换成Json
        /// </summary>
        public static string ListToJson<T>(IList<T> list)
        {
            object obj = list[0];
            return ListToJson(list, obj.GetType().Name);
        }

        /// <summary>
        /// List转换成Json
        /// </summary>
        public static string ListToJson<T>(IList<T> list, string jsonName)
        {
            var json = new StringBuilder();
            if (string.IsNullOrEmpty(jsonName)) jsonName = list[0].GetType().Name;
            json.Append("{\"" + jsonName + "\":[");
            if (list.Count > 0)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var obj = Activator.CreateInstance<T>();
                    var pi = obj.GetType().GetProperties();
                    json.Append("{");
                    for (var j = 0; j < pi.Length; j++)
                    {
                        var type = pi[j].GetValue(list[i], null).GetType();
                        json.Append("\"" + pi[j].Name + "\":" + StringFormat(pi[j].GetValue(list[i], null).ToString(), type));

                        if (j < pi.Length - 1)
                        {
                            json.Append(",");
                        }
                    }
                    json.Append("}");
                    if (i < list.Count - 1)
                    {
                        json.Append(",");
                    }
                }
            }
            json.Append("]}");
            return json.ToString();
        }

        #endregion List转换成Json

        #region 对象转换为Json

        /// <summary>
        /// 对象转换为Json
        /// </summary>
        /// <param name="jsonObject">对象</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(object jsonObject)
        {
            var jsonString = "{";
            var propertyInfo = jsonObject.GetType().GetProperties();
            foreach (var t in propertyInfo)
            {
                var objectValue = t.GetGetMethod().Invoke(jsonObject, null);
                string value;
                switch (objectValue)
                {
                    case DateTime _:
                    case Guid _:
                    case TimeSpan _:
                        value = "'" + objectValue + "'";
                        break;

                    case string _:
                        value = "'" + ToJson(objectValue.ToString()) + "'";
                        break;

                    case IEnumerable enumerable:
                        value = ToJson(enumerable);
                        break;

                    default:
                        value = ToJson(objectValue.ToString());
                        break;
                }
                jsonString += "\"" + ToJson(t.Name) + "\":" + value + ",";
            }
            return jsonString + "}";
        }

        #endregion 对象转换为Json

        #region 对象集合转换Json

        /// <summary>
        /// 对象集合转换Json
        /// </summary>
        /// <param name="array">集合对象</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(IEnumerable array)
        {
            var jsonString = array.Cast<object>().Aggregate("[", (current, item) => current + (ToJson(item) + ","));
            return jsonString + "]";
        }

        #endregion 对象集合转换Json

        #region 普通集合转换Json

        /// <summary>
        /// 普通集合转换Json
        /// </summary>
        /// <param name="array">集合对象</param>
        /// <returns>Json字符串</returns>
        public static string ToArrayString(IEnumerable array)
        {
            var jsonString = "[";
            foreach (var item in array)
            {
                jsonString = ToJson(item.ToString()) + ",";
            }
            return jsonString + "]";
        }

        #endregion 普通集合转换Json

        #region DataSet转换为Json

        /// <summary>
        /// DataSet转换为Json
        /// </summary>
        /// <param name="dataSet">DataSet对象</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(DataSet dataSet)
        {
            var jsonString = dataSet.Tables.Cast<DataTable>().Aggregate("{", (current, table) => current + ("\"" + table.TableName + "\":" + ToJson(table) + ","));
            jsonString = jsonString.TrimEnd(',');
            return jsonString + "}";
        }

        #endregion DataSet转换为Json

        #region Datatable转换为Json

        /// <summary>
        /// DataTable转换为Json
        /// </summary>
        /// <param name="dt">DataTable对象</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(DataTable dt)
        {
            var jsonString = new StringBuilder();
            jsonString.Append("[");
            var drc = dt.Rows;
            for (var i = 0; i < drc.Count; i++)
            {
                jsonString.Append("{");
                for (var j = 0; j < dt.Columns.Count; j++)
                {
                    var strKey = dt.Columns[j].ColumnName;
                    var strValue = drc[i][j].ToString();
                    var type = dt.Columns[j].DataType;
                    jsonString.Append("\"" + strKey + "\":");
                    strValue = StringFormat(strValue, type);
                    if (j < dt.Columns.Count - 1)
                    {
                        jsonString.Append(strValue + ",");
                    }
                    else
                    {
                        jsonString.Append(strValue);
                    }
                }
                jsonString.Append("},");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            return jsonString.ToString();
        }

        /// <summary>
        /// DataTable转换为Json
        /// </summary>
        public static string ToJson(DataTable dt, string jsonName)
        {
            var json = new StringBuilder();
            if (string.IsNullOrEmpty(jsonName)) jsonName = dt.TableName;
            json.Append("{\"" + jsonName + "\":[");
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    json.Append("{");
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        var type = dt.Rows[i][j].GetType();
                        json.Append("\"" + dt.Columns[j].ColumnName + "\":" + StringFormat(dt.Rows[i][j].ToString(), type));
                        if (j < dt.Columns.Count - 1)
                        {
                            json.Append(",");
                        }
                    }
                    json.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        json.Append(",");
                    }
                }
            }
            json.Append("]}");
            return json.ToString();
        }

        #endregion Datatable转换为Json

        #region DataReader转换为Json

        /// <summary>
        /// DataReader转换为Json
        /// </summary>
        /// <param name="dataReader">DataReader对象</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(DbDataReader dataReader)
        {
            var jsonString = new StringBuilder();
            jsonString.Append("[");
            while (dataReader.Read())
            {
                jsonString.Append("{");
                for (var i = 0; i < dataReader.FieldCount; i++)
                {
                    var type = dataReader.GetFieldType(i);
                    var strKey = dataReader.GetName(i);
                    var strValue = dataReader[i].ToString();
                    jsonString.Append("\"" + strKey + "\":");
                    strValue = StringFormat(strValue, type);
                    if (i < dataReader.FieldCount - 1)
                    {
                        jsonString.Append(strValue + ",");
                    }
                    else
                    {
                        jsonString.Append(strValue);
                    }
                }
                jsonString.Append("},");
            }
            dataReader.Close();
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            return jsonString.ToString();
        }

        #endregion DataReader转换为Json

        #region Datatable转换为Json 2

        /// <summary>
        /// DataTable转换为Json 2
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> DataTableToDictionary(DataTable dt)
        {
            return (from DataRow dr in dt.Rows select dt.Columns.Cast<DataColumn>().ToDictionary<DataColumn, string, object>(dc => dc.ColumnName, dc => dr[dc].ToString())).ToList();
        }

        #endregion Datatable转换为Json 2

        #region SerializeObject

        /// <summary>
        /// SerializeObject
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeObject(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        #endregion SerializeObject

        #region 解析JSON字符串生成对象实体

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            var serializer = new JsonSerializer();
            var sr = new StringReader(json);
            var o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            var t = o as T;
            return t;
        }

        #endregion 解析JSON字符串生成对象实体

        #region 解析JSON数组生成对象实体集合

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            var serializer = new JsonSerializer();
            var sr = new StringReader(json);
            var o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            var list = o as List<T>;
            return list;
        }

        #endregion 解析JSON数组生成对象实体集合

        #region 反序列化JSON到给定的匿名对象

        /// <summary>
        /// 反序列化JSON到给定的匿名对象.
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="anonymousTypeObject">匿名对象</param>
        /// <returns>匿名对象</returns>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            var t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }

        #endregion 反序列化JSON到给定的匿名对象
    }
}