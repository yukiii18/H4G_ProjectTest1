using System.Data.SqlClient;
using H4G_Project.Models;

namespace H4G_Project.DAL
{
    public class ShippingRateDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        //Constructor
        public ShippingRateDAL()
        {
            //Read ConnectionString from appsettings.json file
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString(
            "NPCSConnectionString");

            //Instantiate a SqlConnection object with the
            //Connection String read.
            conn = new SqlConnection(strConn);
        }

        public List<ShippingRate> GetAllShipRates()
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SQL statement that select all branches
            cmd.CommandText = @"SELECT * FROM ShippingRate";
            //Open a database connection
            conn.Open();
            //Execute SELCT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            List<ShippingRate> shippingRateList = new List<ShippingRate>();
            while (reader.Read())
            {
                shippingRateList.Add(
                new ShippingRate
                {
                    ShippingRateID = reader.GetInt32(0),
                    fromCity = reader.GetString(1),
                    fromCountry = reader.GetString(2),
                    toCity = reader.GetString(3),
                    toCountry = reader.GetString(4),
                    shippingRate = reader.GetDecimal(5),
                    currency = reader.GetString(6),
                    transitTime = reader.GetInt32(7)
                }
                );
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return shippingRateList;
        }

        public List<String> GetCitiesByCountry(String country)
        {
            List<String> shipList = new List<String>();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM ShippingRate WHERE ToCountry = @selectedCountry";
            cmd.Parameters.AddWithValue("@selectedCountry", country);
            conn.Open();

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    shipList.Add(reader.GetString(3));  
                }
            }
            reader.Close();
            conn.Close();
            return shipList;
        }
        public bool Validation(ShippingRate shipVal)
        {
            bool validate = false;
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM ShippingRate 
                        WHERE  FromCountry = @fromCountry 
                        AND FromCity = @fromCity 
                        AND ToCountry = @toCountry 
                        AND ToCity = @toCity";
            cmd.Parameters.AddWithValue("@shippingrateID", shipVal.ShippingRateID);
            cmd.Parameters.AddWithValue("@fromCity", shipVal.fromCity);
            cmd.Parameters.AddWithValue("@fromCountry", shipVal.fromCountry);
            cmd.Parameters.AddWithValue("@toCity", shipVal.toCity);
            cmd.Parameters.AddWithValue("@toCountry", shipVal.toCountry);
            conn.Open();
            //ExecuteScalar is used to retrieve the auto-generated
            //StaffID after executing the INSERT SQL statement
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            //A connection should be closed after operations.
            conn.Close();
            if(count > 0)
            {
                validate = true;
            }
            return validate;
        }
        public int Add(ShippingRate shippingRates)
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify an INSERT SQL statement which will
            //return the auto-generated StaffID after insertion
            cmd.CommandText = @"INSERT INTO ShippingRate (FromCity, FromCountry, ToCity, ToCountry, ShippingRate, Currency, TransitTime, LastUpdatedBy) 
                                OUTPUT INSERTED.ShippingRateID 
                                VALUES(@fromCity, @fromCountry, @toCity, @toCountry, @shippingrate, 
                                @currency, @transittime, @lastupdatedby)";
            //Define the parameters used in SQL statement, value for each parameter
            //is retrieved from respective class's property.
            cmd.Parameters.AddWithValue("@fromCity", shippingRates.fromCity);
            cmd.Parameters.AddWithValue("@fromCountry", shippingRates.fromCountry);
            cmd.Parameters.AddWithValue("@toCity", shippingRates.toCity);
            cmd.Parameters.AddWithValue("@toCountry", shippingRates.toCountry);
            cmd.Parameters.AddWithValue("@shippingrate", shippingRates.shippingRate);
            cmd.Parameters.AddWithValue("@currency", shippingRates.currency);
            cmd.Parameters.AddWithValue("@transittime", shippingRates.transitTime);
            cmd.Parameters.AddWithValue("@lastupdatedby", shippingRates.lastUpdatedBy);
            //A connection to database must be opened before any operations made.
            conn.Open();
            //ExecuteScalar is used to retrieve the auto-generated
            //StaffID after executing the INSERT SQL statement
            shippingRates.ShippingRateID = (int)cmd.ExecuteScalar();
            //A connection should be closed after operations.
            conn.Close();
            //Return id when no error occurs.
            return shippingRates.ShippingRateID;
        }
        public ShippingRate GetDetails(int shippingRateID)
        {
            ShippingRate shippingRate = new ShippingRate();
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement that
            //retrieves all attributes of a staff record.
            cmd.CommandText = @"SELECT * FROM ShippingRate
                          WHERE ShippingRateID = @selectedShippingRateID";
            //Define the parameter used in SQL statement, value for the
            //parameter is retrieved from the method parameter “staffId”.
            cmd.Parameters.AddWithValue("@selectedShippingRateID", shippingRateID);
            //Open a database connection
            conn.Open();
            //Execute SELCT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                //Read the record from database
                while (reader.Read())
                {
                    // Fill staff object with values from the data reader
                    shippingRate.ShippingRateID = reader.GetInt32(0);
                    shippingRate.fromCity = reader.GetString(1);
                    // (char) 0 - ASCII Code 0 - null value
                    shippingRate.fromCountry = reader.GetString(2);
                    shippingRate.toCity = reader.GetString(3);
                    shippingRate.toCountry = reader.GetString(4);
                    shippingRate.shippingRate = reader.GetDecimal(5);
                    shippingRate.currency = reader.GetString(6);
                    shippingRate.transitTime = reader.GetInt32(7);
                    shippingRate.lastUpdatedBy = reader.GetInt32(8);
                }
            }
            //Close data reader
            reader.Close();
            //Close the database connection
            conn.Close();
            return shippingRate;
        }
        public int Update(ShippingRate ship)
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify an UPDATE SQL statement
            cmd.CommandText = @"UPDATE ShippingRate SET ShippingRate=@shippingrate,
                          LastUpdatedBy=@lastupdatedby, TransitTime=@transittime
                          WHERE ShippingRateID = @selectedShippingRateID";
            //Define the parameters used in SQL statement, value for each parameter
            //is retrieved from respective class's property.
            cmd.Parameters.AddWithValue("@shippingrate", ship.shippingRate);
            cmd.Parameters.AddWithValue("@lastupdatedby", ship.lastUpdatedBy);
            cmd.Parameters.AddWithValue("@transittime", ship.transitTime);
            cmd.Parameters.AddWithValue("@selectedShippingRateID", ship.ShippingRateID);
            //Open a database connection
            conn.Open();
            //ExecuteNonQuery is used for UPDATE and DELETE
            int count = cmd.ExecuteNonQuery();
            //Close the database connection
            conn.Close();
            return count;
        }
        public int Delete(int shippingRateId)
        {
            //Instantiate a SqlCommand object, supply it with a DELETE SQL statement
            //to delete a staff record specified by a Staff ID
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM ShippingRate
                                WHERE ShippingRateID = @selectShippingRateID";
            cmd.Parameters.AddWithValue("@selectShippingRateID", shippingRateId);
            //Open a database connection
            conn.Open();
            int rowAffected = 0;
            //Execute the DELETE SQL to remove the staff record
            rowAffected += cmd.ExecuteNonQuery();
            //Close database connection
            conn.Close();
            //Return number of row of staff record updated or deleted
            return rowAffected;
        }
    }
}
