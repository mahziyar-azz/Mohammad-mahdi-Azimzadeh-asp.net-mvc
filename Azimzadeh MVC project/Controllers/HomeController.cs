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
        public ActionResult Product()
        {
            return View();
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
        public ActionResult Compare()
        {
            return View();
        }
        public ActionResult Blog()
        {
            return View();
        }
        public ActionResult BlogDetails()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
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
