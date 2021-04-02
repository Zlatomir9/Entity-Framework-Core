namespace VaporStore.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
    {
        private const string ErrorMessage = "Invalid Data";

        private const string SuccessfullyImportedGame
            = "Added {0} ({1}) with {2} tags";

        private const string SuccessfullyImportedUser
            = "Imported {0} with {1} cards";

        private const string SuccessfullyImportedPurchase
            = "Imported {0} for {1}";

        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var gameDtos = JsonConvert
                .DeserializeObject<IEnumerable<GameInputModel>>(jsonString);

            foreach (var gameJson in gameDtos)
            {
                if (!IsValid(gameJson) || gameJson.Tags.Count() == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var genre = context.Genres.FirstOrDefault(x => x.Name == gameJson.Genre)
                    ?? new Genre { Name = gameJson.Genre };

                var developer = context.Developers.FirstOrDefault(x => x.Name == gameJson.Developer)
                    ?? new Developer { Name = gameJson.Developer };

                var game = new Game
                {
                    Name = gameJson.Name,
                    Genre = genre,
                    Developer = developer,
                    Price = gameJson.Price,
                    ReleaseDate = gameJson.ReleaseDate.Value
                };

                foreach (var tag in gameJson.Tags)
                {
                    var currTag = context.Tags.FirstOrDefault(x => x.Name == tag)
                        ?? new Tag { Name = tag };

                    game.GameTags.Add(new GameTag { Tag = currTag });
                }

                context.Games.Add(game);
                context.SaveChanges();
                sb.AppendLine(String.Format(SuccessfullyImportedGame, gameJson.Name, gameJson.Genre, gameJson.Tags.Length));
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            var userDtos = JsonConvert
                .DeserializeObject<IEnumerable<UserInputModel>>(jsonString);

            foreach (var user in userDtos)
            {
                if (!IsValid(user) || !user.Cards.All(IsValid))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var currUser = new User
                {
                    Age = user.Age,
                    Email = user.Email,
                    FullName = user.FullName,
                    Username = user.Username,
                    Cards = user.Cards.Select(x => new Card
                    {
                        Cvc = x.Cvc,
                        Number = x.Number,
                        Type = x.Type.Value
                    }).ToList()
                };

                context.Users.Add(currUser);
                context.SaveChanges();
                
                sb.AppendLine(String.Format(SuccessfullyImportedUser, user.Username, user.Cards.Length));
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlSerializer serializer = new XmlSerializer(typeof(PurchaseInputModel[]), new XmlRootAttribute("Purchases"));
            var purchasesDtos = (PurchaseInputModel[])serializer.Deserialize(new StringReader(xmlString));

            foreach (var purchase in purchasesDtos)
            {
                if (!IsValid(purchase))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool parsedDate = DateTime.TryParseExact(purchase.Date, 
                    "dd/MM/yyyy HH:mm", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, 
                    out var date);

                if (!parsedDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var currPurchase = new Purchase
                {
                    Date = date,
                    Type = purchase.Type.Value,
                    ProductKey = purchase.Key
                };

                currPurchase.Card = 
                    context.Cards.FirstOrDefault(x => x.Number == purchase.Card);

                currPurchase.Game = 
                    context.Games.FirstOrDefault(x => x.Name == purchase.Title);

                var username = context.Users.Where(x => x.Id == currPurchase.Card.UserId)
                    .Select(x => x.Username).FirstOrDefault();

                context.Purchases.Add(currPurchase);
                sb.AppendLine(String.Format(SuccessfullyImportedPurchase, purchase.Title, username));
            }
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}