namespace WirelessCom.Domain.Models.Entities;

/// <summary>
///     Base DB entity
/// </summary>
public record BaseEntity
{
    /// <summary>
    ///     The id of a DB <see cref="BaseEntity" />.
    /// </summary>
    public int Id { get; set; }
}