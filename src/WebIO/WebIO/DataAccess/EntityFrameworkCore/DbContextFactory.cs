namespace WebIO.DataAccess.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(
        "Server=localhost;Initial Catalog=webio;user id=sa; password=BVkMcwpF57vCCGQN7Pbr",
        opts => opts.CommandTimeout(500))
      .Options;
    return new AppDbContext(options);
  }
}
