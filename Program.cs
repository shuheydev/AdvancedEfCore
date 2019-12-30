using System;
using System.Threading.Tasks;

namespace AdvancedEfCore
{
    public partial class Program
    {
        static async Task Main(string[] args)
        {
            using var context = new BookDataContext();

            await CleanDatabaseAsync(context);
            await FillGenreAsync(context);
            await FillBooksAsync(context);
            await FillAuthorsAsync(context);

            var books = await QueryBooksWithGnere(context);

            var books2 = await QueryFilteredBooksAsync(context);
        }
    }
}
