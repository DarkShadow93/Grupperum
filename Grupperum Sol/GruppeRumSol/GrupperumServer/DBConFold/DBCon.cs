﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrupperumServer.DBConFold
{

    public class DBCon
    {
        SqlConnection con;
        public void connect()
        {
            string conString = "user id=dmaa0914_3Sem_2_Grupperum;" +
                                       "password=IsAllowed;server=kraka.ucn.dk;" +
                                       "database=dmaa0914_3Sem_2_Grupperum; " +
                                       "connection timeout=30";

            con = new SqlConnection(conString);
            
        }
    public void createGroupRoom()
        {
            connect();
            con.Open();
            SqlCommand sc = new SqlCommand("insert into GroupRoom DEFAULT VALUES;", con);

            sc.ExecuteReader();
            con.Close();
        }
    }
}
