using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace _1000000Users
{
    public static class ConnectManager
    {
        public static string GetConnectionString()
        {
            var json = File.ReadAllText("D:\\Git\\FirstProject\\1000000Users\\1000000Users\\conect.json");
            var jsonDocument = JsonDocument.Parse(json);
            var connectStr = jsonDocument.RootElement.GetProperty("ConnectionString").GetString();
            return connectStr;
        }
    }
}
