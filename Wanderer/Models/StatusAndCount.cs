using System.Collections.Generic;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models;

public record StatusAndCount
{
    public required Status Status { get; init; }
    public required int Count { get; init; }
    public required List<Person> Persons { get; init; }
}