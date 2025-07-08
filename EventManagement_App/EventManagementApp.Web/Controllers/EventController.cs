using EventManagementApp.Business.Interfaces;
using EventManagementApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EventManagementApp.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IEventTypeService _eventTypeService;

        public EventController(IEventService eventService, IEventTypeService eventTypeService)
        {
            _eventService = eventService;
            _eventTypeService = eventTypeService;
        }

        private async Task LoadEventTypesToViewBagAsync(int? selectedId = null)
        {
            try
            {
                var eventTypes = await _eventTypeService.GetAllEventTypesAsync();
                var activeTypes = eventTypes?.Where(et => et.IsActive).ToList() ?? new List<EventType>();
                ViewBag.EventTypes = new SelectList(activeTypes, "Id", "Name", selectedId);
            }
            catch (Exception)
            {
                ViewBag.EventTypes = new SelectList(new List<EventType>(), "Id", "Name");
            }
        }

        public async Task<ActionResult> Index(string searchTitle, int? eventTypeId, string sortBy = "date")
        {
            try
            {
                IEnumerable<Event> events;

                if (!string.IsNullOrWhiteSpace(searchTitle))
                {
                    events = await _eventService.SearchEventsByTitleAsync(searchTitle);
                }
                else if (eventTypeId.HasValue && eventTypeId.Value > 0)
                {
                    events = await _eventService.GetEventsByTypeAsync(eventTypeId.Value);
                }
                else
                {
                    events = await _eventService.GetAllEventsAsync();
                }

                var eventList = events?.Where(e => e.IsActive).ToList() ?? new List<Event>();

                switch (sortBy?.ToLower())
                {
                    case "date_desc":
                        eventList = eventList.OrderByDescending(e => e.EventDate).ToList();
                        break;
                    case "name":
                        eventList = eventList.OrderBy(e => e.Title).ToList();
                        break;
                    case "name_desc":
                        eventList = eventList.OrderByDescending(e => e.Title).ToList();
                        break;
                    default:
                        eventList = eventList.OrderBy(e => e.EventDate).ToList();
                        break;
                }

                await LoadEventTypesToViewBagAsync();
                return View(eventList);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Etkinlikler yüklenirken hata oluştu: " + ex.Message;
                await LoadEventTypesToViewBagAsync();
                return View(new List<Event>());
            }
        }

        public async Task<ActionResult> Create()
        {
            await LoadEventTypesToViewBagAsync();

            return View(new Event
            {
                EventDate = DateTime.Now.AddDays(1).Date.AddHours(9), 
                CreatedDate = DateTime.Now,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Event eventEntity)
        {
            if (string.IsNullOrWhiteSpace(eventEntity.Title))
            {
                ModelState.AddModelError("Title", "Etkinlik başlığı boş bırakılamaz.");
            }
            else if (eventEntity.Title.Trim().Length < 3)
            {
                ModelState.AddModelError("Title", "Etkinlik başlığı en az 3 karakter olmalıdır.");
            }
            else if (eventEntity.Title.Trim().Length > 200)
            {
                ModelState.AddModelError("Title", "Etkinlik başlığı maksimum 200 karakter olabilir.");
            }

            if (eventEntity.EventDate == default(DateTime))
            {
                ModelState.AddModelError("EventDate", "Etkinlik tarihi boş bırakılamaz.");
            }
            else if (eventEntity.EventDate <= DateTime.Now)
            {
                ModelState.AddModelError("EventDate", "Etkinlik tarihi gelecekte olmalıdır.");
            }

            if (string.IsNullOrWhiteSpace(eventEntity.Location))
            {
                ModelState.AddModelError("Location", "Konum boş bırakılamaz.");
            }
            else if (eventEntity.Location.Trim().Length > 200)
            {
                ModelState.AddModelError("Location", "Konum maksimum 200 karakter olabilir.");
            }

            if (eventEntity.EventTypeId <= 0)
            {
                ModelState.AddModelError("EventTypeId", "Etkinlik türü seçilmelidir.");
            }

            if (!string.IsNullOrWhiteSpace(eventEntity.Description) && eventEntity.Description.Length > 1000)
            {
                ModelState.AddModelError("Description", "Açıklama maksimum 1000 karakter olabilir.");
            }

            if (!ModelState.IsValid)
            {
                await LoadEventTypesToViewBagAsync(eventEntity.EventTypeId);
                return View(eventEntity);
            }

            try
            {
                eventEntity.Title = eventEntity.Title.Trim();
                eventEntity.Description = !string.IsNullOrWhiteSpace(eventEntity.Description)
                    ? eventEntity.Description.Trim()
                    : null;
                eventEntity.Location = eventEntity.Location.Trim();
                eventEntity.CreatedDate = DateTime.Now;
                eventEntity.UpdatedDate = null;
                eventEntity.IsActive = true;

                var createdEvent = await _eventService.CreateEventAsync(eventEntity);

                TempData["SuccessMessage"] = "Etkinlik başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Etkinlik kaydedilirken bir hata oluştu: " + ex.Message);
                await LoadEventTypesToViewBagAsync(eventEntity.EventTypeId);
                return View(eventEntity);
            }
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                var eventEntity = await _eventService.GetEventByIdAsync(id.Value);

                if (eventEntity == null || !eventEntity.IsActive)
                {
                    return HttpNotFound();
                }

                return View(eventEntity);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Etkinlik detayları yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                var eventEntity = await _eventService.GetEventByIdAsync(id.Value);

                if (eventEntity == null || !eventEntity.IsActive)
                {
                    return HttpNotFound();
                }

                await LoadEventTypesToViewBagAsync(eventEntity.EventTypeId);
                return View(eventEntity);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Etkinlik bilgileri yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Event eventEntity)
        {
            if (string.IsNullOrWhiteSpace(eventEntity.Title))
            {
                ModelState.AddModelError("Title", "Etkinlik başlığı boş bırakılamaz.");
            }
            else if (eventEntity.Title.Trim().Length < 3)
            {
                ModelState.AddModelError("Title", "Etkinlik başlığı en az 3 karakter olmalıdır.");
            }
            else if (eventEntity.Title.Trim().Length > 200)
            {
                ModelState.AddModelError("Title", "Etkinlik başlığı maksimum 200 karakter olabilir.");
            }

            if (eventEntity.EventDate == default(DateTime))
            {
                ModelState.AddModelError("EventDate", "Etkinlik tarihi boş bırakılamaz.");
            }
            else if (eventEntity.EventDate <= DateTime.Now)
            {
                ModelState.AddModelError("EventDate", "Etkinlik tarihi gelecekte olmalıdır.");
            }

            if (string.IsNullOrWhiteSpace(eventEntity.Location))
            {
                ModelState.AddModelError("Location", "Konum boş bırakılamaz.");
            }
            else if (eventEntity.Location.Trim().Length > 200)
            {
                ModelState.AddModelError("Location", "Konum maksimum 200 karakter olabilir.");
            }

            if (eventEntity.EventTypeId <= 0)
            {
                ModelState.AddModelError("EventTypeId", "Etkinlik türü seçilmelidir.");
            }

            if (!string.IsNullOrWhiteSpace(eventEntity.Description) && eventEntity.Description.Length > 1000)
            {
                ModelState.AddModelError("Description", "Açıklama maksimum 1000 karakter olabilir.");
            }

            if (!ModelState.IsValid)
            {
                await LoadEventTypesToViewBagAsync(eventEntity.EventTypeId);
                return View(eventEntity);
            }

            try
            {
                eventEntity.Title = eventEntity.Title.Trim();
                eventEntity.Description = !string.IsNullOrWhiteSpace(eventEntity.Description)
                    ? eventEntity.Description.Trim()
                    : null;
                eventEntity.Location = eventEntity.Location.Trim();
                eventEntity.UpdatedDate = DateTime.Now;

                await _eventService.UpdateEventAsync(eventEntity);

                TempData["SuccessMessage"] = "Etkinlik başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Etkinlik güncellenirken bir hata oluştu: " + ex.Message);
                await LoadEventTypesToViewBagAsync(eventEntity.EventTypeId);
                return View(eventEntity);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _eventService.DeleteEventAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Etkinlik başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Silinecek etkinlik bulunamadı.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Etkinlik silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<string> TestLog()
        {
            try
            {
                var events = await _eventService.GetAllEventsAsync();

                var logPath = Server.MapPath("~/App_Data/Logs/");
                return $"✅ Test başarılı! Event sayısı: {events?.Count() ?? 0}, Log klasörü: {logPath}";
            }
            catch (Exception ex)
            {
                return $"❌ Test hatası: {ex.Message}";
            }
        }
    }
}