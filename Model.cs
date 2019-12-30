using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AdvancedEfCore
{
    public class Genre
    {
        public int GenreID { get; set; }
        [Required]
        [MaxLength(50)]
        public string GenreTitle { get; set; }
        public List<Book> Books { get; set; }
    }

    public class Book
    {
        public int BookID { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        [MaxLength(50)]
        public string ISBN { get; set; }
        public int GenreID { get; set; }
        [MaxLength(2)]
        public string Language { get; set; }
        [Required]
        public Genre Genre { get; set; }
        public List<BookAuthor> Authors { get; set; }

    }

    public class Author
    {
        public int AuthorID { get; set; }
        [Required]
        [MaxLength(100)]
        public string AuthorName { get; set; }
        [MaxLength(2)]
        public string Nationality { get; set; }
        public List<BookAuthor> Books { get; set; }
    }

    /// <summary>
    /// BookとAuthor間で多対多の関係がある
    /// それを表すための中間テーブル
    /// </summary>
    public class BookAuthor
    {
        //BookAuthorIDはいらない.
        //BookIDとAuthorIDの複合主キーだから

        #region Bookへの参照
        public int BookID { get; set; }
        [Required]
        public Book Book { get; set; }
        #endregion

        #region Authorへの参照
        public int AuthorID { get; set; }
        [Required]
        public Author Author { get; set; }
        #endregion
    }

    //以下の図のように多対多を表すときは,中間テーブルを置いて,
    //それぞれが一対多の関係になるようにする
    /*
 *  +-------------+         +-------------+         +-------------+
 *  |             |         |             |         |             |
 *  |    Book     + 1 --- m + BookAuthor  + m --- 1 +    Author   |
 *  |             |         |             |         |             |
 *  +-------------+         +-------------+         +-------------+
 */


}
