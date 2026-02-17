using Microsoft.EntityFrameworkCore;

public class TerraForgeDbContext : DbContext {
    public DbSet<PlayerData> Players { get; set; }
    public DbSet<WorldChunk> Chunks { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<CompletedQuest> CompletedQuests { get; set; }
    
    public TerraForgeDbContext(DbContextOptions<TerraForgeDbContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<PlayerData>().Property(p => p.LastSeen).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<WorldChunk>().HasIndex(c => new { c.X, c.Z });
        modelBuilder.Entity<InventoryItem>().HasIndex(i => i.PlayerId);
    }
}

public class PlayerData {
    public System.Guid Id { get; set; }
    public string Username { get; set; }
    public float X, Y, Z;
    public int Level { get; set; }
    public int Experience { get; set; }
    public System.DateTime LastSeen { get; set; }
}

public class WorldChunk {
    public int Id { get; set; }
    public int X { get; set; }
    public int Z { get; set; }
    public byte[] BlockData { get; set; }
    public System.DateTime LastModified { get; set; }
}

public class InventoryItem {
    public int Id { get; set; }
    public System.Guid PlayerId { get; set; }
    public int Slot { get; set; }
    public byte ItemId { get; set; }
    public int Count { get; set; }
}

public class CompletedQuest {
    public int Id { get; set; }
    public System.Guid PlayerId { get; set; }
    public string QuestId { get; set; }
    public System.DateTime CompletedAt { get; set; }
}
