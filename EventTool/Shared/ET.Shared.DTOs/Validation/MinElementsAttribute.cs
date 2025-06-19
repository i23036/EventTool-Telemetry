using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace ET.Shared.DTOs.Validation;

/// <summary>Prüft, ob eine Collection mindestens <see cref="Minimum"/> Elemente hat.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class MinElementsAttribute : ValidationAttribute
{
    public int Minimum { get; }

    public MinElementsAttribute(int minimum) => Minimum = minimum;

    public override bool IsValid(object? value)
    {
        if (value is not ICollection list) return false;
        return list.Count >= Minimum;
    }
}