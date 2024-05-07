using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services;

namespace Talabat.APIs.Controllers
{
    [Authorize]
    public class PaymentsController : ApiBaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private const string endpointSecret = "whsec_f36dedfbb8ce6494b93a97361ae106438f871af81497e9ea6c6db2a08344712a";

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }
        [ProducesResponseType(typeof(CustomerBasket), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("{basketId}")] //POST : /api/Payments
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            if (basket is null) return BadRequest(new ApiResponse(400, "An Error with Your Basket"));
            return Ok(basket);
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], endpointSecret);
            var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
            Order order;
            // Handle the event
            if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                order = await _paymentService.UpdatePaymentIntentToSucceededOrFailed(paymentIntent.Id, false);
                _logger.LogInformation("Payment is Failed", paymentIntent.Id);

            }
            else if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                order = await _paymentService.UpdatePaymentIntentToSucceededOrFailed(paymentIntent.Id, true);
                _logger.LogInformation("Payment is Succeeded", paymentIntent.Id);
            }
            // ... handle other event types
            else
            {
                Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
            }
            return Ok();
        }
    }
}
