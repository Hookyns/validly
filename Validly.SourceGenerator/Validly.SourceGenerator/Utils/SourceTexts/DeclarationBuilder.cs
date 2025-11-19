using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Validly.SourceGenerator.Utils.SourceTexts.FileBuilders;

namespace Validly.SourceGenerator.Utils.SourceTexts;

internal class DeclarationBuilder
{
	private readonly List<string> _usings = new();
	private string? _namespace;

	/// <summary>
	/// Class | Record | Interface
	/// </summary>
	private string _declarationKeyword = "class";

	private readonly List<string> _baseClasses = new();
	private readonly List<string> _interfaces = new();
	private readonly List<SourceTextSectionBuilder> _memberSources = new();
	private string _name = string.Empty;
	private string _partial = string.Empty;
	private string _accessModifier = "public";
	private string _static = string.Empty;

	private DeclarationBuilder() { }

	public static DeclarationBuilder CreateClassOrRecord(string keyword, string name)
	{
		var builder = new DeclarationBuilder { _name = name, _declarationKeyword = keyword };

		return builder;
	}

	public static DeclarationBuilder CreateInterface(string name)
	{
		var builder = new DeclarationBuilder { _name = name, _declarationKeyword = "interface" };

		return builder;
	}

	public static DeclarationBuilder CreateClass(string name)
	{
		var builder = new DeclarationBuilder { _name = name, _declarationKeyword = "class" };

		return builder;
	}

	public static DeclarationBuilder CreateRecord(string name)
	{
		var builder = new DeclarationBuilder { _name = name, _declarationKeyword = "record" };

		return builder;
	}

	public DeclarationBuilder AddUsings(params string[] usings)
	{
		_usings.AddRange(usings);
		return this;
	}

	public DeclarationBuilder SetNamespace(string @namespace)
	{
		_namespace = @namespace;
		return this;
	}

	public DeclarationBuilder SetAccessModifier(string accessModifier)
	{
		_accessModifier = accessModifier;
		return this;
	}

	public DeclarationBuilder SetAccessModifier(Accessibility accessModifier)
	{
		_accessModifier = accessModifier switch
		{
			Accessibility.Public => "public",
			Accessibility.Private => "private",
			Accessibility.Protected => "protected",
			Accessibility.Internal => "internal",
			Accessibility.ProtectedOrInternal => "protected internal",
			Accessibility.ProtectedAndInternal => "private protected",
			_ => "public",
		};

		return this;
	}

	public DeclarationBuilder AddBaseClasses(params string[] baseClasses)
	{
		_baseClasses.AddRange(baseClasses);
		return this;
	}

	public DeclarationBuilder AddInterfaces(params string[] interfaces)
	{
		_interfaces.AddRange(interfaces);
		return this;
	}

	public DeclarationBuilder AddMember(SourceTextSectionBuilder memberSource)
	{
		_memberSources.Add(memberSource);
		return this;
	}

	public DeclarationBuilder AddMemberIf(SourceTextSectionBuilder memberSource, bool condition)
	{
		if (!condition)
		{
			return this;
		}

		_memberSources.Add(memberSource);
		return this;
	}

	public DeclarationBuilder Partial()
	{
		_partial = "partial";
		return this;
	}

	public DeclarationBuilder Static()
	{
		_static = "static";
		return this;
	}

	private static readonly Regex MultiWhitespaceReplaceRegex = new(@"\s+", RegexOptions.Compiled);

	public string Build()
	{
		var builder = new FileBuilder();

		// Add USINGs
		AppendUsings(builder);

		var classBuilder = new FileBuilder().AppendLine(
			MultiWhitespaceReplaceRegex.Replace(
				$"{_accessModifier} {_static} {_partial} {_declarationKeyword} {_name}",
				" "
			)
		);

		// Add BASE CLASS and/or INTERFACEs
		AppendBaseTypes(classBuilder);

		// { CLASS body START
		classBuilder.AppendLine("{");

		// Add MEMBERs
		foreach (SourceTextSectionBuilder memberSource in _memberSources)
		{
			memberSource.Indent();
			classBuilder.AddPart(memberSource);
		}

		classBuilder.AddPart();

		// } CLASS body END
		classBuilder.AppendLine("}");

		// Add class to a namespace
		builder.Concat(WithNamespace(classBuilder));

		return builder.ToString();
	}

	private FileBuilder WithNamespace(FileBuilder code)
	{
		if (string.IsNullOrWhiteSpace(_namespace))
		{
			return code;
		}

		var namespaceBuilder = new FileBuilder().AppendLine($"namespace {_namespace}").AppendLine("{");

		foreach (SourceTextSectionBuilder part in code.Parts)
		{
			part.Indent();
		}

		namespaceBuilder.Concat(code).AppendLine("}");

		return namespaceBuilder;
	}

	private void AppendBaseTypes(FileBuilder builder)
	{
		if (_baseClasses.Count > 0 || _interfaces.Count > 0)
		{
			builder.Append("\t: ");
		}
		else
		{
			return;
		}

		builder.Append(string.Join(", ", _baseClasses.Concat(_interfaces)));
		builder.AppendLine();
	}

	private void AppendUsings(FileBuilder builder)
	{
		if (_usings.Any())
		{
			builder.AppendLine("#pragma warning disable CS0105");
		}

		foreach (var @using in _usings)
		{
			builder.AppendLine(@using.StartsWith("using ") ? @using : $"using {@using};");
		}

		if (_usings.Any())
		{
			builder.AppendLine("#pragma warning restore CS0105");
		}

		builder.AppendLine();
	}
}
