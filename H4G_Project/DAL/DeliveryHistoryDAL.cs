using System.Data.SqlClient;
using H4G_Project.Models;
 

namespace H4G_Project.DAL
{
    public class DeliveryHistoryDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        //Constructor
        public DeliveryHistoryDAL()
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
		

		public int AddRecord(DeliveryHistory dh)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO DeliveryHistory (ParcelID, Description) 
                              OUTPUT INSERTED.RecordID 
                              VALUES(@parcelId, @description)";
            cmd.Parameters.AddWithValue("@parcelId", dh.parcelID);
            cmd.Parameters.AddWithValue("@description", dh.description);

            conn.Open();
            dh.recordID = (int)cmd.ExecuteScalar();
            conn.Close();
            return dh.recordID;
        }
        public bool CheckDeliveryActivity(Staff staff,int parcelid)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM DeliveryHistory WHERE Description LIKE '%{staff.loginID}%' AND ParcelID = {parcelid}";
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Close();
                conn.Close();
                return true;
            }
            reader.Close();
            conn.Close();
            return false;   
        }

    }
}
