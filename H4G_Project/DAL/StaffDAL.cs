using System.Data.SqlClient;
using H4G_Project.Models;
using System.Drawing.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace H4G_Project.DAL
{
    public class StaffDAL
    {
        private IConfiguration Configuration { get; set; }
        private SqlConnection conn;

        public StaffDAL()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("NPCSConnectionString");
            conn = new SqlConnection(strConn);
        }
        // check if the staff email exists for account creation in case the admin manager implememnted the ability to create a staff
        public bool IsEmailExists(string email)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT StaffID FROM Staff WHERE LoginID = @email";
            cmd.Parameters.AddWithValue("@email", email);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Close();
                conn.Close();
                return true;
            }
            else
            {
                reader.Close();
                conn.Close();
                return false;
            }
        }
        // check the db if the staff exists based on email and password and allow login
        public Staff Login(string email, string password)
        {
            if (email == null || password == null)
            {
                return null;
            }
            Staff staff = new Staff();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Staff WHERE LoginID = @email AND Password = @password";
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@password", password);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    staff.staffID = reader.GetInt32(0);
                    staff.staffName = reader.GetString(1);
                    staff.loginID = email;
                    staff.password = password;
                    staff.appointment = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                    staff.officeTelNo = !reader.IsDBNull(5) ? reader.GetString(5) : null;
                    staff.location = !reader.IsDBNull(6) ? reader.GetString(6) : null;
                    reader.Close();
                    conn.Close();
                    return staff;
                }
            }
            reader.Close();
            conn.Close();
            return null;
        }
        // get staff based on the loginID
        public Staff FindStaffByLoginID(string LoginID)
        {
            Staff staff = new Staff();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Staff WHERE LoginID = @email";
            cmd.Parameters.AddWithValue("@email", LoginID);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    staff.staffID = reader.GetInt32(0);
                    staff.staffName = reader.GetString(1);
                    staff.loginID = LoginID;
                    staff.password = reader.GetString(3);
                    staff.appointment = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                    staff.officeTelNo = !reader.IsDBNull(5) ? reader.GetString(5) : null;
                    staff.location = !reader.IsDBNull(6) ? reader.GetString(6) : null;
                    reader.Close();
                    conn.Close();
                    return staff;
                }
            }
            reader.Close();
            conn.Close();
            return null;
        }
        // find staff based on ID
        public Staff FindStaffByStaffID(int id)
        {
            Staff staff = new Staff();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Staff WHERE StaffID = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    staff.staffID = id;
                    staff.staffName = reader.GetString(1);
                    staff.loginID = reader.GetString(2);
                    staff.password = reader.GetString(3);
                    staff.appointment = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                    staff.officeTelNo = !reader.IsDBNull(5) ? reader.GetString(5) : null;
                    staff.location = !reader.IsDBNull(6) ? reader.GetString(6) : null;
                    reader.Close();
                    conn.Close();
                    return staff;
                }
            }
            reader.Close();
            conn.Close();
            return null;
        }
        // get all the staff in the db
		public List<Staff> GetAllStaff()
		{
			SqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = @"SELECT * FROM Staff";
			conn.Open();
			SqlDataReader reader = cmd.ExecuteReader();
            List<Staff> staffList = new List<Staff>();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					staffList.Add(
				    new Staff
				    {
					    staffID = reader.GetInt32(0),
						staffName = reader.GetString(1),
				    	loginID = reader.GetString(2),
						password = reader.GetString(3),
						appointment = !reader.IsDBNull(4) ? reader.GetString(4) : null,
                        officeTelNo = !reader.IsDBNull(5) ? reader.GetString(5) : null,
					    location = !reader.IsDBNull(6) ? reader.GetString(6) : null
				    }
				    );
				}
			}
			reader.Close();
			conn.Close();
			return staffList;
		}

        // check if the delivery man has more than 5 parcels assigned to him/her
        public bool CheckStaffAvailability(Staff staff)
        {
            //assuming that the delivery man is still assigned to a parcel until station manager collects for international deliveries
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Parcel WHERE DeliveryManID = @staffID and DeliveryStatus in (1,2)";
            cmd.Parameters.AddWithValue("@staffID", staff.staffID);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetInt32(0) < 5)
                {
                    reader.Close();
                    conn.Close();
                    return true;
                }
                break;
            }
            reader.Close();
            conn.Close();
            return false;
            
        }
	}
}