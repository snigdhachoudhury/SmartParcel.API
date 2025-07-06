public static class ParcelStatus
{
    public const string Created = "Created";
    public const string Scanned = "Scanned";
    public const string HandedOver = "HandedOver";
    public const string OutForDelivery = "Out for Delivery";
    public const string Delivered = "Delivered";
    public const string Returned = "Returned";

    public static string OutforDelivery { get; internal set; }

    public const string Tampered = "Tampered";
    public const string TamperResolved = "TamperResolved";
    public const string ReturnedDueToDamage = "ReturnedDueToDamage";
    public const string Lost = "Lost";
}