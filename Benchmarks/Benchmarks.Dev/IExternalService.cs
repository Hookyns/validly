namespace Benchmarks.Dev;

public interface IExternalService
{
	Task<bool> IsValidValue(string integrationField, CancellationToken ct = default);
}