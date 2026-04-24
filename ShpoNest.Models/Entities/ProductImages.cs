using System;
using System.Collections.Generic;
using System.Text;

namespace ShpoNest.Models.Entities
{
    public class ProductImages
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }
        public DateTime CreatedAt { get; set; }

        //Fk
        public int ProductId { get; set; }

        //Nav Prop
        public Product Product { get; set; }
    }
}
