#!/usr/bin/env dotnet
/*
Quick validation script for skills - minimal version

Usage:
    dotnet run quick-validate.cs <skill_directory>

.NET 10 File-Based App (FBA)
https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

(bool IsValid, string Message) ValidateSkill(string skillPath)
{
    var skillMdPath = Path.Combine(skillPath, "SKILL.md");
    
    // Check SKILL.md exists
    if (!File.Exists(skillMdPath))
        return (false, "SKILL.md not found");

    // Read and validate frontmatter
    var content = File.ReadAllText(skillMdPath);
    
    if (!content.StartsWith("---"))
        return (false, "No YAML frontmatter found");

    // Extract frontmatter
    var match = Regex.Match(content, @"^---\r?\n(.*?)\r?\n---", RegexOptions.Singleline);
    if (!match.Success)
        return (false, "Invalid frontmatter format");

    var frontmatterText = match.Groups[1].Value;

    // Simple YAML parsing for key-value pairs
    var frontmatter = new Dictionary<string, string>();
    string? currentKey = null;
    var currentValue = new List<string>();

    foreach (var line in frontmatterText.Split('\n'))
    {
        var trimmedLine = line.TrimEnd('\r');
        
        // Check for new key
        var keyMatch = Regex.Match(trimmedLine, @"^([a-z][a-z0-9-]*):\s*(.*)$");
        if (keyMatch.Success)
        {
            // Save previous key-value if exists
            if (currentKey != null)
            {
                frontmatter[currentKey] = string.Join(" ", currentValue).Trim();
            }
            
            currentKey = keyMatch.Groups[1].Value;
            var value = keyMatch.Groups[2].Value.Trim();
            currentValue = new List<string> { value };
        }
        else if (currentKey != null && !string.IsNullOrWhiteSpace(trimmedLine))
        {
            // Continuation of previous value
            currentValue.Add(trimmedLine.Trim());
        }
    }
    
    // Save last key-value
    if (currentKey != null)
    {
        frontmatter[currentKey] = string.Join(" ", currentValue).Trim();
    }

    // Define allowed properties
    var allowedProperties = new HashSet<string> { "name", "description", "license", "allowed-tools", "metadata" };

    // Check for unexpected properties
    var unexpectedKeys = frontmatter.Keys.Except(allowedProperties).ToList();

    if (unexpectedKeys.Any())
        return (false, $"Unexpected key(s) in SKILL.md frontmatter: {string.Join(", ", unexpectedKeys.OrderBy(k => k))}. " +
                     $"Allowed properties are: {string.Join(", ", allowedProperties.OrderBy(k => k))}");

    // Check required fields
    if (!frontmatter.ContainsKey("name"))
        return (false, "Missing 'name' in frontmatter");
    if (!frontmatter.ContainsKey("description"))
        return (false, "Missing 'description' in frontmatter");

    // Extract name for validation
    var name = frontmatter["name"]?.Trim() ?? "";
    if (!string.IsNullOrEmpty(name))
    {
        // Check naming convention (hyphen-case: lowercase with hyphens)
        if (!Regex.IsMatch(name, @"^[a-z0-9-]+$"))
            return (false, $"Name '{name}' should be hyphen-case (lowercase letters, digits, and hyphens only)");
        if (name.StartsWith("-") || name.EndsWith("-") || name.Contains("--"))
            return (false, $"Name '{name}' cannot start/end with hyphen or contain consecutive hyphens");
        // Check name length (max 64 characters per spec)
        if (name.Length > 64)
            return (false, $"Name is too long ({name.Length} characters). Maximum is 64 characters.");
    }

    // Extract and validate description
    frontmatter.TryGetValue("description", out var description);
    description = description?.Trim() ?? "";
    if (!string.IsNullOrEmpty(description))
    {
        // Check for angle brackets
        if (description.Contains('<') || description.Contains('>'))
            return (false, "Description cannot contain angle brackets (< or >)");
        // Check description length (max 1024 characters per spec)
        if (description.Length > 1024)
            return (false, $"Description is too long ({description.Length} characters). Maximum is 1024 characters.");
    }

    return (true, "Skill is valid!");
}

// Main
if (args.Length != 1)
{
    Console.WriteLine("Usage: dotnet run quick-validate.cs <skill_directory>");
    return 1;
}

var (isValid, message) = ValidateSkill(args[0]);
Console.WriteLine(message);
return isValid ? 0 : 1;
