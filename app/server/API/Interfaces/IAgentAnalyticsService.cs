using API.DTOs.Analytics;

namespace API.Interfaces;

public interface IAgentAnalyticsService {
    Task<AppAnalyticsResponse> GetAppAnalyticsAsync(int userId);
}