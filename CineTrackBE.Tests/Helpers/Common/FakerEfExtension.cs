using Bogus;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Tests.Helpers.Common;


public static class FakerEfExtensions
{
    public static async Task<List<T>> GenerateAndSaveAsync<T>(
        this Faker<T> faker,
        int count,
        DbContext db,
        CancellationToken ct = default)
        where T : class
    {
        var items = faker.Generate(count);
        db.Set<T>().AddRange(items);
        await db.SaveChangesAsync(ct);
        return items;
    }

    public static async Task<T> GenerateOneAndSaveAsync<T>(
        this Faker<T> faker,
        DbContext db,
        CancellationToken ct = default)
        where T : class
    {
        var item = faker.Generate();
        db.Set<T>().Add(item);
        await db.SaveChangesAsync(ct);
        return item;
    }

    public static async Task<List<T>> GenerateConfigureAndSaveAsync<T>(
        this Faker<T> faker,
        int count,
        DbContext db,
        Action<T, int>? configure = null,
        CancellationToken ct = default)
        where T : class
    {
        var items = faker.Generate(count);
        if (configure != null)
        {
            for (int i = 0; i < items.Count; i++)
                configure(items[i], i);
        }

        db.Set<T>().AddRange(items);
        await db.SaveChangesAsync(ct);
        return items;
    }
}
