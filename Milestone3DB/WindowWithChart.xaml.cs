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
    /// Interaction logic for WindowWithChart.xaml
    /// </summary>
    public partial class WindowWithChart : Window
    {
        private string selectedBusiness;
        public WindowWithChart()
        {
            InitializeComponent();
        }
        public WindowWithChart(string data)
        {
            InitializeComponent();
            selectedBusiness = data;
            addData();

        }

        private string connectionStringBuilder()
        {
            return "Host=localhost; Username=postgres; Password=minh1234; Database=Milestone2DB";
        }

        private void addData()
        {
            List<KeyValuePair<string, int>> data = new List<KeyValuePair<string, int>>();
            List<string> date = new List<string>(new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" });
            for(int i = 0; i < date.Count; i ++)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT SUM(morning + afternoon + evening + night) FROM checkin_entity WHERE business_id = '" + selectedBusiness + "' AND date = '" + date[i] + "'  GROUP BY date";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                data.Add(new KeyValuePair<string, int>(date[i], reader.GetInt32(0)));
                            }
                        }
                    }
                    conn.Close();
                }
            }
            checkinChart.DataContext = data;
        }

    }
}
