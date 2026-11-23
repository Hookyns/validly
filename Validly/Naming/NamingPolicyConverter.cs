using System.Globalization;
using System.Text;

namespace Validly.Naming;

/// <summary>
/// Converts property names according to the <see cref="ValidlyNamingPolicy"/>
/// </summary>
/// <remarks>
/// This file is shared with the source generator (linked), so the names can be converted at compile time
/// and the runtime does not have to call into System.Text.Json at all.
/// The algorithms are ports of the System.Text.Json policies (JsonCamelCaseNamingPolicy and
/// JsonSeparatorNamingPolicy), so the results are the same. Parity is covered by tests.
/// </remarks>
internal static class NamingPolicyConverter
{
	public static string Convert(ValidlyNamingPolicy policy, string name)
	{
		switch (policy)
		{
			case ValidlyNamingPolicy.CamelCase:
				return ToCamelCase(name);

			case ValidlyNamingPolicy.SnakeCaseLower:
				return ToSeparatorCase(name, '_', lowercase: true);

			case ValidlyNamingPolicy.SnakeCaseUpper:
				return ToSeparatorCase(name, '_', lowercase: false);

			case ValidlyNamingPolicy.KebabCaseLower:
				return ToSeparatorCase(name, '-', lowercase: true);

			case ValidlyNamingPolicy.KebabCaseUpper:
				return ToSeparatorCase(name, '-', lowercase: false);

			default:
				return name;
		}
	}

	/// <summary>
	/// Port of System.Text.Json.JsonCamelCaseNamingPolicy
	/// </summary>
	private static string ToCamelCase(string name)
	{
		if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
		{
			return name;
		}

		char[] chars = name.ToCharArray();

		for (int index = 0; index < chars.Length; index++)
		{
			if (index == 1 && !char.IsUpper(chars[index]))
			{
				break;
			}

			bool hasNext = index + 1 < chars.Length;

			// Stop when next char is already lowercase.
			if (index > 0 && hasNext && !char.IsUpper(chars[index + 1]))
			{
				// If the next char is a space, lowercase current char before exiting.
				if (chars[index + 1] == ' ')
				{
					chars[index] = char.ToLowerInvariant(chars[index]);
				}

				break;
			}

			chars[index] = char.ToLowerInvariant(chars[index]);
		}

		return new string(chars);
	}

	/// <summary>
	/// Port of System.Text.Json.JsonSeparatorNamingPolicy
	/// </summary>
	private static string ToSeparatorCase(string name, char separator, bool lowercase)
	{
		if (name.Length == 0)
		{
			return name;
		}

		var sb = new StringBuilder(name.Length + (name.Length / 2));
		SeparatorState state = SeparatorState.NotStarted;

		for (int index = 0; index < name.Length; index++)
		{
			// NB this implementation does not handle surrogate pair letters, the same way System.Text.Json does not.
			char current = name[index];

			switch (CharUnicodeInfo.GetUnicodeCategory(current))
			{
				case UnicodeCategory.UppercaseLetter:
					switch (state)
					{
						case SeparatorState.NotStarted:
							break;

						case SeparatorState.LowercaseLetterOrDigit:
						case SeparatorState.SpaceSeparator:
							// An uppercase letter following a sequence of lowercase letters or spaces
							// denotes the start of a new grouping: emit a separator character.
							sb.Append(separator);
							break;

						case SeparatorState.UppercaseLetter:
							// We are reading through a sequence of two or more uppercase letters.
							// Uppercase letters are grouped together with the exception of the
							// final letter, assuming it is followed by lowercase letters.
							// For example, the value 'XMLReader' should render as 'xml_reader',
							// however 'SHA512Hash' should render as 'sha512_hash'.
							if (index + 1 < name.Length && char.IsLower(name[index + 1]))
							{
								sb.Append(separator);
							}
							break;
					}

					sb.Append(lowercase ? char.ToLowerInvariant(current) : current);
					state = SeparatorState.UppercaseLetter;
					break;

				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.DecimalDigitNumber:
					if (state == SeparatorState.SpaceSeparator)
					{
						// Normalize preceding spaces to one separator.
						sb.Append(separator);
					}

					sb.Append(lowercase ? current : char.ToUpperInvariant(current));
					state = SeparatorState.LowercaseLetterOrDigit;
					break;

				case UnicodeCategory.SpaceSeparator:
					// Space characters are trimmed from the start and end of the input string
					// but are normalized to separator characters if between letters.
					if (state != SeparatorState.NotStarted)
					{
						state = SeparatorState.SpaceSeparator;
					}
					break;

				default:
					// Non-alphanumeric characters (including the separator character and surrogates)
					// are written as-is to the output and reset the separator state.
					sb.Append(current);
					state = SeparatorState.NotStarted;
					break;
			}
		}

		return sb.ToString();
	}

	private enum SeparatorState
	{
		NotStarted,
		UppercaseLetter,
		LowercaseLetterOrDigit,
		SpaceSeparator,
	}
}
