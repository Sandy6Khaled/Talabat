using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories;
using Talabat.Core.Services;
using Talabat.Core.Specifications.Oder_Specification;
using Product = Talabat.Core.Entities.Product;

namespace Talabat.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketRepository _basketRepo;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IBasketRepository basketRepo,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _basketRepo = basketRepo;
            _configuration = configuration;
        }
        public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var basket=await _basketRepo.GetBasketAsync(basketId);
            if (basket is null) return null;
            var shippingPrice = 0M;
            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(basket.DeliveryMethodId.Value);
                basket.ShippingPrice = deliveryMethod.Cost;
                shippingPrice = deliveryMethod.Cost;
            }
            if(basket?.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    if(item.Price != product.Price)
                        item.Price = product.Price;
                }
            }
            PaymentIntentService paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent;
            if(string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions()
                {
                    Amount = (long) basket.Items.Sum(item => item.Price * item.Quantity * 100) +(long) shippingPrice *100,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>() { "card"}
                };
                paymentIntent = await paymentIntentService.CreateAsync(options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)basket.Items.Sum(item => item.Price * item.Quantity * 100) + (long)shippingPrice * 100,

                };
                await paymentIntentService.UpdateAsync(basket.PaymentIntentId, options);
                
            }
            await _basketRepo.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<Order> UpdatePaymentIntentToSucceededOrFailed(string paymentIntentId, bool isSucceeded)
        {
            var spec = new OrderWithPaymentIntentSpecifications(paymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);

            if (isSucceeded)
                order.Status = OrderStatus.PaymentReceived;
            else
                order.Status = OrderStatus.PaymentFailed;
            
            _unitOfWork.Repository<Order>().Update(order);

            await _unitOfWork.CompleteAsync();

            return order;
        }
    }
}
