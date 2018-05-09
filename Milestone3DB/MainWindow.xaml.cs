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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;
using System.Device.Location;
using System.IO;

namespace Milestone3DB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Friends
        {
            public string name { get; set; }
            public string average_stars { get; set; }
            public string yelping_since { get; set; }
            public string user_id { get; set; }
        }

        public class postByFriends
        {
            public string friendName { get; set; }
            public string businessName { get; set; }
            public string city { get; set; }
            public string stars { get; set; }
            public string date { get; set; }
            public string text { get; set; }
            public string useful { get; set; }
            public string funny { get; set; }
            public string cool { get; set; }

        }

        private double currentUserLattitude = -999999;
        private double currentUserLongitude = -999999;
        private string selectedBusiness = "";
        private string selectedState = "";
        private string selectedCity = "";

        private bool firstSearchIsPerformed = false;
       
        private Dictionary<string, Double> bidWithDistance = new Dictionary<string, Double>();


        private string sqlCmd = "SELECT DISTINCT result.name, result.address, result.city, result.state, result.latitude,result.longitude, result.stars, result.review_count, result.reviewrating, result.numcheckins, result.business_id, result.distance FROM yelp_business_entity result LEFT OUTER JOIN attributes_entity att ON result.business_id = att.business_id LEFT OUTER JOIN (SELECT business_id, string_agg(name, ',' ORDER BY name) AS cs FROM attributes_entity WHERE value = 'True' GROUP BY business_id ORDER BY business_id) attTF ON result.business_id = attTF.business_id LEFT OUTER JOIN(SELECT business_id, string_agg(categories, ',') AS cs FROM categories_entity GROUP BY business_id) as cat ON result.business_id = cat.business_id LEFT OUTER JOIN(SELECT business_id, the_date, TO_TIMESTAMP(fromTime.value,'HH24:MI')::TIME f, TO_TIMESTAMP(toTime.value, 'HH24:MI')::TIME t from hour_entity, regexp_split_to_table(hour_entity.the_time, '-') WITH ORDINALITY fromTime(value, r), regexp_split_to_table(hour_entity.the_time,'-') WITH ORDINALITY toTime(value, r) WHERE fromTime.r = 1 AND toTime.r = 2) as openHour ON result.business_id = openHour.business_id";

        private string state = "result.state LIKE '%'";
        private string city = "result.city LIKE '%'";
        private string postal_code = "result.postal_code LIKE '%'";
        private string category = "cat.cs LIKE '%'";
        private string time =  "openHour.the_date LIKE '%'";
        private string priceRange = "('1','2','3','4')";
        private string price = "att.name = 'RestaurantsPriceRange2' AND att.value IN ";
        private string theCheckedAttributes = "'%'";
        private string trueAttributes = "attTF.cs similar to ";
        private string orderBy = "ORDER BY name";
        

        public MainWindow()
        {
            InitializeComponent();
            addCollumns2userFriendsDataGrid();
            initInputUserSearch();
            addCollumns2latestReviewMadeByFriendsDataGrid();
            addStates();
            addCollumns2BusinessGrid();
            categoriesListBox.SelectionMode = SelectionMode.Multiple;
            performUpdates();
            addTriggers();
            addDayOfWeek();
            addTimes();
            addSortingOptions();
            addDistanceCollumn();
            disableFilterFields();
            addRating();
            ShowCheckinButton.IsEnabled = false;
            ShowNumBUSPerZipbutton.IsEnabled = false;
            ShowReviewButton.IsEnabled = false;
    
        }

        public class Business
        {
            public string name { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string distance { get; set; }
            public string stars { get; set; }
            public string numberOfReview { get; set; }
            public string avg_rating { get; set; }
            public string totalNumberCheckin { get; set; }
        }

        private string connectionStringBuilder()
        {
            return "Host=localhost; Username=postgres; Password=minh1234; Database=Milestone2DB";
        }

        /* Part A */
        private void addCollumns2userFriendsDataGrid()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "Name";
            col1.Binding = new Binding("name");
            col1.Width = 70;
            userFriendsDatagrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = "Avg Stars";
            col2.Binding = new Binding("average_stars");
            col2.Width = 60;
            userFriendsDatagrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = "Yelping Since";
            col3.Binding = new Binding("yelping_since");
            col3.Width = 100;
            userFriendsDatagrid.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = "User_id";
            col4.Binding = new Binding("user_id");
            col4.Width = 100;
            userFriendsDatagrid.Columns.Add(col4);
        }

        private void addCollumns2latestReviewMadeByFriendsDataGrid()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "User name";
            col1.Binding = new Binding("friendName");
            col1.Width = 100;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col1);
     
            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = "Business";
            col2.Binding = new Binding("businessName");
            col2.Width = 100;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = "city";
            col3.Binding = new Binding("city");
            col3.Width = 100;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = "Stars";
            col4.Binding = new Binding("stars");
            col4.Width = 50;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col4);

            DataGridTextColumn col5 = new DataGridTextColumn();
            col5.Header = "Date";
            col5.Binding = new Binding("date");
            col5.Width = 100;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col5);

            DataGridTextColumn col6 = new DataGridTextColumn();
            col6.Header = "text";
            col6.Binding = new Binding("text");
            col6.Width = 300;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col6);

            DataGridTextColumn col7 = new DataGridTextColumn();
            col7.Header = "funny";
            col7.Binding = new Binding("funny");
            col7.Width = 50;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col7);

            DataGridTextColumn col8 = new DataGridTextColumn();
            col8.Header = "useful";
            col8.Binding = new Binding("useful");
            col8.Width = 50;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col8);

            DataGridTextColumn col9 = new DataGridTextColumn();
            col9.Header = "cool";
            col9.Binding = new Binding("cool");
            col9.Width = 50;
            latestReviewMadeByFriendsDataGrid.Columns.Add(col9);
        }

        private void initInputUserSearch()
        {
            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT DISTINCT user_id FROM yelp_user_entity ORDER BY user_id;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uidOfInputedUser.Items.Add(reader.GetString(0));
                        }
                    }
                }
                conn.Close();
            }
        }

        private void inputUserSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            uidOfInputedUser.Items.Clear();
            userFriendsDatagrid.Items.Clear();
            latestReviewMadeByFriendsDataGrid.Items.Clear();

            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    if (inputUserSearch.Text == "")
                    {
                        cmd.CommandText = "SELECT DISTINCT user_id FROM yelp_user_entity;";
                    }
                    else
                    {
                        cmd.CommandText = "SELECT DISTINCT user_id FROM yelp_user_entity WHERE UPPER(yelp_user_entity.name) = \'" + inputUserSearch.Text.ToUpper() + "\';";
                    }
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uidOfInputedUser.Items.Add(reader.GetString(0));
                        }
                    }
                }
                conn.Close();
            }
        }

        private void userUIDSelected(object sender, SelectionChangedEventArgs e)
        {
            populateFieldsAndGrid();
        }

        private void populateFieldsAndGrid()
        {
            if (uidOfInputedUser.SelectedItem != null)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        string value = uidOfInputedUser.SelectedValue.ToString();
                        userFriendsDatagrid.Items.Clear();
                        latestReviewMadeByFriendsDataGrid.Items.Clear();
                        cmd.CommandText = "SELECT name FROM yelp_user_entity WHERE user_id = \'" + value + "\';";

                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            userName.Text = reader.GetString(0);
                        }
                        cmd.CommandText = "SELECT average_stars FROM yelp_user_entity WHERE user_id = \'" + value + "\';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            userStars.Text = reader.GetString(0);
                        }
                        cmd.CommandText = "SELECT fans FROM yelp_user_entity WHERE user_id = \'" + value + "\';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            userFans.Text = reader.GetString(0);
                        }
                        cmd.CommandText = "SELECT yelping_since FROM yelp_user_entity WHERE user_id = \'" + value + "\';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            userYelpingSince.Text = reader.GetDate(0).ToString();
                        }
                        cmd.CommandText = "SELECT funny FROM yelp_user_entity WHERE user_id = \'" + value + "\';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            userFunny.Text = reader.GetString(0);
                        }
                        cmd.CommandText = "SELECT cool FROM yelp_user_entity WHERE user_id = \'" + value + "\';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            userCool.Text = reader.GetString(0);
                        }
                        cmd.CommandText = "SELECT useful FROM yelp_user_entity WHERE user_id = \'" + value + "\';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            userUseful.Text = reader.GetString(0);
                        }

                        cmd.CommandText = "SELECT DISTINCT user_id, name, average_stars, yelping_since FROM yelp_user_entity WHERE user_id IN  (SELECT user_id_two FROM isfriend_relationship WHERE user_id_one = \'" + value + "\');";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                userFriendsDatagrid.Items.Add(new Friends() { name = reader.GetString(1), average_stars = reader.GetString(2), yelping_since = reader.GetDate(3).ToString(), user_id = reader.GetString(0).ToString() });
                            }
                        }

                        cmd.CommandText = @"SELECT u.name as username, uJoin.name as business, uJoin.city, uJoin.review_stars, uJoin.publish_date, uJoin.text, uJoin.useful, uJoin.funny, uJoin.cool FROM yelp_user_entity u 
	                                            INNER JOIN (SELECT * FROM yelp_business_entity b 
    	                                            INNER JOIN (SELECT r.user_id, r.business_id, r.text, r.stars as review_stars, r.date as publish_date, r.useful, r.funny, r.cool from review_entity r 
        	                                            INNER JOIN (SELECT user_id, max(date) as latestPost FROM review_entity 
                        	                                            WHERE user_id 
                        	                                            IN  (SELECT user_id_two FROM isfriend_relationship WHERE user_id_one = '" + value + @"') 
                        	                                            GROUP BY user_id 
                        	                                            ORDER by user_id) maxD
    		                                            on r.user_id = maxD.user_id AND maxD.latestPost = r.date ORDER BY r.user_id) bJoin
    	                                            on b.business_id = bJoin.business_id) uJoin 
                                                on uJoin.user_id = u.user_id;";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                latestReviewMadeByFriendsDataGrid.Items.Add(new postByFriends() { friendName = reader.GetString(0), businessName = reader.GetString(1), city = reader.GetString(2), stars = reader.GetString(3), date = reader.GetDate(4).ToString(), text = reader.GetString(5), useful = reader.GetString(6), funny = reader.GetString(7), cool = reader.GetString(8) });
                            }
                        }



                    }
                    conn.Close();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (uidOfInputedUser.SelectedItem != null && userFriendsDatagrid.SelectedItem != null)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE FROM isfriend_relationship WHERE user_id_one = \'" + uidOfInputedUser.SelectedItem.ToString() + "\' AND user_id_two = \'" + ((Friends)userFriendsDatagrid.SelectedItem).user_id + "\';";
                        cmd.ExecuteReader();
                    }
                    conn.Close();
                }

                populateFieldsAndGrid();
            }
        }

        /* Part B */
        public void addCollumns2BusinessGrid()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "Business name";
            col1.Binding = new Binding("name");
            col1.Width = 100;
            businessesGrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = "Address";
            col2.Binding = new Binding("address");
            col2.Width = 120;
            businessesGrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = "City";
            col3.Binding = new Binding("city");
            col3.Width = 70;
            businessesGrid.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = "State";
            col4.Binding = new Binding("state");
            col4.Width = 70;
            businessesGrid.Columns.Add(col4);

            DataGridTextColumn col5 = new DataGridTextColumn();
            col5.Header = "Distances";
            col5.Binding = new Binding("distance");
            col5.Width = 70;
            businessesGrid.Columns.Add(col5);

            DataGridTextColumn col6 = new DataGridTextColumn();
            col6.Header = "Stars";
            col6.Binding = new Binding("stars");
            col6.Width = 70;
            businessesGrid.Columns.Add(col6);

            DataGridTextColumn col7 = new DataGridTextColumn();
            col7.Header = "# of Reviews";
            col7.Binding = new Binding("numberOfReview");
            col7.Width = 70;
            businessesGrid.Columns.Add(col7);

            DataGridTextColumn col8 = new DataGridTextColumn();
            col8.Header = "Rating";
            col8.Binding = new Binding("avg_rating");
            col8.Width = 70;
            businessesGrid.Columns.Add(col8);

            DataGridTextColumn col9 = new DataGridTextColumn();
            col9.Header = "# Checkins";
            col9.Binding = new Binding("totalNumberCheckin");
            col9.Width = 70;
            businessesGrid.Columns.Add(col9);
        }

        private void SetLocationIsClicked(object sender, RoutedEventArgs e)
        {
            if (Lattitude.Text != "" && Longitude.Text != "")
            {
                firstSearchIsPerformed = false;
                setLocationError.Visibility = Visibility.Hidden;
                currentUserLattitude = Double.Parse(Lattitude.Text);
                currentUserLongitude = Double.Parse(Longitude.Text);
                businessesGrid.Items.Clear();
                categoriesListBox.Items.Clear();
                zipcodeListBox.Items.Clear();
                cityListBox.Items.Clear();
                stateList.Items.Clear();
                addStates();
                sortingOptions.IsEnabled = false;
                sortingOptions.SelectedIndex = 0;
                disableFilterFields();
                uncheckFilter();
                backToDefault();
                dayOfWeekListbox.SelectedIndex = -1;
                fromTimeListBox.SelectedIndex = -1;
                toTimeListBox.SelectedIndex = -1;
                reviewData.IsEnabled = false;
                ratingListBox.IsEnabled = false;
                selectedCity = "";
                selectedState = "";


            }
            else
            {
                setLocationError.Content = "Error: Missing Field(s)!";
                setLocationError.Visibility = Visibility.Visible;
            }
            
        }

        private void addStates()
        {
            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT DISTINCT state FROM yelp_business_entity ORDER BY state;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stateList.Items.Add(reader.GetString(0));
                        }
                    }
                }
                conn.Close();
            }
        }

        private void addSortingOptions()
        {
            sortingOptions.Items.Add("Business name (default)");
            sortingOptions.Items.Add("Highest Rating");
            sortingOptions.Items.Add("Most reviewed");
            sortingOptions.Items.Add("Best review rating");
            sortingOptions.Items.Add("Most-Checkins");
            sortingOptions.Items.Add("Nearest");
            sortingOptions.SelectedIndex = 0;
            sortingOptions.IsEnabled = false;
        }

        private void addDistanceCollumn()
        {
            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "ALTER TABLE yelp_business_entity DROP COLUMN IF EXISTS distance ";
                    cmd.ExecuteReader();
                }
                conn.Close();
            }
            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "ALTER TABLE yelp_business_entity ADD COLUMN distance FLOAT DEFAULT 0";
                    cmd.ExecuteReader();
                }
                conn.Close();
            }
        }

        private void performUpdates()
        {

            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = File.ReadAllText(@"../../ByteMe_UPDATE.sql").Replace("'", "\'");
                    cmd.ExecuteReader();
                }
                conn.Close();
            }
        }

        private void addTriggers()
        {
            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = File.ReadAllText(@"../../ByteMe_TRIGGER.sql").Replace("'","\'");
                    cmd.ExecuteReader();
                }
                conn.Close();
            }
        }

        private void stateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            cityListBox.Items.Clear();
            zipcodeListBox.Items.Clear();
            categoriesListBox.Items.Clear();
            if (stateList.Items.Count != 0)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT DISTINCT city FROM  yelp_business_entity WHERE state = \'" + stateList.SelectedItem.ToString() + "\' ORDER BY city;";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cityListBox.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                    conn.Close();
                }
            }
        }

        private void cityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cityListBox.Items.Count != 0)
            {
                zipcodeListBox.Items.Clear();
                categoriesListBox.Items.Clear();
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT DISTINCT postal_code FROM  yelp_business_entity WHERE state = \'" + stateList.SelectedItem.ToString() + "\' AND city = \'" + cityListBox.SelectedItem.ToString() + "\' ORDER BY postal_code;";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                zipcodeListBox.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                    conn.Close();
                }
            }
        }

        private void zipcodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (zipcodeListBox.Items.Count != 0)
            {
                categoriesListBox.Items.Clear();
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT DISTINCT categories FROM categories_entity, (SELECT DISTINCT business_id FROM yelp_business_entity WHERE state = \'" + stateList.SelectedItem.ToString() + "\' AND city = \'" + cityListBox.SelectedItem.ToString() + "\' AND postal_code = \'" + zipcodeListBox.SelectedItem.ToString() + "\' ORDER BY business_id) as c WHERE categories_entity.business_id = c.business_id ORDER BY categories;";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categoriesListBox.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                    conn.Close();
                }
            }
        }

      
      

        private void searchBusinessButtonIsClicked(object sender, RoutedEventArgs e)
        {
            selectedState = stateList.SelectedItem.ToString();
            if(cityListBox.SelectedIndex != -1)
            {
                selectedCity = cityListBox.SelectedItem.ToString();
                ShowNumBUSPerZipbutton.IsEnabled = true;
            }

            uncheckFilter();
            businessesGrid.Items.Clear();
            dayOfWeekListbox.Items.Clear();
            addDayOfWeek();
            dayOfWeekListbox.IsEnabled = true;
            sortingOptions.IsEnabled = true;
            firstSearchIsPerformed = true;
            reviewData.IsEnabled = true;
            ratingListBox.IsEnabled = true;
            ShowCheckinButton.IsEnabled = false;
            ShowReviewButton.IsEnabled = false;
            backToDefault();
            if (categoriesListBox.Items.Count != 0 && categoriesListBox.SelectedItems.Count != 0)
            {
                var catList = "%" + string.Join("%", categoriesListBox.SelectedItems.Cast<string>().ToList()) + "%";
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        category = "cat.cs LIKE '" + catList + "'";
                        state = "result.state LIKE '" + stateList.SelectedItem.ToString() + "'";
                        city = "result.city LIKE '" + cityListBox.SelectedItem.ToString() + "'";
                        postal_code = "result.postal_code LIKE '" + zipcodeListBox.SelectedItem.ToString() + "'";
                        cmd.CommandText = sqlCmd + " WHERE " + state + " AND " + city + " AND " + postal_code + " AND " + category  + " AND " + price + " " + priceRange + " AND " + trueAttributes + " " + theCheckedAttributes + " " + orderBy;                 
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (currentUserLattitude != -999999 && currentUserLattitude != -999999)
                                {
                                    Double dist = Math.Round((new GeoCoordinate(currentUserLattitude, currentUserLongitude).GetDistanceTo(new GeoCoordinate(Double.Parse(reader.GetFloat(4).ToString()), Double.Parse(reader.GetFloat(5).ToString()))) / 1609.344), 2);
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = dist.ToString(), stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString(), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                    bidWithDistance.Add(reader.GetString(10), dist);
                                }
                                else
                                {
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = "0", stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString("#.##"), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                }
                            }
                            if (bidWithDistance.Count != 0)
                            {
                                updateDistance();
                            }
                        }
                    }
                    conn.Close();
                }
            }
            else if (zipcodeListBox.Items.Count != 0 && zipcodeListBox.SelectedIndex != -1)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        state = "result.state LIKE '" + stateList.SelectedItem.ToString() + "'";
                        city = "result.city LIKE '" + cityListBox.SelectedItem.ToString() + "'";
                        postal_code = "result.postal_code LIKE '" + zipcodeListBox.SelectedItem.ToString() + "'";
                        cmd.CommandText = sqlCmd + " WHERE " + state + " AND " + city + " AND " + postal_code + " AND " + category + " AND " + price + " " + priceRange + " AND " + trueAttributes + " " + theCheckedAttributes + " " + orderBy;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (currentUserLattitude != -999999 && currentUserLattitude != -999999)
                                {
                                    Double dist = Math.Round((new GeoCoordinate(currentUserLattitude, currentUserLongitude).GetDistanceTo(new GeoCoordinate(Double.Parse(reader.GetFloat(4).ToString()), Double.Parse(reader.GetFloat(5).ToString()))) / 1609.344), 2);
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = dist.ToString(), stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString(), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                    bidWithDistance.Add(reader.GetString(10), dist);
                                }
                                else
                                {
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = "0", stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString("#.##"), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                }
                            }
                            if (bidWithDistance.Count != 0)
                            {
                                updateDistance();
                            }
                        }
                    }
                    conn.Close();
                }
            }
            else if (cityListBox.Items.Count != 0 && cityListBox.SelectedIndex != -1)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        state = "result.state LIKE '" + stateList.SelectedItem.ToString() + "'";
                        city = "result.city LIKE '" + cityListBox.SelectedItem.ToString() + "'";
                        cmd.CommandText = sqlCmd + " WHERE " + state + " AND " + city + " AND " + postal_code + " AND " + category + " AND " + price + " " + priceRange + " AND " + trueAttributes + " " + theCheckedAttributes + " " + orderBy;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (currentUserLattitude != -999999 && currentUserLattitude != -999999)
                                {
                                    Double dist = Math.Round((new GeoCoordinate(currentUserLattitude, currentUserLongitude).GetDistanceTo(new GeoCoordinate(Double.Parse(reader.GetFloat(4).ToString()), Double.Parse(reader.GetFloat(5).ToString()))) / 1609.344), 2);
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = dist.ToString(), stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString(), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                    bidWithDistance.Add(reader.GetString(10), dist);
                                }
                                else
                                {
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = "0", stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString("#.##"), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                }
                            }
                            if (bidWithDistance.Count != 0)
                            {
                                updateDistance();
                            }
                        }
                    }
                    conn.Close();
                }
            }
            else if(stateList.Items.Count != 0 && stateList.SelectedIndex != -1)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        state = "result.state LIKE '" + stateList.SelectedItem.ToString() + "'";
                        cmd.CommandText = sqlCmd + " WHERE " + state + " AND " + city + " AND " + postal_code + " AND " + category  + " AND " + price + " " + priceRange + " AND " + trueAttributes + " " + theCheckedAttributes + " " + orderBy;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (currentUserLattitude != -999999 && currentUserLattitude != -999999)
                                {
                                    Double dist = Math.Round((new GeoCoordinate(currentUserLattitude, currentUserLongitude).GetDistanceTo(new GeoCoordinate(Double.Parse(reader.GetFloat(4).ToString()), Double.Parse(reader.GetFloat(5).ToString()))) / 1609.344), 2);
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = dist.ToString(), stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString(), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                    bidWithDistance.Add(reader.GetString(10),dist);
                                }
                                else
                                {
                                    businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = "0", stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString("#.##"), totalNumberCheckin = reader.GetInt16(9).ToString() });
                                }
                            }
                            if (bidWithDistance.Count != 0)
                            {
                                updateDistance();
                            }
                        }
                    }
                    conn.Close();
                }
            }
            sizeGrid.Content = businessesGrid.Items.Count.ToString();
            enableFilter();
            
        }

        private void updateDistance()
        {
            foreach(KeyValuePair < string, Double > entry in bidWithDistance)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "UPDATE yelp_business_entity SET distance = " + entry.Value + " WHERE yelp_business_entity.business_id = '" + entry.Key + "'";
                        cmd.ExecuteReader();
                      
                    }
                    conn.Close();
                }
            }
        }

        private void addDayOfWeek()
        {
            dayOfWeekListbox.Items.Add("Monday");
            dayOfWeekListbox.Items.Add("Tuesday");
            dayOfWeekListbox.Items.Add("Wednesday");
            dayOfWeekListbox.Items.Add("Thursday");
            dayOfWeekListbox.Items.Add("Friday");
            dayOfWeekListbox.Items.Add("Saturday");
            dayOfWeekListbox.Items.Add("Sunday");
            dayOfWeekListbox.IsEnabled = false;
        }
        private void addTimes()
        {
            fromTimeListBox.IsEnabled = false;
            toTimeListBox.IsEnabled = false;
            for(int c = 0; c < 24; c++)
            {
                fromTimeListBox.Items.Add(c.ToString() + ":00");
            }
        }

        private void dayOfWeekListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dayOfWeekListbox.SelectedIndex != -1)
            {
                fromTimeListBox.IsEnabled = true;
            }
            else
            {
                fromTimeListBox.IsEnabled = false;
                toTimeListBox.IsEnabled = false;
                fromTimeListBox.Items.Clear();
                toTimeListBox.Items.Clear();
                addTimes();
            }

        }

        private void filterTime()
        {
            time = @"openHour.the_date LIKE '" + dayOfWeekListbox.SelectedItem.ToString() + @"'
                                        AND((TO_TIMESTAMP('" + fromTimeListBox.SelectedItem.ToString() + @"', 'HH24:MI')::TIME >= openHour.f
                                        AND TO_TIMESTAMP('" + toTimeListBox.SelectedItem.ToString() + @"','HH24:MI')::TIME <= openHour.t) OR (openHour.f = openHour.t)) 
                                        AND result.is_open = true";
            updateBusinessGrid();
            
            
        }

        private void fromTimeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fromTimeListBox.SelectedIndex != -1)
            {
                toTimeListBox.IsEnabled = true;
                toTimeListBox.Items.Clear();
                for (int c = fromTimeListBox.SelectedIndex + 1; c < 24; c++)
                {
                    toTimeListBox.Items.Add(c.ToString() + ":00");
                }
            }
        }

        private void toTimeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toTimeListBox.SelectedIndex != -1)
            {     
                filterTime();
            }
        }

        private void categoriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chosenCategories.Items.Clear();
            for (int i = 0; i < (categoriesListBox.SelectedItems.Cast<string>().ToList()).Count; i ++)
            {
                chosenCategories.Items.Add(categoriesListBox.SelectedItems.Cast<string>().ToList()[i]);
            }

                
        }

        private void sortingOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                if (sortingOptions.SelectedIndex == 0)
                {
                    orderBy = "ORDER BY name";
                }
                else if (sortingOptions.SelectedIndex == 1)
                {
                    orderBy = "ORDER BY stars DESC";
                }
                else if (sortingOptions.SelectedIndex == 2)
                {
                    orderBy = "ORDER BY review_count DESC";
                }
                else if (sortingOptions.SelectedIndex == 3)
                {
                    orderBy = "ORDER BY reviewrating DESC";
                }
                else if (sortingOptions.SelectedIndex == 4)
                {
                    orderBy = "ORDER BY numcheckins DESC";
                }
                else if (sortingOptions.SelectedIndex == 5)
                {
                    orderBy = "ORDER BY distance";
                }

                updateBusinessGrid();
            }   
        }

        private void updateBusinessGrid()
        {
            businessesGrid.Items.Clear();
            businessesGrid.SelectedIndex = -1;
            using (var conn = new NpgsqlConnection(connectionStringBuilder()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sqlCmd + " WHERE " + state + " AND " + city + " AND " + postal_code + " AND " + category + " AND " + time + " AND " + price + " " + priceRange + " AND " + trueAttributes + " " + theCheckedAttributes + " " + orderBy;


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (currentUserLattitude != -999999 && currentUserLattitude != -999999)
                            {
                                businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = (new GeoCoordinate(currentUserLattitude, currentUserLongitude).GetDistanceTo(new GeoCoordinate(Double.Parse(reader.GetFloat(4).ToString()), Double.Parse(reader.GetFloat(5).ToString()))) / 1609.344).ToString("#.##"), stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString(), totalNumberCheckin = reader.GetInt16(9).ToString() });
                            }
                            else
                            {
                                businessesGrid.Items.Add(new Business() { name = reader.GetString(0), address = reader.GetString(1), city = reader.GetString(2), state = reader.GetString(3), distance = "0", stars = reader.GetFloat(6).ToString(), numberOfReview = reader.GetInt32(7).ToString(), avg_rating = reader.GetDouble(8).ToString("#.##"), totalNumberCheckin = reader.GetInt16(9).ToString() });
                            }
                        }
                    }
                }
                conn.Close();
            }
            sizeGrid.Content = businessesGrid.Items.Count;
        }

        private void setupPriceAttribute()
        {
            string v = "";
            if (supercheap.IsChecked == false && cheap.IsChecked == false && normal.IsChecked == false && expensive.IsChecked == false)
            {
                v = "('1','2','3','4')";
            }
            else
            {
                v = "(";
                if (supercheap.IsChecked == true)
                {
                    v = v + "'1',";
                }
                if (cheap.IsChecked == true)
                {
                    v = v + "'2',";
                }
                if (normal.IsChecked == true)
                {
                    v = v + "'3',";
                }
                if (expensive.IsChecked == true)
                {
                    v = v + "'4',";
                }
                v = v.Remove(v.Length - 1);
                v = v + ")";
            }
            priceRange = v;
        }


        private void setUpOtherAttributes()
        {
            List<string> att = new List<string>();
            if (acceptCC.IsChecked == true)
            {
                att.Add("BusinessAcceptsCreditCards");
            }
            if(takeReservation.IsChecked == true)
            {
                att.Add("RestaurantsReservations");
            }
            if(wheelchair.IsChecked == true)
            {
                att.Add("WheelchairAccessible");
            }
            if(outdoorSeat.IsChecked == true)
            {
                att.Add("OutdoorSeating");
            }
            if(gfk.IsChecked == true)
            {
                att.Add("GoodForKids");
            }
            if(gfg.IsChecked == true)
            {
                att.Add("RestaurantsGoodForGroups");
            }
            if(deliver.IsChecked == true)
            {
                att.Add("RestaurantsDelivery");
            }
            if(takeOut.IsChecked == true)
            {
                att.Add("RestaurantsTakeOut");
            }
            if(wifi.IsChecked == true)
            {
                att.Add("WiFi");
            }
            if(bikeParking.IsChecked == true)
            {
                att.Add("BikeParking");
            }
            if(breakfast.IsChecked == true)
            {
                att.Add("breakfast");
            }
            if (brunch.IsChecked == true)
            {
                att.Add("brunch");
            }
            if (lunch.IsChecked == true)
            {
                att.Add("lunch");
            }
            if (dinner.IsChecked == true)
            {
                att.Add("dinner");
            }
            if (desert.IsChecked == true)
            {
                att.Add("desert");
            }
            if (latenight.IsChecked == true)
            {
                att.Add("latenight");
            }
            att = att.OrderBy(q => q).ToList();
            if (att.Count == 0)
            {
                theCheckedAttributes = "'%'";
            }
            else
            {
                theCheckedAttributes = "'%" + string.Join("%", att) + "%'";
            }
        }

        private void filter()
        {
            businessesGrid.Items.Clear();
            updateBusinessGrid();
        }

        private void supercheap_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void cheap_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void normal_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void expensive_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void supercheap_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void cheap_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void normal_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void expensive_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void acceptCC_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setupPriceAttribute();
                filter();
            }
        }

        private void takeReservation_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void wheelchair_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void outdoorSeat_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void gfk_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void gfg_Checked(object sender, RoutedEventArgs e)
        {
           if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void deliver_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void takeOut_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void wifi_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void bikeParking_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void breakfast_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void brunch_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void lunch_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void dinner_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void desert_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void latenight_Checked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void acceptCC_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void takeReservation_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void wheelchair_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void gfk_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void gfg_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void deliver_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void takeOut_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void wifi_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void bikeParking_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void breakfast_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void brunch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void lunch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void dinner_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void desert_Unchecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void latenight_Unhecked(object sender, RoutedEventArgs e)
        {
            if (firstSearchIsPerformed)
            {
                setUpOtherAttributes();
                filter();
            }
        }

        private void disableFilterFields()
        {
            supercheap.IsEnabled = false;
            cheap.IsEnabled = false;
            normal.IsEnabled = false;
            expensive.IsEnabled = false;
            acceptCC.IsEnabled = false;
            takeReservation.IsEnabled = false;
            wheelchair.IsEnabled = false;
            outdoorSeat.IsEnabled = false;
            gfk.IsEnabled = false;
            gfg.IsEnabled = false;           
            deliver.IsEnabled = false;          
            takeOut.IsEnabled = false;           
            wifi.IsEnabled = false;           
            bikeParking.IsEnabled = false;           
            breakfast.IsEnabled = false;
            brunch.IsEnabled = false;
            lunch.IsEnabled = false;
            dinner.IsEnabled = false;
            desert.IsEnabled = false;
            latenight.IsEnabled = false;

        }

        private void uncheckFilter()
        {
            supercheap.IsChecked = false;
            cheap.IsChecked = false;
            normal.IsChecked = false;
            expensive.IsChecked = false;
            acceptCC.IsChecked = false;
            takeReservation.IsChecked = false;
            wheelchair.IsChecked = false;
            outdoorSeat.IsChecked = false;
            gfk.IsChecked = false;
            gfg.IsChecked = false;
            deliver.IsChecked = false;
            takeOut.IsChecked = false;
            wifi.IsChecked = false;
            bikeParking.IsChecked = false;
            breakfast.IsChecked = false;
            brunch.IsChecked = false;
            lunch.IsChecked = false;
            dinner.IsChecked = false;
            desert.IsChecked = false;
            latenight.IsChecked = false;


        }

        private void enableFilter()
        {
            supercheap.IsEnabled = true;
            cheap.IsEnabled = true;
            normal.IsEnabled = true;
            expensive.IsEnabled = true;
            acceptCC.IsEnabled = true;
            takeReservation.IsEnabled = true;
            wheelchair.IsEnabled = true;
            outdoorSeat.IsEnabled = true;
            gfk.IsEnabled = true;
            gfg.IsEnabled = true;
            deliver.IsEnabled = true;
            takeOut.IsEnabled = true;
            wifi.IsEnabled = true;
            bikeParking.IsEnabled = true;
            breakfast.IsEnabled = true;
            brunch.IsEnabled = true;
            lunch.IsEnabled = true;
            dinner.IsEnabled = true;
            desert.IsEnabled = true;
            latenight.IsEnabled = true;
        }

        private void backToDefault()
        {
            state = "result.state LIKE '%'";
            city = "result.city LIKE '%'";
            postal_code = "result.postal_code LIKE '%'";
            category = "cat.cs LIKE '%'";
            time = "openHour.the_date LIKE '%'";
            priceRange = "('1','2','3','4')";
            price = "att.name = 'RestaurantsPriceRange2' AND att.value IN ";
            theCheckedAttributes = "'%'";
            trueAttributes = "attTF.cs similar to ";
            orderBy = "ORDER BY name";
        }

        private string generalizeTime()
        {
            List<string> morning = new List<string>(new string[] { "6:00", "7:00", "8:00", "9:00", "10:00", "11:00" });
            List<string> afternoon = new List<string>(new string[] { "12:00", "13:00", "14:00", "15:00", "16:00" });
            List<string> evening = new List<string>(new string[] { "17:00", "18:00", "19:00", "20:00", "21:00", "22:00" });
            List<string> night = new List<string>(new string[] { "23:00", "0:00", "1:00", "2:00", "3:00", "4:00", "5:00" });

            if (morning.Contains(fromTimeListBox.SelectedItem.ToString()))
            {
                return "morning";
            }

            else if (afternoon.Contains(fromTimeListBox.SelectedItem.ToString()))
            {
                return "afternoon";
            }
            else if (evening.Contains(fromTimeListBox.SelectedItem.ToString()))
            {
                return "evening";
            }
            else if (night.Contains(fromTimeListBox.SelectedItem.ToString()))
            {
                return "night";
            }
            return "";
        }

        private void checkinButton_Click(object sender, RoutedEventArgs e)
        {
            if(firstSearchIsPerformed && businessesGrid.SelectedItem != null && selectedBusiness != "" && dayOfWeekListbox.SelectedItem != null && fromTimeListBox.SelectedItem != null)
            {
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO newcustomercheckin(business_id, date, time) VALUES ('" + selectedBusiness + "', '" + dayOfWeekListbox.SelectedItem.ToString() +"', '" + generalizeTime() + "');";
                        cmd.ExecuteReader();
                    }
                    conn.Close();
                }
                updateBusinessGrid();
            }
        }

        private void businessesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(businessesGrid.Items.Count != 0 && businessesGrid.SelectedIndex != -1)
            {
                ShowCheckinButton.IsEnabled = true;
                ShowNumBUSPerZipbutton.IsEnabled = true;
                ShowReviewButton.IsEnabled = true;
                selctedBusinessnamedisplay.Text = (businessesGrid.SelectedItem as Business).name;
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn; 
                        cmd.CommandText = "SELECT business_id FROM yelp_business_entity WHERE name = '" + (businessesGrid.SelectedItem as Business).name + "' AND address = '" + (businessesGrid.SelectedItem as Business).address + "' AND city = '" + (businessesGrid.SelectedItem as Business).city + "' AND state = '" + (businessesGrid.SelectedItem as Business).state + "'";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                selectedBusiness = reader.GetString(0);
                            }
                        }
                    }
                    conn.Close();
                }
            }
        }

        private void addReviewIsClicked(object sender, RoutedEventArgs e)
        {
            if (reviewData.Text != "" && selectedBusiness != "" && uidOfInputedUser.SelectedIndex != -1  && ratingListBox.SelectedIndex != -1)
            {
               
                using (var conn = new NpgsqlConnection(connectionStringBuilder()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO review_entity (review_id,business_id,user_id,stars,date,text,useful,funny,cool) VALUES ('" + RandomString(22) + "', '" + selectedBusiness + "', '" + uidOfInputedUser.SelectedItem.ToString() + "', '" + ratingListBox.SelectedItem.ToString() + "', '" + DateTime.Now.ToString("yyyy-M-d") + "', '" + reviewData.Text.ToString() + "', 0,0,0);";
                        cmd.ExecuteReader();
                    }
                    conn.Close();
                }
            }
            updateBusinessGrid();
        }

        private void addRating()
        {
            ratingListBox.IsEnabled = false;
            for(int i = 1; i <=5; i ++)
            {
                ratingListBox.Items.Add(i.ToString());
            }
        }

        private static Random random = new Random();
        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void ShowCheckinButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBusiness != "")
            {
                new WindowWithChart(selectedBusiness).Show();
            }
        }

        private void ShowNumBUSPerZipbutton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCity != "" && selectedState != "")
            {          
                new WindowWithChartZip(selectedState, selectedCity).Show();
            }
        }

        private void ShowReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBusiness != "")
            {
                new WindowWithReview(selectedBusiness).Show();
            }
        }
    }
}
