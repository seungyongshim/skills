#!/usr/bin/env dotnet
/*
Skill Packager - Creates a distributable .skill file of a skill folder

Usage:
    dotnet run package-skill.cs <path/to/skill-folder> [output-directory]

Example:
    dotnet run package-skill.cs skills/public/my-skill
    dotnet run package-skill.cs skills/public/my-skill ./dist

.NET 10 File-Based App (FBA)
https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

(bool IsValid, string Message) ValidateSkill(string skillPath)
{
    var skillMdPath = Path.Combine(skillPath, "SKILL.md");
    
    if (!File.Exists(skillMdPath))
        return (false, "SKILL.md not found");

    var content = File.ReadAllText(skillMdPath);
    
    if (!content.StartsWith("---"))
        return (false, "No YAML frontmatter found");

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
        
        var keyMatch = Regex.Match(trimmedLine, @"^([a-z][a-z0-9-]*):\s*(.*)$");
        if (keyMatch.Success)
        {
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
            currentValue.Add(trimmedLine.Trim());
        }
    }
    
    if (currentKey != null)
    {
        frontmatter[currentKey] = string.Join(" ", currentValue).Trim();
    }

    var allowedProperties = new HashSet<string> { "name", "description", "license", "allowed-tools", "metadata" };
    var unexpectedKeys = frontmatter.Keys.Except(allowedProperties).ToList();

    if (unexpectedKeys.Any())
        return (false, $"Unexpected key(s) in SKILL.md frontmatter: {string.Join(", ", unexpectedKeys.OrderBy(k => k))}. " +
                     $"Allowed properties are: {string.Join(", ", allowedProperties.OrderBy(k => k))}");

    if (!frontmatter.ContainsKey("name"))
        return (false, "Missing 'name' in frontmatter");
    if (!frontmatter.ContainsKey("description"))
        return (false, "Missing 'description' in frontmatter");

    var name = frontmatter["name"]?.Trim() ?? "";
    if (!string.IsNullOrEmpty(name))
    {
        if (!Regex.IsMatch(name, @"^[a-z0-9-]+$"))
            return (false, $"Name '{name}' should be hyphen-case (lowercase letters, digits, and hyphens only)");
        if (name.StartsWith("-") || name.EndsWith("-") || name.Contains("--"))
            return (false, $"Name '{name}' cannot start/end with hyphen or contain consecutive hyphens");
        if (name.Length > 64)
            return (false, $"Name is too long ({name.Length} characters). Maximum is 64 characters.");
    }

    frontmatter.TryGetValue("description", out var description);
    description = description?.Trim() ?? "";
    if (!string.IsNullOrEmpty(description))
    {
        if (description.Contains('<') || description.Contains('>'))
            return (false, "Description cannot contain angle brackets (< or >)");
        if (description.Length > 1024)
            return (false, $"Description is too long ({description.Length} characters). Maximum is 1024 characters.");
    }

    return (true, "Skill is valid!");
}

string? PackageSkill(string skillPath, string? outputDir)
{
    skillPath = Path.GetFullPath(skillPath);

    if (!Directory.Exists(skillPath))
    {
        Console.WriteLine($"❌ Error: Skill folder not found: {skillPath}");
        return null;
    }

    var skillMdPath = Path.Combine(skillPath, "SKILL.md");
    if (!File.Exists(skillMdPath))
    {
        Console.WriteLine($"❌ Error: SKILL.md not found in {skillPath}");
        return null;
    }

    Console.WriteLine("🔍 Validating skill...");
    var (isValid, message) = ValidateSkill(skillPath);
    if (!isValid)
    {
        Console.WriteLine($"❌ Validation failed: {message}");
        Console.WriteLine("   Please fix the validation errors before packaging.");
        return null;
    }
    Console.WriteLine($"✅ {message}\n");

    var skillName = Path.GetFileName(skillPath);
    string outputPath;
    
    if (!string.IsNullOrEmpty(outputDir))
    {
        outputPath = Path.GetFullPath(outputDir);
        Directory.CreateDirectory(outputPath);
    }
    else
    {
        outputPath = Directory.GetCurrentDirectory();
    }

    var skillFilename = Path.Combine(outputPath, $"{skillName}.skill");

    try
    {
        if (File.Exists(skillFilename))
            File.Delete(skillFilename);

        using var zipArchive = ZipFile.Open(skillFilename, ZipArchiveMode.Create);
        
        foreach (var filePath in Directory.GetFiles(skillPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(Path.GetDirectoryName(skillPath)!, filePath);
            zipArchive.CreateEntryFromFile(filePath, relativePath.Replace('\\', '/'));
            Console.WriteLine($"  Added: {relativePath}");
        }

        Console.WriteLine($"\n✅ Successfully packaged skill to: {skillFilename}");
        return skillFilename;
    }
    catch (Exception e)
    {
        Console.WriteLine($"❌ Error creating .skill file: {e.Message}");
        return null;
    }
}

// Main
if (args.Length < 1)
{
    Console.WriteLine("Usage: dotnet run package-skill.cs <path/to/skill-folder> [output-directory]");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  dotnet run package-skill.cs skills/public/my-skill");
    Console.WriteLine("  dotnet run package-skill.cs skills/public/my-skill ./dist");
    return 1;
}

var skillPath = args[0];
var outputDir = args.Length > 1 ? args[1] : null;

Console.WriteLine($"📦 Packaging skill: {skillPath}");
if (outputDir != null)
    Console.WriteLine($"   Output directory: {outputDir}");
Console.WriteLine();

var result = PackageSkill(skillPath, outputDir);

return result != null ? 0 : 1;
