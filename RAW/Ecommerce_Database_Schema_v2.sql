/* =========================================================================
   E-COMMERCE DATABASE SCHEMA (v3 - Jantrik Theme & Security Aligned)
   Target: Microsoft SQL Server (T-SQL)
   Changes from v2:
     - Introduced RBAC (Roles & UserRoles) for strict security
     - Added Sliders, Banners, Brands tables for Jantrik theme adaptation
     - Re-introduced Coupons table
     - Changed DECIMAL(18,2) to DECIMAL(18,0) for Iranian Rial (IRR) alignment
     - Overhauled SiteSettings seed data to map Jantrik placeholders
   ========================================================================= */

-------------------------------------------------------------------
-- 1. IDENTITY & ACCESS (RBAC)
-------------------------------------------------------------------
USE AzimzadehStoreDb;
GO

CREATE TABLE Roles (
    RoleId          INT IDENTITY(1,1) PRIMARY KEY,
    RoleName        NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(150) NOT NULL,
    Email           NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(300) NOT NULL,
    PhoneNumber     NVARCHAR(20)  NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    LastLoginAt     DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2 NULL
);

CREATE TABLE UserRoles (
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    RoleId          INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
    PRIMARY KEY (UserId, RoleId)
);

CREATE TABLE UserAddresses (
    AddressId       INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    Title           NVARCHAR(100) NULL,           -- "Home", "Office"...
    Province        NVARCHAR(100) NULL,
    City            NVARCHAR(100) NULL,
    PostalCode      NVARCHAR(20)  NULL,
    FullAddress     NVARCHAR(500) NOT NULL,
    IsDefault       BIT NOT NULL DEFAULT 0
);

-------------------------------------------------------------------
-- 2. NAVIGATION (PARENT / CHILD MENUS)
-------------------------------------------------------------------

CREATE TABLE ParentMenus (
    ParentMenuId    INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(150) NOT NULL,
    Url             NVARCHAR(300) NULL,
    IconClass       NVARCHAR(100) NULL,
    DisplayOrder    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1
);

CREATE TABLE ChildMenus (
    ChildMenuId     INT IDENTITY(1,1) PRIMARY KEY,
    ParentMenuId    INT NOT NULL FOREIGN KEY REFERENCES ParentMenus(ParentMenuId),
    Title           NVARCHAR(150) NOT NULL,
    Url             NVARCHAR(300) NULL,
    DisplayOrder    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1
);

-------------------------------------------------------------------
-- 3. THEME CONTENT (Sliders, Banners, Brands)
-------------------------------------------------------------------

CREATE TABLE Sliders (
    SliderId        INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(150) NULL,
    Subtitle        NVARCHAR(150) NULL,
    ButtonText      NVARCHAR(50) NULL,
    ButtonLink      NVARCHAR(300) NULL,
    ImagePath       NVARCHAR(300) NOT NULL,
    DisplayOrder    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1
);

CREATE TABLE Banners (
    BannerId        INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(150) NULL,
    LinkUrl         NVARCHAR(300) NULL,
    ImagePath       NVARCHAR(300) NOT NULL,
    Position        NVARCHAR(50) NOT NULL, -- 'SliderRight', 'MidPage', 'Footer'
    DisplayOrder    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1
);

CREATE TABLE Brands (
    BrandId         INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(100) NOT NULL,
    ImagePath       NVARCHAR(300) NOT NULL,
    DisplayOrder    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1
);

-------------------------------------------------------------------
-- 4. CATALOG
-------------------------------------------------------------------

CREATE TABLE Categories (
    CategoryId          INT IDENTITY(1,1) PRIMARY KEY,
    ParentCategoryId    INT NULL FOREIGN KEY REFERENCES Categories(CategoryId), -- self-ref for subcategories
    Title               NVARCHAR(150) NOT NULL,
    Slug                NVARCHAR(200) NOT NULL UNIQUE,
    ImagePath           NVARCHAR(300) NULL,
    DisplayOrder        INT NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1
);

CREATE TABLE Products (
    ProductId           INT IDENTITY(1,1) PRIMARY KEY,
    Title               NVARCHAR(250) NOT NULL,
    Slug                NVARCHAR(300) NOT NULL UNIQUE,
    CategoryId          INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId),
    ShortDescription    NVARCHAR(500) NULL,
    Description         NVARCHAR(MAX) NULL,
    Price               DECIMAL(18,0) NOT NULL,
    DiscountPrice       DECIMAL(18,0) NULL,
    StockQuantity       INT NOT NULL DEFAULT 0,
    SKU                 NVARCHAR(100) NULL UNIQUE,
    ViewCount           INT NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2 NULL
);

CREATE TABLE ProductImages (
    ProductImageId  INT IDENTITY(1,1) PRIMARY KEY,
    ProductId       INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    ImagePath       NVARCHAR(300) NOT NULL,
    IsMain          BIT NOT NULL DEFAULT 0,
    DisplayOrder    INT NOT NULL DEFAULT 0
);

-- Variants now carry their own attributes as JSON instead of 3 separate tables.
-- Example AttributesJson: {"Color":"Red","Size":"XL"}
CREATE TABLE ProductVariants (
    VariantId       INT IDENTITY(1,1) PRIMARY KEY,
    ProductId       INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    SKU             NVARCHAR(100) NULL,
    AttributesJson  NVARCHAR(MAX) NULL,          -- {"Color":"Red","Size":"XL"}
    Price           DECIMAL(18,0) NULL,          -- override base product price if set
    StockQuantity   INT NOT NULL DEFAULT 0,
    ImagePath       NVARCHAR(300) NULL
);

CREATE TABLE Tags (
    TagId           INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE ProductTags (
    ProductId       INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    TagId           INT NOT NULL FOREIGN KEY REFERENCES Tags(TagId),
    PRIMARY KEY (ProductId, TagId)
);

-------------------------------------------------------------------
-- 5. BLOG
-------------------------------------------------------------------

CREATE TABLE BlogCategories (
    BlogCategoryId      INT IDENTITY(1,1) PRIMARY KEY,
    ParentCategoryId    INT NULL FOREIGN KEY REFERENCES BlogCategories(BlogCategoryId), -- self-ref for subcategories
    Title               NVARCHAR(150) NOT NULL,
    Slug                NVARCHAR(200) NOT NULL UNIQUE,
    DisplayOrder        INT NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1
);

CREATE TABLE BlogPosts (
    PostId              INT IDENTITY(1,1) PRIMARY KEY,
    BlogCategoryId      INT NULL FOREIGN KEY REFERENCES BlogCategories(BlogCategoryId),
    UserId              INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),  -- author
    Title               NVARCHAR(250) NOT NULL,
    Slug                NVARCHAR(300) NOT NULL UNIQUE,
    ShortDescription    NVARCHAR(500) NULL,
    Content             NVARCHAR(MAX) NOT NULL,
    ImagePath           NVARCHAR(300) NULL,       -- cover image
    ViewCount           INT NOT NULL DEFAULT 0,
    IsPublished         BIT NOT NULL DEFAULT 0,
    PublishedAt         DATETIME2 NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2 NULL
);

CREATE TABLE BlogPostTags (
    PostId          INT NOT NULL FOREIGN KEY REFERENCES BlogPosts(PostId),
    TagId           INT NOT NULL FOREIGN KEY REFERENCES Tags(TagId),
    PRIMARY KEY (PostId, TagId)
);

-- One shared Comments table for both blog posts and products,
-- using TargetType + TargetId instead of two separate comment tables.
-- TargetType: 'BlogPost' or 'Product'
CREATE TABLE Comments (
    CommentId       INT IDENTITY(1,1) PRIMARY KEY,
    TargetType      NVARCHAR(20) NOT NULL,        -- 'BlogPost' or 'Product'
    TargetId        INT NOT NULL,                 -- PostId or ProductId depending on TargetType
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ParentCommentId INT NULL FOREIGN KEY REFERENCES Comments(CommentId), -- for replies/threading
    Content         NVARCHAR(1000) NOT NULL,
    IsApproved      BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-------------------------------------------------------------------
-- 6. GALLERY
-------------------------------------------------------------------

CREATE TABLE Gallery (
    GalleryId       INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200) NULL,
    ImagePath       NVARCHAR(300) NOT NULL,
    Description     NVARCHAR(500) NULL,
    DisplayOrder    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-------------------------------------------------------------------
-- 7. CUSTOMER INTERACTION
-------------------------------------------------------------------

CREATE TABLE Reviews (
    ReviewId        INT IDENTITY(1,1) PRIMARY KEY,
    ProductId       INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    Rating          TINYINT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment         NVARCHAR(1000) NULL,
    IsApproved      BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Wishlist (
    WishlistId      INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ProductId       INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Wishlist UNIQUE (UserId, ProductId)
);

-------------------------------------------------------------------
-- 8. CART, COUPONS & CHECKOUT
-------------------------------------------------------------------

CREATE TABLE Carts (
    CartId          INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NULL FOREIGN KEY REFERENCES Users(UserId),  -- NULL = guest cart
    SessionId       NVARCHAR(100) NULL,                              -- for guest tracking
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2 NULL
);

CREATE TABLE CartItems (
    CartItemId      INT IDENTITY(1,1) PRIMARY KEY,
    CartId          INT NOT NULL FOREIGN KEY REFERENCES Carts(CartId),
    ProductId       INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    VariantId       INT NULL FOREIGN KEY REFERENCES ProductVariants(VariantId),
    Quantity        INT NOT NULL DEFAULT 1,
    UnitPrice       DECIMAL(18,0) NOT NULL
);

CREATE TABLE Coupons (
    CouponId        INT IDENTITY(1,1) PRIMARY KEY,
    Code            NVARCHAR(50) NOT NULL UNIQUE,
    DiscountType    NVARCHAR(20) NOT NULL, -- 'Percentage', 'FixedAmount'
    DiscountValue   DECIMAL(18,0) NOT NULL,
    MinSpend        DECIMAL(18,0) NULL,
    MaxDiscount     DECIMAL(18,0) NULL,
    ValidFrom       DATETIME2 NULL,
    ValidUntil      DATETIME2 NULL,
    UsageLimit      INT NULL,
    UsedCount       INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1
);

-- Orders now carry their line items as JSON instead of a separate OrderItems table.
-- Example ItemsJson:
-- [{"ProductId":1,"VariantId":null,"Title":"Product A","Qty":2,"UnitPrice":150000,"Total":300000}]
CREATE TABLE Orders (
    OrderId         INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber     NVARCHAR(50) NOT NULL UNIQUE,
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    AddressId       INT NOT NULL FOREIGN KEY REFERENCES UserAddresses(AddressId),
    CouponId        INT NULL FOREIGN KEY REFERENCES Coupons(CouponId),
    ItemsJson       NVARCHAR(MAX) NOT NULL,      -- full line-item snapshot as JSON array
    Subtotal        DECIMAL(18,0) NOT NULL,
    DiscountAmount  DECIMAL(18,0) NOT NULL DEFAULT 0,
    ShippingCost    DECIMAL(18,0) NOT NULL DEFAULT 0,
    TotalAmount     DECIMAL(18,0) NOT NULL,
    OrderStatus     NVARCHAR(30) NOT NULL DEFAULT 'Pending', -- Pending, Processing, Shipped, Delivered, Cancelled
    PaymentStatus   NVARCHAR(30) NOT NULL DEFAULT 'Unpaid',  -- Unpaid, Paid, Failed, Refunded
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2 NULL
);

CREATE TABLE Payments (
    PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId),
    Amount          DECIMAL(18,0) NOT NULL,
    PaymentMethod   NVARCHAR(50) NOT NULL,      -- Zarinpal, IDPay, COD...
    TransactionId   NVARCHAR(200) NULL,
    Status          NVARCHAR(50) NOT NULL,      -- Success, Failed, Pending
    PaidAt          DATETIME2 NULL
);

-------------------------------------------------------------------
-- 9. SITE SETTINGS (key/value)
-------------------------------------------------------------------

CREATE TABLE SiteSettings (
    SettingId       INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey      NVARCHAR(150) NOT NULL UNIQUE,
    SettingValue    NVARCHAR(MAX) NULL,
    ValueType       NVARCHAR(20) NOT NULL DEFAULT 'text',  -- text, image, number, json, html
    Description     NVARCHAR(300) NULL,
    UpdatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Seed suggested keys (mapped directly to Jantrik theme elements)
INSERT INTO SiteSettings (SettingKey, SettingValue, ValueType, Description) VALUES
(N'SiteTitle',          N'فروشگاه آنلاین چندمنظوره', N'text',  N'Main site title'),
(N'HeaderTitle',         N'Azimzadeh',                 N'text',  N'Header title text'),
(N'HeaderContactPhone', N'+11 222 3333',               N'text',  N'Header phone number'),
(N'SiteLogo',           N'/assets/img/logo/logo.png',    N'image', N'Path to site logo'),
(N'SiteFavicon',        N'/assets/img/icon/favicon.png', N'image', N'Path to favicon'),
(N'NewsletterTitle',    N'عضویت در خبرنامه',             N'text',  N'Newsletter block title'),
(N'NewsletterDesc',     N'از آخرین محصولات و تخفیف‌های ویژه ما باخبر شوید.', N'text', N'Newsletter description'),
(N'FooterAddress',      N'تهران، خیابان آزادی، پلاک ۱۲۳', N'text',  N'Footer address'),
(N'FooterEmail',        N'info@example.com',             N'text',  N'Footer contact email'),
(N'FooterPhone',        N'۰۲۱-۱۲۳۴۵۶۷۸',                 N'text',  N'Footer contact phone number'),
(N'SocialMediaLinks',   N'{"twitter":"#","wifi":"#","google-plus":"#","facebook":"#","youtube":"#"}', N'json', N'Social media links'),
(N'CopyrightText',      N'حقوق کپی رایت © <a href="https://www.mellatweb.com/"> Mellatweb.com</a> محفوظ است.', N'html', N'Footer copyright html');

/* =========================================================================
   Helpful indexes
   ========================================================================= */
CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_CartItems_CartId ON CartItems(CartId);
CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_BlogPosts_BlogCategoryId ON BlogPosts(BlogCategoryId);
CREATE INDEX IX_Comments_TargetType_TargetId ON Comments(TargetType, TargetId);

/* =========================================================================
   GENERIC MULTI-PURPOSE SEED DATA FOR TESTING
   ========================================================================= */

-- 1. SEED ROLES
INSERT INTO Roles (RoleName) VALUES (N'Admin'), (N'Customer');

-- 2. SEED USERS (Password hashes are placeholders)
INSERT INTO Users (FullName, Email, PasswordHash, PhoneNumber, IsActive) VALUES
(N'مدیر سایت', 'admin@example.com', 'AQAAAAEAACcQAAAAE...', '09120000000', 1),
(N'کاربر تستی', 'customer@example.com', 'AQAAAAEAACcQAAAAE...', '09121111111', 1);

INSERT INTO UserRoles (UserId, RoleId) VALUES (1, 1), (2, 2);

-- 3. SEED CATEGORIES (Generic)
INSERT INTO Categories (Title, Slug, DisplayOrder, IsActive) VALUES
(N'کالای دیجیتال', 'electronics', 1, 1),
(N'مد و پوشاک', 'fashion', 2, 1),
(N'خانه و آشپزخانه', 'home-appliances', 3, 1);

-- 4. SEED PRODUCTS (Generic)
INSERT INTO Products (Title, Slug, CategoryId, ShortDescription, Description, Price, DiscountPrice, StockQuantity, SKU, IsActive) VALUES
(N'گوشی موبایل سامسونگ Galaxy S23', 'samsung-galaxy-s23', 1, 
 N'گوشی پرچمدار سامسونگ با دوربین عالی', N'توضیحات کامل گوشی موبایل سامسونگ...', 45000000, 43500000, 10, 'DIGI-001', 1),
(N'لپ تاپ ایسوس VivoBook 15', 'asus-vivobook-15', 1, 
 N'لپ‌تاپ سبک و مقرون‌به‌صرفه ایسوس', N'توضیحات کامل لپ‌تاپ ایسوس...', 28000000, NULL, 5, 'DIGI-002', 1),
(N'تی شرت نخی مردانه', 'mens-cotton-tshirt', 2, 
 N'تی‌شرت نخی با کیفیت عالی و تن‌خور مناسب', N'تی‌شرت مردانه صد در صد نخی...', 350000, 290000, 50, 'FASH-001', 1),
(N'قهوه ساز اسپرسو مباشی', 'mebashi-espresso-maker', 3, 
 N'اسپرسوساز خانگی با فشار 20 بار', N'اسپرسوساز پرقدرت با قابلیت تولید کف شیر...', 5200000, NULL, 12, 'HOME-001', 1);

-- 5. SEED PRODUCT IMAGES (Generic Placeholders)
INSERT INTO ProductImages (ProductId, ImagePath, IsMain, DisplayOrder) VALUES
(1, '/assets/img/products/1.jpg', 1, 1),
(2, '/assets/img/products/2.jpg', 1, 1),
(3, '/assets/img/products/3.jpg', 1, 1),
(4, '/assets/img/products/4.jpg', 1, 1);
