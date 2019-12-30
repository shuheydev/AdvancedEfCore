using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedEfCore
{
    static partial class Program
    {
        /// <summary>
        /// Genre情報付きでBookの情報を取得する
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<Book>> QueryBooksWithGnere(BookDataContext context)
        {
            //例1:
            //var books = await context.Books.ToArrayAsync();
            //foreach (var book in books)
            //{
            //    book.Genre = await context.Genres.FirstAsync(g => g.GenreID == book.GenreID);
            //}

            //例2:
            var books = await context.Books
                .Include(b => b.Genre)
                .ToArrayAsync();
            return books;
        
        }

        private static async Task<IEnumerable<Book>> QueryFilteredBooksAsync(BookDataContext context)
        {
            //DramaジャンルからUSの本を取得する
            var filteredBooks = context.Books
                .Where(b => b.Genre.GenreTitle == "Drama");

            //Deferred Execution(遅延実行)
            //上の時点ではクエリを組み立てただけで,実行はされていない.

            //まだ実行していない.クエリを組み立てただけ
            //filteredBooks = filteredBooks.Where(b => b.Language == "US");
            filteredBooks = filteredBooks.EnglishBooks();//拡張メソッド使ってみた

            filteredBooks = filteredBooks.BooksWithTitleFilter("of");//拡張メソッド使ってみた

            //ここでToArrayAsyncでクエリが実行される.
            //クエリの実行はできるだけ後ろに.
            return await filteredBooks.ToArrayAsync();
        }


        #region この2つは拡張メソッド
        static IQueryable<Book> EnglishBooks(this IQueryable<Book> books)
        {
            return books.Where(b => b.Language == "US");
        }

        static IQueryable<Book> BooksWithTitleFilter(this IQueryable<Book> books,string targetString)
        {
            return books.Where(b => b.Title.Contains(targetString));
        }
        #endregion
    }
}
