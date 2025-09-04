using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace CineTrackBE.Tests.Helpers.TestDataBuilders;

public class TestDataBuilder<T>
{
    private readonly List<T> _list = [];
    private Type _type = null!;

    public static TestDataBuilder<T> Create(int count)
    {
        var builder = new TestDataBuilder<T> { _type = typeof(T) };

        for (int i = 0; i < count; i++)
        {
            builder._list.Add(Activator.CreateInstance<T>());
        }

        return builder;
    }


    public List<T> Build() => _list;

    public async Task<List<T>> BuildAndSaveAsync(ApplicationDbContext context)
    {
        await context.AddRangeAsync(_list.Cast<object>());
        await context.SaveChangesAsync();
        return _list;
    }



}



public class TestPokus
{

    [Fact]
    public async Task Pokus()
    {

        var context = DatabaseTestHelper.CreateSqlLiteContext();
        var list = await TestDataBuilder<Genre>.Create(5).BuildAndSaveAsync(context);





        var result = await context.Genre.ToListAsync();

        result.Should().NotBeNull();



    }

}