using System.Text.Json.Serialization;

namespace ShawzinBot.Models;

public class GitVersion
{
	[JsonPropertyName("tag_name")] public string TagName { get; set; }

	[JsonPropertyName("draft")] public bool Draft { get; set; }

	[JsonPropertyName("prerelease")] public bool Prerelease { get; set; }
}