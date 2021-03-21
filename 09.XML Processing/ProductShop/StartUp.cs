using ProductShop.Data;
using ProductShop.DTO.Input;
using ProductShop.DTO.Output;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var usersXml = File.ReadAllText("./Datasets/users.xml");
            //var productsXml = File.ReadAllText("./Datasets/products.xml");
            //var categoriesXml = File.ReadAllText("./Datasets/categories.xml");
            //var categoriesProductsXml = File.ReadAllText("./Datasets/categories-products.xml");

            //ImportUsers(context, usersXml);
            //ImportProducts(context, productsXml);
            //ImportCategories(context, categoriesXml);
            //Console.WriteLine(ImportCategoryProducts(context, categoriesProductsXml));

            Console.WriteLine(GetUsersWithProducts(context));
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users.ToArray()
                .Where(x => x.ProductsSold.Any())
                .OrderByDescending(x => x.ProductsSold.Count)
                .Take(10)
                .Select(x => new UserOutputModel
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Age = x.Age,
                    UserSoldProducts = new SoldProductsCount
                    {
                        Count = x.ProductsSold.Count,
                        SoldProducts = x.ProductsSold
                        .Select(p => new SoldProductsOutputModel
                        {
                            Name = p.Name,
                            Price = p.Price
                        }).OrderByDescending(p => p.Price).ToArray()
                    }
                })
                .ToArray();

            var finalUsers = new UsersFinalOutputModel
            {
                Count = context.Users.Where(x => x.ProductsSold.Count > 0).ToArray().Count(),
                Users = users
            };

            var xmlSerializer = new XmlSerializer(typeof(UsersFinalOutputModel), new XmlRootAttribute("Users"));

            var textWriter = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xmlSerializer.Serialize(textWriter, finalUsers, ns);

            return textWriter.ToString();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(x => new CategoryOutputModel
                {
                    Name = x.Name,
                    Count = x.CategoryProducts.Count,
                    AveragePrice = x.CategoryProducts.Select(p => p.Product.Price).Average(),
                    TotalRevenue = x.CategoryProducts.Select(s => s.Product.Price).Sum()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<CategoryOutputModel>), new XmlRootAttribute("Categories"));

            var textWriter = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xmlSerializer.Serialize(textWriter, categories, ns);

            return textWriter.ToString();
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Any())
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Take(5)
                .Select(u => new UsersOutputModel
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold.Select(p => new SoldProductsOutputModel
                    {
                        Name = p.Name,
                        Price = p.Price
                    })
                    .ToArray()
                })
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<UsersOutputModel>), new XmlRootAttribute("Users"));

            var textWriter = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xmlSerializer.Serialize(textWriter, users, ns);

            return textWriter.ToString();
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500M && x.Price <= 1000M)
                .Select(x => new ProductOutputModel
                {
                    Name = x.Name,
                    Price = x.Price,
                    Buyer = x.Buyer.FirstName + " " + x.Buyer.LastName
                })
                .OrderBy(x => x.Price)
                .Take(10)
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ProductOutputModel>), new XmlRootAttribute("Products"));

            var textWriter = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xmlSerializer.Serialize(textWriter, products, ns);

            return textWriter.ToString();
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CategoriesProductsInputModel[]), new XmlRootAttribute("CategoryProducts"));
            var categoriesProductsDto = (CategoriesProductsInputModel[])serializer.Deserialize(new StringReader(inputXml));

            var categoriesProducts = categoriesProductsDto
                .Select(x => new CategoryProduct
                {
                    CategoryId = x.CategoryId,
                    ProductId = x.ProductId
                })
                .ToList();

            context.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Count}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CategoriesInputModel[]), new XmlRootAttribute("Categories"));
            var categoriesDto = (CategoriesInputModel[])serializer.Deserialize(new StringReader(inputXml));

            var categories = categoriesDto.Select(x => new Category
            {
                Name = x.Name
            })
                .ToList();

            context.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProductInputModel[]), new XmlRootAttribute("Products"));
            var productsDto = (ProductInputModel[])serializer.Deserialize(new StringReader(inputXml));

            var products = productsDto.Select(x => new Product
            {
                Name = x.Name,
                Price = x.Price,
                SellerId = x.SellerId,
                BuyerId = x.BuyerId
            })
                .ToList();

            context.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count}";
        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserInputModel[]), new XmlRootAttribute("Users"));
            var usersDto = (UserInputModel[])serializer.Deserialize(new StringReader(inputXml));

            var users = usersDto.Select(x => new User
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Age = x.Age
            })
                .ToList();

            context.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count}";
        }
    }
}