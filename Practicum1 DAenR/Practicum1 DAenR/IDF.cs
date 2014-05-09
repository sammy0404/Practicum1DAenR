using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data;

namespace Practicum1_DAenR
{
    class IDF
    {
        private SQLiteConnection dbObject;
        private List<KeyValuePair<string, bool>> tableLayout;
        private string tableName;
        public IDF(string tblName, List<KeyValuePair<string, bool>> table, SQLiteConnection dbCon)
        {
            tableName = tblName;
            dbObject = dbCon;
            tableLayout = table;
        }
        public void IDFBuilder()
        {            
            foreach (KeyValuePair<string, bool> s in tableLayout)
            {
                if (s.Key != "id")
                {
                    makeIDFColumn(s);
                    calculateIDF(s);
                }
            }
        }

        private void makeIDFColumn(KeyValuePair<string, bool> s)
        {
            SQLiteCommand c = new SQLiteCommand("ALTER TABLE autompg ADD COLUMN " + s.Key + "IDF REAL", dbObject);
            c.ExecuteNonQuery();
        }

        private void calculateIDF (KeyValuePair<string, bool> s )
        {
            int max = 0;
            string maxString = "";
            Dictionary<string, int> HashTabel = new Dictionary<string, int>();
            string query = "select " + s.Key + " from autompg";
            SQLiteCommand commando = new SQLiteCommand(query, dbObject);
            SQLiteDataReader reader = commando.ExecuteReader();
            while (reader.Read())
            {
                string a = reader[s.Key].ToString();
                if (HashTabel.ContainsKey(a))
                {
                    HashTabel[a]++;
                    if (HashTabel[a] > max)
                    {
                        max = HashTabel[a];
                        maxString = a;
                    }
                }
                else
                {
                    HashTabel.Add(a, 1);
                    if (HashTabel[a] > max)
                    {
                        max = HashTabel[a];
                        maxString = a;
                    }
                }
            }
            foreach (KeyValuePair<string, int> a in HashTabel)
            {
                SQLiteCommand d = new SQLiteCommand("UPDATE autompg set " + s.Key + "IDF = '" + a.Value + "' WHERE " + s.Key + " = '" + a.Key + "'", dbObject);
                d.ExecuteNonQuery();
            }
        }
    }
}
