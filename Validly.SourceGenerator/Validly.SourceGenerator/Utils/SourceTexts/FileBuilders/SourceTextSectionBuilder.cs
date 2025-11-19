using System.Text;

namespace Validly.SourceGenerator.Utils.SourceTexts.FileBuilders;

public class SourceTextSectionBuilder
{
	private static readonly string[] NewLineSeparators = new[] { Environment.NewLine };
	private readonly List<StringBuilder> _lines = new() { new StringBuilder() };

	private StringBuilder CurrentLine => _lines[_lines.Count - 1];

	public List<StringBuilder> Lines => _lines;

	public SourceTextSectionBuilder Append(string text)
	{
		var lines = text.Split(NewLineSeparators, StringSplitOptions.None);

		if (lines.Length > 1)
		{
			for (var i = 0; i < lines.Length - 1; i++)
			{
				AppendLine(lines[i]);
			}
		}

		CurrentLine.Append(lines[lines.Length - 1]);

		return this;
	}

	public SourceTextSectionBuilder AppendIf(string text, bool condition)
	{
		return condition ? Append(text) : this;
	}

	public SourceTextSectionBuilder AppendLine(string text)
	{
		var lines = text.Split(NewLineSeparators, StringSplitOptions.None);

		for (var i = 0; i < lines.Length; i++)
		{
			CurrentLine.Append(lines[i]);
			AppendLine();
		}

		return this;
	}

	public SourceTextSectionBuilder AppendLineIf(string text, bool condition)
	{
		return condition ? AppendLine(text) : this;
	}

	public SourceTextSectionBuilder AppendLine()
	{
		_lines.Add(new StringBuilder());
		return this;
	}

	public SourceTextSectionBuilder Indent(int count = 1)
	{
		foreach (StringBuilder line in _lines)
		{
			line.Insert(0, new string('\t', count));
		}

		return this;
	}

	public override string ToString() => string.Join(Environment.NewLine, _lines.Select(x => x.ToString()));
}
