using System;
using System.Runtime.Serialization;

namespace ProductServiceLibrary
{
    [DataContract]
    public class Product
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public string SKU { get; set; }

        [DataMember]
        public int StockQuantity { get; set; }

        [DataMember]
        public string ImageUrl { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public DateTime? LastModifiedDate { get; set; }
    }
}
