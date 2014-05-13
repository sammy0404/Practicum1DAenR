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

            foreach(KeyValuePair<string, int> kvp in dict){
                string[] pairs = kvp.Key.Split(';');
                string value1 = pairs[0];
                string value2 = pairs[1];
                
                string table = pairs[2];
                int intersectAmount = kvp.Value;
                int unionAmount = dict[value1 + ";" + value1 + ";" + table] + dict[value2 + ";" + value2 + ";" + table] - intersectAmount;
                double jaccard = (double)intersectAmount / (double)unionAmount;

                string query = "INSERT INTO workload" + table + " (value1, value2, jaccard) VALUES ('" + value1 + "','" + value2 + "'," + jaccard.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + ")";
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
                // voeg hier aan de DB toe!
            }

        }
        private void createWorkloadTables()
        {
            foreach (string column in tableLayout)
            {
                string query = "CREATE TABLE workload" + column + " (value1 real, value2, jaccard real); ";
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
                query = "CREATE INDEX " + column + "values ON workload" + column + " (value1 , value2)";
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



    //    public void parseWorkloads()
    //    {
    //        createWorkloadTables();
    //        StreamReader read = new StreamReader("workload.txt");
    //        read.ReadLine(); read.ReadLine();
    //        Dictionary<string, Dictionary<string, Dictionary<string, int>>> dict = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
    //        foreach (string column in tableLayout)
    //        {
    //            dict.Add(column, new Dictionary<string, Dictionary<string, int>>());
    //        }
    //        string s = "";
    //        List<QueryParser> lijst = new List<QueryParser>();
    //        while ((s = read.ReadLine()) != null)
    //        {
    //            lijst.Add(new QueryParser(s, tableLayout));
    //        }
    //        foreach (QueryParser querP in lijst)
    //        {
    //            foreach (AttributeParser attrP in querP)
    //            {
    //                Dictionary<string, Dictionary<string, int>> attrTabel = dict[attrP.attribute];
    //                foreach (string value in attrP)
    //                {
    //                    if (!attrTabel.ContainsKey(value))
    //                    {
    //                        attrTabel.Add(value, new Dictionary<string, int>());
    //                        SQLiteCommand columnQuery = new SQLiteCommand("ALTER TABLE workload" + attrP.attribute + " ADD COLUMN " + value + " REAL", con);
    //                        columnQuery.ExecuteNonQuery();
    //                    }
    //                }
    //            }

    //            foreach (AttributeParser attrP in querP)
    //            {
    //                Dictionary<string, Dictionary<string, int>> attrTabel = dict[attrP.attribute];
    //                foreach (string value in attrP)
    //                {
    //                    attrTabel[value][value] += querP.times;
    //                    foreach (string value2 in attrP)
    //                    {
    //                        if (value != value2)
    //                            attrTabel[value][value2] += querP.times;
    //                    }
    //                }
    //            }

    //        }
    //        int i = 0;
    //        i++;
    //    }



    //    private void createWorkloadTable()
    //    {
    //        string command = "CREATE TABLE " + tableName + "Workload (";
    //        command += "value1 string, value2 string, aantal integer ";
    //        int i = 1;
    //        while (i < tableLayout.Count)
    //        {
    //            //command += ", " + tableLayout[i].Key + " text ";
    //            i++;
    //        }
    //        command += ");";
    //        SQLiteCommand cmd = new SQLiteCommand(command, con);
    //        cmd.ExecuteNonQuery();
    //    }
    //    private static WorkloadEntry parseEntry(string query, string tabel)
    //    {
    //        string[] splitted = query.Split(new string[] { " times: " }, StringSplitOptions.None);
    //        WorkloadEntry entry = new WorkloadEntry(int.Parse(splitted[0]), tabel);
    //        query = splitted[1];
    //        splitted = query.Split(new string[] { " WHERE " }, StringSplitOptions.None);
    //        query = splitted[1];
    //        splitted = query.Split(new string[] { " AND " }, StringSplitOptions.None);
    //        foreach (string part in splitted)
    //        {
    //            string[] splittedpart = part.Split(new string[] { " IN ", " = " }, StringSplitOptions.None);
    //            string kolom = splittedpart[0];
    //            string info = splittedpart[1];
    //            info = info.Substring(1, info.Length - 2);
    //            info = info.Replace("'", string.Empty);
    //            entry.columns.Add(new KeyValuePair<string, string>(kolom, info));
    //        }
    //        return entry;
    //    }

    //}
    //class WorkloadEntry
    //{
    //    int times;
    //    string tabel;
    //    public List<KeyValuePair<string, string>> columns = new List<KeyValuePair<string, string>>();
    //    public WorkloadEntry(int malen, string tbl)
    //    {
    //        tabel = tbl;
    //        times = malen;
    //    }
    //    public string getInsertQuery()
    //    {
    //        string query = "INSERT INTO " + tabel + "Workload (aantal";
    //        foreach (KeyValuePair<string, string> kvp in columns)
    //        {
    //            query += ", " + kvp.Key;
    //        }
    //        query += ") VALUES (" + times;
    //        foreach (KeyValuePair<string, string> kvp in columns)
    //        {
    //            query += ", '" + kvp.Value + "'";
    //        }
    //        query += ")";
    //        return query;
    //    }
    //}

