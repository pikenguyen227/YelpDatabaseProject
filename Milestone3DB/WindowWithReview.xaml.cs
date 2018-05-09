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
    /// Interaction logic for WindowWithReview.xaml
    /// </summary>
    public partial class WindowWithReview : Window
    {
        private string selectedBusiness;
        public WindowWithReview()
        {
            InitializeComponent();
        }
        public WindowWithReview(string data)
        {
            InitializeComponent();
            selectedBusiness = data;
            addCollumn();
            addReview();
        }

        public class review
        {
            public string date { get; set; }
            public string username { get; set; }
            public string stars { get; set; }
            public string comment { get; set; }
            public string funny { get; set; }
            public string useful { get; set; }
            public string cool { get; set; }
        }

        private void addCollumn()
        {
            GridViewControl.Columns.RemoveAt(0);
            // reviewView.ItemsSource = ItemsSourceObject;   //your query result 
            GridViewColumn col1 = new GridViewColumn();
            col1.Header = "Date";
            col1.DisplayMemberBinding = new Binding("date");
            GridViewControl.Columns.Add(col1);

            GridViewColumn col2 = new GridViewColumn();
            col2.Header = "Username";
            col2.DisplayMemberBinding = new Binding("username");
            GridViewControl.Columns.Add(col2);

            GridViewColumn col3 = new GridViewColumn();
            col3.Header = "Stars";
            col3.DisplayMemberBinding = new Binding("stars");
            GridViewControl.Columns.Add(col3);

            GridViewColumn col4 = new GridViewColumn();
            col4.Header = "Comment";
            col4.DisplayMemberBinding = new Binding("comment");
            GridViewControl.Columns.Add(col4);

            GridViewColumn col5 = new GridViewColumn();
            col5.Header = "Funny";
            col5.DisplayMemberBinding = new Binding("funny");
            GridViewControl.Columns.Add(col5);

            GridViewColumn col6 = new GridViewColumn();
            col6.Header = "Useful";
            col6.DisplayMemberBinding = new Binding("useful");
            GridViewControl.Columns.Add(col6);

            GridViewColumn col7 = new GridViewColumn();
            col7.Header = "Cool";
            col7.DisplayMemberBinding = new Binding("cool");
            GridViewControl.Columns.Add(col7);

           
        }

        private string connectionStringBuilder()
        {
            return "Host=localhost; Username=postgres; Password=minh1234; Database=Milestone2DB";
        }

        private void addReview()
        {
            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT r.date, u.name, r.stars, r.text,r.funny, r.useful, r.cool  FROM review_entity r,yelp_business_entity b, yelp_user_entity u WHERE r.business_id = b.business_id AND u.user_id = r.user_id AND b.business_id = '" + selectedBusiness + "' ORDER BY r.date DESC";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {                         
                           reviewView.Items.Add(new review() { date = reader.GetDate(0).ToString(), username = reader.GetString(1), stars = reader.GetInt32(2).ToString(), comment = breakText(reader.GetString(3), 105), funny = reader.GetInt32(4).ToString(), useful = reader.GetInt32(5).ToString(), cool = reader.GetInt32(6).ToString() });     
                        }
                    }
                }
                conn.Close();
            }
        }

        private string breakText (string data, int max)
        {
            int c = 0;
            StringBuilder sb = new StringBuilder(data);
            for (int i = 0; i < sb.Length; i++)
            {
                if (c > max && sb[i] == ' ')
                {
                    sb[i] = '\n';
                    c = 0;
                }
                c++;
            }
            return sb.ToString();
        }
    }
}
