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

        // POST: Admin/Blog/TogglePublish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TogglePublish(int id)
        {
            var post = db.BlogPosts.Find(id);
            if (post != null)
            {
                post.IsPublished = !post.IsPublished;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
