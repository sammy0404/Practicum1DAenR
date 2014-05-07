using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Practicum1_DAenR
{
    class Program
    {
        static SQLiteConnection dbObject;
        static void Main(string[] args)
        {
            readDB();
            writeDB();
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
            dbObject.Close();
            Console.ReadLine();
        }

        static void parseWorkload(SQLiteConnection con){
            StreamReader read = new StreamReader();

        }
    }

}
