using System.Collections.Generic;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Models;

public record StatusAndCount
{
    public required Status Status { get; init; }
    public required int Count { get; init; }
    public required List<Person> Persons { get; init; }
}