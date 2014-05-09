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
            List<string> tableLayout = getTable(tableName, dbObject);
            List<KeyValuePair<string,bool>> extendedTableLayout = new List<KeyValuePair<string,bool>>();
            foreach (string column in tableLayout)
            {
                Console.WriteLine("Is '" + column +  "' a categorical value? ");
                Console.WriteLine("Please type y//n");
                ConsoleKeyInfo c = Console.ReadKey();
                Console.WriteLine();
                if( c.KeyChar == 'y') 
                    extendedTableLayout.Add(new KeyValuePair<string,bool>(column, true));
                else
                    extendedTableLayout.Add(new KeyValuePair<string,bool>(column, false));
            }
            WorkloadParser p = new WorkloadParser(tableName, extendedTableLayout, dbObject);
            p.parseWorkload();
            IDF builder = new IDF(tableName, extendedTableLayout, dbObject);
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

        private static List<string> getTable(string tabelnaam, SQLiteConnection con)
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
                    return table.AsEnumerable().Select(r => r["name"].ToString()).ToList();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }    
    }


}
