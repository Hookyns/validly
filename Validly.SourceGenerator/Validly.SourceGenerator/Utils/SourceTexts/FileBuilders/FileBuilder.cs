using System.Text;

namespace Validly.SourceGenerator.Utils.SourceTexts.FileBuilders;

internal class FileBuilder
{
	private readonly List<SourceTextSectionBuilder> _parts = new() { new SourceTextSectionBuilder() };

	private SourceTextSectionBuilder CurrentPart => _parts[_parts.Count - 1];

	public List<SourceTextSectionBuilder> Parts => _parts;

	public FileBuilder Append(string text)
	{
		CurrentPart.Append(text);
		return this;
	}

	public FileBuilder AppendLine(string text)
	{
		CurrentPart.AppendLine(text);
		return this;
	}

	public FileBuilder AppendLine()
	{
		CurrentPart.AppendLine();
		return this;
	}

	public FileBuilder IndentCurrentPart(int count = 1)
	{
		CurrentPart.Indent(count);
		return this;
	}

	public FileBuilder AddPart(SourceTextSectionBuilder? filePart = null)
	{
		_parts.Add(filePart ?? new SourceTextSectionBuilder());
		return this;
	}

	public FileBuilder Concat(FileBuilder builder)
	{
		foreach (SourceTextSectionBuilder part in builder._parts)
		{
			_parts.Add(part);
		}

		AddPart();

		return this;
	}

	public override string ToString()
	{
		var builder = new StringBuilder();

		foreach (SourceTextSectionBuilder part in _parts)
		{
			for (int i = 0; i < part.Lines.Count - 1; i++)
			{
				// Do not add an extra line after Part end; so the first line of next Part can be appended to last line of previous Part
				if (i == part.Lines.Count - 1)
				{
					builder.Append(part.Lines[i]);
				}
				else
				{
					builder.AppendLine(part.Lines[i].ToString());
				}
			}
		}

		return builder.ToString();
	}
}
