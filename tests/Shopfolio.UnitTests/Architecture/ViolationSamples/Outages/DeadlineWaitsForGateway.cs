// Deliberate violation sample of the outage independence scan (08 3.3; T204 KK7). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says IEmailGateway or IGoogleIdentityGateway is not a finding.
internal sealed class DeadlineWaitsForGateway
{
    public IEmailGateway Email { get; }
    public SmtpCaptureEmailGateway Capture { get; }
    public IGoogleIdentityGateway Google { get; }
    public void Wait(IEmailGateway email, IGoogleIdentityGateway google) { }
    public string Named => "IEmailGateway";
    public EmailGatewayOptions Options { get; }
    public GoogleIdentityResult Result { get; }
    public void SendEmail(IPaymentGateway payment) { }
}
