using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class SlidersController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Sliders
        public ActionResult Index()
        {
            var sliders = db.Sliders.OrderBy(s => s.DisplayOrder).ToList();
            return View(sliders);
        }

        // GET: Admin/Sliders/Create
        public ActionResult Create()
        {
            return View(new Slider { IsActive = true, DisplayOrder = 0 });
        }

        // POST: Admin/Sliders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Slider slider, HttpPostedFileBase imageFile)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var uploadDir = Server.MapPath("~/assets/img/slider");
                    if (!System.IO.Directory.Exists(uploadDir))
                    {
                        System.IO.Directory.CreateDirectory(uploadDir);
                    }
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                    var path = System.IO.Path.Combine(uploadDir, fileName);
                    imageFile.SaveAs(path);
                    slider.ImagePath = "/assets/img/slider/" + fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "خطا در ذخیره‌سازی تصویر: " + ex.Message);
                    return View(slider);
                }
            }
            else
            {
                ModelState.AddModelError("ImagePath", "انتخاب تصویر برای اسلایدر الزامی است.");
                return View(slider);
            }

            if (ModelState.IsValid)
            {
                db.Sliders.Add(slider);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(slider);
        }

        // GET: Admin/Sliders/Edit/5
        public ActionResult Edit(int id)
        {
            var slider = db.Sliders.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }
            return View(slider);
        }

        // POST: Admin/Sliders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Slider slider, HttpPostedFileBase imageFile)
        {
            var existing = db.Sliders.Find(slider.SliderId);
            if (existing == null)
            {
                return HttpNotFound();
            }

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var uploadDir = Server.MapPath("~/assets/img/slider");
                    if (!System.IO.Directory.Exists(uploadDir))
                    {
                        System.IO.Directory.CreateDirectory(uploadDir);
                    }
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                    var path = System.IO.Path.Combine(uploadDir, fileName);
                    imageFile.SaveAs(path);
                    existing.ImagePath = "/assets/img/slider/" + fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "خطا در ذخیره‌سازی تصویر: " + ex.Message);
                    return View(slider);
                }
            }

            if (ModelState.IsValid)
            {
                existing.Title = slider.Title;
                existing.Subtitle = slider.Subtitle;
                existing.ButtonText = slider.ButtonText;
                existing.ButtonLink = slider.ButtonLink;
                existing.DisplayOrder = slider.DisplayOrder;
                existing.IsActive = slider.IsActive;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(slider);
        }

        // POST: Admin/Sliders/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var slider = db.Sliders.Find(id);
            if (slider != null)
            {
                slider.IsActive = !slider.IsActive;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: Admin/Sliders/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var slider = db.Sliders.Find(id);
            if (slider != null)
            {
                db.Sliders.Remove(slider);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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
