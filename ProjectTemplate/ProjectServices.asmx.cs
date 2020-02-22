using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace ProjectTemplate
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]

    public class ProjectServices : System.Web.Services.WebService
    {
        ////////////////////////////////////////////////////////////////////////
        ///replace the values of these variables with your database credentials
        ////////////////////////////////////////////////////////////////////////
        private string dbID = "codeagainsthuman";
        private string dbPass = "!!Codeagainsthuman";
        private string dbName = "codeagainsthumanity";
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string getConString()
        {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
        }
        ////////////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////////////
        //don't forget to include this decoration above each method that you want
        //to be exposed as a web service!
        [WebMethod(EnableSession = true)]
        /////////////////////////////////////////////////////////////////////////
        public string TestConnection()
        {
            try
            {
                string testQuery = "select * from test";

                ////////////////////////////////////////////////////////////////////////
                ///here's an example of using the getConString method!
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection con = new MySqlConnection(getConString());
                ////////////////////////////////////////////////////////////////////////

                MySqlCommand cmd = new MySqlCommand(testQuery, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return "Success!";
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }
        [WebMethod(EnableSession = true)]
        public Book[] LoadBooks()
        {
            //check out the return type.  It's an array of Account objects.  You can look at our custom Account class in this solution to see that it's 
            //just a container for public class-level variables.  It's a simple container that asp.net will have no trouble converting into json.  When we return
            //sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
            //Keeps everything simple.

            //LOGIC: get all the active accounts and return them!

            DataTable sqlDt = new DataTable("books");
            string sqlSelect = "select * from Books";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //gonna use this to fill a data table
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //filling the data table
            sqlDa.Fill(sqlDt);

            //loop through each row in the dataset, creating instances
            //of our container class Account.  Fill each acciount with
            //data from the rows, then dump them in a list.
            List<Book> accounts = new List<Book>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                accounts.Add(new Book
                {
                    isbn = sqlDt.Rows[i]["ISBN"].ToString(),
                    title = sqlDt.Rows[i]["Title"].ToString(),
                    author = sqlDt.Rows[i]["Author"].ToString(),
                    quantity = Convert.ToInt32(sqlDt.Rows[i]["Quantity"]),
                    rentalDays = Convert.ToInt32(sqlDt.Rows[i]["DaysOut"])
                });
            }
            //convert the list of accounts to an array and return!
            return accounts.ToArray();
        }

        [WebMethod]
        public void CheckoutBook(string ID, string ISBN, double daysOut)
        {
            string sqlSelect = "insert into userbooks (ID, ISBN, returndate, checkoutdate) " +
                "values(@idValue, @ISBNValue, @returndateValue, @checkoutdateValue);";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(ID));
            sqlCommand.Parameters.AddWithValue("@ISBNValue", HttpUtility.UrlDecode(ISBN));
            sqlCommand.Parameters.AddWithValue("@checkoutdateValue", DateTime.UtcNow.AddDays(0));
            sqlCommand.Parameters.AddWithValue("@returndateValue", DateTime.UtcNow.AddDays(daysOut));

            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }

        [WebMethod(EnableSession = true)]
        public UserBook[] LoadUserBooks(string ID)
        {
            //check out the return type.  It's an array of Account objects.  You can look at our custom Account class in this solution to see that it's 
            //just a container for public class-level variables.  It's a simple container that asp.net will have no trouble converting into json.  When we return
            //sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
            //Keeps everything simple.

            //LOGIC: get all the active accounts and return them!

            DataTable sqlDt = new DataTable("userbooks");
            string sqlSelect = "select ub.ISBN, Title, Author, returndate " +
                "from userbooks as ub join Books as b " +
                "on ub.ISBN = b.ISBN " +
                "where ID = @IDvalue";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@IDValue", HttpUtility.UrlDecode(ID));

            //gonna use this to fill a data table
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //filling the data table
            sqlDa.Fill(sqlDt);

            //loop through each row in the dataset, creating instances
            //of our container class Account.  Fill each acciount with
            //data from the rows, then dump them in a list.
            List<UserBook> accounts = new List<UserBook>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                accounts.Add(new UserBook
                {
                    isbn = sqlDt.Rows[i]["ISBN"].ToString(),
                    title = sqlDt.Rows[i]["Title"].ToString(),
                    author = sqlDt.Rows[i]["Author"].ToString(),
                    returndate = sqlDt.Rows[i]["returndate"].ToString()
                });
            }
            //convert the list of accounts to an array and return!
            return accounts.ToArray();


        }

        [WebMethod]
        public User LoadUser(string ID)
        {

            DataTable sqlDt = new DataTable("user");
            string sqlSelect = "select f_name " +
                "from Users " +
                "where ID = @IDvalue";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@IDValue", HttpUtility.UrlDecode(ID));

            //gonna use this to fill a data table
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //filling the data table
            sqlDa.Fill(sqlDt);
            User activeUser = new User
            {
                id = ID,
                name = sqlDt.Rows[0]["f_name"].ToString(),
                books = LoadUserBooks(ID)
            };
            //convert the list of accounts to an array and return!
            return activeUser;

        }

    }
}
