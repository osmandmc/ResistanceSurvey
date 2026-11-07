using System;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// A record two people may edit at the same time (optimistic concurrency). Version goes up by one on every save that
    /// changes one of its fields (RSDBContext does this), and a save that started from an older version is refused. A
    /// form sends back the version it loaded, so a colleague's newer save is never overwritten silently.
    /// </summary>
    public interface IVersioned
    {
        int Version { get; set; }
    }

    /// <summary>
    /// A field the app keeps up to date by itself, not one a person edits (a case's start date follows its protests).
    /// Changing it does not change the version, so adding a protest does not stop a colleague from saving the case form.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotVersionedAttribute : Attribute
    {
    }
}
