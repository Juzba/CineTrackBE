using CineTrackBE.Data;
using Microsoft.AspNetCore.Identity;

namespace CineTrackBE.Tests.Helpers.TestDataBuilders;

public class RoleBuilder
{
    private IdentityRole _role = new();
    private readonly List<IdentityRole> _roleList = [];

    public static RoleBuilder Create() => new RoleBuilder();

    public RoleBuilder WithName(string name)
    {
        _role.Name = name;
        _role.NormalizedName = name.ToUpper();
        return this;
    }

    public RoleBuilder WithRandomData()
    {
        var randomId = Guid.NewGuid().ToString()[..8];
        _role.Name = $"TestRole_{randomId}";
        _role.NormalizedName = _role.Name.ToUpper();
        _role.ConcurrencyStamp = Guid.NewGuid().ToString();
        return this;
    }

    public RoleBuilder ListWithRandomData(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _role = new();
            WithRandomData();
            _roleList.Add(_role);
        }

        return this;
    }

    public IdentityRole Build() => _role;

    public List<IdentityRole> BuildList() => _roleList;

    public async Task<IdentityRole> BuildAndSaveAsync(ApplicationDbContext context)
    {
        context.Roles.Add(_role);
        await context.SaveChangesAsync();
        return _role;
    }

    public async Task<List<IdentityRole>> BuildListAndSaveAsync(ApplicationDbContext context)
    {
        context.Roles.AddRange(_roleList);
        await context.SaveChangesAsync();
        return _roleList;
    }
}
