using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services;
using Talabat.Core.Specifications.Oder_Specification;

namespace Talabat.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        ///private readonly IGenericRepository<Product> _productRepo;
        ///private readonly IGenericRepository<DeliveryMethod> _deliveryRepo;
        ///private readonly IGenericRepository<Order> _ordersRepo;

        public OrderService(IBasketRepository basketRepo,IUnitOfWork unitOfWork,IPaymentService paymentService)
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            ///_productRepo = productRepo;
            ///_deliveryRepo = deliveryRepo;
            ///_ordersRepo = ordersRepo;
        }
        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            // 1. Get Basket From Baskets Repo
            var basket = await _basketRepo.GetBasketAsync(basketId);
            // 2. Get Selected Items at Basket From Products Repo
            var orderItems = new List<OrderItem>();
            if(basket?.Items?.Count > 0)
            {
                foreach(var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    var ProductItemOrder = new ProductItemOrdered(item.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(ProductItemOrder, item.Price, item.Quantity);
                    orderItems.Add(orderItem);
                }
            }
            // 3. Calculate SubTotal
            var subtotal = orderItems.Sum(OI => OI.Price * OI.Quantity);
            // 4. Get Delivery Method From DeliveryMethods Repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(deliveryMethodId);
            var orderRepo = _unitOfWork.Repository<Order>();
            var orderSpecs = new OrderWithPaymentIntentSpecifications(basket.PaymentIntentId);
            var existingOrder = await orderRepo.GetWithSpecAsync(orderSpecs);
            if(existingOrder != null)
            {
                orderRepo.Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }
            // 5. Create Order
            var order = new Order(buyerEmail, shippingAddress, deliveryMethod, orderItems, subtotal,basket.PaymentIntentId);
            await _unitOfWork.Repository<Order>().AddAsync(order);
            // 6. Save To Database [TODO]
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0) return null;
            return order;
        }

        public Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            var deliveryMethodsRepo = _unitOfWork.Repository<DeliveryMethod>();
            var deliveryMethods = deliveryMethodsRepo.GetAllAsync();
            return deliveryMethods;
        }

        public Task<Order?> GetOrderByIdForUserAsync(int orderId, string buyerEmail)
        {
            var orderRepo = _unitOfWork.Repository<Order>();
            var spec = new OrderSpecifications(orderId ,buyerEmail);
            var order = orderRepo.GetWithSpecAsync(spec);
            return order;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var orderRepo = _unitOfWork.Repository<Order>();
            var spec = new OrderSpecifications(buyerEmail);
            var orders = await orderRepo.GetAllWithSpecAsync(spec);
            return orders;
        }
    }
}
