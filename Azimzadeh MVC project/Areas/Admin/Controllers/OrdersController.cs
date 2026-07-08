using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Orders
        public ActionResult Index(int page = 1)
        {
            int pageSize = 15;
            var total = db.Orders.Count();
            
            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.UserAddress)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Total = total;

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int id)
        {
            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.UserAddress)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order != null && !string.IsNullOrWhiteSpace(status))
            {
                order.OrderStatus = status.Trim();
                order.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                TempData["SuccessMessage"] = "وضعیت سفارش با موفقیت بروزرسانی شد.";
            }
            return RedirectToAction("Details", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
