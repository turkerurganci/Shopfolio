// Deliberate violations of the notification channel rules (T328; 09 19.2 item 3). Never compiled; the scan test reads this file as text.
namespace Shopfolio.UnitTests.Architecture.ViolationSamples.NotificationChannel;

public sealed class Misuse
{
    private ISmsGateway _sms;
    private int PushNotificationToken;
    public bool NotificationCenterEnabled;
    public string IysConsentId;
    public bool MarketingOptIn;
    public void Unsubscribe() { }
    public bool NewsletterSubscribed;
    public bool EmailNoticesDisabled;
    public void DisableNotifications() { }
    public bool NotificationPreferences;
    public void AbandonedCartReminder() { }
    public string Route = "/bildirim-tercihleri";
    public string Other = "sms_gateway";
    public string Dashed = "back-in-stock";
    private IEmailGateway _gateway;
    public void Write(AppDbContext db) => db.DispatchRecords.Add(null!);
    public object Make() => DispatchRecord.Queue(default, "a", null, null, null, default);
    public object Built() => new DispatchRecord();

    // SMS, a PushNotification and a marketing newsletter named in a comment are not code.
    /* A block comment with an IEmailGateway and the DispatchRecord.Queue call
       over two lines is not code either. */
    public string Fine = "https://example.com/page"; // an SMS in a trailing comment
    public string AlsoFine = "email gateway";
}
