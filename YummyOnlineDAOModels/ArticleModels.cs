using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YummyOnlineDAO.Models
{
    public enum ArticleStatus
    {
        Ungranted  = 0,
        Granted  =1,
    }
    public class Article
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(128),Required]
        public string Title { get; set; }

        [Required]
        public string PictruePath { get; set; }

        [Required]
        public string Discription { get; set; }

        [Required] 
        public string Body { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateTime { get; set; }

        public ArticleStatus Status { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public string StaffId { get; set; }
        public Staff Staff { get; set; }
    }
}
