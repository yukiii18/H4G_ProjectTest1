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
    public class StaffDAL
    {
        FirestoreDb db;

        public StaffDAL()
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
        public async Task<Staff> GetStaffByEmail(string email)
        {
            CollectionReference staffRef = db.Collection("staff");

            // Create a query against the collection.
            Query query = staffRef.WhereEqualTo("Email", email);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Documents.Count > 0)
            {
                // Assuming email is unique, there should only be one matching document.
                DocumentSnapshot documentSnapshot = querySnapshot.Documents[0];
                if (documentSnapshot.Exists)
                {
                    Staff staff = documentSnapshot.ConvertTo<Staff>();
                    return staff;
                }
            }

            // Return null if no user is found
            return null;
        }

        public async Task<bool> AddStaff(Staff staff)
        {
            // Hash the password before saving to the database
            //user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            //Reference to collection
            CollectionReference collectionReference = db.Collection("staff");

            // Get a snapshot of the documents in the collection
            QuerySnapshot querySnapshot = await collectionReference.GetSnapshotAsync();

            // Count the number of documents
            int numberOfDocuments = querySnapshot.Documents.Count;
            Console.WriteLine($"Number of documents in users: {numberOfDocuments}");

            try
            {
                //DocumentReference docRef = db.Collection("staff").Document(Convert.ToString(numberOfDocuments + 1));

                Dictionary<string, object> NewUser = new Dictionary<string, object>
                {
                    {"Username", staff.Username},
                    {"Email", staff.Email},
                    {"Password", staff.Password }
                    //{"LastDayOfService", staff.LastDayOfService}
                };

                //await docRef.SetAsync(NewUser);
                await db.Collection("staff").AddAsync(NewUser);

                Console.WriteLine("User successfully added to Firestore.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding User to Firestore: {ex.Message}");
                return false;
            }
        }
    }
}