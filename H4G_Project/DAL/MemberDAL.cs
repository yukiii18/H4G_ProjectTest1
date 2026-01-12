using System;
using System.Data.SqlClient;
using H4G_Project.Models;

namespace H4G_Project.DAL
{
    public class MemberDAL
    {
        private IConfiguration Configuration { get; set; }
        private SqlConnection conn;

        public MemberDAL()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("NPCSConnectionString");
            conn = new SqlConnection(strConn);
        }
        //READ
        // check if email already exists in the db for member acc creation
        public bool IsEmailExists(string email)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT MemberID FROM Member WHERE EmailAddr = @email";
            cmd.Parameters.AddWithValue("@email", email);
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
        // check the db if the member email and password details exists.
        public Member Login(string email)
        {
            if (email == null)
            {
                return null;
            }
            Member member = new Member();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Member WHERE EmailAddr = @email";
            cmd.Parameters.AddWithValue("@email", email);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    member.memberID = reader.GetInt32(0);
                    member.name = reader.GetString(1);
                    member.salutation = reader.GetString(2);
                    member.telNo = reader.GetString(3);
                    member.emailAddr = email;
                    member.password = reader.GetString(5);
                    member.birthDate = !reader.IsDBNull(6) ? reader.GetDateTime(6) : null;
                    member.city = !reader.IsDBNull(7) ? reader.GetString(7) : null;
                    member.country = reader.GetString(8);
                    reader.Close();
                    conn.Close();

                    return member;
                }
            }
            reader.Close();
            conn.Close();
            return member;
        }

        //WRITE

        public List<Parcel> GetParcels(Member member)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Parcel WHERE ReceiverTelNo = @telNo order by targetDeliveryDate desc";
            cmd.Parameters.AddWithValue("@telNo", member.telNo);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Parcel> parcels = new List<Parcel>();
            if (reader.HasRows)
            {
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
                    parcel.parcelWeight = (float)reader.GetDouble(11);
                    parcel.deliveryCharge = reader.GetDecimal(12);
                    parcel.currency = reader.GetString(13);
                    parcel.targetDeliveryDate = reader.GetDateTime(14);
                    parcel.deliveryStatus = reader.GetString(15).ToCharArray()[0];
                    // parcel.deliveryManId = !reader.IsDBNull(16) ? reader.GetInt32(16) : null;

                    parcels.Add(parcel);
                }
            }
            reader.Close();
            conn.Close();
            return parcels;
        }

        public void SubmitFeedback(Member member, string feedback, string rating, string comment)
        {

            String formattedContent = "Feedback: " + feedback + "|Rating: " + rating + "|Comment: " + comment;
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO FeedbackEnquiry(MemberID, [Content], DateTimePosted, Status) VALUES(@memberID, @content, @dateTimePosted, @status)";
            cmd.Parameters.AddWithValue("@memberID", member.memberID);
            cmd.Parameters.AddWithValue("@content", formattedContent);
            cmd.Parameters.AddWithValue("@dateTimePosted", DateTime.Now);
            cmd.Parameters.AddWithValue("@status", '0');

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

        }
        public List<CashVoucher> GetVouchers(Member member)
        {
            SqlCommand cmd = conn.CreateCommand();
            Console.WriteLine("Num",member.telNo);
            cmd.Parameters.AddWithValue("@telNo", member.telNo);
            cmd.CommandText = @"SELECT * FROM CashVoucher WHERE ReceiverTelNo = @telNo and status = 0 order by dateTimeIssued desc";


            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<CashVoucher> vouchers = new List<CashVoucher>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {

                    CashVoucher voucher = new CashVoucher();
                    voucher.cashVoucherID = reader.GetInt32(0);
                    voucher.staffID = reader.GetInt32(1);
                    voucher.Amount = reader.GetDecimal(2);
                    voucher.currency = reader.GetString(3);
                    // voucher.issueingCode = reader.GetChar(4);
                    voucher.receiverName = reader.GetString(5);
                    voucher.receiverTelNo = reader.GetString(6);
                    voucher.dateTimeIssued = reader.GetDateTime(7);
                    // voucher.status = reader.GetChar(8);

                    vouchers.Add(voucher);
                }
            }
            reader.Close();
            conn.Close();
            return vouchers;
        }


        //WRITE
        // Create a user into the db
        public int CreateUser(Member member)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Member(Name,Salutation,TelNo,EmailAddr,Password,BirthDate,City,Country) OUTPUT INSERTED.MemberID VALUES (@Name,@Salutation,@TelNo,@EmailAddr,@Password,@BirthDate,@City,@Country)";
            cmd.Parameters.AddWithValue("@Name", member.name);
            cmd.Parameters.AddWithValue("@Salutation", member.salutation);
            cmd.Parameters.AddWithValue("@TelNo", member.telNo);
            cmd.Parameters.AddWithValue("@EmailAddr", member.emailAddr);
            cmd.Parameters.AddWithValue("@Password", member.password);
            cmd.Parameters.AddWithValue("@Country", member.country);
            cmd.Parameters.AddWithValue("@BirthDate", member.birthDate != null ? member.birthDate.Value.ToString("dd-MMM-yyyy") : null);
            cmd.Parameters.AddWithValue("@City", member.city != null ? member.city : null);
            conn.Open();
            member.memberID = (int)cmd.ExecuteScalar();
            conn.Close();
            return member.memberID;
        }
        public List<Member> GetAllMembers()
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SQL statement that select all branches
            cmd.CommandText = @"SELECT * FROM Member";
            //Open a database connection
            conn.Open();
            //Execute SELCT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            List<Member> memberList = new List<Member>();
            while (reader.Read())
            {
                memberList.Add(
               new Member
               {
                   memberID = reader.GetInt32(0),
                   name = reader.GetString(1),
                   salutation = reader.GetString(2),
                   telNo = reader.GetString(3),
                   emailAddr = reader.GetString(4),
                   birthDate = reader.GetDateTime(6),

               }
               );
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return memberList;
        }
        public Member GetDetails(int memberID)
        {
            Member members = new Member();
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement that
            //retrieves all attributes of a staff record.
            cmd.CommandText = @"SELECT * FROM Member
                          WHERE MemberID = @selectedMemberID";
            //Define the parameter used in SQL statement, value for the
            //parameter is retrieved from the method parameter “staffId”.
            cmd.Parameters.AddWithValue("@selectedMemberID", memberID);
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
                    members.memberID = reader.GetInt32(0);
                    members.name = reader.GetString(1);
                    members.salutation = reader.GetString(2);
                    members.telNo = reader.GetString(3);
                    members.emailAddr = reader.GetString(4);
                    members.birthDate = reader.GetDateTime(6);
                }
            }
            //Close data reader
            reader.Close();
            //Close the database connection
            conn.Close();
            return members;
        }
        public int Update(Member member)
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify an UPDATE SQL statement
            cmd.CommandText = @"UPDATE Member SET Name=@name,
                          Salutation=@salutation, TelNo=@telno, EmailAddr=@emailaddr, BirthDate=@birthdate
                          WHERE MemberID = @selectedMemberID";
            //Define the parameters used in SQL statement, value for each parameter
            //is retrieved from respective class's property.
            cmd.Parameters.AddWithValue("@name", member.name);
            cmd.Parameters.AddWithValue("@salutation", member.salutation);
            cmd.Parameters.AddWithValue("@telno", member.telNo);
            cmd.Parameters.AddWithValue("@emailaddr", member.emailAddr);
            cmd.Parameters.AddWithValue("@birthdate", member.birthDate);
            cmd.Parameters.AddWithValue("@selectedMemberID", member.memberID);
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