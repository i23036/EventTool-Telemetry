namespace ET_Backend.Models.Enums;

public enum ProcessStepTrigger
{
    MinParticipantsReached,

    MaxParticipantsReached = 1,

    StatusChanged,

    ParticipantRegisters,

    StartOfEventLogins,

    EndOfEventLogins
}
