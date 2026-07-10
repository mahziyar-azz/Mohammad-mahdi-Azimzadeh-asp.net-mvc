using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class BlogController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Blog
        public ActionResult Index(int page = 1)
        {
            int pageSize = 15;
            var total = db.BlogPosts.Count();
            var posts = db.BlogPosts
                .Include(b => b.User)
                .Include(b => b.BlogCategory)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Total = total;
            return View(posts);
        }

        // GET: Admin/Blog/Create
        public ActionResult Create()
        {
            ViewBag.BlogCategoryId = new SelectList(db.BlogCategories.Where(bc => bc.IsActive).OrderBy(bc => bc.Title), "BlogCategoryId", "Title");
            return View(new BlogPost { IsPublished = true });
        }

        // POST: Admin/Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(BlogPost post, HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrWhiteSpace(post.Title))
            {
                ModelState.AddModelError("Title", "عنوان مقاله الزامی است.");
            }
            if (string.IsNullOrWhiteSpace(post.Content))
            {
                ModelState.AddModelError("Content", "محتوای مقاله الزامی است.");
            }

            if (ModelState.IsValid)
            {
                // Assign Author
                int authorId = 1;
                if (Session["UserId"] != null)
                {
                    int.TryParse(Session["UserId"].ToString(), out authorId);
                }
                post.UserId = authorId;

                post.Slug = GenerateSlug(post.Title);
                post.CreatedAt = DateTime.Now;
                post.ViewCount = 0;
                if (post.IsPublished)
                {
                    post.PublishedAt = DateTime.Now;
                }

                // Handle Cover Image Upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = Server.MapPath("~/assets/img/blog");
                        if (!System.IO.Directory.Exists(uploadDir))
                        {
                            System.IO.Directory.CreateDirectory(uploadDir);
                        }
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                        var path = System.IO.Path.Combine(uploadDir, fileName);
                        imageFile.SaveAs(path);
                        post.ImagePath = "/assets/img/blog/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "خطا در آپلود تصویر: " + ex.Message);
                        ViewBag.BlogCategoryId = new SelectList(db.BlogCategories.Where(bc => bc.IsActive).OrderBy(bc => bc.Title), "BlogCategoryId", "Title", post.BlogCategoryId);
                        return View(post);
                    }
                }

                db.BlogPosts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BlogCategoryId = new SelectList(db.BlogCategories.Where(bc => bc.IsActive).OrderBy(bc => bc.Title), "BlogCategoryId", "Title", post.BlogCategoryId);
            return View(post);
        }

        // GET: Admin/Blog/Edit/5
        public ActionResult Edit(int id)
        {
            var post = db.BlogPosts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            ViewBag.BlogCategoryId = new SelectList(db.BlogCategories.Where(bc => bc.IsActive).OrderBy(bc => bc.Title), "BlogCategoryId", "Title", post.BlogCategoryId);
            return View(post);
        }

        // POST: Admin/Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(BlogPost post, HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrWhiteSpace(post.Title))
            {
                ModelState.AddModelError("Title", "عنوان مقاله الزامی است.");
            }
            if (string.IsNullOrWhiteSpace(post.Content))
            {
                ModelState.AddModelError("Content", "محتوای مقاله الزامی است.");
            }

            if (ModelState.IsValid)
            {
                var existing = db.BlogPosts.Find(post.PostId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.Title = post.Title;
                existing.BlogCategoryId = post.BlogCategoryId;
                existing.ShortDescription = post.ShortDescription;
                existing.Content = post.Content;
                existing.IsPublished = post.IsPublished;
                existing.UpdatedAt = DateTime.Now;

                if (post.IsPublished && existing.PublishedAt == null)
                {
                    existing.PublishedAt = DateTime.Now;
                }

                // Handle Cover Image Upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = Server.MapPath("~/assets/img/blog");
                        if (!System.IO.Directory.Exists(uploadDir))
                        {
                            System.IO.Directory.CreateDirectory(uploadDir);
                        }
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                        var path = System.IO.Path.Combine(uploadDir, fileName);
                        imageFile.SaveAs(path);
                        
                        // Optionally delete old file
                        existing.ImagePath = "/assets/img/blog/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "خطا در آپلود تصویر جدید: " + ex.Message);
                        ViewBag.BlogCategoryId = new SelectList(db.BlogCategories.Where(bc => bc.IsActive).OrderBy(bc => bc.Title), "BlogCategoryId", "Title", post.BlogCategoryId);
                        return View(post);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BlogCategoryId = new SelectList(db.BlogCategories.Where(bc => bc.IsActive).OrderBy(bc => bc.Title), "BlogCategoryId", "Title", post.BlogCategoryId);
            return View(post);
        }

        // POST: Admin/Blog/TogglePublish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TogglePublish(int id)
        {
            var post = db.BlogPosts.Find(id);
            if (post != null)
            {
                post.IsPublished = !post.IsPublished;
                if (post.IsPublished && post.PublishedAt == null)
                {
                    post.PublishedAt = DateTime.Now;
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: Admin/Blog/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var post = db.BlogPosts.Find(id);
            if (post != null)
            {
                // Remove related comments first
                var relatedComments = db.Comments
                    .Where(c => c.TargetType == "Blog" && c.TargetId == id)
                    .ToList();
                db.Comments.RemoveRange(relatedComments);
                db.BlogPosts.Remove(post);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return Guid.NewGuid().ToString();

            string slug = title.Trim().ToLower();

            char[] invalidChars = { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', '[', ']', '{', '}', ';', ':', '\'', '"', ',', '.', '<', '>', '/', '?', '\\', '|', '`', '~' };
            foreach (var c in invalidChars)
            {
                slug = slug.Replace(c, ' ');
            }

            slug = string.Join("-", slug.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            string baseSlug = slug;
            int counter = 1;
            while (db.BlogPosts.Any(p => p.Slug == slug))
            {
                slug = baseSlug + "-" + counter;
                counter++;
            }

            return slug;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
