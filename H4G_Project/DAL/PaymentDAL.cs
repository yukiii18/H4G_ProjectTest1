using System.Data.SqlClient;
using H4G_Project.Models;

namespace H4G_Project.DAL
{
    public class PaymentDAL
    {
        private IConfiguration Configuration { get; set; }
        private SqlConnection conn;

        public PaymentDAL()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("NPCSConnectionString");
            conn = new SqlConnection(strConn);
        }

        public List<PaymentTransaction> GetAllPayment()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM PaymentTransaction";
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<PaymentTransaction> paymentList = new List<PaymentTransaction>();
            while (reader.Read())
            {
                paymentList.Add(
                new PaymentTransaction
                {
                    transactionID = reader.GetInt32(0),
                    parcelID = reader.GetInt32(1),
                    amtTran = Math.Round(reader.GetDecimal(2),2),
                    currency = reader.GetString(3),
                    tranType = reader.GetString(4), 
                    tranDate = reader.GetDateTime(5),
                }
                );
            }
            reader.Close();
            conn.Close();
            return paymentList;
        }

        public List<PaymentTransaction> GetPaymentByParcelId(int id)
        {
			SqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = @"SELECT * FROM PaymentTransaction WHERE ParcelID = @id";
			cmd.Parameters.AddWithValue("@id", id);
			conn.Open();
			SqlDataReader reader = cmd.ExecuteReader();
			List<PaymentTransaction> paymentList = new List<PaymentTransaction>();
			while (reader.Read())
			{
				paymentList.Add(
				new PaymentTransaction
				{
					transactionID = reader.GetInt32(0),
					parcelID = reader.GetInt32(1),
					amtTran = Math.Round(reader.GetDecimal(2), 2),
					currency = reader.GetString(3),
					tranType = reader.GetString(4),
					tranDate = reader.GetDateTime(5),
				}
				);
			}
			reader.Close();
			conn.Close();
			return paymentList;
		}

        // Add payment transaction to database
        public int Add(PaymentTransaction payment)
        {
            Console.WriteLine(payment.tranType);
            Console.WriteLine(payment.amtTran);
            Console.WriteLine(payment.parcelID);
            Console.WriteLine(payment.currency);
            Console.WriteLine(payment.tranDate);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO PaymentTransaction (ParcelID, AmtTran, Currency, TranType, TranDate) 
                                OUTPUT INSERTED.TransactionID
                              VALUES(@parcelid, @amt, @currency, @trantype, @trandate)";
            cmd.Parameters.AddWithValue("@parcelid", payment.parcelID);
            cmd.Parameters.AddWithValue("@amt", payment.amtTran);
            cmd.Parameters.AddWithValue("@currency", payment.currency);
            cmd.Parameters.AddWithValue("@trantype", payment.tranType);
            cmd.Parameters.AddWithValue("@trandate", payment.tranDate);
            conn.Open();
            payment.transactionID = (int)cmd.ExecuteScalar();
            conn.Close();
            return payment.transactionID;
        }
    }
}
