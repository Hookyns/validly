using Validly.SourceGenerator.Utils.Mapping;

namespace Validly.SourceGenerator.Builders;

public class DependenciesTracker
{
	private readonly HashSet<DependencyInjectionInfo> _services = new();

	public bool HasDependencies => _services.Count > 0;

	public IReadOnlyCollection<DependencyInjectionInfo> Services => _services;

	public void AddDependency(DependencyInjectionInfo dependency)
	{
		if (
			dependency.Name
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
