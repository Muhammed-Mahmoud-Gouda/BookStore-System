using BookStore.BLL.Common;
using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShpoNest.Models.Entities;

namespace ShopNest.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //DbContext

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));

            builder.Services.AddApplicationServices();

            builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            // Seed Admin User, Categories, and Products
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    
                    // 1) Seed Admin User
                    var hasAdmin = context.Customers.Any(c => c.IsAdmin);
                    if (!hasAdmin)
                    {
                        var admin = new Customer
                        {
                            FirstName = "ShopNest",
                            LastName = "Admin",
                            Email = "admin@shopnest.com",
                            Phone = "01012345678",
                            IsActive = true,
                            IsAdmin = true,
                            CreatedAt = DateTime.UtcNow,
                            PasswordHash = ShopNest.BLL.Helper.PasswordHasher.HashPassword("Admin@123")
                        };
                        context.Customers.Add(admin);
                        context.SaveChanges();
                    }

                    // 2) Seed Categories and Products
                    if (!context.Categories.Any())
                    {
                        var softwareEng = new Category
                        {
                            Name = "Software Engineering & Architecture",
                            Description = "Core engineering methodologies, software design patterns, Clean Code, and enterprise system design.",
                            ImagePath = "https://images.unsplash.com/photo-1605379399642-870262d3d051?q=80&w=600",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        var aiData = new Category
                        {
                            Name = "AI & Data Science",
                            Description = "Artificial Intelligence, Deep Learning, Python Machine Learning, Data Structures, and Large Language Models.",
                            ImagePath = "https://images.unsplash.com/photo-1527474305487-b87b222841cc?q=80&w=600",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        var cybersecurity = new Category
                        {
                            Name = "Cybersecurity",
                            Description = "Ethical hacking, web application penetration testing, network defense, and Security+ study material.",
                            ImagePath = "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?q=80&w=600",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        var cloudDevops = new Category
                        {
                            Name = "Cloud & DevOps",
                            Description = "Kubernetes, Docker, CI/CD automation, cloud infrastructure design, and DevOps culture.",
                            ImagePath = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?q=80&w=600",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        context.Categories.AddRange(softwareEng, aiData, cybersecurity, cloudDevops);
                        context.SaveChanges();

                        var books = new List<Product>
                        {
                            new Product
                            {
                                Name = "Designing Data-Intensive Applications",
                                Description = "The definitive guide to understanding system design, databases, distributed systems, and modern backend architecture. Learn how to design scalable, reliable, and maintainable data applications.",
                                Price = 850,
                                Stock = 25,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                Author = "Martin Kleppmann",
                                Publisher = "O'Reilly Media",
                                ISBN = "9781449373320",
                                PublicationYear = 2017,
                                Pages = 616,
                                Language = "English",
                                Edition = "1st Edition",
                                Format = ShpoNest.Models.Enums.BookFormat.Paperback,
                                CategoryId = softwareEng.Id,
                                Images = new List<ProductImages>
                                {
                                    new ProductImages { ImagePath = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?q=80&w=600", IsMain = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                                }
                            },
                            new Product
                            {
                                Name = "Clean Code: A Handbook of Agile Software Craftsmanship",
                                Description = "Even bad code can function. But if code isn't clean, it can bring a development organization to its knees. Master the principles, patterns, and practices of writing clean, maintainable code.",
                                Price = 750,
                                Stock = 30,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                Author = "Robert C. Martin",
                                Publisher = "Prentice Hall",
                                ISBN = "9780132350884",
                                PublicationYear = 2008,
                                Pages = 464,
                                Language = "English",
                                Edition = "1st Edition",
                                Format = ShpoNest.Models.Enums.BookFormat.Paperback,
                                CategoryId = softwareEng.Id,
                                Images = new List<ProductImages>
                                {
                                    new ProductImages { ImagePath = "https://images.unsplash.com/photo-1532012197267-da84d127e765?q=80&w=600", IsMain = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                                }
                            },
                            new Product
                            {
                                Name = "The Pragmatic Programmer: Your Journey to Mastery",
                                Description = "One of the most significant books on software development. It cuts through the increasing specialization and technicalities of modern software development to examine the core process.",
                                Price = 800,
                                Stock = 15,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                Author = "David Thomas, Andrew Hunt",
                                Publisher = "Addison-Wesley",
                                ISBN = "9780135957059",
                                PublicationYear = 2019,
                                Pages = 352,
                                Language = "English",
                                Edition = "20th Anniversary Edition",
                                Format = ShpoNest.Models.Enums.BookFormat.Hardcover,
                                CategoryId = softwareEng.Id,
                                Images = new List<ProductImages>
                                {
                                    new ProductImages { ImagePath = "https://images.unsplash.com/photo-1512820790803-83ca734da794?q=80&w=600", IsMain = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                                }
                            },
                            new Product
                            {
                                Name = "Hands-On Machine Learning with Scikit-Learn and TensorFlow",
                                Description = "Through a series of recent breakthroughs, deep learning has boosted the entire field of machine learning. Now, even programmers who know close to nothing about this technology can use simple, efficient tools to learn programs.",
                                Price = 1100,
                                Stock = 20,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                Author = "Aurélien Géron",
                                Publisher = "O'Reilly Media",
                                ISBN = "9781492032649",
                                PublicationYear = 2019,
                                Pages = 856,
                                Language = "English",
                                Edition = "2nd Edition",
                                Format = ShpoNest.Models.Enums.BookFormat.Paperback,
                                CategoryId = aiData.Id,
                                Images = new List<ProductImages>
                                {
                                    new ProductImages { ImagePath = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?q=80&w=600", IsMain = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                                }
                            },
                            new Product
                            {
                                Name = "Deep Learning (Adaptive Computation and Machine Learning)",
                                Description = "Written by three experts in the field, Deep Learning is the only comprehensive book on the subject. It introduces a broad range of topics in machine learning, covering mathematical and conceptual background, deep learning techniques used in industry, and research perspectives.",
                                Price = 1250,
                                Stock = 10,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                Author = "Ian Goodfellow, Yoshua Bengio",
                                Publisher = "MIT Press",
                                ISBN = "9780262035613",
                                PublicationYear = 2016,
                                Pages = 800,
                                Language = "English",
                                Edition = "Illustrated Edition",
                                Format = ShpoNest.Models.Enums.BookFormat.Hardcover,
                                CategoryId = aiData.Id,
                                Images = new List<ProductImages>
                                {
                                    new ProductImages { ImagePath = "https://images.unsplash.com/photo-1501504905252-473c47e087f8?q=80&w=600", IsMain = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                                }
                            },
                            new Product
                            {
                                Name = "The Web Application Hacker's Handbook",
                                Description = "The ultimate reference guide to discovering and exploiting security flaws in Web applications. Learn about security architecture, pen testing tools, authentication attacks, SQL injection, and defensive measures.",
                                Price = 900,
                                Stock = 12,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                Author = "Dafydd Stuttard, Marcus Pinto",
                                Publisher = "Wiley",
                                ISBN = "9781118026472",
                                PublicationYear = 2011,
                                Pages = 912,
                                Language = "English",
                                Edition = "2nd Edition",
                                Format = ShpoNest.Models.Enums.BookFormat.Paperback,
                                CategoryId = cybersecurity.Id,
                                Images = new List<ProductImages>
                                {
                                    new ProductImages { ImagePath = "https://images.unsplash.com/photo-1563986768609-322da13575f3?q=80&w=600", IsMain = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                                }
                            },
                            new Product
                            {
                                Name = "Kubernetes Up & Running: Dive into the Future of Infrastructure",
                                Description = "Legendary tech leaders Brendan Burns, Joe Beda, and Kelsey Hightower show you how Kubernetes and container technology can help you achieve new levels of velocity, agility, reliability, and efficiency.",
                                Price = 720,
                                Stock = 18,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                Author = "Kelsey Hightower, Joe Beda",
                                Publisher = "O'Reilly Media",
                                ISBN = "9781492046530",
                                PublicationYear = 2019,
                                Pages = 280,
                                Language = "English",
                                Edition = "2nd Edition",
                                Format = ShpoNest.Models.Enums.BookFormat.Paperback,
                                CategoryId = cloudDevops.Id,
                                Images = new List<ProductImages>
                                {
                                    new ProductImages { ImagePath = "https://images.unsplash.com/photo-1600132806370-bf17e65e942f?q=80&w=600", IsMain = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                                }
                            }
                        };

                        context.Products.AddRange(books);
                        context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    // Fail silently or log
                }
            }

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
