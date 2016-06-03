using System;
using System.Data;

namespace AlfaDirectAutomation
{
    public class AlfaDirectBase
    { 
 

    public class CallResult
        {
            public string Message { get; set; }
            public Exception Exception { get; set; }
            public bool Success { get; set; }
            public object Data { get; set; }
        }
          
          
        public static string GetTableFirstFieldValue(DataTable table, string columnName)
        {
            if (table.Rows.Count != 0)
            {
                return table.Rows[0][columnName].ToString();
            }

            return null;
        }

        /// <summary>
        /// Преобразование полученной таблицы от Альфа-Директа в формат DataTable
        /// </summary>
        /// <param name="data">данные</param>
        /// <param name="fields">список полей через запятую</param>
        /// <returns></returns>
        public static DataTable ConvertToTable(string data, string fields, string types)
        {
            DataTable table = new DataTable();
            string type, line, field;
            int no = 1;
            Type dbType;
            while (types != "")
            {
                type = CutPart(ref types, "|");
                field = CutPart(ref fields, ",").Trim();
                if (type == "int") dbType = typeof(int);
                else if (type == "double") dbType = typeof(double);
                else if (type == "string") dbType = typeof(string);
                else if (type == "float") dbType = typeof(float);
                else if (type == "datetime") dbType = typeof(DateTime);
                else continue;
                table.Columns.Add(field, dbType);
            }

            object[] row;
            while (data != "")
            {
                line = CutPart(ref data, "\r\n");
                if (line == "") continue;
                row = new object[table.Columns.Count];
                no = 0;
                while (line != "")
                {
                    field = CutPart(ref line, "|");
                    row[no++] = field;
                }
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Преобразование полученной таблицы от Альфа-Директа в формат DataTable
        /// </summary>
        /// <param name="data">данные</param>
        /// <param name="fields">список полей через запятую</param>
        /// <returns></returns>
        public static DataTable ConvertToTable(string data, string fields)
        {
            DataTable table = new DataTable();
            string line, field;
            int no = 1;
            while (fields != "")
            {
                field = CutPart(ref fields, ",").Trim();
                table.Columns.Add(field, typeof(string));
            }

            object[] row;
            while (data != "")
            {
                line = CutPart(ref data, "\r\n");
                if (line == "") continue;
                row = new object[table.Columns.Count];
                no = 0;
                while (line != "")
                {
                    field = CutPart(ref line, "|");
                    row[no++] = field;
                }
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Отрезание от строки части до разделителя
        /// </summary>
        /// <param name="value">строка</param>
        /// <param name="token">разделитель</param>
        /// <returns></returns>
        public static string CutPart(ref string value, string token)
        {
            string result;
            int index = value.IndexOf(token);
            if (index < 0)
            {
                result = value;
                value = "";
            }
            else
            {
                result = value.Substring(0, index);
                value = value.Substring(index + token.Length);
            }
            return result;
        }
    }
}
