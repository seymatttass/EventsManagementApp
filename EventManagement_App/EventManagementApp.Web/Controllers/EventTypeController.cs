using EventManagementApp.Business.Interfaces;
using EventManagementApp.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EventManagementApp.Web.Controllers
{
    public class EventTypeController : Controller
    {
        private readonly IEventTypeService _eventTypeService;

        public EventTypeController(IEventTypeService eventTypeService)
        {
            _eventTypeService = eventTypeService;
        }

        public async Task<ActionResult> Index(bool showInactive = false)
        {
            try
            {
                var eventTypes = await _eventTypeService.GetAllEventTypesAsync();

                if (!showInactive)
                {
                    eventTypes = eventTypes?.Where(et => et.IsActive);
                }
                else
                {
                    eventTypes = eventTypes?.Where(et => !et.IsActive);
                }

                ViewBag.ShowInactive = showInactive;
                return View(eventTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Etkinlik türleri yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        public ActionResult Create()
        {
            return View(new EventType());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EventType eventType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(eventType.Name))
                {
                    TempData["ErrorMessage"] = "Tür adı boş bırakılamaz.";
                    return View(eventType);
                }

                if (eventType.Name.Trim().Length < 2)
                {
                    TempData["ErrorMessage"] = "Tür adı en az 2 karakter olmalıdır.";
                    return View(eventType);
                }

                if (eventType.Name.Trim().Length > 100)
                {
                    TempData["ErrorMessage"] = "Tür adı maksimum 100 karakter olabilir.";
                    return View(eventType);
                }

                if (!string.IsNullOrWhiteSpace(eventType.Description) && eventType.Description.Length > 500)
                {
                    TempData["ErrorMessage"] = "Açıklama maksimum 500 karakter olabilir.";
                    return View(eventType);
                }

                eventType.Name = eventType.Name.Trim();
                eventType.Description = string.IsNullOrWhiteSpace(eventType.Description) ? null : eventType.Description.Trim();
                eventType.IsActive = true;
                eventType.CreatedDate = DateTime.Now;

                await _eventTypeService.CreateEventTypeAsync(eventType);

                TempData["SuccessMessage"] = $"'{eventType.Name}' etkinlik türü başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Etkinlik türü kaydedilirken hata oluştu: " + ex.Message;
                return View(eventType);
            }
        }

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "Geçersiz ID parametresi.";
                    return RedirectToAction("Index");
                }

                var eventType = await _eventTypeService.GetEventTypeByIdAsync(id);
                if (eventType == null)
                {
                    TempData["ErrorMessage"] = "Düzenlenecek etkinlik türü bulunamadı.";
                    return RedirectToAction("Index");
                }

                return View(eventType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Düzenleme sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EventType eventType)
        {
            try
            {
                if (eventType.Id <= 0)
                {
                    TempData["ErrorMessage"] = "Geçersiz ID parametresi.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(eventType.Name))
                {
                    TempData["ErrorMessage"] = "Tür adı boş bırakılamaz.";
                    var errorEventType = await _eventTypeService.GetEventTypeByIdAsync(eventType.Id);
                    return View(errorEventType ?? eventType);
                }

                if (eventType.Name.Trim().Length < 2)
                {
                    TempData["ErrorMessage"] = "Tür adı en az 2 karakter olmalıdır.";
                    var errorEventType = await _eventTypeService.GetEventTypeByIdAsync(eventType.Id);
                    return View(errorEventType ?? eventType);
                }

                if (eventType.Name.Trim().Length > 100)
                {
                    TempData["ErrorMessage"] = "Tür adı maksimum 100 karakter olabilir.";
                    var errorEventType = await _eventTypeService.GetEventTypeByIdAsync(eventType.Id);
                    return View(errorEventType ?? eventType);
                }

                if (!string.IsNullOrWhiteSpace(eventType.Description) && eventType.Description.Length > 500)
                {
                    TempData["ErrorMessage"] = "Açıklama maksimum 500 karakter olabilir.";
                    var errorEventType = await _eventTypeService.GetEventTypeByIdAsync(eventType.Id);
                    return View(errorEventType ?? eventType);
                }

                var existingEventType = await _eventTypeService.GetEventTypeByIdAsync(eventType.Id);
                if (existingEventType == null)
                {
                    TempData["ErrorMessage"] = "Güncellenecek etkinlik türü bulunamadı.";
                    return RedirectToAction("Index");
                }

                existingEventType.Name = eventType.Name.Trim();
                existingEventType.Description = !string.IsNullOrWhiteSpace(eventType.Description)
                    ? eventType.Description.Trim()
                    : null;

                if (ModelState.IsValid)
                {
                    await _eventTypeService.UpdateEventTypeAsync(existingEventType);
                    TempData["SuccessMessage"] = $"'{existingEventType.Name}' etkinlik türü başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Form verileri geçersiz.";
                    return View(existingEventType);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Etkinlik türü güncellenirken hata oluştu: " + ex.Message;

                try
                {
                    var errorEventType = await _eventTypeService.GetEventTypeByIdAsync(eventType.Id);
                    return View(errorEventType ?? eventType);
                }
                catch
                {
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "Geçersiz ID parametresi.";
                    return RedirectToAction("Index");
                }

                var eventType = await _eventTypeService.GetEventTypeByIdAsync(id);
                if (eventType == null)
                {
                    TempData["ErrorMessage"] = "Silinecek etkinlik türü bulunamadı.";
                    return RedirectToAction("Index");
                }

                if (!eventType.IsActive)
                {
                    TempData["WarningMessage"] = "Bu etkinlik türü zaten silinmiş/pasif durumda.";
                    return RedirectToAction("Index");
                }

                eventType.IsActive = false;
                await _eventTypeService.UpdateEventTypeAsync(eventType);

                TempData["SuccessMessage"] = $"'{eventType.Name}' etkinlik türü başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Etkinlik türü silinirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var eventType = await _eventTypeService.GetEventTypeByIdAsync(id);
                if (eventType == null)
                    return HttpNotFound();
                return View(eventType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Detaylar yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<JsonResult> GetActiveEventTypes()
        {
            try
            {
                var eventTypes = await _eventTypeService.GetAllEventTypesAsync();
                var result = eventTypes?.Where(et => et.IsActive)
                    .Select(et => new {
                        Id = et.Id,
                        Name = et.Name,
                        Description = et.Description
                    }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetAllEventTypes(bool includeInactive = false)
        {
            try
            {
                var eventTypes = await _eventTypeService.GetAllEventTypesAsync();

                if (!includeInactive)
                {
                    eventTypes = eventTypes?.Where(et => et.IsActive);
                }

                var result = eventTypes?.Select(et => new {
                    Id = et.Id,
                    Name = et.Name,
                    Description = et.Description,
                    IsActive = et.IsActive,
                    CreatedDate = et.CreatedDate
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}