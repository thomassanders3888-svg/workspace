using Npgsql;
using System.Data;

namespace TerraForgeServer.Data;

/// <summary>
/// Manages PostgreSQL connections and query execution
/// </summary>
public class DatabaseManager : IDisposable, IAsyncDisposable
{
    private readonly string _connectionString;
    private NpgsqlDataSource? _dataSource;
    private bool _disposed;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        _dataSource = new NpgsqlDataSourceBuilder(_connectionString)
            .EnableParameterLogging(false)
            .Build();
        await _dataSource.OpenConnectionAsync();
    }

    public async Task<NpgsqlConnection> GetConnectionAsync()
    {
        if (_dataSource == null)
            throw new InvalidOperationException("Database not initialized");
        return await _dataSource.OpenConnectionAsync();
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = await GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        var result = await cmd.ExecuteScalarAsync();
        return result == null ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<Dictionary<string, object?>> QueryRowAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = await GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return new Dictionary<string, object?>();

        var row = new Dictionary<string, object?>();
        for (int i = 0; i < reader.FieldCount; i++)
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        return row;
    }

    public async Task<List<Dictionary<string, object?>>> QueryAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = await GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        var results = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            results.Add(row);
        }
        return results;
    }

    public async Task<int> ExecuteAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = await GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = await GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        var result = await cmd.ExecuteScalarAsync();
        return result == null ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<T> TransactionAsync<T>(Func<NpgsqlTransaction, Task<T>> action)
    {
        await using var conn = await GetConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var result = await action(tx);
            await tx.CommitAsync();
            return result;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _dataSource?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        if (_dataSource != null)
            await _dataSource.DisposeAsync();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
