using EventManagementApp.Data.Context;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EventManagementApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                using (var context = new EventManagementContext())
                {
                    var today = DateTime.Today;

                    var totalEvents = context.Events.Count();
                    System.Diagnostics.Debug.WriteLine($"Toplam etkinlik sayısı: {totalEvents}");

                    var upcomingEvents = context.Events
                        .Include(e => e.EventType) 
                        .Where(e => e.EventDate >= today && e.IsActive == true)
                        .OrderBy(e => e.EventDate)
                        .Take(4)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Yaklaşan etkinlik sayısı: {upcomingEvents.Count}");

                    if (upcomingEvents.Count < 4)
                    {
                        var remainingCount = 4 - upcomingEvents.Count;
                        var pastEvents = context.Events
                            .Include(e => e.EventType) 
                            .Where(e => e.EventDate < today && e.IsActive == true)
                            .OrderByDescending(e => e.EventDate)
                            .Take(remainingCount)
                            .ToList();

                        upcomingEvents.AddRange(pastEvents);
                        System.Diagnostics.Debug.WriteLine($"Geçmiş etkinlik eklendi. Toplam: {upcomingEvents.Count}");
                    }

                    var eventTypes = context.EventTypes
                        .Where(et => et.IsActive == true)
                        .OrderBy(et => et.Name)
                        .ToList();

                    ViewBag.UpcomingEvents = upcomingEvents.Any() ? upcomingEvents : null;
                    ViewBag.EventTypes = eventTypes.Any() ? eventTypes : null;

                    System.Diagnostics.Debug.WriteLine($"ViewBag'e aktarılan etkinlik sayısı: {upcomingEvents.Count}");
                    System.Diagnostics.Debug.WriteLine($"ViewBag'e aktarılan tür sayısı: {eventTypes.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HATA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                ViewBag.ErrorMessage = "Etkinlikler yüklenirken hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                ViewBag.UpcomingEvents = null;
                ViewBag.EventTypes = null;
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Etkinlik Yönetim Sistemi Hakkında";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "İletişim Sayfası";
            return View();
        }
    }
}