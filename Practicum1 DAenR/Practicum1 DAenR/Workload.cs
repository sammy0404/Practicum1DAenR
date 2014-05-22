using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Globalization;

namespace Practicum1_DAenR
{

    class WorkloadParser
    {
        private SQLiteConnection con;
        private List<string> tableLayout;
        private string tableName;
        public WorkloadParser(string tblName, List<KeyValuePair<string, bool>> table, SQLiteConnection dbCon)
        {
            tableName = tblName;
            con = dbCon;
            tableLayout = table.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

        }
        public void parseWorkload()
        {
            createWorkloadTables();
            foreach (string colmn in tableLayout)
            {
                makeQFColumn(colmn);
            }
            StreamReader read = new StreamReader("workload.txt");
            read.ReadLine(); read.ReadLine();
            string s = "";
            Dictionary<string, int> dict = new Dictionary<string, int>();
            while ((s = read.ReadLine()) != null)
            {
                QueryParser QP = new QueryParser(s, tableLayout);
                foreach (AttributeParser AP in QP)
                {
                    AP.addToDict(dict);
                }
            }

            Dictionary<string , int> RQFMax = new Dictionary<string, int>();

            Dictionary<string, int> RQF = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> kvp in dict)
            {
                string[] pairs = kvp.Key.Split(';');
                if (pairs[0] == pairs[1])
                {
                    if (!RQFMax.ContainsKey(pairs[2])){
                        RQFMax.Add(pairs[2], kvp.Value);
                    }                   
                    else if(kvp.Value > RQFMax[pairs[2]])
                        RQFMax[pairs[2]] = kvp.Value;

                    RQF.Add(pairs[0] + ";" + pairs[2] , kvp.Value);
                }
            }

            foreach (KeyValuePair<string, int> kvp in RQF)
            {                
                string[] split = kvp.Key.Split(';');
                string value = split[0];
                string attr = split[1];
                int RQFval = kvp.Value + 1;
                double QF = (double)RQFval / ((double)RQFMax[attr]+1);
                string QFstring = QF.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                string query = "UPDATE autompg SET " + attr + "QF = " + QFstring + " WHERE " + attr + " = '" + value + "'";
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
            }
            foreach(string column in tableLayout){
                int RQFMx = 0;
                if(!RQFMax.ContainsKey(column))
                    RQFMx = 1;
                else
                    RQFMx = RQFMax[column]+1;
                string query = "UPDATE autompg SET " + column + "QF = " + (1.0 / (double)RQFMx).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " WHERE " + column + "QF ISNULL";
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
            }
            foreach (string col in tableLayout)
            {
                string myquery = "SELECT "+ col + " FROM autompg GROUP by " + col;
                SQLiteCommand kolom = new SQLiteCommand(myquery, con);
                SQLiteDataReader kolomreader = kolom.ExecuteReader();
                List<string> waardenlijst = new List<string>();
                while (kolomreader.Read())
                {
                    waardenlijst.Add(kolomreader.GetString(0));
                }
                foreach (string waarde in waardenlijst)
                {
                    if (!dict.ContainsKey(waarde + ";" + waarde + ";" + col))
                    {
                        dict.Add(waarde + ";" + waarde + ";" + col, 1);
                    }
                }
            }
            foreach(KeyValuePair<string, int> kvp in dict){
                string[] pairs = kvp.Key.Split(';');
                string value1 = pairs[0];
                string value2 = pairs[1];
                
                string table = pairs[2];
                int intersectAmount = kvp.Value;
                int unionAmount = dict[value1 + ";" + value1 + ";" + table] + dict[value2 + ";" + value2 + ";" + table] - intersectAmount;
                double jaccard = (double)intersectAmount / (double)unionAmount;
                string idQuery = "SELECT id FROM autompg WHERE " + table + " = '" + value2 + "'";
                SQLiteCommand idCommand = new SQLiteCommand(idQuery, con);
                SQLiteDataReader idReader = idCommand.ExecuteReader();
                while (idReader.Read())
                {
                    int id =idReader.GetInt32(0);
                    string query = "INSERT INTO workload" + table + " (value1, value2, id, jaccard) VALUES ('" + value1 + "','" + value2 + "'," + id + "," + jaccard.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + ")";
                    SQLiteCommand cmd = new SQLiteCommand(query, con);
                    cmd.ExecuteNonQuery();
                }
                
                // voeg hier aan de DB toe!
            }

        }
        private void makeQFColumn(string s)
        {
            SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE autompg ADD COLUMN " + s + "QF REAL", con);
            cmd.ExecuteNonQuery();
            string indexQuery = "CREATE INDEX " + s + "QFindex ON autompg (" + s + "QF)";
            cmd = new SQLiteCommand(indexQuery, con);
            cmd.ExecuteNonQuery();
        }
        private void createWorkloadTables()
        {
            foreach (string column in tableLayout)
            {
                string query = "CREATE TABLE workload" + column + " (value1 text, value2 text, id, jaccard real); ";
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
                query = "CREATE INDEX " + column + "values ON workload" + column + " (value1 , jaccard)";
                cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
            }
        }
    }

    class QueryParser : List<AttributeParser>
    {
        public int times;
        public QueryParser(string query, List<string> table)
        {
            string[] splitted = query.Split(new string[] { " times: " }, StringSplitOptions.None);
            times = int.Parse(splitted[0]);
            query = splitted[1];
            attributeValueSplit(query, table);
        }

        private void attributeValueSplit(string query, List<string> table)
        {
            string[] splitted = query.Split(new string[] { " WHERE " }, StringSplitOptions.None);
            query = splitted[1];
            splitted = query.Split(new string[] { " AND " }, StringSplitOptions.None);
            foreach (string part in splitted)
            {
                string[] splittedpart = part.Split(new string[] { " = " }, StringSplitOptions.None);
                if (splittedpart.Length == 1)
                {
                    splittedpart = part.Split(new string[] { " IN " }, StringSplitOptions.None);
                    if (table.Contains(splittedpart[0]))
                        this.Add(new MultipleAttributeParser(splittedpart, this));
                }
                else
                {
                    if (table.Contains(splittedpart[0]))
                        this.Add(new SingleAttributeParser(splittedpart, this));
                }
            }
        }
    }


    class AttributeParser : List<string>
    {
        public string attribute;
        public QueryParser queryParser;
        public void addToDict(Dictionary<string, int> dict)
        {
            foreach (string s1 in this)
            {
                string ownInfo = s1 + ";" + s1 + ";" + attribute; ;
                if (!dict.ContainsKey(ownInfo))
                    dict.Add(ownInfo, queryParser.times);
                else
                {
                    dict[ownInfo] += queryParser.times;
                }
                foreach (string s2 in this)
                {
                    if (s1 != s2)
                    {
                        string dictInfo = s1 + ";" + s2 + ";" + attribute;
                        if (!dict.ContainsKey(dictInfo))
                            dict.Add(dictInfo, queryParser.times);
                        else
                        {
                            dict[dictInfo] += queryParser.times;
                        }
                    }
                }
            }
        }
    }
    class SingleAttributeParser : AttributeParser
    {
        public SingleAttributeParser(string[] part, QueryParser QP)
        {
            attribute = part[0];
            analyzePart2(part[1]);
            queryParser = QP;
        }
        private void analyzePart2(string part)
        {
            this.Add(part.Substring(1, part.Length - 2));
        }
    }
    class MultipleAttributeParser : AttributeParser
    {
        public MultipleAttributeParser(string[] part, QueryParser QP)
        {
            attribute = part[0];
            analyzePart2(part[1]);
            queryParser = QP;
        }
        private void analyzePart2(string part)
        {
            part = part.Substring(1, part.Length - 2);
            string[] splittedPart = part.Split(',');
            foreach (string s in splittedPart)
            {
                this.Add(s.Substring(1, s.Length - 2));
            }
        }
    }
}


