using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Globalization;

namespace QueryVerwerker
{
    public abstract class Equality
    {
        public SQLiteConnection con;
        public string attr;

        public List<Reciever> recieveList= new List<Reciever>();
        public double getTreshold()
        {
            double vermenigvuldiging = 1.0;
            foreach (Reciever rec in recieveList)
            {
                vermenigvuldiging *= rec.value();
            }
            return vermenigvuldiging;
        }
        public Dictionary<int,double> getNextOptions(){
            Dictionary<int,double> lijstje = new Dictionary<int,double>();
            foreach (Reciever rec in recieveList)
            {
                KeyValuePair<int, double> kvp = rec.nextValue();
                double value = kvp.Value;
                foreach (Reciever rec2 in recieveList)
                {
                    if (rec2 != rec)
                    {
                        value = rec2.getValueOf(kvp.Key);
                    }
                }
                if(!lijstje.ContainsKey(kvp.Key))                    
                    lijstje.Add(kvp.Key, value);
            }
            return lijstje;
        }
        public double getValueOf(int id)
        {
            double multiply = 1.0;
            foreach (Reciever c in recieveList)
            {
                multiply *= c.getValueOf(id);
            }
            return multiply;
        }
        public double getLocalTreshold()
        {
            double multiply = 1.0;
            foreach (Reciever c in recieveList)
            {
                multiply *= c.value();
            }
            return multiply;
        }
    }
    public class NumEquality : Equality
    {
        public double val;
        public NumEquality(string attribute, string value, SQLiteConnection c)
        {
            con = c;
            attr = attribute;
            val = double.Parse(value);
            double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
            recieveList.Add(new NumIDFReciever(attr, val, con));
            recieveList.Add(new IDFReciever(attr, con));
        }
        
    }
    public class CatEquality : Equality
    {
        public string val;
        public CatEquality(string attribute, string value, SQLiteConnection c)
        {
            con = c;
            attr = attribute;
            val = value;
            recieveList.Add(new IDFReciever(attr, c));
            recieveList.Add(new QFREciever(attribute, c));
            recieveList.Add(new JaccardReciever(attr, val, c));
        }
    }



    public abstract class Reciever
    {
        public int rank = 0;
        public Dictionary<int, double> lijst = new Dictionary<int, double>();
        public double value()
        {
            try
            {
                return lijst.ElementAt(rank).Value;
            }
            catch (Exception e)
            {
                return 0.0001;
            }
        }
        public KeyValuePair<int, double> nextValue()
        {
            try
            {
                KeyValuePair<int, double> a = lijst.ElementAt(rank);
                rank++;
                return a;
            }
            catch (Exception e)
            {
                return new KeyValuePair<int,double>(lijst.ElementAt(0).Key, 0.0);
            }
        }
        public virtual double getValueOf(int i)
        {
            return lijst[i];
        }
    }
    public class IDFReciever : Reciever
    {
        public IDFReciever(string attr, SQLiteConnection c)
        {
            string query = "SELECT id," + attr + "IDF AS attr FROM autompg order by " + attr + "IDF desc";
            SQLiteCommand com = new SQLiteCommand(query, c);
            SQLiteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                double d = reader.GetDouble(1);
                lijst.Add(id, d);
            }
        }
    }
    public class NumIDFReciever : Reciever
    {
        public NumIDFReciever(string attr, double val, SQLiteConnection c)
        {
            string querya = "SELECT id," + attr + " AS attr FROM autompg WHERE " + attr + " < " + val.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " ORDER BY attr desc";
            SQLiteCommand coma = new SQLiteCommand(querya, c);
            List<KeyValuePair<int, double>> la = new List<KeyValuePair<int, double>>();
            SQLiteDataReader readera = coma.ExecuteReader();
            while(readera.Read()){
                int id = readera.GetInt32(0);
                double d = readera.GetDouble(1);
                la.Add(new KeyValuePair<int, double>(id, d));
            }
            string queryb = "SELECT id," + attr + " AS attr FROM autompg WHERE " + attr + " >= " + val.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " ORDER BY attr ASC";
            SQLiteCommand comb = new SQLiteCommand(queryb, c);
            List<KeyValuePair<int,double>> lb = new List<KeyValuePair<int,double>>();
            SQLiteDataReader readerb = comb.ExecuteReader();
            while(readerb.Read()){
                int id = readerb.GetInt32(0);
                double d = readerb.GetDouble(1);
                lb.Add(new KeyValuePair<int, double>(id, d));
            }
            string query2 = "SELECT AVG((autompg." + attr + " - sub.a) * (autompg." + attr + " - sub.a)) as var from autompg, (SELECT AVG(" + attr + ") AS a FROM autompg) AS sub;";

            SQLiteCommand comnd = new SQLiteCommand(query2, c);
            var v = comnd.ExecuteScalar();
            double sigma = Math.Sqrt(double.Parse(v.ToString()));
            double h = 1.06 * sigma + Math.Pow(la.Count + lb.Count, -0.2);
            int placeA = 0;
            int placeB = 0;
            while (placeA < la.Count && placeB < lb.Count)
            {
                int idA = la[placeA].Key;
                double powerA = -0.5 * ((val-la[placeA].Value) / h) * ((val -la[placeA].Value) / h);
                double valA = Math.Pow(Math.E, powerA);

                int idB = lb[placeB].Key;
                double powerB = -0.5 * ((val - lb[placeB].Value) / h) * ((val - lb[placeB].Value) / h);
                double valB = Math.Pow(Math.E, powerB);

                if (valA < valB)
                {
                    lijst.Add(idB, valB);
                    placeB++;
                }
                else
                {
                    lijst.Add(idA, valA);
                    placeA++;
                }
            }
            while (placeA < la.Count)
            {
                int idA = la[placeA].Key;
                double powerA = -0.5 * ((val - la[placeA].Value) / h) * ((val - la[placeA].Value) / h);
                double valA = Math.Pow(Math.E, powerA);
                lijst.Add(idA, valA);
                placeA++;
            }
            while (placeB < lb.Count)
            {
                int idB = lb[placeB].Key;
                double powerB = -0.5 * ((val - lb[placeB].Value) / h) * ((val - lb[placeB].Value) / h);
                double valB = Math.Pow(Math.E, powerB);
                lijst.Add(idB, valB);
                placeB++;
            }

        }
    }
    public class JaccardReciever : Reciever
    {
        public JaccardReciever(string attribute, string value, SQLiteConnection c)
        {
            string query = "SELECT id, jaccard FROM workload" + attribute + " WHERE value1 = " + value + " ORDER BY jaccard desc";
            SQLiteCommand cmd = new SQLiteCommand(query, c);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lijst.Add(reader.GetInt32(0), reader.GetDouble(1));
            }
        }
        public override double getValueOf(int i)
        {
            if (lijst.ContainsKey(i))
                return lijst[i];
            else
                return 0.0001;
        }
    }
    public class QFREciever : Reciever
    {
        public QFREciever(string attribute, SQLiteConnection c)
        {
            string query = "SELECT id," + attribute + "QF AS attr FROM autompg order by " + attribute + "QF desc";
            SQLiteCommand com = new SQLiteCommand(query, c);
            SQLiteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                double d = reader.GetDouble(1);
                lijst.Add(id, d);
            }
        }

    }

}
