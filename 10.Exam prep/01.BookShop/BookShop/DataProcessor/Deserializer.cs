namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var booksDto = DeserializeObject<BookInputModel>("Books", xmlString);

            StringBuilder sb = new StringBuilder();

            List<Book> books = new List<Book>();

            foreach (var book in booksDto)
            {
                if (!IsValid(book))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var date = DateTime.ParseExact(book.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                var currentBook = new Book
                {
                    Name = book.Name,
                    Genre = (Genre)book.Genre,
                    Price = book.Price,
                    Pages = book.Pages,
                    PublishedOn = date
                };

                books.Add(currentBook);

                sb.AppendLine(String.Format(SuccessfullyImportedBook, book.Name, book.Price));
            }            

            context.Books.AddRange(books);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var authorsDto = JsonConvert.DeserializeObject<IEnumerable<AuthorInputModel>>(jsonString);

            StringBuilder sb = new StringBuilder();
            List<Author> authors = new List<Author>();

            foreach (var author in authorsDto)
            {
                if (!IsValid(author))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (authors.Any(x => x.Email == author.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var currentAuthor = new Author
                {
                    FirstName = author.FirstName,
                    LastName = author.LastName,
                    Email = author.Email,
                    Phone = author.Phone,
                };

                foreach (var authorBook in author.Books)
                {
                    var book = context.Books.Find(authorBook.Id);

                    if (book == null)
                    {
                        continue;
                    }

                    currentAuthor.AuthorsBooks.Add(new AuthorBook
                    {
                        Author = currentAuthor,
                        Book = book
                    });
                }

                if (currentAuthor.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                authors.Add(currentAuthor);
                sb.AppendLine(String.Format(SuccessfullyImportedAuthor, author.FirstName + " " + author.LastName, 
                    currentAuthor.AuthorsBooks.Count));
            }

            context.Authors.AddRange(authors);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static T[] DeserializeObject<T>(string rootElement, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(T[]), new XmlRootAttribute(rootElement));
            var deserializedDtos = (T[])xmlSerializer.Deserialize(new StringReader(xmlString));
            return deserializedDtos;
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}