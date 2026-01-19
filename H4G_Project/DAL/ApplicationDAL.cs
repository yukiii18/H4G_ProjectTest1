using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using H4G_Project.Models;
using System.Linq;



namespace H4G_Project.DAL
{
    public class ApplicationDAL
    {
        private readonly FirestoreDb db;
        private readonly string bucketName;
        private readonly string serviceAccountPath;

        public ApplicationDAL()
        {
            // Configure your Firebase project details
            string projectId = "squad-60b0b";
            bucketName = "squad-60b0b.firebasestorage.app"; // Updated bucket name format
            serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "DAL", "config", "squad-60b0b-firebase-adminsdk-fbsvc-582ee8d43f.json");

            Console.WriteLine($"Service account path: {serviceAccountPath}");
            Console.WriteLine($"File exists: {File.Exists(serviceAccountPath)}");
            Console.WriteLine($"Using bucket: {bucketName}");

            // Set environment variable for Firebase
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);

            db = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                JsonCredentials = File.ReadAllText(serviceAccountPath)
            }.Build();
        }

        /// <summary>
        /// Add a new application and upload files to Firebase Storage
        /// </summary>
        public async Task<bool> AddApplication(Application application, IFormFile medicalReport, IFormFile idDocument)
        {
            try
            {
                string medicalReportUrl = null;
                string idDocumentUrl = null;

                // Use environment variable for credentials
                var credential = GoogleCredential.FromFile(serviceAccountPath);
                var storageClient = await StorageClient.CreateAsync(credential);
                Console.WriteLine("Firebase Storage client created successfully");

                // Upload medical report
                if (medicalReport != null && medicalReport.Length > 0)
                {
                    Console.WriteLine($"Uploading medical report: {medicalReport.FileName} ({medicalReport.Length} bytes)");
                    var fileName = $"medicalReports/{Guid.NewGuid()}_{medicalReport.FileName}";

                    using var stream = medicalReport.OpenReadStream();

                    var obj = await storageClient.UploadObjectAsync(
                        bucketName,
                        fileName,
                        medicalReport.ContentType,
                        stream
                    );

                    medicalReportUrl = $"https://storage.googleapis.com/{bucketName}/{fileName}";
                    Console.WriteLine($"Medical report uploaded successfully: {medicalReportUrl}");
                }
                else
                {
                    Console.WriteLine("No medical report file provided");
                }

                // Upload ID document
                if (idDocument != null && idDocument.Length > 0)
                {
                    Console.WriteLine($"Uploading ID document: {idDocument.FileName} ({idDocument.Length} bytes)");
                    var fileName = $"idDocuments/{Guid.NewGuid()}_{idDocument.FileName}";

                    using var stream = idDocument.OpenReadStream();

                    var obj = await storageClient.UploadObjectAsync(
                        bucketName,
                        fileName,
                        idDocument.ContentType,
                        stream
                    );

                    idDocumentUrl = $"https://storage.googleapis.com/{bucketName}/{fileName}";
                    Console.WriteLine($"ID document uploaded successfully: {idDocumentUrl}");
                }
                else
                {
                    Console.WriteLine("No ID document file provided");
                }

                // Save application data to Firestore
                Console.WriteLine("Saving application data to Firestore");
                DocumentReference docRef = db.Collection("applicationForms").Document();
                Dictionary<string, object> NewApplication = new Dictionary<string, object>
                {
                    {"CaregiverName", application.CaregiverName ?? ""},
                    {"ContactNumber", application.ContactNumber ?? ""},
                    {"DisabilityType", application.DisabilityType ?? ""},
                    {"Email", application.Email ?? ""},
                    {"FamilyMemberName", application.FamilyMemberName ?? ""},
                    {"Notes", application.Notes ?? ""},
                    {"Occupation", application.Occupation ?? ""},
                    {"MedicalReportUrl", medicalReportUrl ?? ""}, // Firebase Storage URL
                    {"IdDocumentUrl", idDocumentUrl ?? ""}, // Firebase Storage URL
                    {"Status", "Pending"} // Default status
                };

                Console.WriteLine($"Document data prepared. Document ID: {docRef.Id}");
                await docRef.SetAsync(NewApplication);
                Console.WriteLine("Application saved to Firestore successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding application: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> AddVolunteerApplication(VolunteerApplication application, IFormFile resume)
        {
            try
            {
                string resumeUrl = null;

                // Use environment variable for credentials
                var credential = GoogleCredential.FromFile(serviceAccountPath);
                var storageClient = await StorageClient.CreateAsync(credential);
                Console.WriteLine("Firebase Storage client created successfully");

                // Upload medical report
                if (resume != null && resume.Length > 0)
                {
                    var fileName = $"resumes/{Guid.NewGuid()}_{resume.FileName}";

                    using var stream = resume.OpenReadStream();

                    var obj = await storageClient.UploadObjectAsync(
                        bucketName,
                        fileName,
                        resume.ContentType,
                        stream
                    );

                    resumeUrl = $"https://storage.googleapis.com/{bucketName}/{fileName}";
                    Console.WriteLine($"Resume uploaded successfully: {resumeUrl}");
                }
                else
                {
                    Console.WriteLine("No resume file provided");
                }

                // Save application data to Firestore
                Console.WriteLine("Saving application data to Firestore");
                DocumentReference docRef = db.Collection("volunteerApplicationForms").Document();
                Dictionary<string, object> NewApplication = new Dictionary<string, object>
                    {
                        {"Name", application.Name ?? ""},
                        {"ContactNumber", application.ContactNumber ?? ""},
                        {"DateOfBirth", application.DateOfBirth ?? ""},
                        {"Email", application.Email ?? ""},
                        {"Notes", application.Notes ?? ""},
                        {"Occupation", application.Occupation ?? ""},
                        {"ResumeUrl", resumeUrl ?? ""}, // Firebase Storage URL
                        {"Status", "Pending"} // Default status
                    };

                Console.WriteLine($"Document data prepared. Document ID: {docRef.Id}");
                await docRef.SetAsync(NewApplication);
                Console.WriteLine("Application saved to Firestore successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding application: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Get all applications from Firestore with signed URLs for file access
        /// </summary>
        public async Task<List<Application>> GetAllApplications()
        {
            List<Application> applicationList = new List<Application>();

            Query allApplicationsQuery = db.Collection("applicationForms");
            QuerySnapshot snapshot = await allApplicationsQuery.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Application data = document.ConvertTo<Application>();
                    data.Id = document.Id; // set Firestore document ID

                    // Generate signed URLs for file access
                    if (!string.IsNullOrEmpty(data.MedicalReportUrl))
                    {
                        data.MedicalReportUrl = await GenerateSignedUrl(data.MedicalReportUrl);
                    }

                    if (!string.IsNullOrEmpty(data.IdDocumentUrl))
                    {
                        data.IdDocumentUrl = await GenerateSignedUrl(data.IdDocumentUrl);
                    }

                    applicationList.Add(data);
                }
            }

            return applicationList;
        }

        public async Task<List<VolunteerApplication>> GetAllVolunteerApplications()
        {
            List<VolunteerApplication> applicationList = new List<VolunteerApplication>();

            Query allApplicationsQuery = db.Collection("volunteerApplicationForms");
            QuerySnapshot snapshot = await allApplicationsQuery.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    VolunteerApplication data = document.ConvertTo<VolunteerApplication>();
                    data.Id = document.Id; // set Firestore document ID

                    // Generate signed URLs for file access
                    if (!string.IsNullOrEmpty(data.ResumeUrl))
                    {
                        data.ResumeUrl = await GenerateSignedUrl(data.ResumeUrl);
                    }

                    applicationList.Add(data);
                }
            }

            return applicationList;
        }

        /// <summary>
        /// Generate a signed URL for accessing private Firebase Storage files
        /// </summary>
        private async Task<string> GenerateSignedUrl(string storageUrl)
        {
            try
            {
                // Extract file path from storage URL
                var uri = new Uri(storageUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var fileName = string.Join("/", pathSegments.Skip(2)); // Skip empty and bucket name

                var credential = GoogleCredential.FromFile(serviceAccountPath);
                var urlSigner = UrlSigner.FromCredential(credential);

                // Generate signed URL valid for 1 hour
                var signedUrl = await urlSigner.SignAsync(bucketName, fileName, TimeSpan.FromHours(1), HttpMethod.Get);
                return signedUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating signed URL: {ex.Message}");
                return storageUrl; // Return original URL as fallback
            }
        }

        /// <summary>
        /// Update application status
        /// </summary>
        public async Task<bool> UpdateApplicationStatus(string applicationId, string status)
        {
            try
            {
                DocumentReference docRef = db.Collection("applicationForms").Document(applicationId);
                Dictionary<string, object> updates = new Dictionary<string, object>
                        {
                            {"Status", status}
                        };

                await docRef.UpdateAsync(updates);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating application status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get application by email
        /// </summary>
        public async Task<Application> GetApplicationByEmail(string email)
        {
            try
            {
                Query query = db.Collection("applicationForms").WhereEqualTo("Email", email);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        Application data = document.ConvertTo<Application>();
                        data.Id = document.Id;
                        return data;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting application by email: {ex.Message}");
                return null;
            }
        }
    }
}