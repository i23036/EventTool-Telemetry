using ET_Frontend.Models.Event;
using ET.Shared.DTOs;

namespace ET_Frontend.Mapping;

public static class EventCreateMapper
{
    public static EventDto ToDto(EventCreateViewModel vm)
    {
        return new EventDto(
            vm.Name,
            vm.Description,
            vm.Location,
            vm.Managers,
            new List<string> { vm.ContactPerson },
            0, // ProcessId: Bei Bedarf einfügen!
            vm.StartDate.HasValue ? DateOnly.FromDateTime(vm.StartDate.Value) : DateOnly.MinValue,
            vm.EndDate.HasValue ? DateOnly.FromDateTime(vm.EndDate.Value) : DateOnly.MinValue,
            vm.StartDate.HasValue ? TimeOnly.FromDateTime(vm.StartDate.Value) : TimeOnly.MinValue,
            vm.EndDate.HasValue ? TimeOnly.FromDateTime(vm.EndDate.Value) : TimeOnly.MinValue,
            vm.MinUsers,
            vm.MaxUsers,
            vm.RegistrationStart.HasValue ? DateOnly.FromDateTime(vm.RegistrationStart.Value) : DateOnly.MinValue,
            vm.RegistrationDeadline.HasValue ? DateOnly.FromDateTime(vm.RegistrationDeadline.Value) : DateOnly.MinValue,
            false // IsBlueprint
        );
    }
}