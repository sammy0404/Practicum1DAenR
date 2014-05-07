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

        public void IDFBuilder(SQLiteConnection dbObject)
        {
            Dictionary<string, int> HashTabel = new Dictionary<string, int>();
            string query;
            List<KeyValuePair<string, string>> KVP = getTable("autompg", dbObject);
            foreach (KeyValuePair<string, string> s in KVP)
            {
                if (s.Key != "id")
                {
                    SQLiteCommand c = new SQLiteCommand("ALTER TABLE autompg ADD COLUMN " + s.Key + "IDF REAL", dbObject);
                    c.ExecuteNonQuery();
                    HashTabel.Clear();
                    query = "select " + s.Key + " from autompg";
                    SQLiteCommand commando = new SQLiteCommand(query, dbObject);
                    SQLiteDataReader reader = commando.ExecuteReader();
                    while (reader.Read())
                    {
                        string a = reader[s.Key].ToString();
                        if (HashTabel.ContainsKey(a))
                        {
                            HashTabel[a]++;
                        }
                        else
                        {
                            HashTabel.Add(a, 1);
                        }
                    }
                    foreach (KeyValuePair<string, int> a in HashTabel)
                    {
                        SQLiteCommand d = new SQLiteCommand("UPDATE autompg set "+ s.Key+"IDF = " + a.Value + " WHERE " + s.Key + " = " + a.Key, dbObject);
                        d.ExecuteNonQuery();
                    }
                    Console.WriteLine("sam is lelijk");
                    Console.ReadLine();
                }

            }
            Console.WriteLine("KLAAR");
            Console.ReadLine();

        }


        public static List<KeyValuePair<string, string>> getTable(string tabelnaam, SQLiteConnection con)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA table_info(" + tabelnaam + ");"))
            {
                DataTable table = new DataTable();
                cmd.Connection = con;

                SQLiteDataAdapter adp = null;
                try
                {
                    adp = new SQLiteDataAdapter(cmd);
                    adp.Fill(table);
                    return table.AsEnumerable().Select(r => new KeyValuePair<string, string>(r["name"].ToString(), r["type"].ToString())).ToList();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
