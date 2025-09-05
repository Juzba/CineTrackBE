using Bogus;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Tests.Helpers.Common;


public static class FakerEfExtensions
{
    public static async Task<List<T>> GenerateAndSaveAsync<T>(
        this Faker<T> faker,
        int count,
        DbContext context,
        CancellationToken ct = default)
        where T : class
    {
        var items = faker.Generate(count);
        context.Set<T>().AddRange(items);
        await context.SaveChangesAsync(ct);
        return items;
    }

    public static async Task<T> GenerateOneAndSaveAsync<T>(
        this Faker<T> faker,
        DbContext context,
        CancellationToken ct = default)
        where T : class
    {


        var item = faker.Generate();
        context.Set<T>().Add(item);
        await context.SaveChangesAsync(ct);
        return item;
    }

    public static async Task<List<T>> GenerateConfigureAndSaveAsync<T>(
        this Faker<T> faker,
        int count,
        DbContext context,
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

        context.Set<T>().AddRange(items);
        await context.SaveChangesAsync(ct);
        return items;
    }
}
