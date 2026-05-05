using System.Text.Json;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

string root = @"D:\_Projects\DarwinLingua";
string[] lemmas = args.Length == 0 ? throw new InvalidOperationException("Pass lemmas.") : args;
using FileStream stream = File.OpenRead(Path.Combine(root, @"src\Apps\DarwinLingua.WebApi\appsettings.Development.json"));
using JsonDocument doc = JsonDocument.Parse(stream);
string connection = doc.RootElement.GetProperty("ConnectionStrings").GetProperty("SharedCatalogAdmin").GetString() ?? throw new InvalidOperationException("Missing connection.");
ServiceCollection services = new();
services.AddDarwinLinguaInfrastructureForPostgres(connection);
await using ServiceProvider provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
await using DarwinLinguaDbContext db = await factory.CreateDbContextAsync();
DateTime now = DateTime.UtcNow;
string[] normalized = lemmas.Select(x => x.Trim().ToLowerInvariant()).ToArray();
List<WordEntry> words = await db.WordEntries.Where(w => normalized.Contains(w.NormalizedLemma) && w.PublicationStatus == PublicationStatus.Active).ToListAsync();
if (words.Count != lemmas.Length) throw new InvalidOperationException($"Resolved {words.Count} of {lemmas.Length} ERP words.");
WordCollection? collection = await db.WordCollections.Include(c => c.Entries).Include(c => c.Localizations).SingleOrDefaultAsync(c => c.Slug == "erp");
string desc = "Practical ERP vocabulary for warehouse, purchasing, sales, finance, and business operations.";
if (collection is null)
{
    collection = new WordCollection(Guid.NewGuid(), "erp", "ERP", desc, "/images/collections/erp-core-b1.svg", PublicationStatus.Active, 5, now);
    db.WordCollections.Add(collection);
}
else
{
    collection.UpdateMetadata("ERP", desc, "/images/collections/erp-core-b1.svg", PublicationStatus.Active, 5, now);
}
List<Guid> ids = collection.Entries.OrderBy(e => e.SortOrder).Select(e => e.WordEntryId).ToList();
foreach (WordEntry word in words.OrderBy(w => Array.IndexOf(normalized, w.NormalizedLemma))) if (!ids.Contains(word.Id)) ids.Add(word.Id);
collection.ReplaceEntries(ids.Select((id, i) => (id, i + 1)).ToArray(), now);
var descs = new Dictionary<string,string>{{"de","Praxisnaher ERP-Wortschatz für Lager, Einkauf, Verkauf, Finanzen und betriebliche Abläufe."},{"ar","مفردات ERP عملية للمستودع والمشتريات والمبيعات والمالية والعمليات."},{"ckb","وشەی کرداری ERP بۆ کۆگا، کڕین، فرۆشتن، دارایی و پرۆسەکانی کار."},{"en",desc},{"fa","واژگان کاربردی ERP برای انبار، خرید، فروش، مالی و فرایندهای کاری."},{"kmr","Peyvên ERP yên pratîk ji bo embar, kirîn, firotin, darayî û pêvajoyên kar."},{"pl","Praktyczne słownictwo ERP dla magazynu, zakupów, sprzedaży, finansów i procesów firmowych."},{"ro","Vocabular ERP practic pentru depozit, achiziții, vânzări, finanțe și procese de lucru."},{"ru","Практическая лексика ERP для склада, закупок, продаж, финансов и рабочих процессов."},{"sq","Fjalor praktik ERP për magazinë, blerje, shitje, financa dhe procese pune."},{"tr","Depo, satın alma, satış, finans ve iş süreçleri için pratik ERP söz varlığı."}};
foreach (var kv in descs) { var code = LanguageCode.From(kv.Key); Guid locId = collection.Localizations.Any(x => x.LanguageCode == code) ? Guid.Empty : Guid.NewGuid(); collection.AddOrUpdateLocalization(locId, code, "ERP", kv.Value, now); }
await db.SaveChangesAsync();
Console.WriteLine($"ERP_COLLECTION_WORDS={ids.Count}");

