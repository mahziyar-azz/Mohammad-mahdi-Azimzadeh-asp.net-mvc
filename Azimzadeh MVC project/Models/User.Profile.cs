using System;

namespace Azimzadeh_MVC_project.Models
{
    public partial class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string HomePhoneNumber { get; set; }
        public string CardNumber { get; set; }
        public string CVV { get; set; }
        public string ExpirationDate { get; set; }
        public string Gender { get; set; }
    }
}
