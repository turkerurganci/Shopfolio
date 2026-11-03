using Microsoft.Extensions.Configuration;

namespace Shopfolio.Infrastructure.FileSecrets;

/// <summary>
/// Configuration source that takes the value of one setting from a secret file (09 3.5 item 2; 05 6.x.1): the value of
/// <see cref="DatabaseOptions.ConnectionStringKey"/> is the whole content of the connection string file Compose mounted into this
/// container (<see cref="SecretNames.DatabaseConnectionFiles"/>, first that exists). It is added after every other source, so the
/// file wins. When no such file is mounted (a development or test setup) the source contributes nothing and the setting comes from
/// the other sources. The value is never logged.
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
            foreach (string file in SecretNames.DatabaseConnectionFiles)
            {
                string? value = _secrets.Read(file);
                if (value is not null)
                {
                    Data[DatabaseOptions.ConnectionStringKey] = value;
                    return;
                }
            }
        }
    }
}
