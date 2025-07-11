namespace SmartParcel.API.Services.Interfaces
{
    public interface ITamperHandler
    {
        Task<bool> ReportTamperingAsync(string trackingId, string reason, string? location);
        Task<bool> ResolveTamperingAsync(string trackingId, string resolution, string nextStatus, string? location);
        bool IsValidTamperResolutionStatus(string status);
        bool IsValidTamperResolutionStatus(object nextStatus);
        Task<bool> ResolveTamperingAsync(string trackingId, string resolution, string nextStatus, object location);
    }
}
