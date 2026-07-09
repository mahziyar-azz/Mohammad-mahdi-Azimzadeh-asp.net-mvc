using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Products
        public ActionResult Index(int? categoryId, int page = 1)
        {
            int pageSize = 15;
            var query = db.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var total = query.Count();
            var products = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Total = total;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = db.Categories.Where(c => c.IsActive).OrderBy(c => c.Title).ToList();

            return View(products);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.IsActive).OrderBy(c => c.Title), "CategoryId", "Title");
            return View(new Product { IsActive = true, StockQuantity = 10 });
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase mainImageFile)
        {
            if (string.IsNullOrWhiteSpace(product.Title))
            {
                ModelState.AddModelError("Title", "وارد کردن عنوان محصول الزامی است.");
            }

            if (ModelState.IsValid)
            {
                product.Slug = GenerateSlug(product.Title);
                product.CreatedAt = DateTime.Now;
                product.ViewCount = 0;

                db.Products.Add(product);
                db.SaveChanges();

                // Handle Image Upload
                if (mainImageFile != null && mainImageFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = Server.MapPath("~/assets/img/products");
                        if (!System.IO.Directory.Exists(uploadDir))
                        {
                            System.IO.Directory.CreateDirectory(uploadDir);
                        }
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(mainImageFile.FileName);
                        var path = System.IO.Path.Combine(uploadDir, fileName);
                        mainImageFile.SaveAs(path);

                        var productImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImagePath = "/assets/img/products/" + fileName,
                            IsMain = true,
                            DisplayOrder = 0
                        };
                        db.ProductImages.Add(productImage);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "محصول ذخیره شد اما آپلود تصویر با خطا مواجه گردید: " + ex.Message;
                    }
                }

                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.IsActive).OrderBy(c => c.Title), "CategoryId", "Title", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int id)
        {
            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.IsActive).OrderBy(c => c.Title), "CategoryId", "Title", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product, HttpPostedFileBase mainImageFile)
        {
            if (string.IsNullOrWhiteSpace(product.Title))
            {
                ModelState.AddModelError("Title", "وارد کردن عنوان محصول الزامی است.");
            }

            if (ModelState.IsValid)
            {
                var existing = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.ProductId == product.ProductId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.Title = product.Title;
                existing.CategoryId = product.CategoryId;
                existing.ShortDescription = product.ShortDescription;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.DiscountPrice = product.DiscountPrice;
                existing.StockQuantity = product.StockQuantity;
                existing.SKU = product.SKU;
                existing.IsActive = product.IsActive;
                existing.UpdatedAt = DateTime.Now;

                // Handle Image Upload
                if (mainImageFile != null && mainImageFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = Server.MapPath("~/assets/img/products");
                        if (!System.IO.Directory.Exists(uploadDir))
                        {
                            System.IO.Directory.CreateDirectory(uploadDir);
                        }
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(mainImageFile.FileName);
                        var path = System.IO.Path.Combine(uploadDir, fileName);
                        mainImageFile.SaveAs(path);

                        // Deactivate old main images
                        var oldMains = db.ProductImages.Where(pi => pi.ProductId == product.ProductId && pi.IsMain).ToList();
                        foreach (var img in oldMains)
                        {
                            img.IsMain = false;
                        }

                        var productImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImagePath = "/assets/img/products/" + fileName,
                            IsMain = true,
                            DisplayOrder = 0
                        };
                        db.ProductImages.Add(productImage);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "تغییرات ذخیره شد اما آپلود تصویر جدید با خطا مواجه گردید: " + ex.Message;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.IsActive).OrderBy(c => c.Title), "CategoryId", "Title", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: Admin/Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                // Delete image files from disk
                foreach (var img in product.ProductImages)
                {
                    try
                    {
                        var fullPath = Server.MapPath("~" + img.ImagePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    catch { }
                }

                // Delete relations first
                var reviews = db.Reviews.Where(r => r.ProductId == id).ToList();
                db.Reviews.RemoveRange(reviews);

                var wishlists = db.Wishlists.Where(w => w.ProductId == id).ToList();
                db.Wishlists.RemoveRange(wishlists);

                var cartItems = db.CartItems.Where(c => c.ProductId == id).ToList();
                db.CartItems.RemoveRange(cartItems);

                var variants = db.ProductVariants.Where(pv => pv.ProductId == id).ToList();
                db.ProductVariants.RemoveRange(variants);

                db.ProductImages.RemoveRange(product.ProductImages);
                db.Products.Remove(product);
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
            while (db.Products.Any(p => p.Slug == slug))
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
