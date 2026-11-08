using System.Text;

namespace Validly.Utils;

/// <summary>
/// Minimal JSON string escaping; it keeps System.Text.Json out of the problem details generation
/// </summary>
internal static class JsonEscaper
{
	/// <summary>
	/// Escapes the value so it can be placed inside a JSON string literal
	/// </summary>
	/// <remarks>
	/// The returned instance is the very same string when there is nothing to escape, so the common case
	/// does not allocate. The quote, the backslash and all the control characters (below U+0020) are escaped.
	/// </remarks>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string Escape(string? value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}

		for (int index = 0; index < value!.Length; index++)
		{
			char character = value[index];

			if (character < ' ' || character == '"' || character == '\\')
			{
				return EscapeCore(value);
			}
		}

		return value;
	}

	private static string EscapeCore(string value)
	{
		var sb = new StringBuilder(value.Length + 8);

		for (int index = 0; index < value.Length; index++)
		{
			char character = value[index];

			switch (character)
			{
				case '"':
					sb.Append("\\\"");
					break;

				case '\\':
					sb.Append("\\\\");
					break;

				case '\n':
					sb.Append("\\n");
					break;

				case '\r':
					sb.Append("\\r");
					break;

				case '\t':
					sb.Append("\\t");
					break;

				case '\b':
					sb.Append("\\b");
					break;

				case '\f':
					sb.Append("\\f");
					break;

				default:
					if (character < ' ')
					{
						sb.Append("\\u").Append(((int)character).ToString("x4"));
					}
					else
					{
						sb.Append(character);
					}
					break;
			}
		}

		return sb.ToString();
	}
}
