using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;


namespace debra_man.Controllers
{
    public class PagesController : Controller
    {
        private readonly string connectionString = "server=localhost;port=3306;database=abc;user=root;password=;";
       
        public async Task<IActionResult> UserHome()
        {
            var events = await GetTopEvents(6); 
            ViewBag.Events = events;
            return View();
        }

        private async Task<List<Event>> GetTopEvents(int count)
        {
            var events = new List<Event>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT EventID, EventName, EventDescription, EventDate, Venue, ImagePath FROM Event LIMIT @Count";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Count", count);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            events.Add(new Event
                            {
                                EventID = reader.GetInt32("EventID"),
                                EventName = reader.GetString("EventName"),
                                EventDescription = reader.GetString("EventDescription"),
                                EventDate = reader.GetDateTime("EventDate"),
                                Venue = reader.GetString("Venue"),
                                ImagePath = reader.GetString("ImagePath")
                            });
                        }
                    }
                }
            }
            return events;
        }




       
        [HttpGet]
        public async Task<IActionResult> AdminHome()
        {
            var totalEvents = await GetTotalEvents();
            var totalSales = await GetTotalSales();
            var totalUsers = await GetTotalUsers();

            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalUsers = totalUsers;

            return View();
        }

        public async Task<IActionResult> AddEvent()
        {
            var userId = GetLoggedInUserId();
            var events = await GetEventsByUserId(userId);
            ViewBag.Events = events;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddEvent(string eventName, string eventDescription, DateTime eventDate, string venue, [FromForm] IFormFile eventImage)
        {
            try
            {
                if (eventImage == null || eventImage.Length == 0)
                {
                    ModelState.AddModelError("eventImage", "Event image is required.");
                    return View();
                }

                var imagePath = await SaveEventImageAsync(eventImage);

                var userId = GetLoggedInUserId();

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "INSERT INTO Event (EventName, EventDescription, EventDate, Venue, ImagePath, UserID) " +
                                   "VALUES (@EventName, @EventDescription, @EventDate, @Venue, @ImagePath, @UserID)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventName", eventName);
                        cmd.Parameters.AddWithValue("@EventDescription", eventDescription);
                        cmd.Parameters.AddWithValue("@EventDate", eventDate);
                        cmd.Parameters.AddWithValue("@Venue", venue);
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AddEvent");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error adding event: {ex.Message}");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var eventItem = await GetEventById(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return View(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, string eventName, string eventDescription, DateTime eventDate, string venue, IFormFile eventImage)
        {
            try
            {
                var eventItem = await GetEventById(id);
                if (eventItem == null)
                {
                    return NotFound();
                }

                var imagePath = eventItem.ImagePath;
                if (eventImage != null && eventImage.Length > 0)
                {
                    imagePath = await SaveEventImageAsync(eventImage);
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    var query = "UPDATE Event SET EventName = @EventName, EventDescription = @EventDescription, EventDate = @EventDate, Venue = @Venue, ImagePath = @ImagePath WHERE EventID = @EventID";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventID", id);
                        cmd.Parameters.AddWithValue("@EventName", eventName);
                        cmd.Parameters.AddWithValue("@EventDescription", eventDescription);
                        cmd.Parameters.AddWithValue("@EventDate", eventDate);
                        cmd.Parameters.AddWithValue("@Venue", venue);
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AddEvent");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error editing event: {ex.Message}");
                return View(await GetEventById(id));
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM Event WHERE EventID = @EventID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventID", eventId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AddEvent");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting event: {ex.Message}");
                var events = await GetAllEvents();
                return View("AddEvent", events);
            }
        }


        private async Task<Event> GetEventById(int id)
        {
            Event eventItem = null;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT EventID, EventName, EventDescription, EventDate, Venue, ImagePath FROM Event WHERE EventID = @EventID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EventID", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            eventItem = new Event
                            {
                                EventID = reader.GetInt32("EventID"),
                                EventName = reader.GetString("EventName"),
                                EventDescription = reader.GetString("EventDescription"),
                                EventDate = reader.GetDateTime("EventDate"),
                                Venue = reader.GetString("Venue"),
                                ImagePath = reader.GetString("ImagePath")
                            };
                        }
                    }
                }
            }
            return eventItem;
        }


        private async Task<string> SaveEventImageAsync(IFormFile eventImage)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + eventImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await eventImage.CopyToAsync(fileStream);
            }

            return Path.Combine("images", uniqueFileName);
        }


        private int GetLoggedInUserId()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return userId.Value;
            }
            throw new Exception("User is not logged in");
        }

        [HttpGet] // GET method for displaying the form
        public async Task<IActionResult> SetTicketAsync()
        {
            var userId = GetLoggedInUserId();
            var events = await GetEventsByUserId(userId);
            var tickets = await GetTicketDetailsByUserId(userId);
            ViewBag.Events = events;
            ViewBag.Tickets = tickets;
            return View();
        }
        private async Task<List<TicketDetails>> GetTicketDetailsByUserId(int userId)
        {
            var tickets = new List<TicketDetails>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT t.TicketDetailsID, t.TicketType, t.TicketPrice, t.TicketQuantity, e.EventName " +
                               "FROM TicketDetails t " +
                               "JOIN Event e ON t.EventID = e.EventID " +
                               "WHERE e.UserID = @UserID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tickets.Add(new TicketDetails
                            {
                                TicketDetailsID = reader.GetInt32("TicketDetailsID"),
                                TicketType = reader.GetString("TicketType"),
                                TicketPrice = reader.GetDecimal("TicketPrice"),
                                TicketQuantity = reader.GetInt32("TicketQuantity"),
                                EventName = reader.GetString("EventName")
                            });
                        }
                    }
                }
            }
            return tickets;
        }

        [HttpGet]
        public async Task<IActionResult> EditTicket(int id)
        {
            var ticket = await GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }
            var events = await GetEventsByUserId(GetLoggedInUserId());
            ViewBag.Events = events;
            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> EditTicket(int id, int eventId, string ticketType, decimal ticketPrice, int ticketQuantity)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE TicketDetails SET EventID = @EventID, TicketType = @TicketType, TicketPrice = @TicketPrice, TicketQuantity = @TicketQuantity WHERE TicketDetailsID = @TicketDetailsID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TicketDetailsID", id);
                        cmd.Parameters.AddWithValue("@EventID", eventId);
                        cmd.Parameters.AddWithValue("@TicketType", ticketType);
                        cmd.Parameters.AddWithValue("@TicketPrice", ticketPrice);
                        cmd.Parameters.AddWithValue("@TicketQuantity", ticketQuantity);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("SetTicketAsync");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error editing ticket: {ex.Message}");
                return View();
            }
        }

        public async Task<IActionResult> DeleteTicket(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM TicketDetails WHERE TicketDetailsID = @TicketDetailsID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TicketDetailsID", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("SetTicketAsync");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting ticket: {ex.Message}");
                return View();
            }
        }

        private async Task<TicketDetails> GetTicketById(int id)
        {
            TicketDetails ticket = null;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT TicketDetailsID, EventID, TicketType, TicketPrice, TicketQuantity FROM TicketDetails WHERE TicketDetailsID = @TicketDetailsID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TicketDetailsID", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            ticket = new TicketDetails
                            {
                                TicketDetailsID = reader.GetInt32("TicketDetailsID"),
                                EventID = reader.GetInt32("EventID"),
                                TicketType = reader.GetString("TicketType"),
                                TicketPrice = reader.GetDecimal("TicketPrice"),
                                TicketQuantity = reader.GetInt32("TicketQuantity")
                            };
                        }
                    }
                }
            }
            return ticket;
        }

        [HttpPost]
        public async Task<IActionResult> SetTicket(int eventId, string ticketType, decimal ticketPrice, int ticketQuantity)
        {
            try
            {
                var userId = GetLoggedInUserId();

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "INSERT INTO TicketDetails (EventID, TicketType, TicketPrice, TicketQuantity) " +
                                   "VALUES (@EventID, @TicketType, @TicketPrice, @TicketQuantity)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventID", eventId);
                        cmd.Parameters.AddWithValue("@TicketType", ticketType);
                        cmd.Parameters.AddWithValue("@TicketPrice", ticketPrice);
                        cmd.Parameters.AddWithValue("@TicketQuantity", ticketQuantity);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("SetTicket");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error setting ticket: {ex.Message}");
                var events = await GetEventsByUserId(GetLoggedInUserId());
                var tickets = await GetTicketDetailsByUserId(GetLoggedInUserId());
                ViewBag.Events = events;
                ViewBag.Tickets = tickets;
                return View(await GetEventsByUserId(GetLoggedInUserId()));
            }
        }
        private async Task<List<Event>> GetEventsByUserId(int userId)
        {
            var events = new List<Event>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT EventID, EventName, EventDescription, EventDate, Venue, ImagePath FROM Event WHERE UserID = @UserID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            events.Add(new Event
                            {
                                EventID = reader.GetInt32("EventID"),
                                EventName = reader.GetString("EventName"),
                                EventDescription = reader.GetString("EventDescription"),
                                EventDate = reader.GetDateTime("EventDate"),
                                Venue = reader.GetString("Venue"),
                                ImagePath = reader.GetString("ImagePath")
                            });
                        }
                    }
                }
            }
            return events;
        }

        public async Task<IActionResult> EventsAsync()
        {
            List<Event> events = await GetEventsFromDatabase();

            // Pass the events to the view using ViewBag
            ViewBag.Events = events;

            return View(); 
        }
        private async Task<List<Event>> GetEventsFromDatabase(int count = 100)
        {
            var events = new List<Event>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT EventID, EventName, EventDescription, EventDate, Venue, ImagePath FROM Event LIMIT @Count";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Count", count);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            events.Add(new Event
                            {
                                EventID = reader.GetInt32("EventID"),
                                EventName = reader.GetString("EventName"),
                                EventDescription = reader.GetString("EventDescription"),
                                EventDate = reader.GetDateTime("EventDate"),
                                Venue = reader.GetString("Venue"),
                                ImagePath = reader.GetString("ImagePath")
                            });
                        }
                    }
                }
            }
            return events;
        }
        [HttpGet]
        public async Task<IActionResult> Tickets(int eventId)
        {
            var tickets = await GetTicketsByEventId(eventId);
            ViewBag.EventId = eventId;
            return View(tickets);
        }
        [HttpPost]
        public async Task<IActionResult> BuyTicket(int ticketDetailsId, int quantity)
        {
            var userId = GetLoggedInUserId();
            var ticketDetails = await GetTicketDetailsById(ticketDetailsId);

            if (ticketDetails == null || ticketDetails.TicketQuantity < quantity)
            {
                return BadRequest("Not enough tickets available.");
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // Start a transaction
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Insert into TicketSale
                        string insertQuery = "INSERT INTO TicketSale (UserID, EventID, TicketDetailsID, SaleDate, SaleStatus, SaleAmount) " +
                                             "VALUES (@UserID, @EventID, @TicketDetailsID, @SaleDate, @SaleStatus, @SaleAmount)";
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@EventID", ticketDetails.EventID);
                            cmd.Parameters.AddWithValue("@TicketDetailsID", ticketDetailsId);
                            cmd.Parameters.AddWithValue("@SaleDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SaleStatus", "Completed");
                            cmd.Parameters.AddWithValue("@SaleAmount", ticketDetails.TicketPrice * quantity);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Update TicketDetails
                        string updateQuery = "UPDATE TicketDetails SET TicketQuantity = TicketQuantity - @Quantity WHERE TicketDetailsID = @TicketDetailsID";
                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@TicketDetailsID", ticketDetailsId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Commit the transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        // Rollback the transaction in case of an error
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }

            return RedirectToAction("Tickets", new { eventId = ticketDetails.EventID });
        }

        private async Task<TicketDetails> GetTicketDetailsById(int ticketDetailsId)
        {
            TicketDetails ticketDetails = null;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT TicketDetailsID, EventID, TicketType, TicketPrice, TicketQuantity FROM TicketDetails WHERE TicketDetailsID = @TicketDetailsID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TicketDetailsID", ticketDetailsId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            ticketDetails = new TicketDetails
                            {
                                TicketDetailsID = reader.GetInt32("TicketDetailsID"),
                                EventID = reader.GetInt32("EventID"),
                                TicketType = reader.GetString("TicketType"),
                                TicketPrice = reader.GetDecimal("TicketPrice"),
                                TicketQuantity = reader.GetInt32("TicketQuantity")
                            };
                        }
                    }
                }
            }
            return ticketDetails;
        }

        private async Task<List<TicketDetails>> GetTicketsByEventId(int eventId)
        {
            var tickets = new List<TicketDetails>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT TicketDetailsID, EventID, TicketType, TicketPrice, TicketQuantity FROM TicketDetails WHERE EventID = @EventID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tickets.Add(new TicketDetails
                            {
                                TicketDetailsID = reader.GetInt32("TicketDetailsID"),
                                EventID = reader.GetInt32("EventID"),
                                TicketType = reader.GetString("TicketType"),
                                TicketPrice = reader.GetDecimal("TicketPrice"),
                                TicketQuantity = reader.GetInt32("TicketQuantity")
                            });
                        }
                    }
                }
            }
            return tickets;
        }

        [HttpGet]
        public async Task<IActionResult> BookedTickets()
        {
            var userId = GetLoggedInUserId();
            var bookedTickets = await GetBookedTicketsByUserId(userId);
            return View(bookedTickets);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBookedTicket(int ticketSaleId)
        {
            var userId = GetLoggedInUserId();
            var ticketSale = await GetTicketSaleById(ticketSaleId);

            if (ticketSale == null || ticketSale.UserID != userId)
            {
                return BadRequest("Invalid ticket sale ID or you do not have permission to delete this ticket.");
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // Start a transaction
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Delete from TicketSale
                        string deleteQuery = "DELETE FROM TicketSale WHERE TicketSaleID = @TicketSaleID";
                        using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@TicketSaleID", ticketSaleId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Update TicketDetails to increment the ticket quantity
                        string updateQuery = "UPDATE TicketDetails SET TicketQuantity = TicketQuantity + @Quantity WHERE TicketDetailsID = @TicketDetailsID";
                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Quantity", 1); // Assuming 1 ticket per sale, adjust if needed
                            cmd.Parameters.AddWithValue("@TicketDetailsID", ticketSale.TicketDetailsID);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Commit the transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        // Rollback the transaction in case of an error
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }

            return RedirectToAction("BookedTickets");
        }

        private async Task<List<TicketSale>> GetBookedTicketsByUserId(int userId)
        {
            var bookedTickets = new List<TicketSale>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT ts.TicketSaleID, ts.SaleDate, ts.SaleStatus, ts.SaleAmount, td.TicketType, e.EventName " +
                               "FROM TicketSale ts " +
                               "JOIN TicketDetails td ON ts.TicketDetailsID = td.TicketDetailsID " +
                               "JOIN Event e ON ts.EventID = e.EventID " +
                               "WHERE ts.UserID = @UserID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bookedTickets.Add(new TicketSale
                            {
                                TicketSaleID = reader.GetInt32("TicketSaleID"),
                                SaleDate = reader.GetDateTime("SaleDate"),
                                SaleStatus = reader.GetString("SaleStatus"),
                                SaleAmount = reader.GetDecimal("SaleAmount"),
                                TicketType = reader.GetString("TicketType"),
                                EventName = reader.GetString("EventName")
                            });
                        }
                    }
                }
            }
            return bookedTickets;
        }

        private async Task<TicketSale> GetTicketSaleById(int ticketSaleId)
        {
            TicketSale ticketSale = null;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT TicketSaleID, UserID, TicketDetailsID FROM TicketSale WHERE TicketSaleID = @TicketSaleID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TicketSaleID", ticketSaleId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            ticketSale = new TicketSale
                            {
                                TicketSaleID = reader.GetInt32("TicketSaleID"),
                                UserID = reader.GetInt32("UserID"),
                                TicketDetailsID = reader.GetInt32("TicketDetailsID")
                            };
                        }
                    }
                }
            }
            return ticketSale;
        }
        [HttpGet]
        public async Task<IActionResult> ViewTicketSales()
        {
            var userId = GetLoggedInUserId();
            var ticketSales = await GetTicketSalesByPartnerId(userId);
            return View(ticketSales);
        }
        private async Task<List<TicketSale>> GetTicketSalesByPartnerId(int partnerId)
        {
            var ticketSales = new List<TicketSale>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT ts.TicketSaleID, ts.SaleDate, ts.SaleStatus, ts.SaleAmount, td.TicketType, e.EventName " +
                               "FROM TicketSale ts " +
                               "JOIN TicketDetails td ON ts.TicketDetailsID = td.TicketDetailsID " +
                               "JOIN Event e ON ts.EventID = e.EventID " +
                               "WHERE e.UserID = @PartnerID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PartnerID", partnerId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ticketSales.Add(new TicketSale
                            {
                                TicketSaleID = reader.GetInt32("TicketSaleID"),
                                SaleDate = reader.GetDateTime("SaleDate"),
                                SaleStatus = reader.GetString("SaleStatus"),
                                SaleAmount = reader.GetDecimal("SaleAmount"),
                                TicketType = reader.GetString("TicketType"),
                                EventName = reader.GetString("EventName")
                            });
                        }
                    }
                }
            }
            return ticketSales;
        }
        [HttpGet]
        public async Task<IActionResult> Allevents()
        {
            var events = await GetAllEvents();
            return View(events); 
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent1(int eventId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM Event WHERE EventID = @EventID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventID", eventId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AllEvents");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting event: {ex.Message}");
                var events = await GetAllEvents();
                return View("AllEvents", events);
            }
        }

        private async Task<List<Event>> GetAllEvents()
        {
            var events = new List<Event>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT EventID, EventName, EventDescription, EventDate, Venue, ImagePath FROM Event";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            events.Add(new Event
                            {
                                EventID = reader.GetInt32("EventID"),
                                EventName = reader.GetString("EventName"),
                                EventDescription = reader.GetString("EventDescription"),
                                EventDate = reader.GetDateTime("EventDate"),
                                Venue = reader.GetString("Venue"),
                                ImagePath = reader.GetString("ImagePath")
                            });
                        }
                    }
                }
            }
            return events;
        }
        [HttpGet]
        public async Task<IActionResult> AllTicket()
        {
            var ticketDetails = await GetAllTicketDetails();
            return View(ticketDetails);
        } 

        [HttpPost]
        public async Task<IActionResult> DeleteTicket1(int ticketDetailsId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM TicketDetails WHERE TicketDetailsID = @TicketDetailsID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TicketDetailsID", ticketDetailsId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AllTicket");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting ticket: {ex.Message}");
                var ticketDetails = await GetAllTicketDetails();
                return View("AllTicket", ticketDetails);
            }
        }

        private async Task<List<TicketDetails>> GetAllTicketDetails()
        {
            var tickets = new List<TicketDetails>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT td.TicketDetailsID, td.EventID, td.TicketType, td.TicketPrice, td.TicketQuantity, e.EventName, e.ImagePath " +
                               "FROM TicketDetails td " +
                               "JOIN Event e ON td.EventID = e.EventID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tickets.Add(new TicketDetails
                            {
                                TicketDetailsID = reader.GetInt32("TicketDetailsID"),
                                EventID = reader.GetInt32("EventID"),
                                TicketType = reader.GetString("TicketType"),
                                TicketPrice = reader.GetDecimal("TicketPrice"),
                                TicketQuantity = reader.GetInt32("TicketQuantity"),
                                EventName = reader.GetString("EventName"),
                            });
                        }
                    }
                }
            }
            return tickets;
        }
        [HttpGet]
        public async Task<IActionResult> AllSale()
        {
            var ticketSales = await GetAllTicketSales();
            var totalSalesAmount = ticketSales.Sum(sale => sale.SaleAmount);
            ViewBag.TotalSalesAmount = totalSalesAmount;
            return View(ticketSales);
        }

        private async Task<List<TicketSale>> GetAllTicketSales()
        { 
            var sales = new List<TicketSale>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT ts.TicketSaleID, ts.SaleDate, ts.SaleStatus, ts.SaleAmount, td.TicketType, e.EventName " +
                               "FROM TicketSale ts " +
                               "JOIN TicketDetails td ON ts.TicketDetailsID = td.TicketDetailsID " +
                               "JOIN Event e ON ts.EventID = e.EventID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sales.Add(new TicketSale
                            {
                                TicketSaleID = reader.GetInt32("TicketSaleID"),
                                SaleDate = reader.GetDateTime("SaleDate"),
                                SaleStatus = reader.GetString("SaleStatus"),
                                SaleAmount = reader.GetDecimal("SaleAmount"),
                                TicketType = reader.GetString("TicketType"),
                                EventName = reader.GetString("EventName")
                            });
                        }
                    }
                }
            }
            return sales;
        }


        [HttpGet]
        public async Task<IActionResult> AddCommission()
        {
            var userId = GetLoggedInUserId();
            var events = await GetEventsByUserId(userId);
            var commissions = await GetCommissionsByUserId(userId);
            ViewBag.Events = events;
            return View(commissions);
        }

        [HttpPost]
        public async Task<IActionResult> AddCommission(int eventId, decimal commissionRate)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "INSERT INTO Commission (EventID, CommissionRate) VALUES (@EventID, @CommissionRate)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventID", eventId);
                        cmd.Parameters.AddWithValue("@CommissionRate", commissionRate);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AddCommission");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error adding commission: {ex.Message}");
                var userId = GetLoggedInUserId();
                var events = await GetEventsByUserId(userId);
                var commissions = await GetCommissionsByUserId(userId);
                ViewBag.Events = events;
                return View(commissions);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditCommission(int commissionId, decimal commissionRate)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE Commission SET CommissionRate = @CommissionRate WHERE CommissionID = @CommissionID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CommissionID", commissionId);
                        cmd.Parameters.AddWithValue("@CommissionRate", commissionRate);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AddCommission");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error editing commission: {ex.Message}");
                var userId = GetLoggedInUserId();
                var events = await GetEventsByUserId(userId);
                var commissions = await GetCommissionsByUserId(userId);
                ViewBag.Events = events;
                return View(commissions);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCommission(int commissionId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM Commission WHERE CommissionID = @CommissionID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CommissionID", commissionId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("AddCommission");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting commission: {ex.Message}");
                var userId = GetLoggedInUserId();
                var events = await GetEventsByUserId(userId);
                var commissions = await GetCommissionsByUserId(userId);
                ViewBag.Events = events;
                return View(commissions);
            }
        }
        private async Task<List<Commission>> GetCommissionsByUserId(int userId)
        {
            var commissions = new List<Commission>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT c.CommissionID, c.EventID, c.CommissionRate, e.EventName " +
                               "FROM Commission c " +
                               "JOIN Event e ON c.EventID = e.EventID " +
                               "WHERE e.UserID = @UserID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            commissions.Add(new Commission
                            {
                                CommissionID = reader.GetInt32("CommissionID"),
                                EventID = reader.GetInt32("EventID"),
                                CommissionRate = reader.GetDecimal("CommissionRate"),
                                EventName = reader.GetString("EventName")
                            });
                        }
                    }
                }
            }
            return commissions;
        }

        [HttpGet]
        public async Task<IActionResult> AdminCommission()
        {
            var commissions = await GetCommissionsWithTotal();
            var totalCommission = commissions.Sum(c => c.TotalCommissionAmount);
            ViewBag.TotalCommission = totalCommission;
            return View(commissions);
        }

        private async Task<List<CommissionSummary>> GetCommissionsWithTotal()
        {
            var commissionSummaries = new List<CommissionSummary>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT e.EventID, e.EventName, c.CommissionRate, " +
                               "(SELECT SUM(ts.SaleAmount) FROM TicketSale ts WHERE ts.EventID = e.EventID) AS TotalSales " +
                               "FROM Event e " +
                               "JOIN Commission c ON e.EventID = c.EventID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var totalSales = reader.IsDBNull(reader.GetOrdinal("TotalSales")) ? 0 : reader.GetDecimal("TotalSales");
                            var commissionRate = reader.GetDecimal("CommissionRate");
                            var totalCommissionAmount = totalSales * (commissionRate / 100);

                            commissionSummaries.Add(new CommissionSummary
                            {
                                EventID = reader.GetInt32("EventID"),
                                EventName = reader.GetString("EventName"),
                                CommissionRate = commissionRate,
                                TotalSales = totalSales,
                                TotalCommissionAmount = totalCommissionAmount
                            });
                        }
                    }
                }
            }
            return commissionSummaries;
        }



        private async Task<int> GetTotalEvents()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT COUNT(*) FROM Event";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        private async Task<decimal> GetTotalSales()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT SUM(SaleAmount) FROM TicketSale";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    return Convert.ToDecimal(await cmd.ExecuteScalarAsync());
                }
            }
        }

        private async Task<int> GetTotalUsers()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT COUNT(*) FROM User1"; // Replace with your actual user table
                using (var cmd = new MySqlCommand(query, conn))
                {
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> PartnerHome()
        {
            var userId = GetLoggedInUserId();
            var totalEvents = await GetTotalEventsByUserId(userId);
            var totalSales = await GetTotalSalesByUserId(userId);
            var totalUsers = await GetTotalUsers();

            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalUsers = totalUsers;

            return View();
        }

        private async Task<int> GetTotalEventsByUserId(int userId)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT COUNT(*) FROM Event WHERE UserID = @UserID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        private async Task<decimal> GetTotalSalesByUserId(int userId)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT SUM(ts.SaleAmount) FROM TicketSale ts JOIN Event e ON ts.EventID = e.EventID WHERE e.UserID = @UserID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == DBNull.Value)
                    {
                        return 0; // Return 0 if there are no sales
                    }
                    return Convert.ToDecimal(result);
                }
            }
        }

        



    }
    public class CommissionSummary
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCommissionAmount { get; set; }
    }



    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public string ImagePath { get; set; }
        public int UserID { get; set; }
    }

    public class TicketDetails
    {
        public int TicketDetailsID { get; set; }
        public string TicketType { get; set; }
        public decimal TicketPrice { get; set; }
        public int TicketQuantity { get; set; }
        public string EventName { get; set; }
        public int EventID { get; set; }
    }
    public class TicketSale
    {
        public int TicketSaleID { get; set; }
        public int UserID { get; set; }
        public int EventID { get; set; }
        public int TicketDetailsID { get; set; }
        public DateTime SaleDate { get; set; }
        public string SaleStatus { get; set; }
        public decimal SaleAmount { get; set; }
        public string TicketType { get; set; }
        public string EventName { get; set; }
    }
    public class Commission
    {
        public int CommissionID { get; set; }
        public int EventID { get; set; }
        public decimal CommissionRate { get; set; }
        public string EventName { get; set; }
    }

}