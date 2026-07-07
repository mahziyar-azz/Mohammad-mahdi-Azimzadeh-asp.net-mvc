# Azimzadeh E-Commerce MVC Project

A professional, localized e-commerce web application built using **ASP.NET MVC (.NET Framework 4.8)** and **SQL Server (MSSQL)**. 

The project features a fully mirrored **Right-to-Left (RTL)** storefront utilizing the premium **Jantrik** bootstrap theme, complete with Persian translations, regional digit format optimizations, and custom database integrations.

---

## 🚀 Key Features

*   **ASP.NET MVC 4.8 Architecture**: Structured backend controllers and Razor CSHTML views separated from static assets.
*   **Complete Persian (Farsi) Localization**:
    *   Dynamic Right-to-Left styling overrides in `rtl.css`.
    *   Integrated **Vazirmatn** Persian font.
    *   Corrected character mappings and fixed character corruption (mojibake) by enforcing UTF-8 with BOM and configuring `<globalization>` in `Web.config`.
*   **Database Schema & Seed Data (v3)**:
    *   Strong Role-Based Access Control (RBAC) with normalized `Roles` and `UserRoles` tables.
    *   Dynamic layout management through `SiteSettings` (supports logo paths, phone numbers, copyright tags, and social media links in JSON format).
    *   Integrated support for Sliders, promotional Banners, and Brand logo carousels.
    *   `DECIMAL(18,0)` precision for exact Iranian Rial (IRR) e-commerce transactions.
*   **Entity Framework 6**: Implemented Database-First mapping utilizing `.edmx` templates and generated C# entities for direct, safe database operations.

---

## 🛠️ Database Setup

The database schema script is available in the [RAW/Ecommerce_Database_Schema_v2.sql](file:///c:/Users/Mahziyar%20Azimzadeh/source/repos/Azimzadeh%20MVC%20project/RAW/Ecommerce_Database_Schema_v2.sql) folder.

1. Open **SQL Server Management Studio (SSMS)**.
2. Create a database named `AzimzadehStoreDb`.
3. Open and execute the schema/seed script on your database.

---

## ⚙️ How to Run locally

1. Open the solution file `Azimzadeh MVC project.sln` in **Visual Studio 2022**.
2. Restore NuGet packages:
   ```powershell
   Update-Package -reinstall
   ```
3. Update connection settings in your `Web.config` if your SQL Server instance is not local:
   ```xml
   <connectionStrings>
     <add name="AzimzadehStoreDbEntities" connectionString="metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=YOUR_SERVER;initial catalog=AzimzadehStoreDb;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />
   </connectionStrings>
   ```
4. Press **F5** or click **Start** in Visual Studio to launch the application using IIS Express.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](file:///c:/Users/Mahziyar%20Azimzadeh/source/repos/Azimzadeh%20MVC%20project/LICENSE) file for details.
