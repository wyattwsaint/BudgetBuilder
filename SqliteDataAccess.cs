using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace BudgetBuilder
{
    public class SqliteDataAccess
    {
        public static List<(string Description, string Category)> Load()
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<(string Description, string Category)>("select Description, Category from Tags", new DynamicParameters());
                return output.ToList();
            }
        }
        public static void Save(string Tag, string Category)
        {
            var savedTags = Load();

            if (savedTags.Any(st => st.Description == Tag))
            {
                using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Execute("update Tags set Category = @Category where Description = @Tag", new { Category, Tag });
                }
            }
            else
            {
                using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Execute("insert into Tags (Description, Category) values (@Tag, @Category)", new { Tag, Category });
                }
            }
        }
        private static string LoadConnectionString(string id ="Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }

}
