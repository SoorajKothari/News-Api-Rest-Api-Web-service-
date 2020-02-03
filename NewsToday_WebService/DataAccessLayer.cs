using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using NewsToday_WebService.Models;
using Newtonsoft.Json;


namespace NewsToday_WebService
{
    public class DataAccessLayer
    {

        /*
         * This is our database access layer where whole the code of Sql server database to our WebApi is done
         * every function used or called by controller is set up here.
         */

            //connection string
        private string Connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;



        //To get all news 
        //Basically i have fetch all news from our tables of general/enteratinment/sports
        //and add it to our list of objects and then passing list by sorting with respect to time.
        public HttpResponseMessage Fetch_All_News()
        {
            SqlConnection connection = new SqlConnection(Connection);

            try
            {
                //sorted list
                SortedList<DateTime, News_Item> mynews = new SortedList<DateTime, News_Item>();

                using (connection)
                {
                    SqlCommand command = new SqlCommand(
                      "SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.Sports;",
                      connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            mynews.Add(reader.GetDateTime(6), new News_Item(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.GetInt32(7), reader.GetString(8)));
                        }
                    }
                    reader.Close();
                    connection.Close();


                    command = new SqlCommand(
                    "SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.Entertainment;",
                    connection);
                    connection.Open();
                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!mynews.ContainsKey(reader.GetDateTime(6)))
                            {
                                mynews.Add(reader.GetDateTime(6), new News_Item(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.GetInt32(7), reader.GetString(8)));

                            }
                        }
                    }

                    reader.Close();
                    connection.Close();

                    command = new SqlCommand(
                  "SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.General;",
                  connection);
                    connection.Open();
                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!mynews.ContainsKey(reader.GetDateTime(6)))
                            {

                                mynews.Add(reader.GetDateTime(6), new News_Item(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.GetInt32(7), reader.GetString(8)));
                            }
                        }
                    }

                    reader.Close();
                    connection.Close();


                }
                var responce = new HttpResponseMessage(HttpStatusCode.OK);
                responce.Content = new StringContent(JsonConvert.SerializeObject(mynews.Values.Reverse().ToList()));
                responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return responce;
            }
            catch (Exception ex)
            {
                var responce = new HttpResponseMessage(HttpStatusCode.OK);
                responce.Content = new StringContent(JsonConvert.SerializeObject(ex.Message));
                responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return responce;

            }

        }


        //increment count of popular news in newsID class to get popular news
        public void Increment_Popular_News(int id)
        {
            SqlConnection connection = new SqlConnection(Connection);
            SqlCommand sqlCommand;
            sqlCommand = new SqlCommand("UPDATE News SET News_Count = News_Count + 1 WHERE NewsID = @i", connection);
            sqlCommand.Parameters.Add("@i", id);
            connection.Open();
            int y = sqlCommand.ExecuteNonQuery();
            connection.Close();
        }



        //Function to get trending or popular news
        public HttpResponseMessage Get_trending_news()
        {
            SqlConnection connection = new SqlConnection(Connection);
            SortedList<DateTime, News_Item> popularnews = new SortedList<DateTime, News_Item>();

            try
            {

                using (connection)
                {
                    SqlCommand command = new SqlCommand(
                      "SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.Sports where NiD = (select Top 1 NewsID from dbo.News Order by News_Count Desc)" +
                      "UNION" + " SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.Entertainment where NiD = (select Top 1 NewsID from dbo.News Order by News_Count Desc)" +
                      "UNION" + " SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.General where NiD = (select Top 1 NewsID from dbo.News Order by News_Count Desc);"

                      , connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        popularnews.Add(reader.GetDateTime(6), new News_Item(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.GetInt32(7), reader.GetString(8)));
                    }
                    connection.Close();
                    reader.Close();
                }
                var responce = new HttpResponseMessage(HttpStatusCode.OK);
                responce.Content = new StringContent(JsonConvert.SerializeObject(popularnews.Values.ToList()));
                responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return responce;


            }
            catch (Exception ex)
            {
                var responce = new HttpResponseMessage(HttpStatusCode.OK);
                responce.Content = new StringContent(JsonConvert.SerializeObject(ex.Message + " " + popularnews.Count));
                responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return responce;

            }
        }


        
        //recommendation below
        //first check if user exist else at user imei.
        public void check(string uid)
        {
            SqlConnection connection = new SqlConnection(Connection);
            using (connection)
            {
                SqlCommand command = new SqlCommand(
                  "SELECT Imei from dbo.Users WHERE Imei = @uid", connection);
                command.Parameters.Add("@uid", uid);
                connection.Open();
                int y = command.ExecuteNonQuery();
                connection.Close();
                //if y==0 means we need to insert
                if (y > 0)
                {
                    //do nothing
                }
                else
                {
                    command = new SqlCommand("INSERT INTO dbo.Users (Imei) Values (@uid)", connection);
                    command.Parameters.Add("@uid", uid);
                    connection.Open();
                    y = command.ExecuteNonQuery();
                    connection.Close();

                }

            }

        }


        //increment for users recomnedation in users table

        public void increment_user_count(string uid,int nid)
        {
            string category = "";
            SqlConnection connection = new SqlConnection(Connection);
            using (connection)
            {
                SqlCommand command = new SqlCommand("select Category from News where NewsID = @nid", connection);
                command.Parameters.Add("@nid",nid);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    category = reader.GetString(0);
                }
                connection.Close();
                reader.Close();

                if (category=="Sports")
                {
                    command = new SqlCommand("UPDATE dbo.Users SET CountSports = CountSports + 1 WHERE Imei = @uid ", connection);
                    command.Parameters.Add("@uid", uid);
                    connection.Open();
                    int y = command.ExecuteNonQuery();
                    connection.Close();
                }
                else if (category == "Entertainment")
                {
                    command = new SqlCommand("UPDATE dbo.Users SET CountEntertainment = CountEntertainment + 1 WHERE Imei = @uid ", connection);
                    command.Parameters.Add("@uid", uid);
                    connection.Open();
                    int y = command.ExecuteNonQuery();
                    connection.Close();

                }
                else if (category== "General")
                {
                    command = new SqlCommand("UPDATE dbo.Users SET CountGeneral = CountGeneral + 1 WHERE Imei = @uid ", connection);
                    command.Parameters.Add("@uid", uid);
                    connection.Open();
                    int y = command.ExecuteNonQuery();
                    connection.Close();

                }
                
            }
        }


        //get recommended news and display to user.
        public HttpResponseMessage get_recommended_news(string uid)
        {
            SortedList<DateTime, News_Item> recnews = new SortedList<DateTime, News_Item>();
            string category = "";
            SqlConnection connection = new SqlConnection(Connection);
            using (connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand("Select CASE   WHEN CountSports > CountGeneral and CountSports > CountEntertainment then 'Sports'     WHEN CountGeneral > CountEntertainment then 'General'    ELSE 'Entertainment' End from dbo.Users where imei = @uid", connection);
                    command.Parameters.Add("@uid", uid);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        category = reader.GetString(0);
                    }
                    connection.Close();
                    reader.Close();

                    if (category == "Sports")
                    {
                        command = new SqlCommand(
                          "SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.Sports;",
                          connection);
                        connection.Open();
                        reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                recnews.Add(reader.GetDateTime(6), new News_Item(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.GetInt32(7), reader.GetString(8)));
                            }
                        }
                        reader.Close();
                        connection.Close();
                    }
                    else if (category == "Entertainment")
                    {
                        command = new SqlCommand(
                            "SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.Entertainment;",
                            connection);
                        connection.Open();
                        reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                recnews.Add(reader.GetDateTime(6), new News_Item(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.GetInt32(7), reader.GetString(8)));
                            }
                        }
                        reader.Close();
                        connection.Close();


                    }
                    else if (category == "General")
                    {
                        command = new SqlCommand(
                            "SELECT NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL FROM dbo.General;",
                            connection);
                        connection.Open();
                        reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                recnews.Add(reader.GetDateTime(6), new News_Item(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.GetInt32(7), reader.GetString(8)));
                            }
                        }
                        reader.Close();
                        connection.Close();


                    }

                    var responce = new HttpResponseMessage(HttpStatusCode.OK);
                    responce.Content = new StringContent(JsonConvert.SerializeObject(recnews.Values.Reverse().ToList()));
                    responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return responce;


                }
                catch (Exception ex)
                {
                    var responce = new HttpResponseMessage(HttpStatusCode.OK);
                    responce.Content = new StringContent(JsonConvert.SerializeObject(ex.Message));
                    responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return responce;
                }

               

            }

        }


        //count if news recommendation are satisfying
        public HttpResponseMessage recount(string uid)
        {
            int count =0;
            SqlConnection connection = new SqlConnection(Connection);
            using (connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand("Select CASE   WHEN CountSports > CountGeneral and CountSports > CountEntertainment then CountSports    WHEN CountGeneral > CountEntertainment then CountGeneral    ELSE CountEntertainment End from dbo.Users where imei = @uid", connection);
                    command.Parameters.Add("@uid", uid);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        count = reader.GetInt32(0);
                    }
                    connection.Close();
                    reader.Close();


                    if (count>10)
                    {
                        var responce = new HttpResponseMessage(HttpStatusCode.OK);
                        responce.Content = new StringContent(JsonConvert.SerializeObject("True"));
                        responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        return responce;
                    }
                    else
                    {
                        var responce = new HttpResponseMessage(HttpStatusCode.OK);
                        responce.Content = new StringContent(JsonConvert.SerializeObject("False"));
                        responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        return responce;

                    }

                }
                catch (Exception ex)
                {
                    var responce = new HttpResponseMessage(HttpStatusCode.OK);
                    responce.Content = new StringContent(JsonConvert.SerializeObject(ex.Message));
                    responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return responce;
                }
            }
        }


        //binding imei to mac
        public void bind(string uid,string mac)
        {
            string value = "";
            SqlConnection connection = new SqlConnection(Connection);
            using (connection)
            {
                SqlCommand command = new SqlCommand("UPDATE dbo.Users SET Mac = @mac WHERE Imei = @uid", connection);
                command.Parameters.Add("@Mac", mac);
                command.Parameters.Add("@uid", uid);
                connection.Open();
                int y = command.ExecuteNonQuery();
                connection.Close();
                

            }
        }


        //get imei called by desktop app
        public HttpResponseMessage get_imei_from_mac(string mac)
        {
            string imei = "";
            SqlConnection connection = new SqlConnection(Connection);
            using (connection)
            {
                SqlCommand command = new SqlCommand("SELECT Imei from dbo.Users WHERE Mac = @mac", connection);
                command.Parameters.Add("@Mac", mac);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    imei = reader.GetString(0);
                }
                connection.Close();
                reader.Close();

                var responce = new HttpResponseMessage(HttpStatusCode.OK);
                responce.Content = new StringContent(JsonConvert.SerializeObject(imei));
                responce.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return responce;


            }
        }
    }
}