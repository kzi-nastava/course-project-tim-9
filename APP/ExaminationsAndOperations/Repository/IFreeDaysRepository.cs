public interface IFreeDaysRepository
{
    public Task DeleteStaleFreeDaysRequests();
    public Task DeleteFreeDaysRequest(string id);
    public Task<List<FreeDayRequest>> GetAllFreeDaysRequests();
    public Task<List<FreeDayRequest>> GetAllDoctorsFreeDaysRequests(int doctorId);
    public Task RequestFreeDays(FreeDayRequest freeDayRequest);
}