public interface IVenueService
{
    Task<ServiceResult<IEnumerable<Venue>>> GetAllAsync();
    Task<ServiceResult<Venue?>> GetByIdAsync(Guid id);
    Task<ServiceResult<Venue>> CreateAsync(CreateVenueRequest request);
    Task<ServiceResult<bool>> UpdateAsync(Guid id, UpdateVenueRequest request);
    Task<ServiceResult<bool>> DeleteAsync(Guid id);
}