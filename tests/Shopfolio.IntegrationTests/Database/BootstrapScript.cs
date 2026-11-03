using System.Text;

using Npgsql;

using Shopfolio.UnitTests.Support;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// Runs the SQL files of the database image's setup script (<c>deploy/images/database/bootstrap</c>; 09 10.8 item 2, 17 item 1) against
/// the test database server, so the roles and privileges the tests check are made by the same files an installation uses (09 19.1 item 4).
/// The files are psql files with variables: <c>:"name"</c> is an identifier, <c>:'name'</c> a literal. The setup script supplies the
/// values in an installation, this class supplies them in the tests. No other psql feature is used in them, so the statements are split
/// here at the semicolons that are outside quotes and comments and run one by one (the same way psql runs them).
/// </summary>
internal static class BootstrapScript
{
    public const string RolesFile = "10-roles.sql";

    public const string DatabaseFile = "20-database.sql";

    public const string DatabaseAccessFile = "30-database-access.sql";

    public const string SchemaFile = "40-schema.sql";

    /// <summary>
    /// Runs one file over the connection string (the founding user of the test server) with the given variables.
    /// </summary>
    public static async Task RunAsync(string connectionString, string fileName, IReadOnlyDictionary<string, string> variables, CancellationToken ct)
    {
        string path = Path.Combine(RepositoryRoot.Find(), "deploy", "images", "database", "bootstrap", fileName);
        IReadOnlyList<string> statements = Split(await File.ReadAllTextAsync(path, ct), variables);

        // Not pooled: the files change session settings (statement logging), which must not outlive the run on a pooled connection.
        DsnBuilder builder = new(connectionString) { Pooling = false };
        await using NpgsqlConnection connection = new(builder.ConnectionString);
        await connection.OpenAsync(ct);
        foreach (string statement in statements)
        {
            await using NpgsqlCommand command = new(statement, connection);
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    /// <summary>
    /// Replaces the variables and splits the text into statements. Comments are dropped; quotes (<c>'</c> and <c>"</c>) hide semicolons
    /// and variable names; an unknown variable is an error (psql would leave the text as it is, which then fails at the server).
    /// </summary>
    public static IReadOnlyList<string> Split(string text, IReadOnlyDictionary<string, string> variables)
    {
        List<string> statements = [];
        StringBuilder current = new();
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            if (c == '-' && i + 1 < text.Length && text[i + 1] == '-')
            {
                while (i < text.Length && text[i] != '\n')
                {
                    i++;
                }

                continue;
            }

            if (c is '\'' or '"')
            {
                i = CopyQuoted(text, i, current);
                continue;
            }

            if (c == ':' && IsVariable(text, i))
            {
                char quote = text[i + 1];
                int length = NameLength(text, i + 2);
                string name = text.Substring(i + 2, length);
                if (!variables.TryGetValue(name, out string? value))
                {
                    throw new InvalidOperationException($"The setup SQL uses the variable '{name}', which the test did not set.");
                }

                current.Append(quote).Append(value.Replace(quote.ToString(), new string(quote, 2), StringComparison.Ordinal)).Append(quote);
                i += length + 3;
                continue;
            }

            if (c == ';')
            {
                AddStatement(statements, current);
                i++;
                continue;
            }

            current.Append(c);
            i++;
        }

        AddStatement(statements, current);
        return statements;
    }

    public static string WithDatabase(string connectionString, string databaseName)
    {
        DsnBuilder builder = new(connectionString) { Database = databaseName };
        return builder.ConnectionString;
    }

    private static int CopyQuoted(string text, int start, StringBuilder target)
    {
        char quote = text[start];
        target.Append(quote);
        int i = start + 1;
        while (i < text.Length)
        {
            target.Append(text[i]);
            if (text[i] == quote)
            {
                if (i + 1 < text.Length && text[i + 1] == quote)
                {
                    target.Append(quote);
                    i += 2;
                    continue;
                }

                return i + 1;
            }

            i++;
        }

        throw new InvalidOperationException("The setup SQL has a quote that is never closed.");
    }

    /// <summary>
    /// True at <c>:"name"</c> or <c>:'name'</c> (a colon, a quote, at least one name character, the same quote).
    /// </summary>
    private static bool IsVariable(string text, int colon)
    {
        if (colon + 1 >= text.Length || text[colon + 1] is not ('\'' or '"'))
        {
            return false;
        }

        int length = NameLength(text, colon + 2);
        int closing = colon + 2 + length;
        return length > 0 && closing < text.Length && text[closing] == text[colon + 1];
    }

    private static int NameLength(string text, int start)
    {
        int length = 0;
        while (start + length < text.Length && (char.IsAsciiLetterOrDigit(text[start + length]) || text[start + length] == '_'))
        {
            length++;
        }

        return length;
    }

    private static void AddStatement(List<string> statements, StringBuilder current)
    {
        string statement = current.ToString().Trim();
        current.Clear();
        if (statement.Length > 0)
        {
            statements.Add(statement);
        }
    }
}
