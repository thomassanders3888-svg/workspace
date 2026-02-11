using Npgsql;
using System.IO.Compression;

namespace TerraForgeServer.Data;

/// <summary>
/// Storage for voxel chunk data with compression support
/// </summary>
public class ChunkStorage
{
    private readonly DatabaseManager _db;
    private readonly Dictionary<string, byte[]> _cache;
    private readonly int _maxCacheSize;
    private readonly string _worldName;

    public ChunkStorage(DatabaseManager db, string worldName = "world", int maxCacheSize = 1000)
    {
        _db = db;
        _worldName = worldName;
        _maxCacheSize = maxCacheSize;
        _cache = new Dictionary<string, byte[]>();
    }

    private string GetCacheKey(int x, int y, int z) => $"{_worldName}:{x},{y},{z}";

    public async Task<byte[]?> LoadChunkAsync(int x, int y, int z)
    {
        string key = GetCacheKey(x, y, z);
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var parameters = new NpgsqlParameter[]
        {
            new("@x", x),
            new("@y", y),
            new("@z", z),
            new("@world", _worldName)
        };

        var row = await _db.QueryRowAsync(
            "SELECT data, compressed FROM chunks WHERE x_coord = @x AND y_coord = @y AND z_coord = @z AND world = @world",
            parameters);

        if (row.Count == 0 || !row.ContainsKey("data"))
            return null;

        byte[] data = (byte[])row["data"]!;
        bool compressed = (bool)(row["compressed"] ?? false);

        if (compressed)
        {
            try
            {
                data = Decompress(data);
            }
            catch
            {
                // Log decompression error
            }
        }

        if (_cache.Count < _maxCacheSize)
            _cache[key] = data;

        return data;
    }

    public async Task<bool> SaveChunkAsync(int x, int y, int z, byte[] data, bool compress = true)
    {
        string key = GetCacheKey(x, y, z);
        byte[] storedData = compress ? Compress(data) : data;

        var parameters = new NpgsqlParameter[]
        {
            new("@x", x),
            new("@y", y),
            new("@z", z),
            new("@world", _worldName),
            new("@data", storedData),
            new("@compressed", compress),
            new("@now", DateTime.UtcNow)
        };

        const string sql = @"
            INSERT INTO chunks (x_coord, y_coord, z_coord, world, data, compressed, modified_at)
            VALUES (@x, @y, @z, @world, @data, @compressed, @now)
            ON CONFLICT (x_coord, y_coord, z_coord, world)
            DO UPDATE SET data = @data, compressed = @compressed, modified_at = @now";

        int affected = await _db.ExecuteAsync(sql, parameters);

        if (affected > 0 && _cache.Count < _maxCacheSize)
            _cache[key] = data;

        return affected > 0;
    }

    public async Task<bool> ChunkExistsAsync(int x, int y, int z)
    {
        string key = GetCacheKey(x, y, z);
        if (_cache.ContainsKey(key))
            return true;

        var result = await _db.QuerySingleAsync<long>(
            "SELECT COUNT(*) FROM chunks WHERE x_coord = @x AND y_coord = @y AND z_coord = @z AND world = @world",
            new NpgsqlParameter[] { new("@x", x), new("@y", y), new("@z", z), new("@world", _worldName) });

        return result > 0;
    }

    public async Task<bool> DeleteChunkAsync(int x, int y, int z)
    {
        string key = GetCacheKey(x, y, z);
        _cache.Remove(key);

        int affected = await _db.ExecuteAsync(
            "DELETE FROM chunks WHERE x_coord = @x AND y_coord = @y AND z_coord = @z AND world = @world",
            new NpgsqlParameter[] { new("@x", x), new("@y", y), new("@z", z), new("@world", _worldName) });

        return affected > 0;
    }

    public async Task<List<(int x, int y, int z)>> GetChunksInRegionAsync(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        var results = await _db.QueryAsync(
            "SELECT x_coord, y_coord, z_coord FROM chunks WHERE world = @world AND x_coord BETWEEN @minX AND @maxX AND y_coord BETWEEN @minY AND @maxY AND z_coord BETWEEN @minZ AND @maxZ",
            new NpgsqlParameter[]
            {
                new("@world", _worldName),
                new("@minX", minX), new("@maxX", maxX),
                new("@minY", minY), new("@maxY", maxY),
                new("@minZ", minZ), new("@maxZ", maxZ)
            });

        return results.Select(r => (
            Convert.ToInt32(r["x_coord"]),
            Convert.ToInt32(r["y_coord"]),
            Convert.ToInt32(r["z_coord"])
        )).ToList();
    }

    public void InvalidateCache(int x, int y, int z)
    {
        _cache.Remove(GetCacheKey(x, y, z));
    }

    public void ClearCache() => _cache.Clear();

    private static byte[] Compress(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, CompressionLevel.Fastest))
            gzip.Write(data, 0, data.Length);
        return ms.ToArray();
    }

    private static byte[] Decompress(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var gzip = new GZipStream(ms, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        gzip.CopyTo(outMs);
        return outMs.ToArray();
    }
}
