using ET_Backend.Models;
using ET.Shared.DTOs;

namespace ET_Backend.Services.Mapping;

public static class EventMapper
{
    public static Models.Event ToModel(
        EventDto dto,
        Models.Organization org,
        Process? process = null,
        List<Account>? organizers = null,
        List<Account>? contactPersons = null,
        List<Account>? participants = null)
    {
        return new Models.Event
        {
            Name = dto.Name,
            EventType = dto.EventType,
            Description = dto.Description,
            Location = dto.Location,
            Organizers = organizers ?? new List<Account>(),
            ContactPersons = contactPersons ?? new List<Account>(),
            Participants = participants ?? new List<Account>(),
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
            Status = dto.Status,
            IsBlueprint = dto.IsBlueprint
        };
    }

    public static EventDto ToDto(Models.Event e)
    {
        return new EventDto(
            e.Id,
            e.Name,
            e.EventType,
            e.Description,
            e.Location,
            e.Participants.Select(p => new EventParticipantDto(
                p.Id,
                p.User?.Firstname ?? "",
                p.User?.Lastname ?? "",
                p.EMail
            )).ToList(),
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
            e.Status,
            e.IsBlueprint
        );
    }
}