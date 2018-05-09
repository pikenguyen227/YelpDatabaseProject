using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace Milestone3DB
{
    /// <summary>
    /// Interaction logic for WindowWithChartZip.xaml
    /// </summary>
    public partial class WindowWithChartZip : Window
    {
        private string selectedState;
        private string selectedCity;
        public WindowWithChartZip()
        {
            InitializeComponent();
        }
        public WindowWithChartZip(string state, string city)
        {
            InitializeComponent();
            selectedState = state;
            selectedCity = city;
            addData();
        }

        private string connectionStringBuilder()
        {
            return "Host=localhost; Username=postgres; Password=minh1234; Database=Milestone2DB";
        }

        private void addData()
        {
            List<KeyValuePair<string, int>> data = new List<KeyValuePair<string, int>>();


            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT postal_code, COUNT(business_id) FROM yelp_business_entity WHERE state = '" + selectedState + "' AND city = '" + selectedCity + "' GROUP BY postal_code ORDER BY postal_code ";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new KeyValuePair<string, int>(reader.GetString(0), reader.GetInt32(1)));
                        }
                    }
                }
                conn.Close();
            }

            zipCodeChart.DataContext = data;
        }
        
    }
}
