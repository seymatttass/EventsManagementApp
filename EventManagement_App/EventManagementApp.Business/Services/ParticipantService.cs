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
    public class ParticipantService : IParticipantService
    {
        private readonly IBaseRepository<Participant> _participantRepository;
        private readonly IBaseRepository<EventParticipant> _eventParticipantRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger _logger;

        public ParticipantService(
            IBaseRepository<Participant> participantRepository,
            IBaseRepository<EventParticipant> eventParticipantRepository,
            IEventRepository eventRepository)
        {
            _participantRepository = participantRepository;
            _eventParticipantRepository = eventParticipantRepository;
            _eventRepository = eventRepository;
            _logger = Log.ForContext<ParticipantService>();
        }

        public async Task<Participant> GetParticipantByIdAsync(int id)
        {
            try
            {
                _logger.Information("Katılımcı getiriliyor. ParticipantId: {ParticipantId}", id);

                if (id <= 0) return null;

                var participant = await _participantRepository.GetByIdAsync(id);

                _logger.Information(participant != null
                    ? "Katılımcı getirildi. ParticipantId: {ParticipantId}, Name: {FirstName} {LastName}"
                    : "Katılımcı bulunamadı. ParticipantId: {ParticipantId}",
                    id, participant?.FirstName, participant?.LastName);

                return participant;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcı getirme hatası. ParticipantId: {ParticipantId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Participant>> GetAllParticipantsAsync()
        {
            try
            {
                _logger.Information("Tüm katılımcılar getiriliyor");

                var participants = await _participantRepository.GetAllAsync();

                _logger.Information("Katılımcılar getirildi. Toplam: {ParticipantCount}",
                    participants?.Count() ?? 0);

                return participants ?? new List<Participant>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcıları getirme hatası");
                throw;
            }
        }

        public async Task<Participant> CreateParticipantAsync(Participant participant)
        {
            try
            {
                _logger.Information("Katılımcı oluşturuluyor. Name: {FirstName} {LastName}",
                    participant?.FirstName, participant?.LastName);

                if (participant == null)
                    throw new ArgumentNullException(nameof(participant));

                if (string.IsNullOrWhiteSpace(participant.FirstName))
                    throw new ArgumentException("Ad boş olamaz.", nameof(participant.FirstName));

                if (string.IsNullOrWhiteSpace(participant.LastName))
                    throw new ArgumentException("Soyad boş olamaz.", nameof(participant.LastName));

                participant.IsActive = true;
                participant.CreatedDate = DateTime.Now;

                var result = await _participantRepository.AddAsync(participant);

                _logger.Information("Katılımcı oluşturuldu. ParticipantId: {ParticipantId}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcı oluşturma hatası. Name: {FirstName} {LastName}",
                    participant?.FirstName, participant?.LastName);
                throw;
            }
        }

        public async Task<Participant> UpdateParticipantAsync(Participant participant)
        {
            try
            {
                _logger.Information("Katılımcı güncelleniyor. ParticipantId: {ParticipantId}", participant?.Id);

                if (participant == null)
                    throw new ArgumentNullException(nameof(participant));

                if (participant.Id <= 0)
                    throw new ArgumentException("Geçersiz ParticipantId.", nameof(participant.Id));

                var existingParticipant = await _participantRepository.GetByIdAsync(participant.Id);
                if (existingParticipant == null)
                    throw new ArgumentException($"ParticipantId {participant.Id} bulunamadı");

                existingParticipant.FirstName = participant.FirstName;
                existingParticipant.LastName = participant.LastName;
                existingParticipant.Email = participant.Email;
                existingParticipant.Phone = participant.Phone;
                existingParticipant.IsActive = participant.IsActive;

                var result = await _participantRepository.UpdateAsync(existingParticipant);

                _logger.Information("Katılımcı güncellendi. ParticipantId: {ParticipantId}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcı güncelleme hatası. ParticipantId: {ParticipantId}", participant?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteParticipantAsync(int id)
        {
            try
            {
                _logger.Information("Katılımcı siliniyor. ParticipantId: {ParticipantId}", id);

                if (id <= 0) return false;

                var result = await _participantRepository.DeleteAsync(id);

                _logger.Information("Katılımcı silme {Result}. ParticipantId: {ParticipantId}",
                    result ? "başarılı" : "başarısız", id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcı silme hatası. ParticipantId: {ParticipantId}", id);
                throw;
            }
        }

        public async Task<bool> AddParticipantToEventAsync(int eventId, int participantId)
        {
            try
            {
                _logger.Information("Katılımcı etkinliğe ekleniyor. EventId: {EventId}, ParticipantId: {ParticipantId}",
                    eventId, participantId);

                if (eventId <= 0 || participantId <= 0)
                    throw new ArgumentException("EventId ve ParticipantId geçerli olmalıdır.");

                var existingRegistration = await _eventParticipantRepository.GetAsync(
                    ep => ep.EventId == eventId && ep.ParticipantId == participantId);

                if (existingRegistration.Any())
                {
                    _logger.Warning("Katılımcı zaten kayıtlı. EventId: {EventId}, ParticipantId: {ParticipantId}",
                        eventId, participantId);
                    return false;
                }

                var eventParticipant = new EventParticipant
                {
                    EventId = eventId,
                    ParticipantId = participantId
                };

                await _eventParticipantRepository.AddAsync(eventParticipant);

                _logger.Information("Katılımcı etkinliğe eklendi. EventId: {EventId}, ParticipantId: {ParticipantId}",
                    eventId, participantId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcı etkinliğe ekleme hatası. EventId: {EventId}, ParticipantId: {ParticipantId}",
                    eventId, participantId);
                throw;
            }
        }

        public async Task<bool> RemoveParticipantFromEventAsync(int eventId, int participantId)
        {
            try
            {
                _logger.Information("Katılımcı etkinlikten çıkarılıyor. EventId: {EventId}, ParticipantId: {ParticipantId}",
                    eventId, participantId);

                var eventParticipant = (await _eventParticipantRepository.GetAsync(
                    ep => ep.EventId == eventId && ep.ParticipantId == participantId)).FirstOrDefault();

                if (eventParticipant == null)
                {
                    _logger.Warning("Çıkarılacak kayıt bulunamadı. EventId: {EventId}, ParticipantId: {ParticipantId}",
                        eventId, participantId);
                    return false;
                }

                await _eventParticipantRepository.DeleteAsync(eventParticipant.Id);

                _logger.Information("Katılımcı etkinlikten çıkarıldı. EventId: {EventId}, ParticipantId: {ParticipantId}",
                    eventId, participantId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcı etkinlikten çıkarma hatası. EventId: {EventId}, ParticipantId: {ParticipantId}",
                    eventId, participantId);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetParticipantEventsAsync(int participantId)
        {
            try
            {
                _logger.Information("Katılımcının etkinlikleri getiriliyor. ParticipantId: {ParticipantId}", participantId);

                if (participantId <= 0)
                    return new List<Event>();

                var eventParticipants = await _eventParticipantRepository.GetAsync(
                    ep => ep.ParticipantId == participantId);

                if (!eventParticipants.Any())
                {
                    _logger.Information("Katılımcının etkinliği yok. ParticipantId: {ParticipantId}", participantId);
                    return new List<Event>();
                }

                var eventIds = eventParticipants.Select(ep => ep.EventId).ToList();
                var events = await _eventRepository.GetAsync(e => eventIds.Contains(e.Id) && e.IsActive == true);
                var result = events.OrderBy(e => e.EventDate).ToList();

                _logger.Information("Katılımcının etkinlikleri getirildi. ParticipantId: {ParticipantId}, EventCount: {EventCount}",
                    participantId, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Katılımcının etkinlikleri getirme hatası. ParticipantId: {ParticipantId}", participantId);
                throw;
            }
        }
    }
}