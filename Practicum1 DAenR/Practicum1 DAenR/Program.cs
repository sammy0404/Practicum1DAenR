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

        static void testdb()
        {
            SQLiteConnection.CreateFile("cars.sqlite");
            dbObject = new SQLiteConnection("Data Source=test.sqlite; Version=3;");
            dbObject.Open();
            string invoer1 = ("CREATE TABLE aantalaardigheidspunten (name VARCHAR(20), score INT)");
            SQLiteCommand opdracht = new SQLiteCommand(invoer1, dbObject);
            opdracht.ExecuteNonQuery();
            string invoer2 = "insert into aantalaardigheidspunten (name, score) values ('Gerben', 12)";
            SQLiteCommand command = new SQLiteCommand(invoer2, dbObject);
            command.ExecuteNonQuery();
            invoer2 = "insert into aantalaardigheidspunten (name, score) values ('Gurbo', 3)";
            command = new SQLiteCommand(invoer2, dbObject);
            command.ExecuteNonQuery();
            invoer2 = "insert into aantalaardigheidspunten (name, score) values ('Sammie', 345673)";
            command = new SQLiteCommand(invoer2, dbObject);
            command.ExecuteNonQuery();
            string tijdelijkeuitvoer = "select * from aantalaardigheidspunten order by score desc";
            SQLiteCommand commando = new SQLiteCommand(tijdelijkeuitvoer, dbObject);
            SQLiteDataReader reader = commando.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);


            dbObject.Close();
            Console.ReadLine();

        }
    }
}
