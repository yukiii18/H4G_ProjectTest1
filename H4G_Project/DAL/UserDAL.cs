using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Google.Cloud.Storage.V1;
using Grpc.Auth;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using H4G_Project.Controllers;
using System.Collections;
using System.ComponentModel;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using H4G_Project.Models;

namespace H4G_Project.DAL
{
    public class UserDAL
    {
        FirestoreDb db;

        public UserDAL()
        {
            string jsonPath = "./DAL/config/squad-60b0b-firebase-adminsdk-fbsvc-582ee8d43f.json";
            string projectId = "squad-60b0b";
            using StreamReader r = new StreamReader(jsonPath);
            string json = r.ReadToEnd();


            db = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                JsonCredentials = json
            }.Build();
        }

        public async Task<User> GetUsername(string username)
        {
            CollectionReference usersRef = db.Collection("users");

            // Create a query against the collection.
            Query query = usersRef.WhereEqualTo("Username", username);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Documents.Count > 0)
            {
                // Assuming email is unique, there should only be one matching document.
                DocumentSnapshot documentSnapshot = querySnapshot.Documents[0];
                if (documentSnapshot.Exists)
                {
                    User user = documentSnapshot.ConvertTo<User>();
                    return user;
                }
            }

            // Return null if no user is found
            return null;
        }

        //Get User Email
        public async Task<User> GetUserByEmail(string email)
        {
            CollectionReference usersRef = db.Collection("users");

            // Create a query against the collection.
            Query query = usersRef.WhereEqualTo("Email", email);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Documents.Count > 0)
            {
                // Assuming email is unique, there should only be one matching document.
                DocumentSnapshot documentSnapshot = querySnapshot.Documents[0];
                if (documentSnapshot.Exists)
                {
                    User user = documentSnapshot.ConvertTo<User>();
                    return user;
                }
            }

            // Return null if no user is found
            return null;
        }

        public async Task<bool> AddUser(User user)
        {
            // Hash the password before saving to the database
            //user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            //Reference to collection
            CollectionReference collectionReference = db.Collection("users");

            // Get a snapshot of the documents in the collection
            QuerySnapshot querySnapshot = await collectionReference.GetSnapshotAsync();

            // Count the number of documents
            int numberOfDocuments = querySnapshot.Documents.Count;
            Console.WriteLine($"Number of documents in users: {numberOfDocuments}");

            try
            {
                DocumentReference docRef = db.Collection("users").Document(Convert.ToString(numberOfDocuments + 1));

                Dictionary<string, object> NewUser = new Dictionary<string, object>
                {
                    {"Username", user.Username},
                    {"Email", user.Email},
                    {"Password", user.Password }
                };

                //await docRef.SetAsync(NewUser);
                await db.Collection("users").AddAsync(NewUser);

                Console.WriteLine("User successfully added to Firestore.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding User to Firestore: {ex.Message}");
                return false;
            }
        }
        public async Task<List<User>> GetUser()
        {
            int id = 1;
            List<User> userList = new List<User>();

            while (true)
            {
                DocumentReference docRef = db.Collection("users").Document(Convert.ToString(id));
                DocumentSnapshot documentSnapshot = await docRef.GetSnapshotAsync();

                if (documentSnapshot.Exists)
                {
                    User data = documentSnapshot.ConvertTo<User>();

                    userList.Add(new User
                    {
                        Username = data.Username,
                        Email = data.Email,
                        Password = data.Password

                    });
                }
                else
                {
                    // Exit the loop if the document with the current ID doesn't exist
                    break;
                }

                // Increment the ID for the next iteration
                id++;
                Console.WriteLine("done");
            }

            return userList;
        }


        /*public async Task<bool> AddStaff(Staff staff)
        {
            //Reference to a collection
            CollectionReference collectionReference = db.Collection("Staff");

            // Get a snapshot of the documents in the collection
            QuerySnapshot querySnapshot = await collectionReference.GetSnapshotAsync();

            // Count the number of Staff
            int numberOfStaff = querySnapshot.Documents.Count;
            Console.WriteLine($"Number of documents in users: {numberOfStaff}");

            try
            {
                DocumentReference docRef = db.Collection("Staff").Document(Convert.ToString(numberOfStaff + 1));

                Dictionary<string, object> NewStaff = new Dictionary<string, object>
                {
                    {"Username", staff.Username},
                    {"Email", staff.Email},
                    {"Password", staff.Password }
                };

                await docRef.SetAsync(NewStaff);

                Console.WriteLine("Staff successfully added to Firestore.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding Staff to Firestore: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Staff>> GetStaff()
        {
            int id = 1;
            List<Staff> userList = new List<Staff>();

            while (true)
            {
                DocumentReference docRef = db.Collection("Staff").Document(Convert.ToString(id));
                DocumentSnapshot documentSnapshot = await docRef.GetSnapshotAsync();

                if (documentSnapshot.Exists)
                {
                    Staff data = documentSnapshot.ConvertTo<Staff>();

                    userList.Add(new Staff
                    {
                        Username = data.Username,
                        Email = data.Email,
                        Password = data.Password

                    });
                }
                else
                {
                    // Exit the loop if the document with the current ID doesn't exist
                    Console.WriteLine("Error");
                    break;
                }

                // Increment the ID for the next iteration
                id++;
                Console.WriteLine("done");
            }

            return userList;
        }*/

        /*
                // check the db if the member email and password details exists.
                public User Login(string email)
                {
                    if (email == null)
                    {
                        return null;
                    }
                    User member = new User();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = @"SELECT * FROM User WHERE EmailAddr = @email";
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

                public List<Parcel> GetParcels(User member)
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

                public void SubmitFeedback(User member, string feedback, string rating, string comment)
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
                public List<CashVoucher> GetVouchers(User member)
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
                public int CreateUser(User member)
                {
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = @"INSERT INTO User(Name,Salutation,TelNo,EmailAddr,Password,BirthDate,City,Country) OUTPUT INSERTED.MemberID VALUES (@Name,@Salutation,@TelNo,@EmailAddr,@Password,@BirthDate,@City,@Country)";
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
                public List<User> GetAllMembers()
                {
                    //Create a SqlCommand object from connection object
                    SqlCommand cmd = conn.CreateCommand();
                    //Specify the SQL statement that select all branches
                    cmd.CommandText = @"SELECT * FROM User";
                    //Open a database connection
                    conn.Open();
                    //Execute SELCT SQL through a DataReader
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<User> memberList = new List<User>();
                    while (reader.Read())
                    {
                        memberList.Add(
                       new User
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
                public User GetDetails(int memberID)
                {
                    User members = new User();
                    //Create a SqlCommand object from connection object
                    SqlCommand cmd = conn.CreateCommand();
                    //Specify the SELECT SQL statement that
                    //retrieves all attributes of a staff record.
                    cmd.CommandText = @"SELECT * FROM User
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
                public int Update(User member)
                {
                    //Create a SqlCommand object from connection object
                    SqlCommand cmd = conn.CreateCommand();
                    //Specify an UPDATE SQL statement
                    cmd.CommandText = @"UPDATE User SET Name=@name,
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

                */
    }
}