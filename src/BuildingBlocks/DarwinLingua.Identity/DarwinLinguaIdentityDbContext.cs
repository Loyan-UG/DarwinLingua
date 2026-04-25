using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Identity;

public class DarwinLinguaIdentityDbContext(DbContextOptions options)
    : IdentityDbContext<DarwinLinguaIdentityUser, IdentityRole, string>(options)
{
}
