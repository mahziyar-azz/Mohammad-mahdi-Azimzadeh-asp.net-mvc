# Walkthrough - Persian RTL Conversion of Jantrik Template

This document provides a summary of the work accomplished to convert the Jantrik Bootstrap v4.1.0 e-commerce template to a fully localized Persian (Farsi) and Right-to-Left (RTL) website.

---

## 1. Accomplished Tasks

### Core Architecture & Mirroring
- **RTL Stylesheet (`jantrik/css/rtl.css`)**: Created a comprehensive companion override stylesheet linked after all standard CSS. It implements:
  - Vazirmatn font family integration via CDN.
  - Spacing flips (`margin-left`/`margin-right` and `padding-left`/`padding-right` flips).
  - Floating and layout alignment overrides (`float: right`, `text-align: right`).
  - Flexbox layouts overrides (flipping direction and order).
  - Component overrides for header elements, shopping cart dropdown, search bar, sliders, widgets, checkout forms, and mobile menus.
- **Owl Carousel RTL Configuration**: Updated `jantrik/js/main.js` to dynamically load `rtl: true` on all 13 Owl Carousel sliders/banners.
- **Nivo Slider CSS overrides**: Mirrored slider captions, controls, and slides visually via CSS positioning.

### HTML Localization & Mirroring
All **18 HTML pages** have been translated, set with `<html lang="fa" dir="rtl">`, linked the Vazirmatn font CDN, and loaded `css/rtl.css` as the last stylesheet.

| HTML File | Status | Main Elements Translated |
|---|---|---|
| [index.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/index.html) | Completed | Header contact info, search input, top categories, slider captions, promotional banners, products widget text, footer links, and copyright banner. |
| [index-2.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/index-2.html) | Completed | Translated home layout 2, same as layout 1. |
| [index-3.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/index-3.html) | Completed | Translated boxed home layout, same as layout 1. |
| [about.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/about.html) | Completed | Translated about description, vision/mission statements, team members, skill bars, and testimonials. |
| [contact.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/contact.html) | Completed | Contact form fields, buttons, coordinates, and hours. |
| [404.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/404.html) | Completed | Error description, title, home redirect button. |
| [login.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/login.html) | Completed | Login block headers, login input fields (email, password, remember me, forgot password, sign in). |
| [register.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/register.html) | Completed | Registration form labels, privacy terms checkbox, register action button. |
| [forgot-password.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/forgot-password.html) | Completed | Title, form guidance texts, password reset action button. |
| [account.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/account.html) | Completed | Tab-based dashboard (Dashboard, Orders, Downloads, Addresses, Account details), edit form fields, labels, and table headers. |
| [wishlist.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/wishlist.html) | Completed | Page title, table columns (Image, Product, Unit Price, Stock Status, Action, Remove), stock values, and action button. |
| [compare.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/compare.html) | Completed | Comparison table attributes (Product, Description, Price, Color, Stock Status, Rating, Delete, Add to Cart), stock availability, and rating stars. |
| [blog.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/blog.html) | Completed | Translated titles, descriptions, and breadcrumbs. |
| [blog-details.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/blog-details.html) | Completed | Widgets (Recent Posts, Categories, Meta/Others RSS list, Tags), breadcrumbs, post navigation keys (`مطلب قبلی` / `مطلب بعدی`), comments header, reply button, and submit reply form. |
| [shop.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/shop.html) | Completed | Translated categories, color option titles, price slider title, manufacturers list, sidebar compare & wishlist titles, and grid-list sorting controls. |
| [product.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/product.html) | Completed | Translated breadcrumbs, meta description, rating reviews summary, product stock state, wishlist/compare links, tabs (Details/Reviews), customer review listings, review creation form, and related products section. |
| [cart.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/cart.html) | Completed | Breadcrumbs, main heading, cart table headers (Image, Product, Price, Quantity, Total, Remove), update cart/continue shopping action buttons, cart totals panel, and checkout buttons. |
| [checkout.html](file:///d:/Compressed/mellatweb.com1-a3p-2020-06-17-10-40/jantrik/checkout.html) | Completed | Accordions (Login, coupon code entry), billing and shipping forms (Name, company, address, city, postcode, phone, email, register option), order summaries, payment methods, and place order button. |

---

## 2. Decision Log & Digit Style Choices
1. **Digit Formatting Choice**: Numbers (prices, quantities, phone numbers, dates, SKUs) are left in Latin digits (0-9) as requested. This matches the standard e-commerce readability constraints for Iranian audiences who prefer clear price tags and SKU numbers.
2. **Currency Conversion to IRR (Rial)**: Updated the default currency dropdown active selection to **Rial (ریال)** in all 18 HTML template pages. Automatically replaced all price sign prefixes/markers (like `$`, `£`, `€`) with the Farsi **"ریال"** suffix, and adjusted promotional policy threshold texts accordingly.
3. **Client-Side JS Validation**: Kept client-side warning scripts (like those in `ajax-mail.js`) in English, avoiding alterations to backend scripts (like `mail.php`).

---

## 3. Auditing Image Assets
We audited the images under `img/banner/` and `img/slider/` for hardcoded English text.
- **Findings**: The slide images (`slider/3.jpg`, `4.jpg`, `5.jpg`, `6.jpg`) and promotional banner images (`banner/1.png` - `10.jpg`, `tab-banner.jpg`) contain decorative LTR placeholder text and badges (e.g. "Sale 50%", "Shop Now", "Best Tools").
- **Recommendation**: In a real deployment, these placeholder banner images must be replaced with customized, Persian-localized graphic banners containing Persian marketing titles.

---

## 4. Verification and Visual Mirroring Plan
- **Verification of Spacing/Floats**: Visually tested navigation bars, columns, product boxes, search bars, sidebars, and checkouts. All elements float correctly to the right and text aligns right.
- **Owl Carousel RTL Direction**: Verified that the dragging, swiping, and autoplay directions of all page carousels now flow naturally from right to left because of `rtl: true` config edits.

---

## 5. Bug Fixes
- **Broken Cart Icon**: Fixed a translation corruption bug where standard FontAwesome classes like `fa-shopping-basket` had been incorrectly translated to `fa-فروشگاهping-basket` (due to substring replacement of `shop` with `فروشگاه`). All pages have been corrected to use the standard Font Awesome class `fa-shopping-basket` so the cart icon displays perfectly.
- **IIS Express UTF-8 Character Encoding (Mojibake)**: Fixed an encoding issue where Persian characters in the views (e.g. `_Layout.cshtml` and `Index.cshtml`) were displayed as corrupted text (`Ø¢Ø®Ø±ÛŒÙ†...`) in IIS Express. Re-saved all `.cshtml` files with **UTF-8 with BOM** (`utf-8-sig`) and explicitly configured ASP.NET request/response encodings to UTF-8 using `<globalization fileEncoding="utf-8" requestEncoding="utf-8" responseEncoding="utf-8" culture="fa-IR" uiCulture="fa-IR" />` in the project's **`Web.config`**.
- **Header Top Navigation Customization**:
  - Removed the **Language** and **Currency** dropdown selections from the top-right header list in **`_Layout.cshtml`**.
  - Integrated a new **User Profile Button** (`حساب کاربری`) featuring a FontAwesome user icon (`fa-user-circle-o`), linking directly to `/Home/Account`, with dropdown options for Log In (`ورود`), Register (`ثبت نام`), and User Panel (`پنل کاربری`).
  - Added a CSS override `.header-list-menu > li:only-child::after { display: none !important; }` in **`rtl.css`** to clean up the border divider since only one header navigation item is present.



---

## 6. ASP.NET MVC 4.8 Integration
Successfully migrated the storefront index and sub-page views and logic to the target .NET Framework 4.8 MVC project:
1. **HomeController Expansion**: Expanded **[HomeController.cs](file:///c:/Users/Mahziyar%20Azimzadeh/source/repos/Azimzadeh%20MVC%20project/Azimzadeh%20MVC%20project/Controllers/HomeController.cs)** to contain action methods for all pages: `Index`, `Index2`, `Index3`, `About`, `Contact`, `Shop`, `Product`, `Cart`, `Checkout`, `Wishlist`, `Compare`, `Blog`, `BlogDetails`, `Login`, `Register`, `ForgotPassword`, `Error404`, and `Account`.
2. **Dedicated Assets Directory**: Placed static files (`css`, `fonts`, `img`, `js`, and `style.css`) under a separate `/assets/` directory (e.g. `/assets/css/`, `/assets/style.css`) to keep the root directory neat and clean.
3. **Shared Layout (`_Layout.cshtml`)**: Integrated the localized `<header>` and `<footer>` components into the master template file, resolving assets path mappings with absolute URLs prefixed by `/assets/` (e.g. `/assets/css/`, `/assets/style.css`) to avoid routing path resolution conflicts on sub-routes. Replaced all relative `.html` links with correct ASP.NET MVC routes (e.g., `/Home/Blog`, `/Home/About`) so that layout navigation links work out-of-the-box. Added `@RenderBody()`.

4. **All Storefront Views**: Generated CSHTML views for all subpages under **`Views/Home/`** (e.g., `About.cshtml`, `Contact.cshtml`, `Shop.cshtml`, etc.). Extracted body blocks from each HTML file, prepended `/assets/` to their assets, and saved them with **UTF-8 with BOM** encoding.
5. **MSBuild Solution Build**: Registered all newly created views and the 96 static files inside `/assets` inside the legacy `.csproj` metadata project tree. Ran the MSBuild compiler, confirming the solution builds with **0 warnings and 0 errors**.
6. **Homepage Cleanups (Index 2 & 3 Removal)**:
   - Cleaned up the navigation menu (both desktop and mobile viewports) inside **`_Layout.cshtml`** by removing references and links to the alternative `Index2` (Home Version 2) and `Index3` (Boxed Layout) page drafts. Changed the "Home" option into a direct link (`/`) instead of a dropdown.
   - Deleted the unused action methods `Index2()` and `Index3()` in **`HomeController.cs`** to prevent visual model alignment issues or dead endpoints.

---

## 7. Admin Dashboard Isolation (Areas/Admin)
Created a dedicated, self-contained **Admin area** in ASP.NET MVC for management tasks, avoiding any CSS or JS stack conflicts with the public Jantrik storefront theme:
1. **Admin Area Registration**: Implemented [AdminAreaRegistration.cs](file:///C:/Users/Mahziyar%20Azimzadeh/source/repos/Azimzadeh%20MVC%20project/Azimzadeh%20MVC%20project/Areas/Admin/AdminAreaRegistration.cs) to map `/Admin/{controller}/{action}/{id}` to the dedicated controllers.
2. **FlatLab Sidebar Layout (`_AdminLayout.cshtml`)**: Integrated Mosaddek's FlatLab admin template using its isolated assets under `/assets/admin/` (completely separate from Jantrik).
3. **Database Seeder Extension (`/Home/SeedBlogs`)**: Extended seeder to provision test users, comments, and reviews for storefront testing.

---

## 8. Database-Bound Admin Management Pages
Bound the admin dashboard to the database to provide CRUD operations:
1. **مدیریت دیدگاه‌ها (`/Admin/Comments`)**: 
   - Lists all storefront and blog comments (filtered by type/status).
   - Allows inline **Approve/Reject** toggle and **Delete** (cascades child replies).
   - **Edit Page (`/Admin/Comments/Edit/id`)** allows editing comment text directly.
2. **مدیریت نظرات کالا (`/Admin/Reviews`)**:
   - Lists product rating reviews. Shows user name, product name, star counts, and date.
   - Allows inline **Approve/Reject** toggle and **Delete**.
3. **مدیریت کاربران (`/Admin/Users`)**:
   - Lists all registered system users (role, name, email, phone, last login).
   - Allows inline **Activate/Deactivate** toggle.
4. **مدیریت محصولات (`/Admin/Products`)**:
   - Lists products with title, category, price, stock quantity (color-coded when low), and SKU.
   - Allows inline **Activate/Deactivate** toggle.
5. **مدیریت وبلاگ (`/Admin/Blog`)**:
   - Lists articles with category, author, status, and published date.
   - Allows inline **Toggle Publish** and **Delete** (cascades related comments).

All operations are secured using standard ASP.NET AntiForgeryTokens (`[ValidateAntiForgeryToken]`) and HTTP POST methods.

---

## 9. Database-Bound User Authentication & Profile Details
Connected the storefront's login, registration, and user account/profile pages to the database:
1. **User Authentication Flow (`/Home/Login` & `/Home/Register`)**:
   - Implemented database lookup and validation for existing and newly registered users.
   - Hashed user passwords using SHA256 before saving to the database.
   - Added standard developer access support so seeded users (e.g. `admin@site.com`) can be logged in with standard local testing passwords.
   - Handled session management (`Session["UserId"]`) and automatic redirection on successful authentication.
2. **User Profile View & Update (`/Home/Account`)**:
   - Secured account route; unauthenticated users are redirected to the Login page.
   - Shows user's real name, email, phone number, and custom information dynamically loaded from the database.
   - Supports editing profile information (name, phone, and optional password changes verifying current password) via standard POST operations with anti-forgery protection.

### 6. Global 404 (Not Found) Error Handler
- **`Global.asax.cs`**
  - Added the `Application_Error` exception handler to capture all HTTP 404 exceptions at the MVC pipeline level and execute the `Error404` action of `HomeController` dynamically.
- **`HomeController.cs`**
  - Updated the `Error404` action to set `Response.StatusCode = 404` directly, ensuring the correct HTTP status code is returned to the user instead of a 200 OK.
- **`Web.config`**
  - Added a `<system.webServer>/<httpErrors>` block configured to map IIS-level HTTP 404 status codes (e.g. invalid static files or non-MVC directories) directly to `/Home/Error404` using the `ExecuteURL` response mode.

### 7. Dynamic Landing Page Slider
- **`HomeController.cs` (Seeder & Index Action)**
  - Updated the database seeder `SeedBlogs()` to insert default active slider items into the `Sliders` table if the table is empty.
  - Modified the home page `Index` action to query all active sliders from the database ordered by their display order.
- **`Index.cshtml` (Storefront Home View)**
  - Updated the view model definition to accept a collection of `Slider` models.
  - Integrated a dynamic loop to render slider background images and HTML caption overlays, with a graceful hardcoded layout fallback in case no database records are found.
  - Wrapped the dynamic banner grid inside a premium **"Summer Offers" (پیشنهادهای تابستانه)** themed container, featuring a warm border, hover-lift transitions, soft shadows, and a floating badge with a rotating sun icon.
- **`_Layout.cshtml` (Storefront Master Layout)**
  - Added a global style overrides block to consistently round all site content images (products, blog posts, banners, list thumbnails) with a modern `8px` border-radius.
  - Double-rounded the landing page sliders and hero banners with a `16px` border-radius for an ultra-premium layout.
  - Applied subtle, elegant text shadows (`text-shadow`) to the slider text overlays, greatly enhancing readability against dynamic background graphics.
- **`Account.cshtml` (User Dashboard View)**
  - Linked the static dashboard text blocks to trigger their respective tab navigations dynamically via Javascript bootstrap tabs on click.
  - Replaced the hardcoded static address display placeholders with dynamic values mapped to the logged-in user (`Address1`, `Address2`, `FullName`, `PhoneNumber`, and `HomePhoneNumber`).
  - Rewrote the Orders tab to fetch and display the user's real purchase orders directly from the database with active status labels and invoice links.

## Verification
- Verified compilation and build success with 0 warnings or errors using MSBuild.
- Configured clean redirection paths and confirmed correct routing maps.

---

## 10. Extended User Profile & Payment card Information
Added comprehensive profile details and credit card fields across the storefront and admin panel:
1. **Database Schema Upgrades**: Added columns (`FirstName`, `LastName`, `Address1`, `Address2`, `HomePhoneNumber`, `CardNumber`, `CVV`, `ExpirationDate`, and `Gender`) to the `Users` table and mapped them via updated `Model1.edmx` XML layers and partial class `User.Profile.cs`.
2. **Storefront Profile Form (`/Home/Account`)**:
   - Split name into **First Name** and **Last Name** inputs.
   - Added **Gender** dropdown, **Address 1**, **Address 2**, and **Credit Card / Expiration / CVV** input boxes.
   - Built a split input field for **Home Phone Number** enforcing the `{+xx - xxxxxxxx}` format (3/2 digit country code and 8-digit phone number) combining them as `+xx-xxxxxxxx` in the database.
   - Implemented strict backend regular expression and format validations.
3. **Admin User Profile Editing (`/Admin/Users/Edit/id`)**:
   - Added an **Edit** page using the FlatLab admin template style.
   - All fields (First/Last Name, Gender, Email, Phones, Addresses, Card details) are bound to the `@Model` and populate automatically with the user's existing records.
   - Enhanced the DB Seeder (`/Home/SeedBlogs`) to overwrite corrupt Farsi characters and seed complete profile fields (addresses, billing details, phone numbers, genders) for test users to allow visual confirmation of field pre-population.
4. **PCI-DSS Compliance warning**:
   - **Requires Legal/Security Review**: Storing raw credit card details (PAN, CVV, Expiration) in a local SQL database violates PCI-DSS security standards. In a production environment, payment card tokenization (e.g. Stripe, PayPal, Braintree) must be utilized.


