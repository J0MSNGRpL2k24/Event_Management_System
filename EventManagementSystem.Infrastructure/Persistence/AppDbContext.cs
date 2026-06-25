using EventManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EventManagementSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events { get; set; }
    public DbSet<TicketCategory> TicketCategories { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    public DbSet<Refund> Refunds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); 

            
            entity.Ignore(e => e.DomainEvents);

            entity.Property(e => e.Status).HasConversion<string>();

            entity.Metadata.FindNavigation(nameof(Event.Categories))
                  ?.SetPropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(e => e.Categories)
                  .WithOne()
                  .HasForeignKey(c => c.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TicketCategory>(entity =>
        {
            entity.ToTable("TicketCategories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever(); 

            entity.OwnsOne(c => c.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnName("PriceAmount").HasColumnType("decimal(18,2)");
                price.Property(p => p.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).ValueGeneratedNever(); 

            entity.Ignore(b => b.DomainEvents);
            entity.Property(b => b.Status).HasConversion<string>();

            entity.OwnsOne(b => b.TotalPrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("TotalPriceAmount").HasColumnType("decimal(18,2)");
                price.Property(p => p.Currency).HasColumnName("TotalPriceCurrency").HasMaxLength(3);
            });

            entity.Metadata.FindNavigation(nameof(Booking.Tickets))
                  ?.SetPropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(b => b.Tickets)
                  .WithOne()
                  .HasForeignKey(t => t.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedNever(); 

            entity.Ignore(t => t.DomainEvents);
            entity.Property(t => t.Status).HasConversion<string>();

            entity.HasIndex(t => t.TicketCode).IsUnique();
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.ToTable("Refunds");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedNever(); 

            entity.Ignore(r => r.DomainEvents);
            entity.Property(r => r.Status).HasConversion<string>();

            entity.OwnsOne(r => r.Amount, amount =>
            {
                amount.Property(p => p.Amount).HasColumnName("RefundAmount").HasColumnType("decimal(18,2)");
                amount.Property(p => p.Currency).HasColumnName("RefundCurrency").HasMaxLength(3);
            });
        });
    }
}