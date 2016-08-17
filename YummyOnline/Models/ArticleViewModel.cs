using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace YummyOnline.Models {
	public class ArticleViewModel {
		public string Title { get; set; }
		public string PicturePath { get; set; }
		public string Description { get; set; }
		public string Body { get; set; }
		public int? HotelId { get; set; }
	}
}