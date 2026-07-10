using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Azimzadeh_MVC_project.Models
{
    public static class SiteSettingsHelper
    {
        private static Dictionary<string, string> _settingsCache;
        private static DateTime _lastLoaded = DateTime.MinValue;
        private static readonly object _lock = new object();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private static void LoadSettings()
        {
            lock (_lock)
            {
                if (_settingsCache != null && (DateTime.Now - _lastLoaded) < CacheDuration)
                {
                    return;
                }

                try
                {
                    using (var db = new AzimzadehStoreDbEntities())
                    {
                        _settingsCache = db.SiteSettings
                            .ToDictionary(s => s.SettingKey, s => s.SettingValue ?? string.Empty);
                        _lastLoaded = DateTime.Now;
                    }
                }
                catch (Exception)
                {
                    if (_settingsCache == null)
                    {
                        _settingsCache = new Dictionary<string, string>();
                    }
                }
            }
        }

        public static string Get(string key, string defaultValue = "")
        {
            if (_settingsCache == null || (DateTime.Now - _lastLoaded) >= CacheDuration)
            {
                LoadSettings();
            }

            if (_settingsCache.TryGetValue(key, out string value))
            {
                return value;
            }

            return defaultValue;
        }

        public static void ClearCache()
        {
            lock (_lock)
            {
                _settingsCache = null;
                _lastLoaded = DateTime.MinValue;
            }
        }

        // Strongly-typed settings matching db seed keys
        public static string SiteTitle => Get("SiteTitle", "فروشگاه آنلاین چندمنظوره");
        public static string HeaderTitle => Get("HeaderTitle", "Azimzadeh");
        public static string HeaderContactPhone => Get("HeaderContactPhone", "+11 222 3333");
        public static string SiteLogo => Get("SiteLogo", "/assets/img/logo/logo.png");
        public static string SiteFavicon => Get("SiteFavicon", "/assets/img/icon/favicon.png");
        public static string NewsletterTitle => Get("NewsletterTitle", "عضویت در خبرنامه");
        public static string NewsletterDesc => Get("NewsletterDesc", "از آخرین محصولات و تخفیف‌های ویژه ما باخبر شوید.");
        public static string FooterAddress => Get("FooterAddress", "تهران، خیابان آزادی، پلاک ۱۲۳");
        public static string FooterEmail => Get("FooterEmail", "info@example.com");
        public static string FooterPhone => Get("FooterPhone", "۰۲۱-۱۲۳۴۵۶۷۸");
        public static string CopyrightText => Get("CopyrightText", "حقوق کپی رایت © محفوظ است.");
        public static string AboutImage => Get("AboutImage", "/assets/img/banner/about.jpg");
    }
}
