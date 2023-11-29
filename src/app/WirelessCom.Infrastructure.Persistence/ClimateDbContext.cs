using Microsoft.EntityFrameworkCore;
using WirelessCom.Domain.Entities;

namespace WirelessCom.Infrastructure.Persistence;

public class ClimateDbContext : DbContext
{
    public ClimateDbContext()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DbPath = Path.Join(path, "climate.db");
    }

    public required DbSet<Temperature> Temperatures { get; set; }
    public required DbSet<Humidity> Humidities { get; set; }

    private string DbPath { get; }

    /// <summary>
    ///     Configure the <see cref="ClimateDbContext" /> to create a Sqlite database file.
    /// </summary>
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}