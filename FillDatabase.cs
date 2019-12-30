using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdvancedEfCore
{
    public partial class Program
    {
        private static async Task CleanDatabaseAsync(BookDataContext context)
        {
            //Delete all data from all tables

            //悪い例
            //レコードひとつひとつを消してセーブを繰り返すので
            //めちゃくちゃ遅い
            //var books = await context.Books.ToArrayAsync();
            //foreach(var book in books)
            //{
            //    context.Books.Remove(book);
            //    await context.SaveChangesAsync();
            //}

            //トランザクションを使って,すべて成功か変更しないかを保証する
            using var transaction = await context.Database.BeginTransactionAsync();

            //生SQLを実行.
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM {nameof(context.BookAuthors)}");
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM {nameof(context.Books)}");
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM {nameof(context.Authors)}");
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM {nameof(context.Genres)}");

            //何もエラーが起こらなかった場合に限り,それをDBに反映する.
            await transaction.CommitAsync();
        }

        private static async Task FillGenreAsync(BookDataContext context)
        {
            var genreData = await File.ReadAllTextAsync("Data/Genre.json");
            var genre = JsonSerializer.Deserialize<IEnumerable<Genre>>(genreData);

            using var transaction = context.Database.BeginTransaction();

            // Note how we combine transaction with exception handling
            try
            {
                // Note that we add all genre data rows BEFORE calling SaveChanges.
                foreach (var g in genre)
                {
                    context.Genres.Add(g);
                }

                await context.SaveChangesAsync();

                // Commit transaction if all commands succeed, transaction will auto-rollback
                // when disposed if either commands fails
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Something bad happened: {ex.Message}");

                // Note re-throw of exception
                throw;
            }
        }

        private static async Task FillBooksAsync(BookDataContext context)
        {
            // Demonstrate tracking queries here. Set a breakpoint up in
            // FillGenreAsync when genre rows are added. Afterwards, show that
            // the query returns THE SAME objects because of identical primary keys.
            var genres = await context.Genres.ToArrayAsync();

            var books = JsonSerializer.Deserialize<IEnumerable<Book>>(
                await File.ReadAllTextAsync("Data/Books.json"));

            using var transaction = context.Database.BeginTransaction();

            var rand = new Random();
            foreach (var book in books)
            {
                var dbBook = new Book
                {
                    Genre = genres[rand.Next(genres.Length)],
                    Title = book.Title,
                    ISBN = book.ISBN,
                    Language = book.Language
                };
                context.Books.Add(dbBook);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private static async Task FillAuthorsAsync(BookDataContext context)
        {
            // Note that we are jus reading primary keys of books, not entire 
            // book records. Tip: Always read only those columns that you REALLY need.
            var bookIDs = await context.Books.Select(b => b.BookID).ToArrayAsync();

            var authors = JsonSerializer.Deserialize<IEnumerable<Author>>(
                await File.ReadAllTextAsync("Data/Authors.json"));

            using var transaction = context.Database.BeginTransaction();

            var rand = new Random();
            foreach (var author in authors)
            {
                //Authors.jsonから読み込んだAuthorデータから
                //新しくAuthorインスタンスを作る
                //直接authorを使ってはいけない?
                //直接authorを使っても良いけれど,
                //もしauthorが変更された場合,それがDBに登録されることになる.
                //それを避けるためか?
                var dbAuthor = new Author
                {
                    AuthorName = author.AuthorName,
                    Nationality = author.Nationality
                };

                // Randomly assign each author one book.
                // Note that we can use the dbAuthor, although we have not yet written
                // it to the database. Also note that we are using the book ID as a
                // foreign key.
                // 最後に,AuthorをDBに追加するときは
                // Bookとの多対多の関係を登録する必要がある
                // ここはちょっとくせがあるな.
                // AuthorオブジェクトとBookIDを使う.
                // AuthorとBookじゃないんだな.
                var dbBookAuthor = new BookAuthor
                {
                    Author = dbAuthor,
                    BookID = bookIDs[rand.Next(bookIDs.Length)]
                };

                //ここまででまだcontext.Authors.Addが出てきていないことに注目!

                // Note that we do NOT need to add dbAuthor. It is referenced by
                // dbBookAuthor, that is enough.
                // BookAuthorsにAddするだけよい.
                // これだけでAuthorsテーブルにもAuthorが登録される.
                // これはEntityFrameworkがやってくれる.
                context.BookAuthors.Add(dbBookAuthor);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
