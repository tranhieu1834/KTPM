using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nhom7
{
    public static class DATABASECONNECT
    {
        public static  string getConnectionString()
        {
            string sqlCon = @"Data Source=DESKTOP-5TBTUUI;Initial Catalog=demosach3;Integrated Security=True";
            return sqlCon;
        }
    }
}