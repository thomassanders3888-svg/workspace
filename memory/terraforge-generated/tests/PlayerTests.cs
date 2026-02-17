using Xunit;

public class PlayerTests {
    [Fact]
    public void TakeDamage_ReducesHealth() {
        var player = new Player { Health = 100 };
        player.TakeDamage(25);
        Assert.Equal(75, player.Health);
    }
    
    [Fact]
    public void Heal_IncreasesHealth() {
        var player = new Player { Health = 50 };
        player.Heal(30);
        Assert.Equal(80, player.Health);
    }
    
    [Fact]
    public void Die_HealthGoesToZero() {
        var player = new Player { Health = 10 };
        player.TakeDamage(20);
        Assert.True(player.IsDead);
    }
}
