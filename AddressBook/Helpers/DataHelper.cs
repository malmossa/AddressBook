using AddressBook.Data;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.Helpers
{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //Get an instance of the db application context
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            //Mugration: this is equivalent to update-database
            await dbContextSvc.Database.MigrateAsync();
        }
    }
}
