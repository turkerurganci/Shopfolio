using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Win32.SafeHandles;

namespace Shopfolio.Infrastructure.LogRead;

/// <summary>
/// The server's logs as the read command's container mounts them read-only (05 9.1.4, 9.3.1; deploy/compose.yaml read-command; DEPLOY_RUNBOOK L.1 rows 3-6): the database's volume
/// (the server's log and pgBackRest's), the backup tool's volume and the operating system's journal directory.
/// </summary>
internal sealed record ServerLogDirectories(string Database, string Backup, string Journal)
{
    public const string DefaultDatabaseDirectory = "/var/log/shopfolio/server/db";
    public const string DefaultBackupDirectory = "/var/log/shopfolio/server/backup";
    public const string DefaultJournalDirectory = "/var/log/shopfolio/server/journal";

    public static ServerLogDirectories Default { get; } = new(DefaultDatabaseDirectory, DefaultBackupDirectory, DefaultJournalDirectory);
}

/// <summary>
/// The text lines of the server's file logs (05 9.1.4; DEPLOY_RUNBOOK L.1 rows 4-6): the database's log (<c>log_line_prefix</c> <c>%m</c>: <c>2026-10-05 10:00:00.123 UTC [57] LOG: …</c>),
/// pgBackRest's (<c>2026-10-05 10:00:00.123 P00   INFO: …</c>, the container's clock, UTC) and the backup tool's (a round starts with <c>… round starts at 2026-10-05T01:30:00Z</c>).
/// A line's time is the time it starts with (no zone: UTC, the containers' clock); otherwise an ISO UTC moment inside it; otherwise the time of the line before it in the same file
/// (a multi-line message, a tool's output inside a round). A line before any time is unreadable.
/// </summary>
internal static partial class ServerTextLine
{
    public static DateTimeOffset? TimeOf(string line, DateTimeOffset? previous)
    {
        Match leading = LeadingTime().Match(line);
        if (leading.Success && TryMoment(leading.Groups["date"].Value, leading.Groups["clock"].Value, leading.Groups["zone"].Value, out DateTimeOffset start))
        {
            return start;
        }

        Match inner = EmbeddedUtcTime().Match(line);
        if (inner.Success && TryMoment(inner.Groups["date"].Value, inner.Groups["clock"].Value, "Z", out DateTimeOffset moment))
        {
            return moment;
        }

        return previous;
    }

    /// <summary>
    /// Whether <paramref name="value"/> stands in <paramref name="text"/> as a whole token: not inside a longer id, address or number (<c>10.0.0.1</c> does not match <c>10.0.0.15</c>
    /// nor <c>110.0.0.1</c>; a sentence's full stop or a comma after it does not hide it).
    /// </summary>
    public static bool ContainsToken(string text, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            int end = index + value.Length;
            if (!ContinuesBefore(text, index) && !ContinuesAfter(text, end))
            {
                return true;
            }

            index++;
        }

        return false;
    }

    private static bool ContinuesBefore(string text, int index)
    {
        if (index == 0)
        {
            return false;
        }

        char before = text[index - 1];
        return IsTokenChar(before) || (before is '.' or ':' && index >= 2 && char.IsAsciiLetterOrDigit(text[index - 2]));
    }

    private static bool ContinuesAfter(string text, int end)
    {
        if (end >= text.Length)
        {
            return false;
        }

        char after = text[end];
        return IsTokenChar(after) || (after is '.' or ':' && end + 1 < text.Length && char.IsAsciiLetterOrDigit(text[end + 1]));
    }

    private static bool IsTokenChar(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_';

    private static bool TryMoment(string date, string clock, string zone, out DateTimeOffset moment)
    {
        moment = default;
        if (!DateTime.TryParseExact(date + "T" + clock, ["yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF"], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime local))
        {
            return false;
        }

        TimeSpan offset = TimeSpan.Zero;
        if (zone.Length > 0 && zone[0] is '+' or '-')
        {
            string digits = zone[1..].Replace(":", string.Empty, StringComparison.Ordinal);
            int hours = int.Parse(digits[..2], CultureInfo.InvariantCulture);
            int minutes = digits.Length >= 4 ? int.Parse(digits[2..4], CultureInfo.InvariantCulture) : 0;
            if (hours > 14 || minutes > 59)
            {
                return false;
            }

            offset = new TimeSpan(hours, minutes, 0);
            offset = zone[0] == '-' ? -offset : offset;
        }

        moment = new DateTimeOffset(local, offset).ToUniversalTime();
        return true;
    }

    [GeneratedRegex(@"^(?<date>[0-9]{4}-[0-9]{2}-[0-9]{2})[ T](?<clock>[0-9]{2}:[0-9]{2}:[0-9]{2}(?:\.[0-9]{1,7})?)(?:\s?(?<zone>Z|UTC|GMT|[+-][0-9]{2}(?::?[0-9]{2})?))?(?=\s|$)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LeadingTime();

    [GeneratedRegex(@"(?<![0-9])(?<date>[0-9]{4}-[0-9]{2}-[0-9]{2})T(?<clock>[0-9]{2}:[0-9]{2}:[0-9]{2})Z(?![0-9A-Za-z])", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EmbeddedUtcTime();
}

/// <summary>One entry of the operating system's journal: its time, the program that wrote it (<c>SYSLOG_IDENTIFIER</c>, else <c>_COMM</c>), its unit and its text.</summary>
internal sealed record JournalEntry(DateTimeOffset Time, string? Identifier, string? Unit, string Line);

/// <summary>
/// The reader of a systemd journal file (05 9.1.4; DEPLOY_RUNBOOK L.4), the file format systemd documents ("Journal File Format"): the header, then objects one after another, each
/// eight-byte aligned; an entry object carries its realtime moment and the offsets of its data objects (<c>FIELD=value</c>), sixteen bytes per item in the regular format and four in
/// the compact one. The file is opened for reading and shared with journald; it is never changed. A data object the journal compressed is not decoded: the installation turns
/// compression off (<c>Compress=no</c>, DEPLOY_RUNBOOK L.4), and an entry whose message is compressed or cut off is counted as unreadable (<see langword="null"/>).
/// </summary>
internal static class JournalFile
{
    private const int HeaderLength = 160;
    private const int ObjectHeaderLength = 16;
    private const uint IncompatibleCompact = 16;
    private const byte DataObject = 1;
    private const byte EntryObject = 3;
    private const byte CompressedFlags = 1 | 2 | 4;
    private const int MaxObjectLength = 1024 * 1024;

    // "LPKSHHRH", the journal file's signature.
    private static readonly byte[] _signature = "LPKSHHRH"u8.ToArray();

    public static IEnumerable<JournalEntry?> ReadEntries(string path)
    {
        using SafeFileHandle handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        long length = RandomAccess.GetLength(handle);
        byte[] header = new byte[HeaderLength];
        if (length < HeaderLength || ReadAt(handle, header, 0) < HeaderLength || !header.AsSpan(0, _signature.Length).SequenceEqual(_signature))
        {
            yield return null;
            yield break;
        }

        bool compact = (BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(12)) & IncompatibleCompact) != 0;
        ulong offset = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(88));
        ulong tail = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(136));
        byte[] objectHeader = new byte[ObjectHeaderLength];
        while (offset >= HeaderLength && offset <= tail && offset <= (ulong)(length - ObjectHeaderLength))
        {
            if (ReadAt(handle, objectHeader, (long)offset) < ObjectHeaderLength)
            {
                yield break;
            }

            ulong size = BinaryPrimitives.ReadUInt64LittleEndian(objectHeader.AsSpan(8));
            if (size < ObjectHeaderLength || size > (ulong)length - offset)
            {
                yield break;
            }

            if (objectHeader[0] == EntryObject)
            {
                yield return ReadEntry(handle, (long)offset, (long)size, length, compact);
            }

            offset += (size + 7) & ~7UL;
        }
    }

    private static JournalEntry? ReadEntry(SafeFileHandle handle, long offset, long size, long length, bool compact)
    {
        const int itemsStart = 64;
        if (size < itemsStart || size > MaxObjectLength)
        {
            return null;
        }

        byte[] entry = new byte[size];
        if (ReadAt(handle, entry, offset) < size)
        {
            return null;
        }

        ulong realtime = BinaryPrimitives.ReadUInt64LittleEndian(entry.AsSpan(24));
        if (realtime >= 253402300799UL * 1_000_000UL)
        {
            return null;
        }

        int itemLength = compact ? 4 : 16;
        string? message = null;
        string? identifier = null;
        string? command = null;
        string? unit = null;
        string? pid = null;
        for (int i = itemsStart; i + itemLength <= size; i += itemLength)
        {
            ulong dataOffset = compact ? BinaryPrimitives.ReadUInt32LittleEndian(entry.AsSpan(i)) : BinaryPrimitives.ReadUInt64LittleEndian(entry.AsSpan(i));
            if (ReadField(handle, dataOffset, length, compact) is not (string name, string value))
            {
                continue;
            }

            switch (name)
            {
                case "MESSAGE":
                    message = value;
                    break;
                case "SYSLOG_IDENTIFIER":
                    identifier = value;
                    break;
                case "_COMM":
                    command = value;
                    break;
                case "_SYSTEMD_UNIT":
                    unit = value;
                    break;
                case "_PID":
                    pid = value;
                    break;
                default:
                    break;
            }
        }

        if (message is null)
        {
            return null;
        }

        string? program = identifier ?? command;
        StringBuilder line = new();
        if (unit is not null)
        {
            line.Append(unit).Append(' ');
        }

        if (program is not null)
        {
            line.Append(program);
            if (pid is not null)
            {
                line.Append('[').Append(pid).Append(']');
            }

            line.Append(": ");
        }

        line.Append(message);
        return new JournalEntry(DateTimeOffset.UnixEpoch.AddTicks((long)realtime * 10), program, unit, line.ToString());
    }

    // A data object's field, or null when the offset is not a data object, the object is compressed or cut off, or the payload has no '='.
    private static (string Name, string Value)? ReadField(SafeFileHandle handle, ulong offset, long length, bool compact)
    {
        int payloadStart = compact ? 72 : 64;
        if (offset < HeaderLength || offset > (ulong)(length - payloadStart))
        {
            return null;
        }

        byte[] objectHeader = new byte[ObjectHeaderLength];
        if (ReadAt(handle, objectHeader, (long)offset) < ObjectHeaderLength || objectHeader[0] != DataObject || (objectHeader[1] & CompressedFlags) != 0)
        {
            return null;
        }

        ulong size = BinaryPrimitives.ReadUInt64LittleEndian(objectHeader.AsSpan(8));
        if (size <= (ulong)payloadStart || size > MaxObjectLength || size > (ulong)length - offset)
        {
            return null;
        }

        byte[] payload = new byte[(int)size - payloadStart];
        if (ReadAt(handle, payload, (long)offset + payloadStart) < payload.Length)
        {
            return null;
        }

        int equals = Array.IndexOf(payload, (byte)'=');
        if (equals <= 0)
        {
            return null;
        }

        return (Encoding.ASCII.GetString(payload, 0, equals), Encoding.UTF8.GetString(payload, equals + 1, payload.Length - equals - 1));
    }

    private static int ReadAt(SafeFileHandle handle, byte[] buffer, long offset)
    {
        int total = 0;
        while (total < buffer.Length)
        {
            int read = RandomAccess.Read(handle, buffer.AsSpan(total), offset + total);
            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }
}
