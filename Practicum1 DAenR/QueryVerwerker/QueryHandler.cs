﻿using System;
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
        //Dictionary<string, double> numeriek = new Dictionary<string,double>();
        //Dictionary<string, string> categorisch = new Dictionary<string, string>();
        Dictionary<string, Equality> equalities = new Dictionary<string, Equality>();
        public Dictionary<string, bool> columns = new Dictionary<string, bool>();
        public QueryHandler(string s)
        {
            con = new SQLiteConnection("Data Source=C:\\Users\\Gerben\\Documents\\GitHub\\Practicum1DAenR\\Practicum1 DAenR\\Practicum1 DAenR\\bin\\Debug\\cars.sqlite; Version=3;");
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
                    this.equalities.Add(kvp.Key, new NumEquality(kvp.Key, kvp.Value, con));
                    //numeriek.Add(kvp.Key, double.Parse(kvp.Value));
                }

                else
                {
                    this.equalities.Add(kvp.Key, new CatEquality(kvp.Key, kvp.Value, con));
                    //categorisch.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
    
}