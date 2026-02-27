using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.Validation;

/// <summary>
/// Collects <see cref="ValidationResult"/> instances from multiple validation
/// passes and provides query methods for filtering by member name, severity,
/// and scope.
/// </summary>
public sealed class ValidationResultAggregator
{
    private readonly List<(ValidationResult Result, ValidationSeverity Severity)> _entries = [];

    /// <summary>
    /// Gets the total number of collected results.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Gets a value indicating whether any results have been collected.
    /// </summary>
    public bool HasResults => _entries.Count > 0;

    /// <summary>
    /// Gets a value indicating whether any error-level results have been collected.
    /// </summary>
    public bool HasErrors => _entries.Exists(e => e.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Adds a <see cref="ValidationResult"/> with the specified severity.
    /// Results equal to <see cref="ValidationResult.Success"/> are ignored.
    /// </summary>
    /// <param name="result">The validation result.</param>
    /// <param name="severity">The severity of the result.</param>
    public void Add(ValidationResult? result, ValidationSeverity severity = ValidationSeverity.Error)
    {
        if (result is null || result == ValidationResult.Success)
            return;

        _entries.Add((result, severity));
    }

    /// <summary>
    /// Adds multiple <see cref="ValidationResult"/> instances with the specified severity.
    /// </summary>
    /// <param name="results">The validation results.</param>
    /// <param name="severity">The severity for all results.</param>
    public void AddRange(IEnumerable<ValidationResult> results, ValidationSeverity severity = ValidationSeverity.Error)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
            Add(result, severity);
    }

    /// <summary>
    /// Gets all collected results.
    /// </summary>
    /// <returns>A read-only list of results with their severities.</returns>
    public IReadOnlyList<(ValidationResult Result, ValidationSeverity Severity)> GetAll() =>
        _entries.ToList();

    /// <summary>
    /// Gets results filtered by severity.
    /// </summary>
    /// <param name="severity">The severity to filter by.</param>
    /// <returns>Matching results.</returns>
    public IReadOnlyList<ValidationResult> GetBySeverity(ValidationSeverity severity) =>
        _entries.Where(e => e.Severity == severity).Select(e => e.Result).ToList();

    /// <summary>
    /// Gets results that apply to the specified member.
    /// </summary>
    /// <param name="memberName">The member name to filter by.</param>
    /// <returns>Matching results.</returns>
    public IReadOnlyList<ValidationResult> GetByMember(string memberName)
    {
        ArgumentNullException.ThrowIfNull(memberName);

        return _entries
            .Where(e => e.Result.MemberNames.Contains(memberName, StringComparer.Ordinal))
            .Select(e => e.Result)
            .ToList();
    }

    /// <summary>
    /// Gets all distinct member names that have at least one result.
    /// </summary>
    /// <returns>The distinct member names.</returns>
    public IReadOnlyList<string> GetAffectedMembers() =>
        _entries
            .SelectMany(e => e.Result.MemberNames)
            .Distinct(StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Converts all collected results to <see cref="ErrorInfo"/> instances.
    /// </summary>
    /// <returns>A list of <see cref="ErrorInfo"/> instances.</returns>
    public IReadOnlyList<ErrorInfo> ToErrorInfos() =>
        _entries.SelectMany(e =>
        {
            var members = e.Result.MemberNames.ToList();

            if (members.Count == 0)
                return [new ErrorInfo(e.Result.ErrorMessage ?? "Validation failed.", e.Severity)];

            return members.Select(m =>
                new ErrorInfo(e.Result.ErrorMessage ?? "Validation failed.", e.Severity, m));
        }).ToList();

    /// <summary>
    /// Removes all collected results.
    /// </summary>
    public void Clear() => _entries.Clear();
}
