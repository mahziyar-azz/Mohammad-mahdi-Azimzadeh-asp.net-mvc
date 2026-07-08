using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class CommentsController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Comments
        public ActionResult Index(string filter = "all", int page = 1)
        {
            int pageSize = 15;
            var query = db.Comments.Include(c => c.User).AsQueryable();

            if (filter == "product")
                query = query.Where(c => c.TargetType == "Product");
            else if (filter == "blog")
                query = query.Where(c => c.TargetType == "Blog");
            else if (filter == "pending")
                query = query.Where(c => !c.IsApproved);

            var total = query.Count();
            var comments = query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Filter = filter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Total = total;
            return View(comments);
        }

        // POST: Admin/Comments/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id, string returnUrl)
        {
            var comment = db.Comments.Find(id);
            if (comment != null)
            {
                comment.IsApproved = !comment.IsApproved;
                db.SaveChanges();
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        // POST: Admin/Comments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string returnUrl)
        {
            var comment = db.Comments.Find(id);
            if (comment != null)
            {
                // Remove child comments first
                var children = db.Comments.Where(c => c.ParentCommentId == id).ToList();
                db.Comments.RemoveRange(children);
                db.Comments.Remove(comment);
                db.SaveChanges();
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        // GET: Admin/Comments/Edit/5
        public ActionResult Edit(int id)
        {
            var comment = db.Comments.Include(c => c.User).FirstOrDefault(c => c.CommentId == id);
            if (comment == null) return HttpNotFound();
            return View(comment);
        }

        // POST: Admin/Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("content", "متن دیدگاه نمی‌تواند خالی باشد.");
                var comment = db.Comments.Include(c => c.User).FirstOrDefault(c => c.CommentId == id);
                return View(comment);
            }
            var comm = db.Comments.Find(id);
            if (comm != null)
            {
                comm.Content = content.Trim();
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
