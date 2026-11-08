using System.Numerics;
using System.Runtime.InteropServices;
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

		return NeedsEscaping(value!) ? EscapeCore(value!) : value!;
	}

	/// <summary>
	/// Detects the quote, the backslash and the control characters; vectorized when supported
	/// </summary>
	private static bool NeedsEscaping(string value)
	{
		ReadOnlySpan<ushort> characters = MemoryMarshal.Cast<char, ushort>(value.AsSpan());
		int index = 0;

		if (Vector.IsHardwareAccelerated && characters.Length >= Vector<ushort>.Count)
		{
			var space = new Vector<ushort>(' ');
			var quote = new Vector<ushort>('"');
			var backslash = new Vector<ushort>('\\');
			ReadOnlySpan<Vector<ushort>> vectors = MemoryMarshal.Cast<ushort, Vector<ushort>>(characters);

			for (int vectorIndex = 0; vectorIndex < vectors.Length; vectorIndex++)
			{
				Vector<ushort> vector = vectors[vectorIndex];
				Vector<ushort> matches =
					Vector.LessThan(vector, space) | Vector.Equals(vector, quote) | Vector.Equals(vector, backslash);

				if (matches != Vector<ushort>.Zero)
				{
					return true;
				}
			}

			index = vectors.Length * Vector<ushort>.Count;
		}

		for (; index < characters.Length; index++)
		{
			ushort character = characters[index];

			if (character < ' ' || character == '"' || character == '\\')
			{
				return true;
			}
		}

		return false;
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
