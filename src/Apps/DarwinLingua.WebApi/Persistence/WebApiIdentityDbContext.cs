using DarwinLingua.Identity;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Persistence;

public sealed class WebApiIdentityDbContext(DbContextOptions<WebApiIdentityDbContext> options)
    : DarwinLinguaIdentityDbContext(options)
{
}
