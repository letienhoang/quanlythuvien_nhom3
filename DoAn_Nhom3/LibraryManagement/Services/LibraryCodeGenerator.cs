using System.Linq.Expressions;
using System.Text.RegularExpressions;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public interface ILibraryCodeGenerator
{
    Task<string> GenerateNextAsync<TEntity>(
        Expression<Func<TEntity, string>> idSelector,
        string prefix,
        int digits,
        int offset = 0)
        where TEntity : class;

    Task<string> GenerateNextWithRetriesAsync<TEntity>(
        Expression<Func<TEntity, string>> idSelector,
        string prefix,
        int digits,
        int maxAttempts = 10)
        where TEntity : class;
}

public class LibraryCodeGenerator : ILibraryCodeGenerator
{
    private readonly LibraryDbContext _db;
    
    public LibraryCodeGenerator(LibraryDbContext db)
    {
        _db = db;
    }
    
    public async Task<string> GenerateNextAsync<TEntity>(
        Expression<Func<TEntity, string>> idSelector,
        string prefix,
        int digits,
        int offset = 0)
        where TEntity : class
    {
        var allIds = await _db.Set<TEntity>()
            .Select(idSelector)
            .ToListAsync();

        int maxNum = 0;
        var rx = new Regex(@"(\d+)$");

        foreach (var id in allIds)
        {
            if (string.IsNullOrWhiteSpace(id)) continue;
            if (!string.IsNullOrEmpty(prefix) && !id.StartsWith(prefix)) continue;

            var m = rx.Match(id);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int val))
            {
                if (val > maxNum) maxNum = val;
            }
        }

        int next = maxNum + 1 + offset;
        return $"{prefix}{next.ToString($"D{digits}")}";
    }

    public async Task<string> GenerateNextWithRetriesAsync<TEntity>(
        Expression<Func<TEntity, string>> idSelector,
        string prefix,
        int digits,
        int maxAttempts = 10)
        where TEntity : class
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var candidate = await GenerateNextAsync(idSelector, prefix, digits, offset: attempt);
            bool exists = await _db.Set<TEntity>()
                .Select(idSelector)
                .AnyAsync(x => x == candidate);
            if (!exists) return candidate;
        }
        throw new InvalidOperationException("Không tạo được mã duy nhất sau nhiều lần thử.");
    }
}