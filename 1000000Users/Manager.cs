using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;

namespace _1000000Users{
    public class CategoryManager{
        private readonly string connection = ConnectManager.GetConnectionString();
        public CategoryManager(string str) {
            connection = str;
        }
    }
}
