using DddApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace DddApi.Context;

public class DddDb : DbContext
{
    public DddDb(DbContextOptions<DddDb> options) 
    : base(options){}

    public DbSet<Ddd> Ddds => Set<Ddd>();

     protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Ddd>()
            .ToTable("ddd") 
            .HasKey(c => c.Id);


        builder.Entity<Ddd>()
        .Property(c => c.Id)
        .ValueGeneratedOnAdd();

        builder.Entity<Ddd>()
            .Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(100); 
        
        builder.Entity<Ddd>()
            .Property(c => c.RegiaoId)
            .IsRequired();
    }
}