using System;
using System.IO;

namespace DayCareManagement.Infrastructure.Configuration;

public static class ConnectionStringResolver
{
    public static string ResolveOrThrow(string? configuredValue = null)
    {
        LoadDotEnvIfPresent();

        var resolvedConnectionString =
            Environment.GetEnvironmentVariable("DAYCAREMANAGEMENT_CONNECTIONSTRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuredValue;

        if (string.IsNullOrWhiteSpace(resolvedConnectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is not configured. Set DAYCAREMANAGEMENT_CONNECTIONSTRING or ConnectionStrings__DefaultConnection, or provide ConnectionStrings:DefaultConnection in configuration.");
        }

        return resolvedConnectionString;
    }

    private static void LoadDotEnvIfPresent()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        for (var level = 0; level <= 3 && currentDirectory is not null; level++)
        {
            var envFilePath = Path.Combine(currentDirectory.FullName, ".env");
            if (File.Exists(envFilePath))
            {
                LoadEnvironmentFile(envFilePath);
                return;
            }

            currentDirectory = currentDirectory.Parent;
        }
    }

    private static void LoadEnvironmentFile(string filePath)
    {
        foreach (var rawLine in File.ReadLines(filePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (Environment.GetEnvironmentVariable(key) is not null)
            {
                continue;
            }

            var value = line[(separatorIndex + 1)..].Trim();
            value = StripSurroundingQuotes(value);

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string StripSurroundingQuotes(string value)
    {
        if (value.Length < 2)
        {
            return value;
        }

        var startsAndEndsWithDoubleQuotes = value[0] == '"' && value[^1] == '"';
        var startsAndEndsWithSingleQuotes = value[0] == '\'' && value[^1] == '\'';

        if (startsAndEndsWithDoubleQuotes || startsAndEndsWithSingleQuotes)
        {
            return value[1..^1];
        }

        return value;
    }
}