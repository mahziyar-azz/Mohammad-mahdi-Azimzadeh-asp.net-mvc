USE AzimzadehStoreDb;
GO

-- 1. Translate English categories to Persian titles
UPDATE Categories SET Title = N'لوازم آرایشی' WHERE Slug = 'beauty';
UPDATE Categories SET Title = N'عطر و ادکلن' WHERE Slug = 'fragrances';
UPDATE Categories SET Title = N'مبلمان' WHERE Slug = 'furniture';
UPDATE Categories SET Title = N'کالاهای اساسی' WHERE Slug = 'groceries';
UPDATE Categories SET Title = N'دکوراسیون منزل' WHERE Slug = 'home-decoration';
UPDATE Categories SET Title = N'لوازم آشپزخانه' WHERE Slug = 'kitchen-accessories';
UPDATE Categories SET Title = N'لپ‌تاپ و رایانه' WHERE Slug = 'laptops';
UPDATE Categories SET Title = N'پیراهن مردانه' WHERE Slug = 'mens-shirts';
UPDATE Categories SET Title = N'کفش مردانه' WHERE Slug = 'mens-shoes';
UPDATE Categories SET Title = N'ساعت مردانه' WHERE Slug = 'mens-watches';
UPDATE Categories SET Title = N'لوازم جانبی موبایل' WHERE Slug = 'mobile-accessories';

-- Deactivate older dummy categories that are not used by any products
UPDATE Categories SET IsActive = 0 WHERE Title LIKE '%???%' OR Title LIKE '%دسته‌بندی%';

-- 2. Insert standard Persian tags if they do not exist
IF NOT EXISTS (SELECT * FROM Tags WHERE Title = N'تخفیف ویژه') INSERT INTO Tags (Title) VALUES (N'تخفیف ویژه');
IF NOT EXISTS (SELECT * FROM Tags WHERE Title = N'دیجیتال') INSERT INTO Tags (Title) VALUES (N'دیجیتال');
IF NOT EXISTS (SELECT * FROM Tags WHERE Title = N'جدید') INSERT INTO Tags (Title) VALUES (N'جدید');
IF NOT EXISTS (SELECT * FROM Tags WHERE Title = N'لوکس') INSERT INTO Tags (Title) VALUES (N'لوکس');
IF NOT EXISTS (SELECT * FROM Tags WHERE Title = N'کاربردی') INSERT INTO Tags (Title) VALUES (N'کاربردی');
IF NOT EXISTS (SELECT * FROM Tags WHERE Title = N'ارگانیک') INSERT INTO Tags (Title) VALUES (N'ارگانیک');
IF NOT EXISTS (SELECT * FROM Tags WHERE Title = N'پرطرفدار') INSERT INTO Tags (Title) VALUES (N'پرطرفدار');

-- Clear existing mappings in ProductTags if any
DELETE FROM ProductTags;

-- 3. Map tags to products dynamically based on properties
-- Tag: 'تخفیف ویژه'
INSERT INTO ProductTags (ProductId, TagId)
SELECT ProductId, (SELECT TagId FROM Tags WHERE Title = N'تخفیف ویژه')
FROM Products WHERE DiscountPrice IS NOT NULL AND IsActive = 1;

-- Tag: 'دیجیتال'
INSERT INTO ProductTags (ProductId, TagId)
SELECT ProductId, (SELECT TagId FROM Tags WHERE Title = N'دیجیتال')
FROM Products WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE Slug IN ('laptops', 'mobile-accessories')) AND IsActive = 1;

-- Tag: 'جدید'
INSERT INTO ProductTags (ProductId, TagId)
SELECT ProductId, (SELECT TagId FROM Tags WHERE Title = N'جدید')
FROM Products WHERE ProductId % 3 = 0 AND IsActive = 1;

-- Tag: 'لوکس'
INSERT INTO ProductTags (ProductId, TagId)
SELECT ProductId, (SELECT TagId FROM Tags WHERE Title = N'لوکس')
FROM Products WHERE Price > 15000000 AND IsActive = 1;

-- Tag: 'کاربردی'
INSERT INTO ProductTags (ProductId, TagId)
SELECT ProductId, (SELECT TagId FROM Tags WHERE Title = N'کاربردی')
FROM Products WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE Slug IN ('groceries', 'kitchen-accessories', 'furniture')) AND IsActive = 1;

-- Tag: 'پرطرفدار'
INSERT INTO ProductTags (ProductId, TagId)
SELECT ProductId, (SELECT TagId FROM Tags WHERE Title = N'پرطرفدار')
FROM Products WHERE (ViewCount > 20 OR ProductId % 4 = 0) AND IsActive = 1;
