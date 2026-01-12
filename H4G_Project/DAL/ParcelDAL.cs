using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using H4G_Project.Models;

namespace H4G_Project.DAL
{
    public class ParcelDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;
        private StaffDAL staffContext = new StaffDAL();

        //Constructor   
        public ParcelDAL()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString(
            "NPCSConnectionString");
            conn = new SqlConnection(strConn);
        }

        // Add parcel to database
        public int Add(Parcel parcel)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Parcel (ItemDescription, SenderName, SenderTelNo, ReceiverName, 
                              ReceiverTelNo, DeliveryAddress, FromCity, FromCountry, ToCity, ToCountry,
                                ParcelWeight, DeliveryCharge, Currency, TargetDeliveryDate, DeliveryStatus) 
                              OUTPUT INSERTED.ParcelID 
                              VALUES(@description, @sendername, @sendertel, @receivername, 
                              @receivertel, @address, @fromcity, @fromcountry, @tocity, @tocountry,
                                @weight, @deliverycharge, @currency, @targetdate, @deliverystatus)";
            cmd.Parameters.AddWithValue("@description", parcel.itemDescription);
            cmd.Parameters.AddWithValue("@sendername", parcel.senderName);
            cmd.Parameters.AddWithValue("@sendertel", parcel.senderTelNo);
            cmd.Parameters.AddWithValue("@receivername", parcel.receiverName);
            cmd.Parameters.AddWithValue("@receivertel", parcel.receiverTelNo);
            cmd.Parameters.AddWithValue("@address", parcel.deliveryAddress);
            cmd.Parameters.AddWithValue("@fromcity", "Singapore");
            cmd.Parameters.AddWithValue("@fromcountry", "Singapore");
            cmd.Parameters.AddWithValue("@tocity", parcel.toCity);
            cmd.Parameters.AddWithValue("@tocountry", parcel.toCountry);
            cmd.Parameters.AddWithValue("@weight", parcel.parcelWeight);
            cmd.Parameters.AddWithValue("@deliverycharge", parcel.deliveryCharge);
            cmd.Parameters.AddWithValue("@currency", "SGD");
            cmd.Parameters.AddWithValue("@targetdate", parcel.targetDeliveryDate);
            cmd.Parameters.AddWithValue("@deliverystatus", '0');

            conn.Open();
            parcel.parcelId = (int)cmd.ExecuteScalar();
            conn.Close();
            return parcel.parcelId;
        }

        // Get all parcel records from database
        // Just get all the parcels from the parcel table in the db
        public List<Parcel> GetAllParcels()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Parcel";
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Parcel> parcelList = new List<Parcel>();
            while (reader.Read())
            {
                parcelList.Add(
                new Parcel
                {
                    parcelId = reader.GetInt32(0), 
                    itemDescription = !reader.IsDBNull(1) ? reader.GetString(1) : (String?)null,
                    senderName = reader.GetString(2),   
                    senderTelNo = reader.GetString(3),  
                    receiverName = reader.GetString(4),
                    receiverTelNo = reader.GetString(5),
                    deliveryAddress = reader.GetString(6),
                    fromCity = reader.GetString(7),
                    fromCountry = reader.GetString(8),
                    toCity = reader.GetString(9),
                    toCountry = reader.GetString(10),
                    parcelWeight = reader.GetDouble(11),
                    deliveryCharge = Math.Round(reader.GetDecimal(12),2),
                    currency = reader.GetString(13),    
                    targetDeliveryDate = !reader.IsDBNull(14) ? reader.GetDateTime(14) : (DateTime?)null,
                    deliveryStatus = char.Parse(reader.GetString(15)),
                    deliveryManId = !reader.IsDBNull(16) ? reader.GetInt32(16) : (int?)null
                }
                );
            }
            reader.Close();
            conn.Close();
            return parcelList;
        }
        // update the parcel details according to the id, column to be updated and the new value
        public int UpdateParcel(int id,string columnName,string? newValue)//Update Parcel details based on Parcel,columnName,newValue
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"UPDATE Parcel SET {columnName} = {newValue} WHERE ParcelID = {id}";
            cmd.Parameters.AddWithValue("@columnName", columnName);
            cmd.Parameters.AddWithValue("@newValue", newValue);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            int result = cmd.ExecuteNonQuery();
            conn.Close();
            return result;
        }

        // Get a parcel by its id
        public Parcel GetParceById(int id)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Parcel WHERE ParcelID = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            Parcel parcel = new Parcel();
            if (reader.HasRows)
            {
                while (reader.Read()) 
                { 
                    parcel.parcelId = reader.GetInt32(0);
                    parcel.itemDescription = !reader.IsDBNull(1) ? reader.GetString(1) : (string?) null;
                    parcel.senderName = reader.GetString(2);
                    parcel.senderTelNo = reader.GetString(3);
                    parcel.receiverName = reader.GetString(4);
                    parcel.receiverTelNo = reader.GetString(5);
                    parcel.deliveryAddress = reader.GetString(6);
                    parcel.fromCity = reader.GetString(7);
                    parcel.fromCountry = reader.GetString(8);
                    parcel.toCity = reader.GetString(9);
                    parcel.toCountry = reader.GetString(10);
                    parcel.parcelWeight = reader.GetDouble(11);
                    parcel.deliveryCharge = reader.GetDecimal(12);
                    parcel.currency = reader.GetString(13);
                    parcel.targetDeliveryDate = !reader.IsDBNull(14) ? reader.GetDateTime(14) : (DateTime?) null;
                    parcel.deliveryStatus = reader.GetString(15)[0];
                    parcel.deliveryManId = ! reader.IsDBNull(16) ? reader.GetInt32(16) : (int?) null; 
                }
            }
            reader.Close();
            conn.Close();
            return parcel;
        }
        // get the parcel's current location based on the delivery history and staff that handled the parcel location
		public string GetLastKnowParcelLocation(Parcel parcel)
		{
			string description = "";
			SqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = @"SELECT TOP 1 * FROM DeliveryHistory WHERE ParcelID = @id ORDER BY RecordID DESC";
			cmd.Parameters.AddWithValue("@id", parcel.parcelId);
			conn.Open();
			SqlDataReader reader = cmd.ExecuteReader();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					description = reader.GetString(2);
					reader.Close();
					conn.Close();
					break;
				}
			}
			if (description.ToLower().Contains("airport"))
			{
				return $"airport,";
			}
			if (description.ToLower().Contains("received"))
			{
				if (parcel.deliveryManId != null)
				{
					Staff staff = staffContext.FindStaffByStaffID((int)parcel.deliveryManId);
					return $"{staff.location},{staff.loginID}";
				}
				else
				{
					string[] descriptionarray = description.Split(" ");
					Staff staff = staffContext.FindStaffByLoginID(descriptionarray[Array.IndexOf(descriptionarray, "by") + 1]);
					return $"{staff.location},{staff.loginID}";
				}
			}
			else
			{
				Staff staff = staffContext.FindStaffByStaffID((int)parcel.deliveryManId);
				return $"{staff.location},{staff.loginID}";
			}
		}
        public List<Parcel> SearchParcelByUser(Member member, String query)
        {
            List<Parcel> parcels = new List<Parcel>();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Parcel WHERE ReceiverTelNo = @telNo and ParcelID LIKE @query";
            cmd.Parameters.AddWithValue("@telNo", member.telNo);
            cmd.Parameters.AddWithValue("@query", "%" + query + "%");
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Parcel parcel = new Parcel();
                parcel.parcelId = reader.GetInt32(0);
                parcel.itemDescription = reader.GetString(1);
                parcel.senderName = reader.GetString(2);
                parcel.senderTelNo = reader.GetString(3);
                parcel.receiverName = reader.GetString(4);
                parcel.receiverTelNo = reader.GetString(5);
                parcel.deliveryAddress = reader.GetString(6);
                parcel.fromCity = reader.GetString(7);
                parcel.fromCountry = reader.GetString(8);
                parcel.toCity = reader.GetString(9);
                parcel.toCountry = reader.GetString(10);
                // parcel.parcelWeight = reader.GetFloat(11);
                parcel.currency = reader.GetString(13);
                parcel.targetDeliveryDate = reader.GetDateTime(14);
                parcel.deliveryStatus = reader.GetString(15).ToCharArray()[0];
                // parcel.deliveryManId = !reader.IsDBNull(16) ? reader.GetInt32(16) : null;
                parcels.Add(parcel);
            }
            reader.Close();
            conn.Close();
            return parcels;
        }

        public List<Parcel> SearchParcelByStaff(string staffID, String query)
        {
            List<Parcel> parcels = new List<Parcel>();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Parcel WHERE DeliveryManID = @staffID and ParcelID LIKE @query";
            cmd.Parameters.AddWithValue("@staffID", staffID);
            cmd.Parameters.AddWithValue("@query", "%" + query + "%");
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Parcel parcel = new Parcel();
                parcel.parcelId = reader.GetInt32(0);
                parcel.itemDescription = reader.GetString(1);
                parcel.senderName = reader.GetString(2);
                parcel.senderTelNo = reader.GetString(3);
                parcel.receiverName = reader.GetString(4);
                parcel.receiverTelNo = reader.GetString(5);
                parcel.deliveryAddress = reader.GetString(6);
                parcel.fromCity = reader.GetString(7);
                parcel.fromCountry = reader.GetString(8);
                parcel.toCity = reader.GetString(9);
                parcel.toCountry = reader.GetString(10);
                // parcel.parcelWeight = reader.GetFloat(11);
                parcel.currency = reader.GetString(13);
                parcel.targetDeliveryDate = reader.GetDateTime(14);
                parcel.deliveryStatus = reader.GetString(15).ToCharArray()[0];
                // parcel.deliveryManId = !reader.IsDBNull(16) ? reader.GetInt32(16) : null;
                parcels.Add(parcel);
            }
            reader.Close();
            conn.Close();
            return parcels;
        }
        // get the delivery failure parcel and check if it has yet to be reported on
        public List<Parcel> GetPendingFailures(Staff staff)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT p.* FROM Parcel p WHERE DeliveryStatus = '4' and ParcelID NOT IN (SELECT ParcelID FROM DeliveryFailure) AND DeliveryManID = @id";
            cmd.Parameters.AddWithValue("@id", staff.staffID.ToString());
            List<Parcel> failedParcels = new List<Parcel>();
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    failedParcels.Add(new Parcel
                    {
                        parcelId = reader.GetInt32(0),
                        itemDescription = !reader.IsDBNull(1) ? reader.GetString(1) : (string?) null,
                        senderName = reader.GetString(2),
                        senderTelNo = reader.GetString(3),
                        receiverName =  reader.GetString(4), 
                        receiverTelNo = reader.GetString(5),
                        deliveryAddress = reader.GetString(6),
                        fromCity = reader.GetString(7),
                        fromCountry = reader.GetString(8),
                        toCity = reader.GetString(9),
                        toCountry = reader.GetString(10),
                        parcelWeight = reader.GetDouble(11),
                        deliveryCharge = reader.GetDecimal(12),
                        currency = reader.GetString(13),
                        targetDeliveryDate = !reader.IsDBNull(14) ? reader.GetDateTime(14) : (DateTime?) null,
                        deliveryStatus = reader.GetString(15)[0],
                        deliveryManId = !reader.IsDBNull(16) ? reader.GetInt32(16) : (int?) null,
                    }) ;
                }
                reader.Close();
                conn.Close();
                return failedParcels;
            }
            else
            {
                reader.Close();
                conn.Close();
                return null;
            }
        
        }


		public List<ParcelWithDelvHist> GetSpecifiedParcel(int? parcelID, string? CustomerName)
		{
			List<ParcelWithDelvHist> ParcelList = new();
			SqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = "";
			if (parcelID != null && CustomerName != "")
			{
				cmd.CommandText = @"SELECT * FROM Parcel WHERE ((SenderName = @MemberName) or (ReceiverName = @MemberName)) and (ParcelID = @parcelID)";
				cmd.Parameters.AddWithValue("@MemberName", CustomerName);
				cmd.Parameters.AddWithValue("@ParcelID", parcelID);
			}
			else if (parcelID == null)
			{
				cmd.CommandText = @"SELECT * FROM Parcel WHERE (SenderName = @MemberName) or (ReceiverName = @MemberName)";
				cmd.Parameters.AddWithValue("@MemberName", CustomerName);
			}
			else if (CustomerName == "")
			{

				cmd.CommandText = @"SELECT * FROM Parcel WHERE (ParcelID = @parcelID)";
				cmd.Parameters.AddWithValue("@ParcelID", parcelID);
			}

			conn.Open();
			SqlDataReader reader = cmd.ExecuteReader();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					ParcelList.Add(
					new ParcelWithDelvHist
					{
						parcelId = reader.GetInt32(0),
						itemDescription = reader.GetString(1),
						senderName = reader.GetString(2),
						senderTelNo = reader.GetString(3),
						receiverName = reader.GetString(4),
						receiverTelNo = reader.GetString(5),
						deliveryAddress = reader.GetString(6),
						fromCity = reader.GetString(7),
						fromCountry = reader.GetString(8),
						toCity = reader.GetString(9),
						toCountry = reader.GetString(10),
						parcelWeight = reader.GetDouble(11),
						deliveryCharge = reader.GetDecimal(12),
						currency = reader.GetString(13),
						targetDeliveryDate = reader.GetDateTime(14),
						deliveryStatus = reader.GetString(15).ToString()[0],

						deliveryManId = !reader.IsDBNull(16) ?
									   reader.GetInt32(16) : (int?)null


					});
				}
			}
			//Close DataReader
			reader.Close();
			//Close the database connection
			conn.Close();
			return ParcelList;
		}
	}
}
