using System.Data.SqlClient;
using H4G_Project.Models;

namespace H4G_Project.DAL
{
    public class CashVoucherDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        //Constructor
        public CashVoucherDAL()
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

        public List<CashVoucher> GetAllCashVoucher()
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SQL statement that select all branches
            cmd.CommandText = @"SELECT * FROM CashVoucher";
            //Open a database connection
            conn.Open();
            //Execute SELCT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            List<CashVoucher> cashVoucherList = new List<CashVoucher>();
            while (reader.Read())
            {
                cashVoucherList.Add(
                new CashVoucher
                {
                    cashVoucherID = reader.GetInt32(0),
                    staffID = reader.GetInt32(1),
                    Amount = reader.GetDecimal(2),
                    currency = reader.GetString(3),
                    issueingCode = reader.GetString(4)[0],
                    receiverName = reader.GetString(5),
                    receiverTelNo = reader.GetString(6),
                    dateTimeIssued = reader.GetDateTime(7),
                    status = reader.GetString(8)
                }
                );
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return cashVoucherList;
        }
        public List<CashVoucher> GetSpecificCashVoucher(string name, string telNo)
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SQL statement that select all branches
            cmd.CommandText = @"SELECT * FROM CashVoucher Where ReceiverName = @selectname and ReceiverTelNo = @selectTel";
            //Open a database connection
            cmd.Parameters.AddWithValue("@selectname", name);
            cmd.Parameters.AddWithValue("@selectTel", telNo);
            conn.Open();
            //Execute SELCT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            List<CashVoucher> cashVoucherList = new List<CashVoucher>();
            while (reader.Read())
            {
                cashVoucherList.Add(
                new CashVoucher
                {
                    cashVoucherID = reader.GetInt32(0),
                    staffID = reader.GetInt32(1),
                    Amount = reader.GetDecimal(2),
                    currency = reader.GetString(3),
                    issueingCode = reader.GetString(4)[0],
                    receiverName = reader.GetString(5),
                    receiverTelNo = reader.GetString(6),
                    dateTimeIssued = reader.GetDateTime(7),
                    status = reader.GetString(8)
                }
                );
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return cashVoucherList;
        }
        public int Create(CashVoucher voucher)
        {
            // Create a SqlCommand object from the connection object
            SqlCommand cmd = conn.CreateCommand();
            // Specify the INSERT SQL statement
            cmd.CommandText = @"INSERT INTO CashVoucher(StaffID, Amount, Currency, IssuingCode, ReceiverName, ReceiverTelNo, DateTimeIssued)
                        OUTPUT INSERTED.CashVoucherID
                        VALUES(@staffID, @amount, @currency, @issuingCode, @receiverName, @receiverTelNo, @dateTimeIssued)";

            // Define the parameters used in the SQL statement, value for each parameter
            // is retrieved from the respective class's property.
            cmd.Parameters.AddWithValue("@staffID", voucher.staffID);
            cmd.Parameters.AddWithValue("@amount", voucher.Amount);
            cmd.Parameters.AddWithValue("@currency", voucher.currency);
            cmd.Parameters.AddWithValue("@issuingCode", voucher.issueingCode);
            cmd.Parameters.AddWithValue("@receiverName", voucher.receiverName);
            cmd.Parameters.AddWithValue("@receiverTelNo", voucher.receiverTelNo);
            cmd.Parameters.AddWithValue("@dateTimeIssued", voucher.dateTimeIssued);
            //cmd.Parameters.AddWithValue("@status", voucher.status);

            // A connection to the database must be opened before any operations are made.
            conn.Open();
            // ExecuteScalar is used to retrieve the auto-generated
            // CashVoucherID after executing the INSERT SQL statement.
            voucher.cashVoucherID = (int)cmd.ExecuteScalar();
            // A connection should be closed after operations.
            conn.Close();

            return voucher.cashVoucherID;
        }
        public CashVoucher GetDetails(int cashVoucherID)
        {
            CashVoucher cashVoucher = new CashVoucher();
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement that
            //retrieves all attributes of a staff record.
            cmd.CommandText = @"SELECT * FROM CashVoucher
                          WHERE CashVoucherID = @selectedCashVoucherID";
            //Define the parameter used in SQL statement, value for the
            //parameter is retrieved from the method parameter “staffId”.
            cmd.Parameters.AddWithValue("@selectedCashVoucherID", cashVoucherID);
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
                    cashVoucher.cashVoucherID = reader.GetInt32(0);
                    cashVoucher.staffID = reader.GetInt32(1);
                    cashVoucher.Amount = reader.GetDecimal(2);
                    cashVoucher.currency = reader.GetString(3);
                    cashVoucher.issueingCode = reader.GetString(4)[0];
                    cashVoucher.receiverName = reader.GetString(5);
                    cashVoucher.receiverTelNo = reader.GetString(6);
                    cashVoucher.dateTimeIssued = reader.GetDateTime(7);
                    cashVoucher.status = reader.GetString(8);



                }
            }
            //Close data reader
            reader.Close();
            //Close the database connection
            conn.Close();
            return cashVoucher;
        }
        public int Update(CashVoucher cash)
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify an UPDATE SQL statement
            cmd.CommandText = @"UPDATE CashVoucher SET Status=@status
                        
                          WHERE CashVoucherID = @selectedCashVoucherID";
            //Define the parameters used in SQL statement, value for each parameter
            //is retrieved from respective class's property.
            cmd.Parameters.AddWithValue("@status", cash.status);
            cmd.Parameters.AddWithValue("@selectedCashVoucherID", cash.cashVoucherID);
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
