namespace VideoGameStore.Services.Payments
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(decimal amount, string returnUrl, string cancelUrl);
        Task<bool> CaptureOrderAsync(string orderId);
    }
}
