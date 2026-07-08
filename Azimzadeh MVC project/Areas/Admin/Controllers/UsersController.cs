using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Azimzadeh_MVC_project.Models;

namespace Azimzadeh_MVC_project.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private AzimzadehStoreDbEntities db = new AzimzadehStoreDbEntities();

        // GET: Admin/Users
        public ActionResult Index(int page = 1)
        {
            int pageSize = 15;
            var total = db.Users.Count();
            var users = db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Total = total;
            return View(users);
        }

        // POST: Admin/Users/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: Admin/Users/Edit/5
        public ActionResult Edit(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string firstName, string lastName, string email, string phone, 
            string address1, string address2, string homePhoneCountry, string homePhoneBody, 
            string cardNumber, string cvv, string expirationDate, string gender, string newPassword)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "نام، نام خانوادگی و ایمیل نمی‌توانند خالی باشند.");
                return View(user);
            }

            // Home phone validation
            string combinedHomePhone = null;
            if (!string.IsNullOrWhiteSpace(homePhoneCountry) || !string.IsNullOrWhiteSpace(homePhoneBody))
            {
                if (string.IsNullOrWhiteSpace(homePhoneCountry) || string.IsNullOrWhiteSpace(homePhoneBody))
                {
                    ModelState.AddModelError("", "لطفاً هر دو بخش پیش‌شماره و شماره تلفن ثابت را وارد کنید.");
                    return View(user);
                }

                string countryClean = homePhoneCountry.Trim();
                string bodyClean = homePhoneBody.Trim();

                if (!System.Text.RegularExpressions.Regex.IsMatch(countryClean, @"^\d{2,3}$"))
                {
                    ModelState.AddModelError("", "پیش‌شماره تلفن ثابت باید بین ۲ تا ۳ رقم باشد.");
                    return View(user);
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(bodyClean, @"^\d{8}$"))
                {
                    ModelState.AddModelError("", "شماره تلفن ثابت باید دقیقاً ۸ رقم باشد.");
                    return View(user);
                }

                combinedHomePhone = "+" + countryClean + "-" + bodyClean;
            }

            // Billing information validation (Requires Legal/Security Review)
            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                string cardClean = cardNumber.Trim().Replace(" ", "").Replace("-", "");
                if (!System.Text.RegularExpressions.Regex.IsMatch(cardClean, @"^\d{16}$"))
                {
                    ModelState.AddModelError("", "شماره کارت بانکی باید ۱۶ رقم باشد.");
                    return View(user);
                }
                user.CardNumber = cardClean;
            }
            else
            {
                user.CardNumber = null;
            }

            if (!string.IsNullOrWhiteSpace(cvv))
            {
                string cvvClean = cvv.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(cvvClean, @"^\d{3,4}$"))
                {
                    ModelState.AddModelError("", "کد CVV2 باید ۳ یا ۴ رقم باشد.");
                    return View(user);
                }
                user.CVV = cvvClean;
            }
            else
            {
                user.CVV = null;
            }

            if (!string.IsNullOrWhiteSpace(expirationDate))
            {
                string expClean = expirationDate.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(expClean, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    ModelState.AddModelError("", "تاریخ انقضا باید به فرمت MM/YY باشد (مثلا 08/29).");
                    return View(user);
                }
                user.ExpirationDate = expClean;
            }
            else
            {
                user.ExpirationDate = null;
            }

            // Email uniqueness validation
            string cleanEmail = email.Trim().ToLower();
            var existingUser = db.Users.FirstOrDefault(u => u.Email == cleanEmail && u.UserId != id);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "این آدرس ایمیل قبلاً توسط کاربر دیگری ثبت شده است.");
                return View(user);
            }

            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();
            user.FullName = (firstName.Trim() + " " + lastName.Trim()).Trim();
            user.Email = cleanEmail;
            user.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            user.Address1 = string.IsNullOrWhiteSpace(address1) ? null : address1.Trim();
            user.Address2 = string.IsNullOrWhiteSpace(address2) ? null : address2.Trim();
            user.HomePhoneNumber = combinedHomePhone;
            user.Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();

            // Optional password update
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));
                    user.PasswordHash = Convert.ToBase64String(bytes);
                }
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "اطلاعات کاربر با موفقیت ویرایش شد.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
