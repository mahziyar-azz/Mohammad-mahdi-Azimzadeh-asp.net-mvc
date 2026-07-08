using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        public ActionResult Index()
        {
            ViewBag.UsersCount = db.Users.Count();
            ViewBag.ProductsCount = db.Products.Count();
            ViewBag.BlogsCount = db.BlogPosts.Count();
            ViewBag.ReviewsCount = db.Reviews.Count();
            
            // Get latest comments
            ViewBag.LatestComments = db.Comments
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToList();

            // Get latest reviews
            ViewBag.LatestReviews = db.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            return View();
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
