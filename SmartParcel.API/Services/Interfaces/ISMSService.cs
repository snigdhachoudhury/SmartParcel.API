namespace SmartParcel.API.Services
{
    public interface ISMSServices
    {
        Task SendSMSAsync(string phoneNumber, string message);

    }
}
