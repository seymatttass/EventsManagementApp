using EventManagementApp.Business.Interfaces;
using EventManagementApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EventManagementApp.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IParticipantService _participantService;
        private readonly IEventService _eventService;

        public ParticipantController(IParticipantService participantService, IEventService eventService)
        {
            _participantService = participantService;
            _eventService = eventService;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                var participants = await _participantService.GetAllParticipantsAsync();

                var activeParticipants = participants?.Where(p => p.IsActive == true);

                return View(activeParticipants);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Katılımcılar yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Participant participant)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    participant.IsActive = true;
                    participant.CreatedDate = DateTime.Now;

                    await _participantService.CreateParticipantAsync(participant);
                    TempData["SuccessMessage"] = "Katılımcı başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Katılımcı oluşturulurken hata oluştu: " + ex.Message;
            }

            return View(participant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var participant = await _participantService.GetParticipantByIdAsync(id);
                if (participant == null)
                {
                    TempData["ErrorMessage"] = "Silinecek katılımcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                participant.IsActive = false;
                await _participantService.UpdateParticipantAsync(participant);

                var participantName = $"{participant.FirstName} {participant.LastName}";
                TempData["SuccessMessage"] = $"'{participantName}' katılımcısı başarıyla silindi.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Katılımcı silinirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<JsonResult> GetParticipantEvents(int id)
        {
            try
            {
                var participant = await _participantService.GetParticipantByIdAsync(id);
                if (participant == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Katılımcı bulunamadı"
                    }, JsonRequestBehavior.AllowGet);
                }

                var participantEvents = await _participantService.GetParticipantEventsAsync(id);

                if (participantEvents == null || !participantEvents.Any())
                {
                    return Json(new
                    {
                        success = true,
                        events = new object[0],
                        participantName = $"{participant.FirstName} {participant.LastName}",
                        message = "Bu katılımcının etkinlik kaydı bulunmuyor."
                    }, JsonRequestBehavior.AllowGet);
                }

                var events = participantEvents.Select(e => new {
                    Id = e.Id,
                    Title = e.Title ?? "Başlık Yok",
                    Description = e.Description ?? "Açıklama Yok",
                    EventDate = e.EventDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Location = e.Location ?? "Konum Belirtilmemiş",
                    IsActive = e.IsActive
                }).OrderBy(e => e.EventDate).ToList();

                return Json(new
                {
                    success = true,
                    events = events,
                    participantName = $"{participant.FirstName} {participant.LastName}"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Hata oluştu: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetAvailableEvents(int participantId)
        {
            try
            {
                var participantEvents = await _participantService.GetParticipantEventsAsync(participantId);
                var participantEventIds = participantEvents?.Select(e => e.Id).ToList() ?? new List<int>();

                var allEvents = await _eventService.GetAllEventsAsync();
                var availableEvents = allEvents?
                    .Where(e => e.IsActive == true && !participantEventIds.Contains(e.Id))
                    .Select(e => new {
                        Id = e.Id,
                        Title = e.Title ?? "Başlık Yok",
                        Description = e.Description ?? "Açıklama Yok",
                        EventDate = e.EventDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Location = e.Location ?? "Konum Belirtilmemiş",
                        IsActive = e.IsActive
                    })
                    .OrderBy(e => e.EventDate)
                    .ToList();

                return Json(new
                {
                    success = true,
                    events = availableEvents,
                    message = availableEvents?.Any() == true ? "" : "Bu katılımcının eklenebileceği etkinlik bulunmuyor."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Etkinlikler yüklenirken hata oluştu: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddToMultipleEvents(int participantId, List<int> eventIds)
        {
            try
            {
                if (eventIds == null || !eventIds.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Hiç etkinlik seçilmedi."
                    });
                }

                var participant = await _participantService.GetParticipantByIdAsync(participantId);
                if (participant == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Katılımcı bulunamadı."
                    });
                }

                var successCount = 0;
                var alreadyRegisteredCount = 0;
                var errorCount = 0;
                var successfulEvents = new List<string>();
                var alreadyRegisteredEvents = new List<string>();

                foreach (var eventId in eventIds)
                {
                    try
                    {
                        var success = await _participantService.AddParticipantToEventAsync(eventId, participantId);
                        if (success)
                        {
                            successCount++;
                            try
                            {
                                var eventEntity = await _eventService.GetEventByIdAsync(eventId);
                                successfulEvents.Add(eventEntity?.Title ?? $"Etkinlik #{eventId}");
                            }
                            catch
                            {
                                successfulEvents.Add($"Etkinlik #{eventId}");
                            }
                        }
                        else
                        {
                            alreadyRegisteredCount++;
                            try
                            {
                                var eventEntity = await _eventService.GetEventByIdAsync(eventId);
                                alreadyRegisteredEvents.Add(eventEntity?.Title ?? $"Etkinlik #{eventId}");
                            }
                            catch
                            {
                                alreadyRegisteredEvents.Add($"Etkinlik #{eventId}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                    }
                }

                var message = "";
                if (successCount > 0)
                {
                    message += $"{participant.FirstName} {participant.LastName} kişisi {successCount} etkinliğe başarıyla eklendi";
                    if (successfulEvents.Any())
                    {
                        message += $" ({string.Join(", ", successfulEvents)})";
                    }
                    message += ". ";
                }
                if (alreadyRegisteredCount > 0)
                {
                    message += $"{alreadyRegisteredCount} etkinliğe zaten kayıtlı";
                    if (alreadyRegisteredEvents.Any())
                    {
                        message += $" ({string.Join(", ", alreadyRegisteredEvents)})";
                    }
                    message += ". ";
                }
                if (errorCount > 0)
                {
                    message += $"{errorCount} etkinlik için hata oluştu. ";
                }

                return Json(new
                {
                    success = successCount > 0,
                    message = message.Trim(),
                    details = new
                    {
                        successCount,
                        alreadyRegisteredCount,
                        errorCount,
                        totalRequested = eventIds.Count,
                        successfulEvents,
                        alreadyRegisteredEvents
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"İşlem sırasında hata oluştu: {ex.Message}"
                });
            }
        }

        public async Task<ActionResult> AddToEvent(int id)
        {
            try
            {
                var participant = await _participantService.GetParticipantByIdAsync(id);
                if (participant == null)
                    return HttpNotFound();

                var allEvents = await _eventService.GetAllEventsAsync();
                var activeEvents = allEvents?.Where(e => e.IsActive == true && e.EventDate > DateTime.Now);

                ViewBag.Events = new SelectList(activeEvents, "Id", "Title");
                ViewBag.ParticipantName = $"{participant.FirstName} {participant.LastName}";
                ViewBag.ParticipantId = id;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sayfa yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToEvent(int participantId, int eventId)
        {
            try
            {
                var success = await _participantService.AddParticipantToEventAsync(eventId, participantId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Katılımcı etkinliğe başarıyla eklendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Katılımcı zaten bu etkinliğe kayıtlı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Etkinliğe ekleme sırasında hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var participant = await _participantService.GetParticipantByIdAsync(id);
                if (participant == null)
                    return HttpNotFound();

                var participantEvents = await _participantService.GetParticipantEventsAsync(id);
                ViewBag.ParticipantEvents = participantEvents;

                return View(participant);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Detay sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<JsonResult> DebugEventParticipants()
        {
            try
            {
                var allParticipants = await _participantService.GetAllParticipantsAsync();
                var result = new
                {
                    totalParticipants = allParticipants?.Count() ?? 0,
                    participants = allParticipants?.Select(p => new {
                        Id = p.Id,
                        Name = $"{p.FirstName} {p.LastName}",
                        IsActive = p.IsActive
                    }).ToList(),
                    message = "Debug bilgileri"
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}