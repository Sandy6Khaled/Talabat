﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities.Order_Aggregate
{
    public class Order : BaseEntity
    {
        public Order()
        {

        }

        /// Here, Accessible Empty Parameterless Constructor must be Exist
        public Order(string buyerEmail, Address shippingAddress, DeliveryMethod deliveryMethod, ICollection<OrderItem> items, decimal subtotal,string paymentIntentId)
        {
            BuyerEmail = buyerEmail;
            ShippingAddress = shippingAddress;
            DeliveryMethod = deliveryMethod;
            Items = items;
            Subtotal = subtotal;
            PaymentIntentId = paymentIntentId;
        }

        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Address ShippingAddress { get; set; }

        //public int DeliveryMethodId { get; set; } // Foreign Key
        public DeliveryMethod? DeliveryMethod { get; set; } // Navigational Property [ONE]
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>(); // Navigational Property [MANY]
        public decimal Subtotal { get; set; }
        //[NotMapped]
        //public decimal Total { get { return Subtotal + DeliveryMethod.Cost; } }
        public decimal GetTotal()
            => (Subtotal + DeliveryMethod.Cost);
        public string PaymentIntentId { get; set; }
    }
}
