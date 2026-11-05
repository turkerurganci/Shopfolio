// Deliberate violation sample of the link layer scan (08 3.1 rules 1 and 2; 09 11.2). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says HttpClient, new HttpClient( or Polly is not a finding of rules 2-5.
internal sealed class ProviderLeak
{
    public PayTrPaymentGateway Gateway { get; }
    public string Name => "PayTR";
    public SesEmailGateway Email { get; }
    public string Cloud => "Amazon SES";
    public string Region => AwsRegion.Default + " AWS";
    public string LongName => "SimpleEmail";
    public HttpClient Client { get; }
    public HttpClient Made => new HttpClient();
    public void Typed() { HttpClient typed = new(); }
    public void Smtp(SmtpClient client) { }
    public void Retry(IHttpClientBuilder b) => b.AddStandardResilienceHandler();
    public object Config => new { MaxErrorRetry = 3 };
    public object Off => new { MaxErrorRetry = 0 };
    public void Calls(IPaymentGateway payment, IEmailGateway email) { }
    public Session Session { get; }
}
