using System.ComponentModel.DataAnnotations;

namespace giggreen.Models
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(50)]
        public string Password { get; set; }

   

    }


    public class UserDetails
    {
       
        public string Username { get; set; }


        public string Password { get; set; }


        public string RoleName { get; set; }
    }
}
