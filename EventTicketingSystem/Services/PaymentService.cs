public class PaymentService : IPaymentService
{
    public async Task<ServiceResult<bool>> ProcessPaymentAsync(decimal amount, object paymentDetails)
    {
        // Simulate payment processing logic
        await Task.Delay(1000); // Simulate network delay

        // Here you would integrate with a real payment gateway
        bool paymentSuccess = new Random().Next(0, 1) == 0; // Simulate random success or failure

        if (paymentSuccess)
        {
            return ServiceResult<bool>.Ok(true);
        }
        else
        {
            return ServiceResult<bool>.Fail(["Payment failed"]);
        }
    }
}