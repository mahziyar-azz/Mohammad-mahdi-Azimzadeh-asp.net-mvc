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
            return View();
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
            return View();
        }
        public ActionResult Checkout()
        {
            return View();
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
                    new { FullName = "مدیر سیستم", Email = "admin@site.com", PasswordHash = "SEED_ADMIN_HASH" },
                    new { FullName = "علی محمدی", Email = "ali@gmail.com", PasswordHash = "SEED_USER_HASH" },
                    new { FullName = "مریم احمدی", Email = "maryam@gmail.com", PasswordHash = "SEED_USER_HASH" },
                    new { FullName = "رضا رضایی", Email = "reza@gmail.com", PasswordHash = "SEED_USER_HASH" }
                };
                var dbUsers = new List<User>();
                foreach (var u in usersToSeed)
                {
                    var dbUser = db.Users.FirstOrDefault(x => x.Email == u.Email);
                    if (dbUser == null)
                    {
                        dbUser = new User
                        {
                            FullName = u.FullName,
                            Email = u.Email,
                            PasswordHash = u.PasswordHash,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        db.Users.Add(dbUser);
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
                db.SaveChanges();

                return Content($"Successfully seeded {dbUsers.Count} Users, {seededPosts.Count} Blog Posts, and {commIdx - 1} Comments (Blogs/Products).");
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

        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "لطفاً تمامی فیلدها را پر کنید.";
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

                TempData["SuccessMessage"] = "خوش آمدید!";
                return RedirectToAction("Account");
            }

            TempData["ErrorMessage"] = "ایمیل یا رمز عبور اشتباه است.";
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
            var user = db.Users.Find(userId);
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
