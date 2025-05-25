using ET.Shared.DTOs;
using ET_Frontend.Models;

namespace ET_Frontend.Mapping;

/// <summary>
/// Enthält Mapping-Methoden zwischen UserDto und UserEditViewModel.
/// </summary>
public static class ProcessViewMapper
{
    /// <summary>
    /// Wandelt einen ProcessDto in ein ViewModel zur Anzeige/Bearbeitung um.
    /// </summary>
    public static ProcessViewModel ToViewModel(ProcessDto dto)
    {
        List<ProcessStepDto> processStepDtoList = dto.ProcessSteps;
        List<ProcessStepViewModel> processStepViewModelList = new List<ProcessStepViewModel>();
        foreach (ProcessStepDto processStepDto in processStepDtoList)
        {
            processStepViewModelList.Add(ProcessStepViewMapper.ToViewModel(processStepDto));
        }

        return new ProcessViewModel
        {
            Id = dto.Id,
            ProcessSteps = processStepViewModelList
        };
    }

    /// <summary>
    /// Wandelt ein ViewModel zurück in einen ProcessDto zur API-Übertragung.
    /// </summary>
    public static ProcessDto ToDto(ProcessViewModel vm)
    {
        List<ProcessStepViewModel> processStepViewModelList = vm.ProcessSteps;
        List<ProcessStepDto> processStepDtoList = new List<ProcessStepDto>();
        foreach(ProcessStepViewModel processStepViewModel in processStepViewModelList)
        {
            processStepDtoList.Add(ProcessStepViewMapper.ToDto(processStepViewModel));
        }

        return new ProcessDto(
            vm.Id,
            processStepDtoList
        );
    }
}