public interface IPaymentService
{
    Task<ServiceResult<bool>> ProcessPaymentAsync(decimal amount, object paymentDetails);
}   