using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;


namespace QueryVerwerker
{
    class QueryHandler
    {
        SQLiteConnection con;
        int k = 10;
        List<Equality> equalities = new List<Equality>();
        public Dictionary<string, bool> columns = new Dictionary<string, bool>();
        public QueryHandler(string s)
        {
             string source = System.Environment.CurrentDirectory;
            string replaced = "Data Source=" + source.Replace("QueryVerwerker\\bin", "Practicum1 DAenR\\bin") + "\\cars.sqlite; Version=3;";
            con = new SQLiteConnection(replaced);
      //      con = new SQLiteConnection("Data Source=C:\\Users\\Gerben\\Documents\\GitHub\\Practicum1DAenR\\Practicum1 DAenR\\Practicum1 DAenR\\bin\\Debug\\cars.sqlite; Version=3;");
            //Data Source=C:\\Users\\Gebruiker\\Documents\\GitHub\\Practicum1DAenR\\Practicum1 DAenR\\Practicum1 DAenR\\bin\\Debug\\cars.sqlite; Version=3;
            con.Open();
            Dictionary<string, string> equalities = new Dictionary<string, string>();
            string[] invoer = s.Split(',');
            foreach (string eq in invoer)
            {
                string eqTrim = eq.Trim();
                string[] eqSplit = eqTrim.Split(new string[] { " = " }, StringSplitOptions.None);
                if (eqSplit[0] == "k")
                    k = int.Parse(eqSplit[1]);
                else
                {
                    equalities.Add(eqSplit[0], eqSplit[1]);
                }
            }
            getColumns();
            Splitternumcat(equalities);
        }
        private void getColumns()
        {
            using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA table_info(autompg);"))
            {
                DataTable table = new DataTable();
                cmd.Connection = con;

                SQLiteDataAdapter adp = null;
                try
                {
                    adp = new SQLiteDataAdapter(cmd);
                    adp.Fill(table);
                    List<string> allColumns =  table.AsEnumerable().Where(r => !r["name"].ToString().EndsWith("IDF") && !r["name"].ToString().EndsWith("QF")).Select(r => r["name"].ToString()).ToList();
                    List<string> catColumns = table.AsEnumerable().Where(r => r["name"].ToString().EndsWith("QF")).Select(r => r["name"].ToString().Substring(0, r["name"].ToString().Length-2)).ToList();
                    foreach (string column in allColumns)
                    {
                        columns.Add(column, false);
                    }
                    foreach (string catcol in catColumns)
                    {
                        columns[catcol] = true;
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }
        private void Splitternumcat(Dictionary<string, string>equalities)
        {
            foreach (KeyValuePair<string, string> kvp in equalities)
            {
                if(!columns[kvp.Key])
                {
                    this.equalities.Add(new NumEquality(kvp.Key, kvp.Value, con));
                    //numeriek.Add(kvp.Key, double.Parse(kvp.Value));
                }

                else
                {
                    this.equalities.Add(new CatEquality(kvp.Key, kvp.Value, con));
                    //categorisch.Add(kvp.Key, kvp.Value);
                }
            }
        }
        public List<int> getTopK()
        {
            double treshold = getGlobalTreshold();
            SortedList<DoubleDing, int> lijst = new SortedList<DoubleDing, int>();
            Dictionary<int, double> dict = new Dictionary<int, double>();
            // duplicaten
            while ((lijst.Count <= k || lijst.ElementAt(lijst.Count-k).Key.d < treshold) && treshold > 0.0)
            {
                Dictionary<int, double> iterationDict = nextIteration();
                foreach (KeyValuePair<int, double> kvp in iterationDict)
                {
                    if (!dict.ContainsKey(kvp.Key) && !lijst.ContainsKey(new DoubleDing(kvp.Value, kvp.Key)))
                    {
                        lijst.Add(new DoubleDing(kvp.Value, kvp.Key), kvp.Key);
                        dict.Add(kvp.Key, kvp.Value);
                    }
                }
                treshold = getGlobalTreshold();
            }
            List<int> ids = new List<int>();
            int p = Math.Min(lijst.Count, k);
            for (int i = 0; i < p; i++)
            {
                KeyValuePair<DoubleDing, int> kvp = lijst.ElementAt(lijst.Count - 1 - i);
                if(kvp.Key.d != 0.0)
                    ids.Add(kvp.Value);
            }
            


            return ids;
        }
        public void appender(StringBuilder b, List<int> ids)
        {
            List<string> columnsList = columns.Keys.ToList();
            string selectpart = columnsList[0];
            for (int j = 1; j < columns.Count; j++)
            {
                selectpart += ", " + columnsList[j];
            }
            foreach (int id in ids)
            {
                string idQuery = " SELECT " + selectpart + " FROM autompg WHERE id = " + id;
                SQLiteCommand cmd = new SQLiteCommand(idQuery, con);
                SQLiteDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    object[] valuesresult = new object[columns.Count];
                    reader.GetValues(valuesresult);
                    int i = 0;
                    foreach (object s in valuesresult)
                    {
                        b.Append(columnsList[i] + ": " + s.ToString() + ", ");
                        i++;
                    }
                    b.AppendLine("");
                }

            }
        }
        private Dictionary<int, double> nextIteration()
        {
            Dictionary<int, double> dict = new Dictionary<int, double>();
            foreach (Equality eq in equalities)
            {
                Dictionary<int, double> p = eq.getNextOptions();
                foreach (KeyValuePair<int, double> value in p)
                {
                    if (!dict.ContainsKey(value.Key))
                    {
                        dict.Add(value.Key, getGlobalValue(value.Key));
                    }
                }
            }
            return dict;
        }
        private double getGlobalValue(int id){
            double sum = 0.0;
            foreach (Equality eq in equalities)
            {
                sum += eq.getValueOf(id);
            }
            return sum;
        }
        private double getGlobalTreshold()
        {
            double sum = 0.0;
            foreach (Equality eq in equalities)
            {
                sum += eq.getLocalTreshold();
            }
            return sum;
        }
    }

    public class DoubleDing : IComparable
    {
        public int id;
        public double d;
        public DoubleDing(double d, int i)
        {
            this.d = d;
            id = i;
        }
        public int CompareTo(Object de)
        {
            DoubleDing dd = (DoubleDing)de;
            if (dd.d == this.d)
            {
                return this.id.CompareTo(dd.id);
            }
            return this.d.CompareTo(dd.d);
        }
    }
}
