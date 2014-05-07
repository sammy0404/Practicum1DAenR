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
    class Program
    {
        static SQLiteConnection dbObject;
        static void Main(string[] args)
        {
            string tableName = "autompg"; 
            readDB();
            //writeDB();
            List<KeyValuePair<string, string>> tableLayout = getTable(tableName, dbObject);
            WorkloadParser p = new WorkloadParser(tableName, tableLayout, dbObject);
            p.parseWorkload();
            IDF builder = new IDF(tableName, tableLayout, dbObject);
            builder.IDFBuilder();
        }

        static void readDB()
        {
            SQLiteConnection.CreateFile("cars.sqlite");
            dbObject = new SQLiteConnection("Data Source=cars.sqlite; Version=3;");
            dbObject.Open();
            StreamReader reader = new StreamReader("autompg.sql.txt");
            string line;
            SQLiteCommand command;
            while ((line = reader.ReadLine()) != null)
            {
                command = new SQLiteCommand(line, dbObject);
                command.ExecuteNonQuery();
            }
        }
        static void writeDB()
        {
            string tijdelijkeuitvoer = "select * from autompg order by model_year desc";
            SQLiteCommand commando = new SQLiteCommand(tijdelijkeuitvoer, dbObject);
            SQLiteDataReader reader = commando.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("modeljaar: " + reader["model_year"] + "\tModel: " + reader["model"]);
            Console.ReadLine();
        }

        private static List<KeyValuePair<string, string>> getTable(string tabelnaam, SQLiteConnection con)
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
