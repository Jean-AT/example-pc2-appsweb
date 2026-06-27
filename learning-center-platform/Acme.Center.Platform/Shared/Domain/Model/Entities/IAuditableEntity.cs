namespace Acme.Center.Platform.Shared.Domain.Model.Entities;
/// <summary>
///     Marks an entity as carrying audit timestamps managed by the persistence layer
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    ///     Gets or sets the UTC timestamp when the entity was first persisted.
    /// </summary>
    DateTimeOffset? CreatedAt { get; set; }
    
    /// <summary>
    ///     Gets or sets the UTC timestamp when the entity was last saved.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }
}