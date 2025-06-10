using ET_Backend.Models;
using ET.Shared.DTOs;

namespace ET_Backend.Services.Mapping;

public static class EventMapper
{
    public static Models.Event ToModel(EventDto dto, Models.Organization org, Process? process = null)
    {
        return new Models.Event
        {
            Name = dto.Name,
            Description = dto.Description,
            Location = dto.Location,
            Organizers = new List<Account>(), // Werden separat via API-Aufruf gemappt!
            ContactPersons = new List<Account>(), // Ebenso
            Organization = org,
            Process = process,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            MinParticipants = dto.MinParticipants,
            MaxParticipants = dto.MaxParticipants,
            RegistrationStart = dto.RegistrationStart,
            RegistrationEnd = dto.RegistrationEnd,
            IsBlueprint = dto.IsBlueprint
        };
    }

    public static EventDto ToDto(Models.Event e)
    {
        return new EventDto(
            e.Name,
            e.Description,
            e.Location,
            e.Organizers.Select(o => o.EMail).ToList(),
            e.ContactPersons.Select(c => c.EMail).ToList(),
            e.Process?.Id ?? 0,
            e.StartDate,
            e.EndDate,
            e.StartTime,
            e.EndTime,
            e.MinParticipants,
            e.MaxParticipants,
            e.RegistrationStart,
            e.RegistrationEnd,
            e.IsBlueprint
        );
    }
}