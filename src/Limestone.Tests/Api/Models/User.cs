namespace Limestone.Tests.Api.Models;

public sealed record User
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public Address? Address { get; init; }
    public Company? Company { get; init; }
}

public sealed record Address
{
    public string? Street { get; init; }
    public string? Suite { get; init; }
    public string? City { get; init; }
    public string? Zipcode { get; init; }
    public Geo? Geo { get; init; }
}

public sealed record Geo
{
    public string? Lat { get; init; }
    public string? Lng { get; init; }
}

public sealed record Company
{
    public string? Name { get; init; }
    public string? CatchPhrase { get; init; }
    public string? Bs { get; init; }
}
