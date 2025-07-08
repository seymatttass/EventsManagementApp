using EventManagementApp.Business.Interfaces;
using EventManagementApp.Business.Services;
using EventManagementApp.Data.Context;
using EventManagementApp.Data.Interfaces;
using EventManagementApp.Data.Repositories;
using EventManagementApp.Data.Entities;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace EventManagementApp.Web.App_Start
{
    public static class DependencyConfig
    {
        public static void RegisterDependencies()
        {
            var container = new UnityContainer();

            container.RegisterType<EventManagementContext>();

            container.RegisterType<IEventRepository, EventRepository>();
            container.RegisterType<IEventTypeRepository, EventTypeRepository>();
            container.RegisterType<IBaseRepository<Participant>, BaseRepository<Participant>>();
            container.RegisterType<IBaseRepository<EventParticipant>, BaseRepository<EventParticipant>>();

            container.RegisterType<IEventService, EventService>();
            container.RegisterType<IEventTypeService, EventTypeService>();
            container.RegisterType<IParticipantService, ParticipantService>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}