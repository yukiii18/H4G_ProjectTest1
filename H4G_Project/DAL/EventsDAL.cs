using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using H4G_Project.Models;

namespace H4G_Project.DAL
{
    public class EventsDAL
    {
        private readonly FirestoreDb db;

        public EventsDAL()
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

        // 🔹 Get all events (for FullCalendar)
        public async Task<List<Event>> GetAllEvents()
        {
            CollectionReference eventsRef = db.Collection("events");
            QuerySnapshot snapshot = await eventsRef.GetSnapshotAsync();

            List<Event> events = new List<Event>();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    Event ev = doc.ConvertTo<Event>();
                    ev.Id = doc.Id;
                    events.Add(ev);
                }
            }

            return events;
        }

        // 🔹 Add event
        public async Task<bool> AddEvent(Event ev)
        {
            try
            {
                await db.Collection("events").AddAsync(ev);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding event: {ex.Message}");
                return false;
            }
        }


        // 🔹 Update event
        public async Task<bool> UpdateEvent(Event ev)
        {
            try
            {
                DocumentReference docRef = db.Collection("events").Document(ev.Id);

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "name", ev.Name },
                    { "start", ev.Start },
                    { "end", ev.End }
                };

                await docRef.UpdateAsync(updates);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating event: {ex.Message}");
                return false;
            }
        }

        // 🔹 Delete event
        public async Task<bool> DeleteEvent(string eventId)
        {
            try
            {
                DocumentReference docRef = db.Collection("events").Document(eventId);
                await docRef.DeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting event: {ex.Message}");
                return false;
            }
        }

        // 🔹 Add event registration
        public async Task<string> AddRegistration(EventRegistration registration)
        {
            try
            {
                DocumentReference docRef = await db.Collection("eventRegistrations").AddAsync(registration);
                return docRef.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding registration: {ex.Message}");
                return null;
            }
        }

        // 🔹 Get registration by ID
        public async Task<EventRegistration> GetRegistrationById(string registrationId)
        {
            try
            {
                DocumentReference docRef = db.Collection("eventRegistrations").Document(registrationId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                
                if (snapshot.Exists)
                {
                    EventRegistration registration = snapshot.ConvertTo<EventRegistration>();
                    registration.Id = snapshot.Id;
                    return registration;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting registration: {ex.Message}");
                return null;
            }
        }

        // 🔹 Update payment status
        public async Task<bool> UpdatePaymentStatus(string registrationId, string paymentStatus, string qrCode = null)
        {
            try
            {
                DocumentReference docRef = db.Collection("eventRegistrations").Document(registrationId);
                
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "paymentStatus", paymentStatus },
                    { "paymentDate", Timestamp.FromDateTime(DateTime.UtcNow) }
                };

                if (!string.IsNullOrEmpty(qrCode))
                {
                    updates["qrCode"] = qrCode;
                }

                await docRef.UpdateAsync(updates);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating payment status: {ex.Message}");
                return false;
            }
        }
    }
}
