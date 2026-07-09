using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class SettingsController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Settings
        public ActionResult Index()
        {
            var settings = db.SiteSettings.OrderBy(s => s.SettingKey).ToList();
            return View(settings);
        }

        // GET: Admin/Settings/Edit/5
        public ActionResult Edit(int id)
        {
            var setting = db.SiteSettings.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        // POST: Admin/Settings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, string settingValue)
        {
            var setting = db.SiteSettings.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }

            setting.SettingValue = settingValue;
            setting.UpdatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["SuccessMessage"] = $"تنظیم '{setting.Description}' با موفقیت بروزرسانی شد.";
                return RedirectToAction("Index");
            }

            return View(setting);
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
