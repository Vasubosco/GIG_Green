using System.ComponentModel.DataAnnotations;

namespace giggreen.Models
{
    public class RIAYATIViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(50)]
        public string Password { get; set; }

   

    }

 
}
