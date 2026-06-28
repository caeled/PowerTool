using System.Text;
using System.Text.RegularExpressions;

namespace SupportToolApp.Helpers;

internal static partial class EmailHeaderParser
{
    public static string Parse(string raw)
    {
        var lines = UnfoldHeader(raw);
        var sb = new StringBuilder();

        // Key summary fields
        AppendField(sb, "From",        FindField(lines, "From"));
        AppendField(sb, "To",          FindField(lines, "To"));
        AppendField(sb, "Subject",     FindField(lines, "Subject"));
        AppendField(sb, "Date",        FindField(lines, "Date"));
        AppendField(sb, "Message-ID",  FindField(lines, "Message-ID"));
        AppendField(sb, "Reply-To",    FindField(lines, "Reply-To"));
        AppendField(sb, "Return-Path", FindField(lines, "Return-Path"));

        // SPF / DKIM / DMARC
        var authResults = FindField(lines, "Authentication-Results");
        if (!string.IsNullOrEmpty(authResults))
        {
            sb.AppendLine();
            sb.AppendLine("── Authentication ──────────────────────────────");
            foreach (var check in new[] { "spf", "dkim", "dmarc" })
            {
                var m = Regex.Match(authResults, $@"{check}=(\S+)", RegexOptions.IgnoreCase);
                if (m.Success) sb.AppendLine($"  {check.ToUpper(),-6}: {m.Groups[1].Value}");
            }
        }

        // Received hop chain
        var hops = FindAllFields(lines, "Received");
        if (hops.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("── Received Hops (newest first) ────────────────");
            for (int i = 0; i < hops.Count; i++)
            {
                sb.AppendLine($"  [{i + 1}] {hops[i].Trim()}");
            }
        }

        // X-Originating-IP / X-Forwarded-To
        AppendFieldOptional(sb, "X-Originating-IP", FindField(lines, "X-Originating-IP"));
        AppendFieldOptional(sb, "X-Forwarded-To",   FindField(lines, "X-Forwarded-To"));
        AppendFieldOptional(sb, "X-Spam-Status",    FindField(lines, "X-Spam-Status"));
        AppendFieldOptional(sb, "X-Spam-Score",     FindField(lines, "X-Spam-Score"));

        return sb.ToString().TrimEnd();
    }

    private static List<string> UnfoldHeader(string raw)
    {
        // RFC 2822 unfolding: continuation lines start with whitespace
        var folded = raw.Replace("\r\n", "\n").Replace("\r", "\n");
        var unfolded = Regex.Replace(folded, @"\n[ \t]+", " ");
        return [.. unfolded.Split('\n')];
    }

    private static string? FindField(List<string> lines, string name)
    {
        var prefix = name + ":";
        var line = lines.FirstOrDefault(l => l.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return line == null ? null : line[prefix.Length..].Trim();
    }

    private static List<string> FindAllFields(List<string> lines, string name)
    {
        var prefix = name + ":";
        return lines
            .Where(l => l.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(l => l[prefix.Length..].Trim())
            .ToList();
    }

    private static void AppendField(StringBuilder sb, string label, string? value)
    {
        sb.AppendLine($"{label,-14}: {value ?? "(not found)"}");
    }

    private static void AppendFieldOptional(StringBuilder sb, string label, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            sb.AppendLine($"{label,-14}: {value}");
    }
}
