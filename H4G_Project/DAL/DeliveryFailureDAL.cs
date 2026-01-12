using System.Configuration;
using System.Data.SqlClient;
using H4G_Project.Models;


namespace H4G_Project.DAL
{
    public class DeliveryFailureDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        public DeliveryFailureDAL()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString(
            "NPCSConnectionString");
            conn = new SqlConnection(strConn);
        }
        // Add a delivery failure record into the db using a model class
        public int AddRecord(DeliveryFailure failure)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO DeliveryFailure (ParcelID,DeliveryManID,FailureType,Description,DateCreated) OUTPUT INSERTED.ReportID VALUES (@parcelID,@deliveryManID,@type,@description,@dateCreated)";
            cmd.Parameters.AddWithValue("@parcelID", failure.parcelID);
            cmd.Parameters.AddWithValue("@deliveryManID", failure.deliveryManID);
            cmd.Parameters.AddWithValue("@type", failure.failureType);
            cmd.Parameters.AddWithValue("@description", failure.description);
            cmd.Parameters.AddWithValue("@dateCreated", failure.dateCreated.ToString("d MMM yyyy h:mm tt"));
            conn.Open();
            int result = (int) cmd.ExecuteScalar();
            conn.Close();
            return result;   
            
        }
        public int UpdateRecord()
        {
            return 0;
        }
        public bool CheckFollowUp(Parcel parcel)
        {
            return false;
        }
        // get the failure reports based on the staff object id
        public List<DeliveryFailure> GetReports(Staff staff)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM DeliveryFailure WHERE DeliveryManID = @id OR   StationMgrID = @id";
            cmd.Parameters.AddWithValue("@id", staff.staffID.ToString());
            List<DeliveryFailure> failureList = new List<DeliveryFailure>();
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    failureList.Add(new DeliveryFailure
                    {
                        reportID = reader.GetInt32(0),
                        parcelID = reader.GetInt32(1),
                        deliveryManID = reader.GetInt32(2),
                        failureType = reader.GetString(3)[0],
                        description = reader.GetString(4),
                        stationMgrID = !reader.IsDBNull(5) ? reader.GetInt32(5) : (int?) null,
                        followUpAction = !reader.IsDBNull(6) ? reader.GetString(6) : (string?) null,
                        dateCreated = reader.GetDateTime(7),
                    });
                }
                return failureList;
            }
            else
            {
                return null;
            }
        }
        public List<DeliveryFailure> GetAllDeliveryFailure()
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SQL statement that select all branches
            cmd.CommandText = @"SELECT * FROM DeliveryFailure";
            //Open a database connection
            conn.Open();
            //Execute SELCT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            List<DeliveryFailure> deliveryFailureList = new List<DeliveryFailure>();
            while (reader.Read())
            {
                deliveryFailureList.Add(
                new DeliveryFailure
                {
                    reportID = reader.GetInt32(0),
                    parcelID = reader.GetInt32(1),
                    deliveryManID = reader.GetInt32(2),
                    failureType = reader.GetString(3)[0],
                    description = reader.GetString(4),
                    stationMgrID = !reader.IsDBNull(5) ? reader.GetInt32(5) : (int?)null,
                    followUpAction = !reader.IsDBNull(6) ? reader.GetString(6) : (string?)null,
                    dateCreated = reader.GetDateTime(7),
                }
                );
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return deliveryFailureList;
        }
        public DeliveryFailure GetDetails(int deliveryID)
        {
            DeliveryFailure deliveryFailure = new DeliveryFailure();
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement that
            //retrieves all attributes of a staff record.
            cmd.CommandText = @"SELECT * FROM DeliveryFailure
                          WHERE ReportID = @selectedReportID";
            //Define the parameter used in SQL statement, value for the
            //parameter is retrieved from the method parameter “staffId”.
            cmd.Parameters.AddWithValue("@selectedReportID", deliveryID);
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
                    deliveryFailure.reportID = reader.GetInt32(0);
                    deliveryFailure.parcelID = reader.GetInt32(1);
                    deliveryFailure.deliveryManID = reader.GetInt32(2);
                    deliveryFailure.failureType = reader.GetString(3)[0];
                    deliveryFailure.description = reader.GetString(4);
                    deliveryFailure.stationMgrID = reader.GetInt32(5);
                    deliveryFailure.followUpAction = reader.GetString(6);
                    deliveryFailure.dateCreated = reader.GetDateTime(7);

                }
            }
            //Close data reader
            reader.Close();
            //Close the database connection
            conn.Close();
            return deliveryFailure;
        }
        public int Update(DeliveryFailure deliveryFailure)
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify an UPDATE SQL statement
            cmd.CommandText = @"UPDATE DeliveryFailure SET FollowUpAction=@followUpAction                       
                          WHERE ReportID = @selectedReportID";
            //Define the parameters used in SQL statement, value for each parameter
            //is retrieved from respective class's property.
            cmd.Parameters.AddWithValue("@followUpAction", deliveryFailure.followUpAction);
            
            cmd.Parameters.AddWithValue("@selectedReportID", deliveryFailure.reportID);
            //Open a database connection
            conn.Open();
            //ExecuteNonQuery is used for UPDATE and DELETE
            int count = cmd.ExecuteNonQuery();
            //Close the database connection
            conn.Close();
            return count;
        }
    }
}
