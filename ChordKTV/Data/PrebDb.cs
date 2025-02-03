namespace ChordKTV.Data;

using Microsoft.EntityFrameworkCore;

public static class PrepDb
{
    public static void Prep(IApplicationBuilder application, IWebHostEnvironment environment)
    {
        using IServiceScope? serviceScope = application.ApplicationServices.CreateScope();
        AppDbContext? dbContext = serviceScope.ServiceProvider.GetService<AppDbContext>() ?? throw new InvalidOperationException("No DbContext found");
        Seed(dbContext, true);
    }

    private static void Seed(AppDbContext context, bool isProduction)
    {
        if (isProduction)
        {
            Console.WriteLine("Migrating Database");

            try
            {
                context.Database.Migrate();
                Console.WriteLine("Database Migrated");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
