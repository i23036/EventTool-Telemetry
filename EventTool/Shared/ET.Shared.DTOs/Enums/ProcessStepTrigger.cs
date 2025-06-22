namespace ET.Shared.DTOs.Enums;

public enum ProcessStepTrigger
{
    MinParticipantsReached,

    MaxParticipantsReached,

    StatusChanged,

    ParticipantRegisters,

    OpenSubscription,

    CloseSubscription,
    
    /// <summary>
    /// Wird ausgelöst, sobald der angegebene Vorgänger-Schritt
    /// erfolgreich abgeschlossen wurde.
    /// </summary>
    StepCompleted
}