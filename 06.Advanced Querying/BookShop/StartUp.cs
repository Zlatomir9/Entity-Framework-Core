using BookShop.Data;
using BookShop.Initializer;
using System.Linq;
using System.Text;
using System;
using BookShop.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BookShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new BookShopContext();
            //DbInitializer.ResetDatabase(context);

            Console.WriteLine(RemoveBooks(context));
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.Copies < 4200)
                .ToList();

            context.Books
                .RemoveRange(books);
            context.SaveChanges();

            return books.Count;
        }

        public static void IncreasePrices(BookShopContext context)
        {
             context.Books
                .Where(x => x.ReleaseDate.Value.Year < 2010)
                .ToList()
                .ForEach(x => x.Price += 5);

            context.SaveChanges();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categoryBooks = context.Categories
                .Select(x => new
                {
                    x.Name,
                    Books = x.CategoryBooks.Select(x => new 
                                        { x.Book.ReleaseDate, 
                                          x.Book.Title })
                              .OrderByDescending(x => x.ReleaseDate).Take(3)
                })
                .OrderBy(x => x.Name)
                .ToList();

            var result = new StringBuilder();

            foreach (var category in categoryBooks)
            {
                result.AppendLine($"--{category.Name}");

                foreach (var book in category.Books)
                {
                    result.AppendLine($"{book.Title} ({book.ReleaseDate.Value.Year})");
                }
            }

            return result.ToString().TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var profitsByCategory = context.Categories
                .Select(x => new
                {
                    x.Name,
                    Profit = x.CategoryBooks.Select(x => x.Book.Copies * x.Book.Price).Sum()
                })
                .OrderByDescending(x => x.Profit)
                .ThenBy(x => x.Name)
                .ToList();

            var result = new StringBuilder();

            foreach (var category in profitsByCategory)
            {
                result.AppendLine($"{category.Name} ${category.Profit:F2}");
            }

            return result.ToString().TrimEnd();
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    BooksCopies = x.Books.Select(x => x.Copies).Sum()
                })
                .OrderByDescending(x => x.BooksCopies)
                .ToList();

            var result = new StringBuilder();

            foreach (var author in authors)
            {
                result.AppendLine($"{author.FirstName} {author.LastName} - {author.BooksCopies}");
            }

            return result.ToString().TrimEnd();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .Where(x => x.Title.Length > lengthCheck)
                .ToList();

            return books.Count;
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var booksAndAuthors = context.Books
                .Select(x => new
                {
                    x.Title,
                    x.BookId,
                    x.Author.FirstName,
                    x.Author.LastName
                })
                .Where(x => x.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(x => x.BookId)
                .ToList();

            var result = new StringBuilder();

            foreach (var book in booksAndAuthors)
            {
                result.AppendLine($"{book.Title} ({book.FirstName +  " " + book.LastName})");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(x => x.Title.ToLower().Contains(input.ToLower()))
                .OrderBy(x => x.Title)
                .Select(x => x.Title)
                .ToList();

            var result = new StringBuilder();

            foreach (var book in books)
            {
                result.AppendLine(book);
            }

            return result.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .Select(x => new
                {
                    x.FirstName,
                    FullName = x.FirstName + ' ' + x.LastName
                })
                .Where(x => x.FirstName.EndsWith(input))
                .OrderBy(x => x.FullName)
                .ToList();

            var result = new StringBuilder();

            foreach (var author in authors)
            {
                result.AppendLine(author.FullName);
            }

            return result.ToString().TrimEnd();
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            parsedDate.ToString("yyyyMMdd");
            var books = context.Books
                .Select(x => new
                {
                    x.Title,
                    x.EditionType,
                    x.Price,
                    x.ReleaseDate
                })
                .Where(x => x.ReleaseDate < parsedDate)
                .OrderByDescending(x => x.ReleaseDate)
                .ToList();

            var result = new StringBuilder();

            foreach (var book in books)
            {
                result.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:F2}");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var splittedInput = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLower())
                .ToArray();

            var books = context.Books
                .Where(x => x.BookCategories
                        .Any(y => splittedInput.Contains(y.Category.Name.ToLower())))
                .OrderBy(x => x.Title)
                .Select(x => x.Title)
                .ToList();
                

            var result = new StringBuilder();

            foreach (var book in books)
            {
                result.AppendLine(book);
            }

            return result.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Select(x => new { x.Title, x.ReleaseDate, x.BookId })
                .Where(x => x.ReleaseDate.Value.Year != year)
                .OrderBy(x => x.BookId)
                .ToList();

            var result = new StringBuilder();

            foreach (var book in books)
            {
                result.AppendLine(book.Title);
            }

            return result.ToString().TrimEnd();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Select(x => new { x.Price, x.Title })
                .Where(x => x.Price > 40)
                .OrderByDescending(x => x.Price)
                .ToList();

            var result = new StringBuilder();

            foreach (var book in books)
            {
                result.AppendLine($"{book.Title} - ${book.Price:F2}");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            EditionType enumResult;

            if (Enum.TryParse("Gold", out enumResult))
            {
                var goldenBooks = context.Books
                .Select(x => new { x.Title, x.EditionType, x.Copies, x.BookId })
                .Where(x => x.EditionType == enumResult && x.Copies < 5000)
                .OrderBy(x => x.BookId)
                .ToList();

                var result = new StringBuilder();

                foreach (var book in goldenBooks)
                {
                    result.AppendLine(book.Title);
                }

                return result.ToString().TrimEnd();
            }

            return null;
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            AgeRestriction eResult;

            command = char.ToUpper(command[0]).ToString() + command.Substring(1).ToLower();

            if (Enum.TryParse(command, out eResult))
            {
                var books = context.Books
                .Select(x => new { x.Title, x.AgeRestriction })
                .Where(x => x.AgeRestriction == eResult)
                .OrderBy(x => x.Title)
                .ToList();
                var result = new StringBuilder();

                foreach (var book in books)
                {
                    result.AppendLine(book.Title);
                }

                return result.ToString().TrimEnd();
            }

            return null;
        }
    }
}
