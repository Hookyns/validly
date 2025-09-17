namespace Validly.SourceGenerator.Builders;

public class DependenciesTracker
{
	private readonly HashSet<string> _services = new();

	public bool HasDependencies => _services.Count > 0;

	public IReadOnlyCollection<string> Services => _services;

	public void AddDependency(string dependency)
	{
		if (
			dependency
			is Consts.ValidationContextName
				or Consts.ValidationContextQualifiedName
				or Consts.ValidationResultName
				or Consts.ValidationResultQualifiedName
				or Consts.ExtendableValidationResultName
				or Consts.ExtendableValidationResultQualifiedName
				or Consts.CancellationTokenName
		)
		{
			return;
		}

		_services.Add(dependency);
	}
}
