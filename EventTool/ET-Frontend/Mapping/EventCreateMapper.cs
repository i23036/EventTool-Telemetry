using ET_Frontend.Models.Event;
using ET.Shared.DTOs;
using ET.Shared.DTOs.Enums;

namespace ET_Frontend.Mapping;

public static class EventCreateMapper
{
    public static EventDto ToDto(EventCreateViewModel vm)
    {
        return new EventDto(
            vm.Id, // bei Erstellung 0
            vm.Name,
            vm.EventType,
            vm.Description,
            vm.Location,
            vm.Participants,
            vm.Managers, // Organizers
            vm.ContactPersons,
            0, // ProcessId bei Bedarf setzen
            vm.StartDate.HasValue ? DateOnly.FromDateTime(vm.StartDate.Value) : DateOnly.MinValue,
            vm.EndDate.HasValue ? DateOnly.FromDateTime(vm.EndDate.Value) : DateOnly.MinValue,
            vm.StartDate.HasValue ? TimeOnly.FromDateTime(vm.StartDate.Value) : TimeOnly.MinValue,
            vm.EndDate.HasValue ? TimeOnly.FromDateTime(vm.EndDate.Value) : TimeOnly.MinValue,
            vm.MinUsers,
            vm.MaxUsers,
            vm.RegistrationStart.HasValue ? DateOnly.FromDateTime(vm.RegistrationStart.Value) : DateOnly.MinValue,
            vm.RegistrationDeadline.HasValue ? DateOnly.FromDateTime(vm.RegistrationDeadline.Value) : DateOnly.MinValue,
            vm.Status,
            false // IsBlueprint
        );
    }

    public static EventCreateViewModel ToViewModel(EventDto dto)
    {
        return new EventCreateViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            EventType = dto.EventType,
            Description = dto.Description,
            Location = dto.Location,
            Managers = dto.Organizers,
            ContactPersons = dto.ContactPersons,
            StartDate = dto.StartDate.ToDateTime(dto.StartTime),
            EndDate = dto.EndDate.ToDateTime(dto.EndTime),
            MinUsers = dto.MinParticipants,
            MaxUsers = dto.MaxParticipants,
            RegistrationStart = dto.RegistrationStart.ToDateTime(TimeOnly.MinValue),
            RegistrationDeadline = dto.RegistrationEnd.ToDateTime(TimeOnly.MinValue),
            Status = dto.Status,
            Participants = dto.Participants
        };
    }
}