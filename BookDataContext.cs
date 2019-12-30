using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedEfCore
{
    public class BookDataContext : DbContext
    {
        /// <summary>
        /// https://thedatafarm.com/data-access/logging-in-ef-core-2-2-has-a-simpler-syntax-more-like-asp-net-core/
        /// EntityFrameworkが発行するSQLの表示方法がわからん...
        /// </summary>
        /// <returns></returns>
        //private ILoggerFactory GetLoggerFactory()
        //{
        //    IServiceCollection serviceCollection = new ServiceCollection();
        //    serviceCollection.AddLogging(builder =>
        //           builder.AddConsole()
        //                  .AddFilter(DbLoggerCategory.Database.Command.Name,
        //                             LogLevel.Information));
        //    return serviceCollection.BuildServiceProvider()
        //            .GetService<ILoggerFactory>();
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Sqliteを使うことを指定する
            //ASP.NET CoreならStartup.csに書くのだろうけれど
            var dbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mydb.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            //SqliteはサーバーではなくファイルがDB

            //発行されるSQLを出力する方法がわからん...
            //optionsBuilder.UseLoggerFactory(GetLoggerFactory());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)//ISBNプロパティにインデックスを張る
                .IsUnique();//Unique制約をつける
            modelBuilder.Entity<Genre>()
                .HasIndex(b => b.GenreTitle)
                .IsUnique();//Unique制約をつける

            //BookAuthorクラスの複合主キーを指定
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookID, ba.AuthorID });

            //Book -< Authors
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithOne(a => a.Book);

            //Author -< Books
            modelBuilder.Entity<Author>()
                .HasMany(a => a.Books)
                .WithOne(b => b.Author);
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
    }
}
