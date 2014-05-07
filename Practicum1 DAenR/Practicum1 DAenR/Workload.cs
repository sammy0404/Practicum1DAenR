using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.IO;

namespace Practicum1_DAenR
{

    class WorkloadParser
    {
        private SQLiteConnection con;
        private List<KeyValuePair<string, string>> tableLayout;
        private string tableName;
        public WorkloadParser(string tblName, List<KeyValuePair<string, string>> table, SQLiteConnection dbCon)
        {
            tableName = tblName;
            con = dbCon;
            tableLayout = table;
        }

        public void parseWorkload()
        {
            createWorkloadTable();
            StreamReader read = new StreamReader("workload.txt");
            read.ReadLine(); read.ReadLine();
            string s = "";
            while ((s = read.ReadLine()) != null)
            {
                WorkloadEntry w = parseEntry(s, tableName);
                SQLiteCommand cmd = new SQLiteCommand(w.getInsertQuery(), con);
                cmd.ExecuteNonQuery();
            }
        }
        private void createWorkloadTable()
        {
            string command = "CREATE TABLE " + tableName + "Workload (";
            command += "aantal integer";
            int i = 1;
            while (i < tableLayout.Count)
            {
                command += ", " + tableLayout[i].Key + " text ";
                i++;
            }
            command += ");";
            SQLiteCommand cmd = new SQLiteCommand(command, con);
            cmd.ExecuteNonQuery();
        }
        private static WorkloadEntry parseEntry(string query, string tabel)
        {
            string[] splitted = query.Split(new string[] { " times: " }, StringSplitOptions.None);
            WorkloadEntry entry = new WorkloadEntry(int.Parse(splitted[0]), tabel);
            query = splitted[1];
            splitted = query.Split(new string[] { " WHERE " }, StringSplitOptions.None);
            query = splitted[1];
            splitted = query.Split(new string[] { " AND " }, StringSplitOptions.None);
            foreach (string part in splitted)
            {
                string[] splittedpart = part.Split(new string[] { " IN ", " = " }, StringSplitOptions.None);
                string kolom = splittedpart[0];
                string info = splittedpart[1];
                info = info.Substring(1, info.Length - 2);
                info = info.Replace("'", string.Empty);
                entry.columns.Add(new KeyValuePair<string, string>(kolom, info));
            }
            return entry;
        }

    }
    class WorkloadEntry
    {
        int times;
        string tabel;
        public List<KeyValuePair<string, string>> columns = new List<KeyValuePair<string, string>>();
        public WorkloadEntry(int malen, string tbl)
        {
            tabel = tbl;
            times = malen;
        }
        public string getInsertQuery()
        {
            string query = "INSERT INTO " + tabel + "Workload (aantal";
            foreach (KeyValuePair<string, string> kvp in columns)
            {
                query += ", " + kvp.Key;
            }
            query += ") VALUES (" + times;
            foreach (KeyValuePair<string, string> kvp in columns)
            {
                query += ", '" + kvp.Value + "'";
            }
            query += ")";
            return query;
        }
    }
}
