using Entity_Framework_Practice.Models;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Data;

public class LibraryDbContext : DbContext
{
    private const string ConnectionString = "Data Source=LibraryDatabase.db";
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberCard> MemberCards => Set<MemberCard>();
    public DbSet<Loan> Loans => Set<Loan>();
    
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
    public LibraryDbContext(){}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(ConnectionString);

        optionsBuilder.EnableSensitiveDataLogging(); //TURN THIS OFF IF THE ENVIRONMENT IS IN PRODUCTION!
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l=>l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l=>l.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MemberCard>()
            .HasOne(c => c.Member)
            .WithOne(m => m.MemberCard)
            .HasForeignKey<MemberCard>(c => c.MemberId);
    }
}