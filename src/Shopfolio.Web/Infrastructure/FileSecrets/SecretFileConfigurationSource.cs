using Microsoft.Extensions.Configuration;

namespace Shopfolio.Infrastructure.FileSecrets;

/// <summary>
/// Configuration source that takes the value of a secret setting from its secret file (09 3.5 item 2; 05 6.x.1): for each entry of
/// <see cref="SecretNames.Settings"/> the value of the setting is the whole content of the first of its files that is mounted into this
/// container (the database connection has two files, a container is given at most one). It is added after every other source, so a file
/// wins. When no file is mounted, or the mounted file is empty (the development machine's e-mail credential, 05 8.1.2), the source
/// contributes nothing and the setting comes from the other sources. The value is never logged.
/// </summary>
internal sealed class SecretFileConfigurationSource : IConfigurationSource
{
    private readonly SecretFiles _secrets;

    public SecretFileConfigurationSource(SecretFiles secrets)
    {
        _secrets = secrets;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(_secrets);

    private sealed class Provider : ConfigurationProvider
    {
        private readonly SecretFiles _secrets;

        public Provider(SecretFiles secrets)
        {
            _secrets = secrets;
        }

        public override void Load()
        {
            foreach (SecretSetting setting in SecretNames.Settings)
            {
                foreach (string file in setting.Files)
                {
                    string? value = _secrets.Read(file);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        Data[setting.Key] = value;
                        break;
                    }
                }
            }
        }
    }
}
