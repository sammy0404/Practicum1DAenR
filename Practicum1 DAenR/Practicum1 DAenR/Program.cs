using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Practicum1_DAenR
{
    class Program
    {
        static SQLiteConnection dbtestobject;
        static void Main(string[] args)
        {
            testdb();
        }

        static void testdb()
        {
            SQLiteConnection.CreateFile("test.sqlite");
            dbtestobject = new SQLiteConnection("Data Source=test.sqlite; Version=3;");
            dbtestobject.Open();
            string invoer1 = ("CREATE TABLE aantalaardigheidspunten (name VARCHAR(20), score INT)");
            SQLiteCommand opdracht = new SQLiteCommand(invoer1, dbtestobject);
            opdracht.ExecuteNonQuery();
            string invoer2 = "insert into aantalaardigheidspunten (name, score) values ('Gerben', 12)";
            SQLiteCommand command = new SQLiteCommand(invoer2, dbtestobject);
            command.ExecuteNonQuery();
            invoer2 = "insert into aantalaardigheidspunten (name, score) values ('Gurbo', 3)";
            command = new SQLiteCommand(invoer2, dbtestobject);
            command.ExecuteNonQuery();
            invoer2 = "insert into aantalaardigheidspunten (name, score) values ('Sammie', 345673)";
            command = new SQLiteCommand(invoer2, dbtestobject);
            command.ExecuteNonQuery();
            string tijdelijkeuitvoer = "select * from aantalaardigheidspunten order by score desc";
            SQLiteCommand commando = new SQLiteCommand(tijdelijkeuitvoer, dbtestobject);
            SQLiteDataReader reader = commando.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);


            dbtestobject.Close();
            Console.ReadLine();

        }
    }
}
