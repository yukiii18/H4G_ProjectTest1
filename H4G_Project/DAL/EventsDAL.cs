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

        // 🔹 Get all event registrations (FOR REPORTS)
        public async Task<List<EventRegistration>> GetAllRegistrations()
        {
            CollectionReference regRef = db.Collection("eventRegistrations");
            QuerySnapshot snapshot = await regRef.GetSnapshotAsync();

            List<EventRegistration> registrations = new();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    EventRegistration reg = doc.ConvertTo<EventRegistration>();
                    reg.Id = doc.Id;
                    registrations.Add(reg);
                }
            }

            return registrations;
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

        public async Task<List<Event>> GetEventsByUserEmail()
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

        public async Task<List<Event>> GetEventsByUserEmail(string userEmail)
        {
            // 1. Get all registrations for this user
            CollectionReference regRef = db.Collection("eventRegistrations");
            QuerySnapshot regSnapshot = await regRef.WhereEqualTo("email", userEmail).GetSnapshotAsync();

            List<string> registeredEventNames = new();
            foreach (var doc in regSnapshot.Documents)
            {
                if (doc.Exists)
                {
                    var data = doc.ToDictionary();
                    if (data.ContainsKey("eventName"))
                        registeredEventNames.Add(data["eventName"].ToString());
                }
            }

            // 2. Get all events
            CollectionReference eventsRef = db.Collection("events");
            QuerySnapshot eventsSnapshot = await eventsRef.GetSnapshotAsync();

            List<Event> events = new();
            foreach (var doc in eventsSnapshot.Documents)
            {
                if (doc.Exists)
                {
                    Event ev = doc.ConvertTo<Event>();
                    ev.Id = doc.Id;

                    // Only include events the user registered for
                    if (registeredEventNames.Contains(ev.Name))
                        events.Add(ev);
                }
            }

            // Sort by Start date ascending
            events.Sort((a, b) => a.Start.ToDateTime().CompareTo(b.Start.ToDateTime()));

            return events;
        }


        /*
        public async Task<Event> ExtractEventByID(string eid)
        {
            CollectionReference eventRef = db.Collection("events");
            QuerySnapshot snapshot = await eventRef.GetSnapshotAsync(); //Once connected to the database, this calls out specifally for the documents inside the Events collection
            Event @event = new Event();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Id == eid)
                {
                    int eventId;
                    if (int.TryParse(document.Id, out eventId))
                    {
                        @event.eventID = eventId;

                    }

                    Dictionary<string, dynamic> documentDictionary = document.ToDictionary();

                    @event.name = documentDictionary["name"].ToString();
                    @event.Details = documentDictionary["details"].ToString();
                    @event.eventPhoto = documentDictionary["eventPhoto"].ToString();

                }
            }
        }*/

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

        // For Attendance
        public async Task<List<EventRegistration>> GetRegistrationsByEventId(string eventId)
        {
            var snapshot = await db.Collection("eventRegistrations")
                                   .WhereEqualTo("eventId", eventId)
                                   .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<EventRegistration>()).ToList();
        }

        public async Task<Event?> GetEventById(string id)
        {
            var doc = await db.Collection("events").Document(id).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<Event>() : null;
        }

        public async Task<bool> UpdateRegistration(EventRegistration registration)
        {
            try
            {
                var docRef = db.Collection("eventRegistrations").Document(registration.Id);
                var updates = new Dictionary<string, object>
                {
                    { "attendance", registration.Attendance }
                };
                await docRef.UpdateAsync(updates);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating registration: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateEventQrCode(string eventId, string qrCode)
        {
            try
            {
                DocumentReference docRef = db.Collection("events").Document(eventId);

                // Use SetAsync with merge option instead of UpdateAsync
                await docRef.SetAsync(new { qrCode = qrCode }, SetOptions.MergeAll);

                return true;
            }
            catch (Exception ex)
            {
                return false;
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

        // 🔹 Count confirmed registrations for an event
        public async Task<int> CountConfirmedRegistrations(string eventId)
        {
            try
            {
                QuerySnapshot snapshot = await db.Collection("eventRegistrations")
                                                 .WhereEqualTo("eventId", eventId)
                                                 .WhereEqualTo("waitlistStatus", "Confirmed")
                                                 .GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting confirmed registrations: {ex.Message}");
                return 0;
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


        // Add a comment OR reply to an event
        public async Task<bool> AddComment(string eventId, string username, string email, string comment, string role, string parentId)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    { "username", username },
                    { "email", email },
                    { "comment", comment },
                    { "role", role },
                    { "parentId", string.IsNullOrEmpty(parentId) ? null : parentId },
                    { "timestamp", Timestamp.FromDateTime(DateTime.UtcNow) }
                };

                await db.Collection("events")
                        .Document(eventId)
                        .Collection("comments")
                        .AddAsync(data);

                return true;
            }
            catch
            {
                return false;
            }
        }




        // Get comments for an event (role is stored in the comment document)
        public async Task<List<(string username, string role, string comment)>> GetComments(string eventId)
        {
            var comments = new List<(string username, string role, string comment)>();

            try
            {
                var snapshot = await db.Collection("events")
                                       .Document(eventId)
                                       .Collection("comments")
                                       .OrderBy("timestamp")
                                       .GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var data = doc.ToDictionary();
                        string username = data.ContainsKey("username") ? data["username"].ToString() : "Anonymous";
                        string comment = data.ContainsKey("comment") ? data["comment"].ToString() : "";
                        string role = data.ContainsKey("role") ? data["role"].ToString() : "";

                        comments.Add((username, role, comment));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting comments: {ex.Message}");
            }

            return comments;
        }


        public async Task<List<CommentVM>> GetThreadedComments(string eventId)
        {
            var list = new List<CommentVM>();

            var snapshot = await db.Collection("events")
                                   .Document(eventId)
                                   .Collection("comments")
                                   .OrderBy("timestamp")
                                   .GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var d = doc.ToDictionary();

                list.Add(new CommentVM
                {
                    Id = doc.Id,
                    Username = d["username"].ToString(),
                    Role = d["role"].ToString(),
                    Comment = d["comment"].ToString(),
                    ParentCommentId = d.ContainsKey("parentId")
                        ? d["parentId"]?.ToString()
                        : ""
                });
            }

            return list;
        }


        // Get threaded comment tree
        public async Task<List<CommentVM>> GetCommentTree(string eventId)
        {
            var snapshot = await db.Collection("events")
                                   .Document(eventId)
                                   .Collection("comments")
                                   .OrderBy("timestamp")
                                   .GetSnapshotAsync();

            var all = new Dictionary<string, CommentVM>();

            foreach (var doc in snapshot.Documents)
            {
                var d = doc.ToDictionary();

                // Safe extraction of fields
                string username = d.ContainsKey("username") ? d["username"]?.ToString() ?? "Unknown" : "Unknown";
                string role = d.ContainsKey("role") ? d["role"]?.ToString() ?? "Unknown" : "Unknown";
                string commentText = d.ContainsKey("comment") ? d["comment"]?.ToString() ?? "" : "";
                string parentId = d.ContainsKey("parentId") ? d["parentId"]?.ToString() : null;
                Timestamp timestamp = d.ContainsKey("timestamp") && d["timestamp"] is Timestamp ts
                                      ? ts
                                      : Timestamp.FromDateTime(DateTime.UtcNow);

                all[doc.Id] = new CommentVM
                {
                    Id = doc.Id,
                    Username = username,
                    Role = role,
                    Comment = commentText,
                    ParentCommentId = parentId,
                    Timestamp = timestamp
                };
            }

            // Build threaded comment tree
            var roots = new List<CommentVM>();

            foreach (var c in all.Values)
            {
                if (!string.IsNullOrEmpty(c.ParentCommentId) && all.ContainsKey(c.ParentCommentId))
                {
                    all[c.ParentCommentId].Replies.Add(c);
                }
                else
                {
                    roots.Add(c);
                }
            }

            return roots;
        }




    }
}
