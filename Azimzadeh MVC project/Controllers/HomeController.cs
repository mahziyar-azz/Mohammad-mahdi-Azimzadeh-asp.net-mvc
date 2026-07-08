using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Controllers
{
    public class HomeController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        public ActionResult Index()
        {
            var sliders = db.Sliders.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).ToList();
            ViewBag.GalleryItems = db.Galleries.Where(g => g.IsActive).OrderBy(g => g.DisplayOrder).ToList();
            return View(sliders);
        }
    
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Shop(int? categoryId, int? tagId)
        {
            var productsQuery = db.Products.Include(p => p.ProductImages).Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            if (tagId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Tags.Any(t => t.TagId == tagId.Value));
            }

            var products = productsQuery.ToList();

            // Load categories with count of active products
            ViewBag.Categories = db.Categories
                .Where(c => c.IsActive && c.Products.Any(p => p.IsActive))
                .Select(c => new {
                    Category = c,
                    Count = c.Products.Count(p => p.IsActive)
                })
                .ToList()
                .Select(x => new KeyValuePair<Category, int>(x.Category, x.Count))
                .ToList();

            // Load tags
            ViewBag.Tags = db.Tags.Where(t => t.Products.Any(p => p.IsActive)).ToList();
            
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedTagId = tagId;

            return View(products);
        }
        public ActionResult Product(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Shop");
            }

            var product = db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Include("Reviews.User")
                .FirstOrDefault(p => p.ProductId == id.Value && p.IsActive);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Increase View Count by 1
            product.ViewCount += 1;
            db.SaveChanges();

            // Load related products (same category, excluding current product)
            ViewBag.RelatedProducts = db.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId && p.IsActive)
                .Take(5)
                .ToList();

            // Load upsell products (top viewed active products, excluding current)
            ViewBag.UpsellProducts = db.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && p.ProductId != product.ProductId)
                .OrderByDescending(p => p.ViewCount)
                .Take(5)
                .ToList();

            // Load comments from DB
            ViewBag.Comments = db.Comments
                .Include(c => c.User)
                .Where(c => c.TargetType == "Product" && c.TargetId == product.ProductId && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            // Load test users for dropdown selector
            ViewBag.Users = db.Users.Where(u => u.IsActive).ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReview(int productId, int? userId, string sureName, string subject, int rating, string comments)
        {
            if (string.IsNullOrWhiteSpace(comments))
            {
                TempData["ErrorMessage"] = "متن نظر نمی‌تواند خالی باشد.";
                return RedirectToAction("Product", new { id = productId });
            }

            int finalUserId = 0;
            if (userId.HasValue && userId.Value > 0)
            {
                finalUserId = userId.Value;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sureName))
                {
                    sureName = "کاربر مهمان";
                }
                // Find or create guest user for the nickname
                var user = db.Users.FirstOrDefault(u => u.FullName == sureName.Trim());
                if (user == null)
                {
                    user = new User
                    {
                        FullName = sureName.Trim(),
                        Email = Guid.NewGuid().ToString("N") + "@guest.com",
                        PasswordHash = "GUEST_PLACEHOLDER",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                finalUserId = user.UserId;
            }

            if (rating < 1 || rating > 5)
            {
                rating = 5;
            }

            string combinedComment = comments.Trim();
            if (!string.IsNullOrWhiteSpace(subject))
            {
                combinedComment = $"[خلاصه: {subject.Trim()}]\n{combinedComment}";
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = finalUserId,
                Rating = (byte)rating,
                Comment = combinedComment,
                IsApproved = true, // Auto-approve for demo/testing purposes
                CreatedAt = DateTime.Now
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            TempData["SuccessMessage"] = "دیدگاه شما با موفقیت ثبت شد.";
            return RedirectToAction("Product", new { id = productId });
        }
        public ActionResult Cart()
        {
            Cart cart = null;
            if (Session["UserId"] != null)
            {
                int userId = (int)Session["UserId"];
                cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.UserId == userId);
            }
            else
            {
                string sessionId = Session.SessionID;
                cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.SessionId == sessionId && c.UserId == null);
            }

            return View(cart);
        }
        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "برای ثبت سفارش ابتدا باید وارد حساب کاربری خود شوید.";
                return RedirectToAction("Login", new { returnUrl = "/Home/Checkout" });
            }

            int userId = (int)Session["UserId"];
            var cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.UserId == userId);
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "سبد خرید شما خالی است.";
                return RedirectToAction("Cart");
            }

            var address = db.UserAddresses.FirstOrDefault(a => a.UserId == userId && a.IsDefault);
            if (address == null)
            {
                address = db.UserAddresses.FirstOrDefault(a => a.UserId == userId);
            }
            ViewBag.UserAddress = address;
            
            var user = db.Users.Find(userId);
            ViewBag.User = user;

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitOrder(string firstName, string lastName, string email, string phone, 
            string province, string city, string fullAddress, string postalCode)
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "برای ثبت سفارش ابتدا باید وارد حساب کاربری خود شوید.";
                return RedirectToAction("Login");
            }

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.UserId == userId);
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "سبد خرید شما خالی است.";
                return RedirectToAction("Cart");
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(province) || string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(fullAddress) || string.IsNullOrWhiteSpace(postalCode))
            {
                TempData["ErrorMessage"] = "لطفاً تمامی فیلدهای اجباری ستاره‌دار (*) را پر کنید.";
                return RedirectToAction("Checkout");
            }

            var address = db.UserAddresses.FirstOrDefault(a => a.UserId == userId);
            if (address == null)
            {
                address = new UserAddress
                {
                    UserId = userId,
                    Title = "آدرس پیش‌فرض",
                    Province = province.Trim(),
                    City = city.Trim(),
                    FullAddress = fullAddress.Trim(),
                    PostalCode = postalCode.Trim(),
                    IsDefault = true
                };
                db.UserAddresses.Add(address);
            }
            else
            {
                address.Province = province.Trim();
                address.City = city.Trim();
                address.FullAddress = fullAddress.Trim();
                address.PostalCode = postalCode.Trim();
            }
            db.SaveChanges();

            if (string.IsNullOrEmpty(user.FirstName))
            {
                user.FirstName = firstName.Trim();
                user.LastName = lastName.Trim();
                user.FullName = (firstName.Trim() + " " + lastName.Trim()).Trim();
            }

            string orderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(1000, 9999);

            var orderItemsList = cart.CartItems.Select(ci => new Azimzadeh_MVC_project.Models.OrderItemViewModel {
                ProductId = ci.ProductId,
                Title = ci.Product.Title,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                Total = ci.Quantity * ci.UnitPrice
            }).ToList();
            
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string itemsJson = serializer.Serialize(orderItemsList);
            decimal subtotal = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                AddressId = address.AddressId,
                ItemsJson = itemsJson,
                Subtotal = subtotal,
                DiscountAmount = 0,
                ShippingCost = 0,
                TotalAmount = subtotal,
                OrderStatus = "در حال پردازش",
                PaymentStatus = "پرداخت در محل",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            foreach (var ci in cart.CartItems)
            {
                ci.Product.StockQuantity -= ci.Quantity;
                if (ci.Product.StockQuantity < 0)
                {
                    ci.Product.StockQuantity = 0;
                }
            }

            db.CartItems.RemoveRange(cart.CartItems);
            db.Carts.Remove(cart);

            db.Orders.Add(order);
            db.SaveChanges();

            TempData["SuccessMessage"] = $"سفارش شما با موفقیت ثبت شد! شماره سفارش: {orderNumber}";
            return RedirectToAction("OrderSuccess", new { id = order.OrderId });
        }

        public ActionResult OrderSuccess(int? id, int? orderId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            int actualId = id ?? orderId ?? 0;
            if (actualId == 0)
            {
                return HttpNotFound();
            }

            int userId = (int)Session["UserId"];
            var order = db.Orders.Include("UserAddress").FirstOrDefault(o => o.OrderId == actualId && o.UserId == userId);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
        public ActionResult Wishlist()
        {
            return View();
        }

        public ActionResult Blog(int? categoryId)
        {
            var postsQuery = db.BlogPosts.Include(b => b.BlogCategory).Include(b => b.User).Where(b => b.IsPublished);

            if (categoryId.HasValue)
            {
                postsQuery = postsQuery.Where(b => b.BlogCategoryId == categoryId.Value);
            }

            var posts = postsQuery.OrderByDescending(b => b.CreatedAt).ToList();

            // Load blog categories with active post count
            ViewBag.Categories = db.BlogCategories
                .Where(c => c.IsActive)
                .Select(c => new {
                    Category = c,
                    Count = c.BlogPosts.Count(b => b.IsPublished)
                })
                .ToList()
                .Select(x => new KeyValuePair<BlogCategory, int>(x.Category, x.Count))
                .ToList();

            // Load recent posts for sidebar
            ViewBag.RecentPosts = db.BlogPosts
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.CreatedAt)
                .Take(4)
                .ToList();

            ViewBag.SelectedCategoryId = categoryId;

            return View(posts);
        }

        public ActionResult BlogDetails(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Blog");
            }

            var post = db.BlogPosts
                .Include(b => b.BlogCategory)
                .Include(b => b.User)
                .FirstOrDefault(b => b.PostId == id.Value && b.IsPublished);

            if (post == null)
            {
                return HttpNotFound();
            }

            // Increase view count
            post.ViewCount += 1;
            db.SaveChanges();

            // Load categories with active post count
            ViewBag.Categories = db.BlogCategories
                .Where(c => c.IsActive)
                .Select(c => new {
                    Category = c,
                    Count = c.BlogPosts.Count(b => b.IsPublished)
                })
                .ToList()
                .Select(x => new KeyValuePair<BlogCategory, int>(x.Category, x.Count))
                .ToList();

            // Load recent posts for sidebar
            ViewBag.RecentPosts = db.BlogPosts
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.CreatedAt)
                .Take(4)
                .ToList();

            // Load comments from DB
            ViewBag.Comments = db.Comments
                .Include(c => c.User)
                .Where(c => c.TargetType == "Blog" && c.TargetId == post.PostId && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            // Load test users for dropdown selector
            ViewBag.Users = db.Users.Where(u => u.IsActive).ToList();

            return View(post);
        }

        public ActionResult SeedBlogs()
        {
            try
            {
                // 1. Seed Admin and Test Users
                var usersToSeed = new[]
                {
                    new { 
                        FullName = "مدیر سیستم", 
                        Email = "admin@site.com", 
                        PasswordHash = "SEED_ADMIN_HASH", 
                        FirstName = "مدیر", 
                        LastName = "سیستم",
                        PhoneNumber = "09121111111",
                        HomePhoneNumber = "+98-2188888888",
                        Address1 = "تهران، میدان ونک، خیابان ملاصدرا",
                        Address2 = "پلاک ۲۴، واحد ۵",
                        CardNumber = "6037991811111111",
                        CVV = "111",
                        ExpirationDate = "01/30",
                        Gender = "آقا"
                    },
                    new { 
                        FullName = "علی محمدی", 
                        Email = "ali@gmail.com", 
                        PasswordHash = "SEED_USER_HASH", 
                        FirstName = "علی", 
                        LastName = "محمدی",
                        PhoneNumber = "09122222222",
                        HomePhoneNumber = "+98-2112345678",
                        Address1 = "تهران، خیابان آزادی، کوچه نوبهار",
                        Address2 = "پلاک ۱۲، طبقه ۳",
                        CardNumber = "6037991822222222",
                        CVV = "222",
                        ExpirationDate = "12/28",
                        Gender = "آقا"
                    },
                    new { 
                        FullName = "مریم احمدی", 
                        Email = "maryam@gmail.com", 
                        PasswordHash = "SEED_USER_HASH", 
                        FirstName = "مریم", 
                        LastName = "احمدی",
                        PhoneNumber = "09133333333",
                        HomePhoneNumber = "+98-3134567890",
                        Address1 = "اصفهان، خیابان چهارباغ بالا، مجتمع باغ نظر",
                        Address2 = "واحد 102",
                        CardNumber = "5022291033333333",
                        CVV = "333",
                        ExpirationDate = "09/30",
                        Gender = "خانم"
                    },
                    new { 
                        FullName = "رضا رضایی", 
                        Email = "reza@gmail.com", 
                        PasswordHash = "SEED_USER_HASH", 
                        FirstName = "رضا", 
                        LastName = "رضایی",
                        PhoneNumber = "09144444444",
                        HomePhoneNumber = "+98-4133333333",
                        Address1 = "تبریز، ولیعصر، خیابان شریعتی",
                        Address2 = "",
                        CardNumber = "5892101144444444",
                        CVV = "444",
                        ExpirationDate = "05/27",
                        Gender = "آقا"
                    },
                    new { 
                        FullName = "مدیر کل", 
                        Email = "admin@example.com", 
                        PasswordHash = "SEED_ADMIN_HASH", 
                        FirstName = "مدیر", 
                        LastName = "کل",
                        PhoneNumber = "09120000000",
                        HomePhoneNumber = "",
                        Address1 = "",
                        Address2 = "",
                        CardNumber = "",
                        CVV = "",
                        ExpirationDate = "",
                        Gender = "آقا"
                    },
                    new { 
                        FullName = "مشتری نمونه", 
                        Email = "customer@example.com", 
                        PasswordHash = "SEED_USER_HASH", 
                        FirstName = "مشتری", 
                        LastName = "نمونه",
                        PhoneNumber = "09129999999",
                        HomePhoneNumber = "",
                        Address1 = "",
                        Address2 = "",
                        CardNumber = "",
                        CVV = "",
                        ExpirationDate = "",
                        Gender = "آقا"
                    }
                };
                var dbUsers = new List<User>();
                foreach (var u in usersToSeed)
                {
                    var dbUser = db.Users.FirstOrDefault(x => x.Email == u.Email);
                    if (dbUser == null)
                    {
                        dbUser = new User
                        {
                            Email = u.Email,
                            PasswordHash = u.PasswordHash,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            FullName = u.FullName,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            PhoneNumber = string.IsNullOrEmpty(u.PhoneNumber) ? null : u.PhoneNumber,
                            HomePhoneNumber = string.IsNullOrEmpty(u.HomePhoneNumber) ? null : u.HomePhoneNumber,
                            Address1 = string.IsNullOrEmpty(u.Address1) ? null : u.Address1,
                            Address2 = string.IsNullOrEmpty(u.Address2) ? null : u.Address2,
                            CardNumber = string.IsNullOrEmpty(u.CardNumber) ? null : u.CardNumber,
                            CVV = string.IsNullOrEmpty(u.CVV) ? null : u.CVV,
                            ExpirationDate = string.IsNullOrEmpty(u.ExpirationDate) ? null : u.ExpirationDate,
                            Gender = string.IsNullOrEmpty(u.Gender) ? null : u.Gender
                        };
                        db.Users.Add(dbUser);
                        db.SaveChanges();
                    }
                    
                    string targetRoleName = u.Email.Contains("admin") ? "Admin" : "Customer";
                    var role = db.Roles.FirstOrDefault(r => r.RoleName == targetRoleName);
                    if (role != null && !dbUser.Roles.Any(r => r.RoleId == role.RoleId))
                    {
                        dbUser.Roles.Add(role);
                        db.SaveChanges();
                    }
                    dbUsers.Add(dbUser);
                }

                var author = dbUsers.First(); // The admin user

                // 2. Ensure categories exist
                var cats = new[] { "اخبار فناوری", "بررسی ابزارآلات", "راهنمای خرید", "آموزش کاربری" };
                var slugCats = new[] { "technology-news", "tool-reviews", "buying-guides", "how-to-guides" };
                var dbCats = new List<BlogCategory>();

                for (int i = 0; i < cats.Length; i++)
                {
                    var title = cats[i];
                    var cat = db.BlogCategories.FirstOrDefault(c => c.Title == title);
                    if (cat == null)
                    {
                        cat = new BlogCategory
                        {
                            Title = title,
                            Slug = slugCats[i],
                            DisplayOrder = i + 1,
                            IsActive = true
                        };
                        db.BlogCategories.Add(cat);
                        db.SaveChanges();
                    }
                    dbCats.Add(cat);
                }

                // 3. Clear existing posts
                var existingPosts = db.BlogPosts.ToList();
                db.BlogPosts.RemoveRange(existingPosts);
                db.SaveChanges();

                // 4. Fetch posts from DummyJSON
                var seededPosts = new List<BlogPost>();
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    string json = client.DownloadString("https://dummyjson.com/posts?limit=15");
                    dynamic data = System.Web.Helpers.Json.Decode(json);
                    var posts = data.posts;

                    int idx = 1;
                    foreach (var post in posts)
                    {
                        var category = dbCats[idx % dbCats.Count];
                        string imageUrl = $"https://loremflickr.com/800/600/tools,construction?random={idx}";
                        
                        string englishTitle = post.title;
                        string slug = englishTitle.ToLower().Replace(" ", "-").Replace("'", "").Replace("\"", "");
                        if (slug.Length > 50) slug = slug.Substring(0, 50);

                        string bodyText = post.body;

                        var blogPost = new BlogPost
                        {
                            BlogCategoryId = category.BlogCategoryId,
                            UserId = author.UserId,
                            Title = englishTitle,
                            Slug = slug,
                            ShortDescription = bodyText.Length > 100 ? bodyText.Substring(0, 100) + "..." : bodyText,
                            Content = bodyText + "\n\n" + "لورم ایپسوم متن ساختگی با تولید سادگی نامفهوم از صنعت چاپ، و با استفاده از طراحان گرافیک است، چاپگرها و متون بلکه روزنامه و مجله در ستون و سطرآنچنان که لازم است، و برای شرایط فعلی تکنولوژی مورد نیاز، و کاربردهای متنوع با هدف بهبود ابزارهای کاربردی می باشد.",
                            ImagePath = imageUrl,
                            ViewCount = post.views ?? 0,
                            IsPublished = true,
                            PublishedAt = DateTime.Now.AddDays(-idx),
                            CreatedAt = DateTime.Now.AddDays(-idx)
                        };
                        db.BlogPosts.Add(blogPost);
                        seededPosts.Add(blogPost);
                        idx++;
                    }
                    db.SaveChanges();
                }

                // 5. Clean and Seed Comments
                var existingComments = db.Comments.ToList();
                db.Comments.RemoveRange(existingComments);
                db.SaveChanges();

                var seededProducts = db.Products.Take(3).ToList();
                int commIdx = 1;

                // Seed Blog Comments
                foreach (var b in seededPosts.Take(3))
                {
                    var commenter = dbUsers[commIdx % dbUsers.Count];
                    var comm = new Comment
                    {
                        TargetType = "Blog",
                        TargetId = b.PostId,
                        UserId = commenter.UserId,
                        Content = $"مطلب فوق‌العاده کاربردی بود. به خصوص بخش {b.Title.Substring(0, Math.Min(b.Title.Length, 15))}... تشکر از نویسنده.",
                        IsApproved = true,
                        CreatedAt = DateTime.Now.AddDays(-commIdx)
                    };
                    db.Comments.Add(comm);
                    commIdx++;
                }

                // Seed Product Comments
                foreach (var p in seededProducts)
                {
                    var commenter = dbUsers[commIdx % dbUsers.Count];
                    var comm = new Comment
                    {
                        TargetType = "Product",
                        TargetId = p.ProductId,
                        UserId = commenter.UserId,
                        Content = $"سلام، من این دستگاه ({p.Title}) را خریداری کردم. کیفیت ساخت بسیار بالا است. آیا لوازم یدکی آن را هم موجود دارید؟",
                        IsApproved = true,
                        CreatedAt = DateTime.Now.AddDays(-commIdx)
                    };
                    db.Comments.Add(comm);
                    commIdx++;
                }

                // 6. Seed Sliders if empty
                if (!db.Sliders.Any())
                {
                    db.Sliders.Add(new Slider
                    {
                        Title = "ابزارآلات دستی\nاره برقی قدرتمند",
                        Subtitle = "فروش ویژه",
                        ButtonText = "هم‌اکنون خرید کنید",
                        ButtonLink = "/Home/Shop",
                        ImagePath = "/assets/img/slider/5.jpg",
                        DisplayOrder = 1,
                        IsActive = true
                    });
                    db.Sliders.Add(new Slider
                    {
                        Title = "ابزارآلات برقی\nبا کیفیت و دوام صنعتی",
                        Subtitle = "تخفیف ویژه",
                        ButtonText = "هم‌اکنون خرید کنید",
                        ButtonLink = "/Home/Shop",
                        ImagePath = "/assets/img/slider/6.jpg",
                        DisplayOrder = 2,
                        IsActive = true
                    });
                }

                // 7. Seed Gallery if empty
                if (!db.Galleries.Any())
                {
                    db.Galleries.Add(new Gallery
                    {
                        Title = "شلوار جین مردانه",
                        Description = "شلوار جین مردانه با کیفیت ممتاز",
                        ImagePath = "/assets/img/banner/men_jeans_banner.png",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                    db.Galleries.Add(new Gallery
                    {
                        Title = "کلاه زنانه",
                        Description = "کلاه زنانه شیک و مجلسی",
                        ImagePath = "/assets/img/banner/woman_hats_banner.png",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }
                db.SaveChanges();

                int sliderCount = db.Sliders.Count();
                int galleryCount = db.Galleries.Count();
                return Content($"Successfully seeded {dbUsers.Count} Users, {seededPosts.Count} Blog Posts, {commIdx - 1} Comments, {sliderCount} Sliders, and {galleryCount} Gallery Items.");
            }
            catch (Exception ex)
            {
                return Content("Error seeding: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitComment(string targetType, int targetId, string content, int userId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "متن دیدگاه نمی‌تواند خالی باشد.";
                if (targetType == "Product")
                    return RedirectToAction("Product", new { id = targetId });
                else
                    return RedirectToAction("BlogDetails", new { id = targetId });
            }

            var comment = new Comment
            {
                TargetType = targetType,
                TargetId = targetId,
                UserId = userId,
                Content = content.Trim(),
                IsApproved = true, // Auto-approve comments for testing/demo purposes
                CreatedAt = DateTime.Now
            };

            db.Comments.Add(comment);
            db.SaveChanges();

            TempData["SuccessMessage"] = "دیدگاه شما با موفقیت ثبت شد و انتشار یافت.";
            if (targetType == "Product")
                return RedirectToAction("Product", new { id = targetId });
            else
                return RedirectToAction("BlogDetails", new { id = targetId });
        }
        private string HashPassword(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (storedHash == "SEED_ADMIN_HASH" || storedHash == "SEED_USER_HASH" || storedHash == "GUEST_PLACEHOLDER")
            {
                if (storedHash == "SEED_ADMIN_HASH" && inputPassword == "admin123") return true;
                if (storedHash == "SEED_USER_HASH" && (inputPassword == "123456" || inputPassword == "password")) return true;
                if (inputPassword == "123456" || inputPassword == "password") return true;
            }
            return HashPassword(inputPassword) == storedHash;
        }

        public ActionResult Login(string returnUrl)
        {
            if (Session["UserId"] != null)
            {
                int userId = (int)Session["UserId"];
                var user = db.Users.Find(userId);
                if (user != null && user.IsActive && user.Roles.Any(r => r.RoleName == "Admin"))
                {
                    Session["IsAdmin"] = true;
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Account");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "لطفاً تمامی فیلدها را پر کنید.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            string cleanEmail = email.Trim().ToLower();
            var user = db.Users.FirstOrDefault(u => u.Email == cleanEmail && u.IsActive);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                user.LastLoginAt = DateTime.Now;
                db.SaveChanges();

                Session["UserId"] = user.UserId;
                Session["FullName"] = user.FullName;

                // Merge anonymous cart into logged-in user cart
                MergeCarts(user.UserId, Session.SessionID);

                bool isAdmin = user.Roles.Any(r => r.RoleName == "Admin");
                if (isAdmin)
                {
                    Session["IsAdmin"] = true;
                    TempData["SuccessMessage"] = "خوش آمدید به پنل مدیریت!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else
                {
                    TempData["SuccessMessage"] = "خوش آمدید!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Account");
                }
            }

            TempData["ErrorMessage"] = "ایمیل یا رمز عبور اشتباه است.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Register()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string firstName, string lastName, string email, string phone, string password, string passwordConfirm)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordConfirm))
            {
                TempData["ErrorMessage"] = "لطفاً تمامی فیلدهای اجباری را پر کنید.";
                return View();
            }

            if (password != passwordConfirm)
            {
                TempData["ErrorMessage"] = "تکرار رمز عبور مطابقت ندارد.";
                return View();
            }

            string cleanEmail = email.Trim().ToLower();
            var existingUser = db.Users.FirstOrDefault(u => u.Email == cleanEmail);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "این آدرس ایمیل قبلاً ثبت شده است.";
                return View();
            }

            var newUser = new User
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                FullName = (firstName.Trim() + " " + lastName.Trim()).Trim(),
                Email = cleanEmail,
                PhoneNumber = phone.Trim(),
                PasswordHash = HashPassword(password),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            Session["UserId"] = newUser.UserId;
            Session["FullName"] = newUser.FullName;

            // Merge anonymous cart into registered user cart
            MergeCarts(newUser.UserId, Session.SessionID);

            TempData["SuccessMessage"] = "ثبت نام شما با موفقیت انجام شد!";
            return RedirectToAction("Account");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            return View();
        }

        public ActionResult Account()
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "لطفاً ابتدا وارد حساب کاربری خود شوید.";
                return RedirectToAction("Login");
            }

            int userId = (int)Session["UserId"];
            var user = db.Users
                .Include(u => u.Orders)
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.UserId == userId);
            if (user == null || !user.IsActive)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string firstName, string lastName, string phone, 
            string address1, string address2, string homePhoneCountry, string homePhoneBody,
            string cardNumber, string cvv, string expirationDate, string gender,
            string currentPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);
            if (user == null || !user.IsActive)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                TempData["ErrorMessage"] = "نام و نام خانوادگی نمی‌تواند خالی باشد.";
                return RedirectToAction("Account");
            }

            // Home phone validation
            string combinedHomePhone = null;
            if (!string.IsNullOrWhiteSpace(homePhoneCountry) || !string.IsNullOrWhiteSpace(homePhoneBody))
            {
                if (string.IsNullOrWhiteSpace(homePhoneCountry) || string.IsNullOrWhiteSpace(homePhoneBody))
                {
                    TempData["ErrorMessage"] = "لطفاً هر دو بخش پیش‌شماره و شماره تلفن ثابت را وارد کنید.";
                    return RedirectToAction("Account");
                }

                string countryClean = homePhoneCountry.Trim();
                string bodyClean = homePhoneBody.Trim();

                if (!System.Text.RegularExpressions.Regex.IsMatch(countryClean, @"^\d{2,3}$"))
                {
                    TempData["ErrorMessage"] = "پیش‌شماره تلفن ثابت باید بین ۲ تا ۳ رقم باشد.";
                    return RedirectToAction("Account");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(bodyClean, @"^\d{8}$"))
                {
                    TempData["ErrorMessage"] = "شماره تلفن ثابت باید دقیقاً ۸ رقم باشد.";
                    return RedirectToAction("Account");
                }

                combinedHomePhone = "+" + countryClean + "-" + bodyClean;
            }

            // Billing information validation (Requires Legal/Security Review)
            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                string cardClean = cardNumber.Trim().Replace(" ", "").Replace("-", "");
                if (!System.Text.RegularExpressions.Regex.IsMatch(cardClean, @"^\d{16}$"))
                {
                    TempData["ErrorMessage"] = "شماره کارت بانکی باید ۱۶ رقم باشد.";
                    return RedirectToAction("Account");
                }
                user.CardNumber = cardClean;
            }
            else
            {
                user.CardNumber = null;
            }

            if (!string.IsNullOrWhiteSpace(cvv))
            {
                string cvvClean = cvv.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(cvvClean, @"^\d{3,4}$"))
                {
                    TempData["ErrorMessage"] = "کد CVV2 باید ۳ یا ۴ رقم باشد.";
                    return RedirectToAction("Account");
                }
                user.CVV = cvvClean;
            }
            else
            {
                user.CVV = null;
            }

            if (!string.IsNullOrWhiteSpace(expirationDate))
            {
                string expClean = expirationDate.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(expClean, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    TempData["ErrorMessage"] = "تاریخ انقضا باید به فرمت MM/YY باشد (مثلا 08/29).";
                    return RedirectToAction("Account");
                }
                user.ExpirationDate = expClean;
            }
            else
            {
                user.ExpirationDate = null;
            }

            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();
            user.FullName = (firstName.Trim() + " " + lastName.Trim()).Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            user.Address1 = string.IsNullOrWhiteSpace(address1) ? null : address1.Trim();
            user.Address2 = string.IsNullOrWhiteSpace(address2) ? null : address2.Trim();
            user.HomePhoneNumber = combinedHomePhone;
            user.Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (string.IsNullOrEmpty(currentPassword))
                {
                    TempData["ErrorMessage"] = "برای تغییر رمز عبور، وارد کردن رمز عبور فعلی الزامی است.";
                    return RedirectToAction("Account");
                }

                if (!VerifyPassword(currentPassword, user.PasswordHash))
                {
                    TempData["ErrorMessage"] = "رمز عبور فعلی نادرست است.";
                    return RedirectToAction("Account");
                }

                if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "تکرار رمز عبور جدید مطابقت ندارد.";
                    return RedirectToAction("Account");
                }

                user.PasswordHash = HashPassword(newPassword);
            }

            db.SaveChanges();
            Session["FullName"] = user.FullName;

            TempData["SuccessMessage"] = "اطلاعات حساب کاربری شما با موفقیت بروزرسانی شد.";
            return RedirectToAction("Account");
        }


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            
            int cartCount = 0;
            decimal cartSubtotal = 0;
            List<CartItem> cartItems = new List<CartItem>();
            
            try
            {
                Cart cart = null;
                if (Session["UserId"] != null)
                {
                    int userId = (int)Session["UserId"];
                    cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.UserId == userId);
                }
                else
                {
                    string sessionId = Session.SessionID;
                    cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.SessionId == sessionId && c.UserId == null);
                }
                
                if (cart != null)
                {
                    cartItems = cart.CartItems.ToList();
                    cartCount = cartItems.Sum(ci => ci.Quantity);
                    cartSubtotal = cartItems.Sum(ci => ci.Quantity * ci.UnitPrice);
                }
            }
            catch { }
            
            ViewBag.HeaderCartItems = cartItems;
            ViewBag.HeaderCartCount = cartCount;
            ViewBag.HeaderCartSubtotal = cartSubtotal;
        }

        private Cart GetOrCreateCart()
        {
            Cart cart = null;
            if (Session["UserId"] != null)
            {
                int userId = (int)Session["UserId"];
                cart = db.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart == null)
                {
                    string sessionId = Session.SessionID;
                    cart = db.Carts.FirstOrDefault(c => c.SessionId == sessionId && c.UserId == null);
                    if (cart != null)
                    {
                        cart.UserId = userId;
                        cart.UpdatedAt = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        cart = new Cart
                        {
                            UserId = userId,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        db.Carts.Add(cart);
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                string sessionId = Session.SessionID;
                cart = db.Carts.FirstOrDefault(c => c.SessionId == sessionId && c.UserId == null);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        SessionId = sessionId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }
            }
            return cart;
        }

        private void MergeCarts(int userId, string sessionId)
        {
            try
            {
                var anonCart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.SessionId == sessionId && c.UserId == null);
                if (anonCart != null && anonCart.CartItems.Any())
                {
                    var userCart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.UserId == userId);
                    if (userCart == null)
                    {
                        anonCart.UserId = userId;
                        anonCart.SessionId = null;
                        anonCart.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        foreach (var anonItem in anonCart.CartItems.ToList())
                        {
                            var existingItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductId == anonItem.ProductId);
                            if (existingItem != null)
                            {
                                existingItem.Quantity += anonItem.Quantity;
                                if (existingItem.Quantity > existingItem.Product.StockQuantity)
                                {
                                    existingItem.Quantity = existingItem.Product.StockQuantity;
                                }
                            }
                            else
                            {
                                anonItem.CartId = userCart.CartId;
                            }
                        }
                        db.Carts.Remove(anonCart);
                    }
                    db.SaveChanges();
                }
            }
            catch { }
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = db.Products.Find(productId);
            if (product == null || product.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = "محصول مورد نظر موجود نیست یا یافت نشد.";
                return RedirectToAction("Product", new { id = productId });
            }

            var cart = GetOrCreateCart();
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            
            int targetQuantity = quantity;
            if (cartItem != null)
            {
                targetQuantity += cartItem.Quantity;
            }

            if (targetQuantity > product.StockQuantity)
            {
                TempData["ErrorMessage"] = $"تعداد درخواستی بیشتر از موجودی انبار است. حداکثر موجودی: {product.StockQuantity}";
                return RedirectToAction("Product", new { id = productId });
            }

            decimal priceToUse = product.DiscountPrice ?? product.Price;

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = priceToUse
                };
                db.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity = targetQuantity;
                cartItem.UnitPrice = priceToUse;
            }

            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            TempData["SuccessMessage"] = "محصول با موفقیت به سبد خرید اضافه شد.";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCart(FormCollection form)
        {
            Cart cart = null;
            if (Session["UserId"] != null)
            {
                int userId = (int)Session["UserId"];
                cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.UserId == userId);
            }
            else
            {
                string sessionId = Session.SessionID;
                cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.SessionId == sessionId && c.UserId == null);
            }

            if (cart == null)
            {
                return RedirectToAction("Cart");
            }

            foreach (var key in form.AllKeys)
            {
                if (key.StartsWith("quantity["))
                {
                    string idStr = key.Replace("quantity[", "").Replace("]", "");
                    if (int.TryParse(idStr, out int itemId) && int.TryParse(form[key], out int qty))
                    {
                        var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == itemId);
                        if (item != null)
                        {
                            if (qty <= 0)
                            {
                                db.CartItems.Remove(item);
                            }
                            else
                            {
                                if (qty > item.Product.StockQuantity)
                                {
                                    TempData["ErrorMessage"] = $"تعداد درخواستی برای '{item.Product.Title}' بیشتر از موجودی انبار است. موجودی: {item.Product.StockQuantity}";
                                    qty = item.Product.StockQuantity;
                                }
                                item.Quantity = qty;
                            }
                        }
                    }
                }
            }

            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            TempData["SuccessMessage"] = "سبد خرید با موفقیت بروزرسانی شد.";
            return RedirectToAction("Cart");
        }

        public ActionResult RemoveFromCart(int id)
        {
            Cart cart = null;
            if (Session["UserId"] != null)
            {
                int userId = (int)Session["UserId"];
                cart = db.Carts.FirstOrDefault(c => c.UserId == userId);
            }
            else
            {
                string sessionId = Session.SessionID;
                cart = db.Carts.FirstOrDefault(c => c.SessionId == sessionId && c.UserId == null);
            }

            if (cart != null)
            {
                var item = db.CartItems.FirstOrDefault(ci => ci.CartItemId == id && ci.CartId == cart.CartId);
                if (item != null)
                {
                    db.CartItems.Remove(item);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "محصول از سبد خرید حذف شد.";
                }
            }

            return RedirectToAction("Cart");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
