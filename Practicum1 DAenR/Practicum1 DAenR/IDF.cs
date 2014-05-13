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
        private double calcIDFNum(double v, Dictionary<string, int> values, double sigma)
        {
            double n = values.Sum(x => x.Value);
            double h = 1.06 * sigma * Math.Pow(n, 1.0 / 5.0);

            double sum = 0;
            foreach (KeyValuePair<string, int> kvp in values)
            {
                double vi = double.Parse(kvp.Key);
                sum += Math.Exp(-0.5 * Math.Pow((vi - v) / h, 2)) * (double)kvp.Value;
            }
            return Math.Log10(n / sum);
        }
        private double calcIDFCat(double n, double freq)
        {
            return Math.Log10(n / freq);
        }
        private void makeIDFColumn(KeyValuePair<string, bool> s)
        {
            SQLiteCommand c = new SQLiteCommand("ALTER TABLE autompg ADD COLUMN " + s.Key + "IDF REAL", dbObject);
            c.ExecuteNonQuery();
        }

        private double getStandardDeviation(Dictionary<string, int> doubleList, int n)
        {
            double sum = 0;
            foreach (KeyValuePair<string, int> kvp in doubleList)
            {
                sum += (double)kvp.Value * double.Parse(kvp.Key);
            }
            double average = sum / (double)n;
            double sumOfDerivation = 0;
            foreach (KeyValuePair<string, int> value in doubleList)
            {
                sumOfDerivation += (double.Parse(value.Key)) * (double.Parse(value.Key)) * (double)value.Value;
            }
            double sumOfDerivationAverage = sumOfDerivation / (n - 1);
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
        } 

        private void calculateIDF(KeyValuePair<string, bool> s)
        {
            string query = "select " + s.Key + " from autompg";
            SQLiteCommand commando = new SQLiteCommand(query, dbObject);
            SQLiteDataReader reader = commando.ExecuteReader();
            int sum = 0;
            Dictionary<string, int> HashTabel = new Dictionary<string, int>();
            while (reader.Read())
            {
                sum++;
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

            if (s.Value == true)
            {
                //categorisch
                foreach (KeyValuePair<string, int> kvp in HashTabel)
                {
                    double d = calcIDFCat(sum, kvp.Value);
                    SQLiteCommand command = new SQLiteCommand("UPDATE autompg set " + s.Key + "IDF = '" + d + "' WHERE " + s.Key + " = '" + kvp.Key + "'", dbObject);
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                // numeriek
                double sigma = getStandardDeviation(HashTabel, sum);
                foreach (KeyValuePair<string, int> kvp in HashTabel)
                {
                    double d = calcIDFNum(double.Parse(kvp.Key), HashTabel, sigma);
                    SQLiteCommand command = new SQLiteCommand("UPDATE autompg set " + s.Key + "IDF = '" + d + "' WHERE " + s.Key + " = '" + kvp.Key + "'", dbObject);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
