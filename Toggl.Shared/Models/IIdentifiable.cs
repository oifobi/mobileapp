namespace Toggl.Shared.Models
{
    public interface IIdentifiable
    {
        long Id { get; }
        string UniqueId { get; }
    }
}
