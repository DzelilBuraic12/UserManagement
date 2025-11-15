using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
       
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<User>(entity =>
                {
                    entity.Property(e => e.Email)
                    .IsRequired();
                    entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                    entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                    entity.Property(e => e.Role)
                    .HasMaxLength(30)
                    .HasDefaultValue("User");
                });

                modelBuilder.Entity<Request>()
                .HasOne(r => r.CreatedBy)
                .WithMany(r => r.CreatedRequests)
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Technician)
                .WithMany(r => r.AssignedRequests)
                .HasForeignKey(r => r.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Status)
                .WithMany(r => r.Requests)
                .HasForeignKey(r => r.StatusId);


            modelBuilder.Entity<RequestStatus>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<RequestStatus>().HasData(
                new RequestStatus { Id = 1, Name = "Open" ,  Order = 1, CreatedAt = new DateTime(2025, 10, 22, 0, 0, 0, DateTimeKind.Utc) },
                new RequestStatus { Id = 2, Name = "InProgress",  Order = 2, CreatedAt = new DateTime(2025, 10, 22, 0, 0, 0, DateTimeKind.Utc) },
                new RequestStatus { Id = 3, Name = "Resolved", Order = 3, CreatedAt = new DateTime(2025, 10, 22, 0, 0, 0, DateTimeKind.Utc) },
                new RequestStatus { Id = 4, Name = "Closed", Order = 4, CreatedAt = new DateTime(2025, 10, 22, 0, 0, 0, DateTimeKind.Utc) } 
                );

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(rt => rt.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            }
    }
}
