using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AddressBook.Models;

namespace AddressBook.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contact> Contacts { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;

}
