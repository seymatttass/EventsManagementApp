using EventManagementApp.Business.Interfaces;
using EventManagementApp.Data.Entities;
using EventManagementApp.Data.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementApp.Business.Services
{
    public class EventTypeService : IEventTypeService
    {
        private readonly IEventTypeRepository _eventTypeRepository;
        private readonly ILogger _logger;

        public EventTypeService(IEventTypeRepository eventTypeRepository)
        {
            _eventTypeRepository = eventTypeRepository;
            _logger = Log.ForContext<EventTypeService>();
        }

        public async Task<EventType> GetEventTypeByIdAsync(int id)
        {
            try
            {
                _logger.Information("Etkinlik türü getiriliyor. EventTypeId: {EventTypeId}", id);

                if (id <= 0) return null;

                var eventType = await _eventTypeRepository.GetByIdAsync(id);

                _logger.Information(eventType == null
                    ? "Etkinlik türü bulunamadı. EventTypeId: {EventTypeId}"
                    : "Etkinlik türü getirildi. EventTypeId: {EventTypeId}, Name: {EventTypeName}",
                    id, eventType?.Name);

                return eventType;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik türü getirme hatası. EventTypeId: {EventTypeId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<EventType>> GetAllEventTypesAsync()
        {
            try
            {
                _logger.Information("Tüm etkinlik türleri getiriliyor");

                var eventTypes = await _eventTypeRepository.GetAllAsync();

                _logger.Information("Etkinlik türleri getirildi. Toplam: {EventTypeCount}",
                    eventTypes?.Count() ?? 0);

                return eventTypes ?? new List<EventType>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik türleri getirme hatası");
                throw;
            }
        }

        public async Task<EventType> CreateEventTypeAsync(EventType eventType)
        {
            try
            {
                _logger.Information("Etkinlik türü oluşturuluyor. Name: {EventTypeName}", eventType?.Name);

                if (eventType == null)
                    throw new ArgumentNullException(nameof(eventType));

                if (string.IsNullOrWhiteSpace(eventType.Name))
                    throw new ArgumentException("Etkinlik türü adı boş olamaz.", nameof(eventType.Name));

                eventType.IsActive = true;
                eventType.CreatedDate = DateTime.Now;

                var createdEventType = await _eventTypeRepository.AddAsync(eventType);

                _logger.Information("Etkinlik türü oluşturuldu. EventTypeId: {EventTypeId}", createdEventType.Id);
                return createdEventType;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik türü oluşturma hatası. Name: {EventTypeName}", eventType?.Name);
                throw;
            }
        }

        public async Task<EventType> UpdateEventTypeAsync(EventType eventType)
        {
            try
            {
                _logger.Information("Etkinlik türü güncelleniyor. EventTypeId: {EventTypeId}", eventType?.Id);

                if (eventType == null)
                    throw new ArgumentNullException(nameof(eventType));

                if (eventType.Id <= 0)
                    throw new ArgumentException("Geçersiz EventTypeId.", nameof(eventType.Id));

                if (string.IsNullOrWhiteSpace(eventType.Name))
                    throw new ArgumentException("Etkinlik türü adı boş olamaz.", nameof(eventType.Name));

                var updatedEventType = await _eventTypeRepository.UpdateAsync(eventType);

                _logger.Information("Etkinlik türü güncellendi. EventTypeId: {EventTypeId}", updatedEventType.Id);
                return updatedEventType;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik türü güncelleme hatası. EventTypeId: {EventTypeId}", eventType?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteEventTypeAsync(int id)
        {
            try
            {
                _logger.Information("Etkinlik türü siliniyor. EventTypeId: {EventTypeId}", id);

                if (id <= 0) return false;

                var result = await _eventTypeRepository.DeleteAsync(id);

                _logger.Information("Etkinlik türü silme {Result}. EventTypeId: {EventTypeId}",
                    result ? "başarılı" : "başarısız", id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik türü silme hatası. EventTypeId: {EventTypeId}", id);
                throw;
            }
        }
    }
}