using Microsoft.EntityFrameworkCore;
using YarnBot.Models;

namespace YarnBot.Data;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Promocode> Promocodes => Set<Promocode>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
           entity.ToTable("Users");
           entity.HasKey(e => e.ClientId);
           entity.HasIndex(e => e.ChatId).IsUnique();
           entity.HasMany(x => x.Promocodes).WithOne(x => x.User).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Promocode>(entity =>
        {
           entity.ToTable("Promocodes");
           entity.HasKey(e => e.PromoId); 
        });
    }
}