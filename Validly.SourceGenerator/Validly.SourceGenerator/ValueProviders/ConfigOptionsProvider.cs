using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Validly.Naming;

namespace Validly.SourceGenerator.ValueProviders;

internal static class ConfigOptionsProvider
{
	private static readonly XmlSerializer Serializer = new(typeof(RawValidlyConfiguration));

	public static IncrementalValueProvider<ValidlyConfiguration> Get(
		IncrementalGeneratorInitializationContext initContext
	)
	{
		return initContext
			.AnalyzerConfigOptionsProvider.Select(ReadRawConfigurationOptions)
			.Select(ToValidlyConfiguration);
	}

	private static string? ReadRawConfigurationOptions(
		AnalyzerConfigOptionsProvider provider,
		CancellationToken cancellationToken
	)
	{
		return provider.GlobalOptions.TryGetValue("build_property.ValidlyConfiguration", out string? configuration)
			? configuration
			: null;
	}

	private static ValidlyConfiguration ToValidlyConfiguration(
		string? configurationXml,
		CancellationToken cancellationToken
	)
	{
		RawValidlyConfiguration raw = Read(configurationXml);
		ValidlyNamingPolicy? namingPolicy = ToNamingPolicy(raw.JsonNamingPolicy);

		return new ValidlyConfiguration
		{
			AutoRequired = ToBool(raw.AutoRequired ?? "enabled"),
			AutoInEnum = ToBool(raw.AutoInEnum ?? "enabled"),
			ExitEarly = ToBool(raw.ExitEarly),
			JsonNamingPolicy = namingPolicy,
			UnknownJsonNamingPolicy =
				namingPolicy is null && !string.IsNullOrWhiteSpace(raw.JsonNamingPolicy) ? raw.JsonNamingPolicy : null,
		};
	}

	private static RawValidlyConfiguration Read(string? configuration)
	{
		try
		{
			if (configuration != null)
			{
				return Deserialize(configuration);
			}
		}
		catch
		{
			// Ignore
		}

		return new() { AutoRequired = "enable", AutoInEnum = "enable" };
	}

	private static RawValidlyConfiguration Deserialize(string configuration)
	{
		using var stringReader = new StringReader(
			$"<RawValidlyConfiguration>{configuration}</RawValidlyConfiguration>"
		);
		using var reader = new XmlTextReader(stringReader);
		return (RawValidlyConfiguration)Serializer.Deserialize(reader);
	}

	/// <summary>
	/// Parses the configured naming policy; null when it is not configured or not recognized
	/// </summary>
	private static ValidlyNamingPolicy? ToNamingPolicy(string? value)
	{
		if (value is null)
		{
			return null;
		}

		switch (value.Trim().Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant())
		{
			case "none":
			case "asdeclared":
				return ValidlyNamingPolicy.None;

			case "camelcase":
				return ValidlyNamingPolicy.CamelCase;

			case "snakecase":
			case "snakecaselower":
				return ValidlyNamingPolicy.SnakeCaseLower;

			case "snakecaseupper":
				return ValidlyNamingPolicy.SnakeCaseUpper;

			case "kebabcase":
			case "kebabcaselower":
				return ValidlyNamingPolicy.KebabCaseLower;

			case "kebabcaseupper":
				return ValidlyNamingPolicy.KebabCaseUpper;

			default:
				return null;
		}
	}

	private static bool ToBool(string? value)
	{
		return value is not null
			&& (
				value.Equals("enable", StringComparison.OrdinalIgnoreCase)
				|| value.Equals("enabled", StringComparison.OrdinalIgnoreCase)
				|| value.Equals("true", StringComparison.OrdinalIgnoreCase)
			);
	}
}

public class RawValidlyConfiguration
{
	public string? AutoRequired { get; set; }
	public string? AutoInEnum { get; set; }
	public string? ExitEarly { get; set; }
	public string? JsonNamingPolicy { get; set; }
}
