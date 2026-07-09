using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class GalleryController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Gallery
        public ActionResult Index()
        {
            var items = db.Galleries.OrderBy(g => g.DisplayOrder).ToList();
            return View(items);
        }

        // GET: Admin/Gallery/Create
        public ActionResult Create()
        {
            return View(new Gallery { IsActive = true, DisplayOrder = 0 });
        }

        // POST: Admin/Gallery/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Gallery gallery, HttpPostedFileBase imageFile)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var uploadDir = Server.MapPath("~/assets/img/gallery");
                    if (!System.IO.Directory.Exists(uploadDir))
                    {
                        System.IO.Directory.CreateDirectory(uploadDir);
                    }
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                    var path = System.IO.Path.Combine(uploadDir, fileName);
                    imageFile.SaveAs(path);
                    gallery.ImagePath = "/assets/img/gallery/" + fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "خطا در ذخیره‌سازی تصویر: " + ex.Message);
                    return View(gallery);
                }
            }
            else
            {
                ModelState.AddModelError("ImagePath", "انتخاب تصویر الزامی است.");
                return View(gallery);
            }

            gallery.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Galleries.Add(gallery);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(gallery);
        }

        // GET: Admin/Gallery/Edit/5
        public ActionResult Edit(int id)
        {
            var item = db.Galleries.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Admin/Gallery/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Gallery gallery, HttpPostedFileBase imageFile)
        {
            var existing = db.Galleries.Find(gallery.GalleryId);
            if (existing == null)
            {
                return HttpNotFound();
            }

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var uploadDir = Server.MapPath("~/assets/img/gallery");
                    if (!System.IO.Directory.Exists(uploadDir))
                    {
                        System.IO.Directory.CreateDirectory(uploadDir);
                    }
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                    var path = System.IO.Path.Combine(uploadDir, fileName);
                    imageFile.SaveAs(path);
                    existing.ImagePath = "/assets/img/gallery/" + fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "خطا در ذخیره‌سازی تصویر: " + ex.Message);
                    return View(gallery);
                }
            }

            if (ModelState.IsValid)
            {
                existing.Title = gallery.Title;
                existing.Description = gallery.Description;
                existing.DisplayOrder = gallery.DisplayOrder;
                existing.IsActive = gallery.IsActive;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(gallery);
        }

        // POST: Admin/Gallery/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var item = db.Galleries.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: Admin/Gallery/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var item = db.Galleries.Find(id);
            if (item != null)
            {
                db.Galleries.Remove(item);
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
