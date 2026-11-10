using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Makes the one client of the e-mail service (09 11.2 items 1 and 2; 08 2.2 "Retry stratejisi"): the library's own retry is off
/// (<c>MaxErrorRetry = 0</c>; its default would resend a SendEmail the code never sees), the call limit is the job limit (30 seconds), the
/// region is the installation setting's and the credential is given openly - the access key id and the secret key of the credential file
/// (<see cref="EmailCredential"/>). The library's own credential chain (environment variables, profile, instance role) is never used, and a
/// service endpoint configured in the environment is ignored: a server that has another identity or endpoint would otherwise be used silently.
/// </summary>
internal static class SesClientFactory
{
    /// <summary>The client's configuration for <paramref name="options"/>; the tests read it, and give an HTTP source of their own through <paramref name="httpSource"/>.</summary>
    public static AmazonSimpleEmailServiceV2Config CreateConfig(EmailOptions options, HttpClientFactory? httpSource = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        AmazonSimpleEmailServiceV2Config config = new()
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
            MaxErrorRetry = 0,
            Timeout = ExternalCallTimeouts.Job,
            IgnoreConfiguredEndpointUrls = true,
        };
        if (httpSource is not null)
        {
            config.HttpClientFactory = httpSource;
        }

        return config;
    }

    /// <summary>The explicit credential of <paramref name="options"/>; the credential file's content must have its two lines (the start-up check refuses the application otherwise).</summary>
    public static BasicAWSCredentials CreateCredentials(EmailOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        EmailCredential credential = EmailCredential.Parse(options.Credential)
            ?? throw new InvalidOperationException(SettingFailures.Malformed(EmailOptions.CredentialKey, EmailCredential.Expectation));
        return new BasicAWSCredentials(credential.AccessKeyId, credential.SecretKey);
    }

    public static AmazonSimpleEmailServiceV2Client Create(EmailOptions options, HttpClientFactory? httpSource = null) =>
        new(CreateCredentials(options), CreateConfig(options, httpSource));
}
