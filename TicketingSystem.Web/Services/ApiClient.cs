using System.Net.Http.Json;
using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ApiClient(HttpClient http, AuthService auth) { _http = http; _auth = auth; }

    private async Task SetAuthHeader()
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    // Auth
    public async Task<LoginResult> LoginAsync(string username, string password, string? tfaCode = null)
    {
        var body = new { UserName = username, Password = password, TfaCode = tfaCode };
        var response = await _http.PostAsJsonAsync("api/auth/login", body);
        if (!response.IsSuccessStatusCode) throw new UnauthorizedAccessException();
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result?.Token != null) await _auth.SetCurrentUserAsync(result.User);
        return result!;
    }

    // Tickets
    public async Task<TicketDto> CreateTicketAsync(CreateTicketRequest request)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/tickets", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TicketDto>())!;
    }

    public async Task<TicketDetailDto?> GetTicketAsync(int id)
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/tickets/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TicketDetailDto>();
    }

    public async Task<PaginatedList<TicketListDto>> GetTicketsAsync(FilterRequest filter)
    {
        await SetAuthHeader();
        var query = $"?search={filter.Search}&systemId={filter.SystemId}&statusId={filter.StatusId}&priorityId={filter.PriorityId}&page={filter.Page}&pageSize={filter.PageSize}";
        if (filter.FromDate.HasValue) query += $"&fromDate={filter.FromDate.Value:yyyy-MM-dd}";
        if (filter.ToDate.HasValue) query += $"&toDate={filter.ToDate.Value:yyyy-MM-dd}";
        var response = await _http.GetAsync($"api/tickets{query}");
        response.EnsureSuccessStatusCode();
        var items = (await response.Content.ReadFromJsonAsync<List<TicketListDto>>()) ?? new();
        return new PaginatedList<TicketListDto>(items, 1, items.Count);
    }

    public async Task UpdateStatusAsync(int ticketId, int newStatusId, string? comment)
    {
        await SetAuthHeader();
        await _http.PutAsJsonAsync($"api/tickets/{ticketId}/status", new StatusUpdateRequest(newStatusId, comment));
    }

    public async Task CloseTicketAsync(int ticketId, string resolutionSummary)
    {
        await SetAuthHeader();
        await _http.PostAsJsonAsync($"api/tickets/{ticketId}/close", new CloseRequest(resolutionSummary));
    }

    public async Task ApproveClosureAsync(int ticketId)
    {
        await SetAuthHeader();
        await _http.PostAsync($"api/tickets/{ticketId}/approve-closure", null);
    }

    public async Task ReopenTicketAsync(int ticketId, string reason)
    {
        await SetAuthHeader();
        await _http.PostAsJsonAsync($"api/tickets/{ticketId}/reopen", new ReopenRequest(reason));
    }

    // Comments
    public async Task AddCommentAsync(int ticketId, string content)
    {
        await SetAuthHeader();
        await _http.PostAsJsonAsync($"api/tickets/{ticketId}/comments", new AddCommentRequest(content, null, false));
    }

    public async Task UploadWithCommentAsync(int ticketId, string content, IFormFile file)
    {
        await SetAuthHeader();
        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(content), "content");
        formData.Add(new StreamContent(file.OpenReadStream()), "file", file.Name);
        await _http.PostAsync($"api/tickets/{ticketId}/attachments/with-comment", formData);
    }

    // Attachments
    public async Task UploadAttachmentAsync(int ticketId, IFormFile file)
    {
        await SetAuthHeader();
        using var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(file.OpenReadStream()), "file", file.Name);
        await _http.PostAsync($"api/tickets/{ticketId}/attachments", formData);
    }

    // Reports
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        await SetAuthHeader();
        var response = await _http.GetAsync("api/reports/dashboard");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DashboardStats>())!;
    }

    public async Task<List<StatusDistributionDto>> GetStatusDistributionAsync(string query = "")
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/reports/status-distribution{query}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<StatusDistributionDto>>()) ?? new();
    }

    public async Task<List<PriorityDistributionDto>> GetPriorityDistributionAsync(string query = "")
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/reports/priority-distribution{query}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<PriorityDistributionDto>>()) ?? new();
    }

    public async Task<List<SystemReportDto>> GetBySystemAsync(string query = "")
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/reports/by-system{query}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<SystemReportDto>>()) ?? new();
    }

    public async Task<List<DepartmentReportDto>> GetByDepartmentAsync(string query = "")
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/reports/by-department{query}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<DepartmentReportDto>>()) ?? new();
    }

    public async Task<List<SupportPerformanceDto>> GetSupportPerformanceAsync(string query = "")
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/reports/support-performance{query}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<SupportPerformanceDto>>()) ?? new();
    }

    public async Task<List<IssueTypeReportDto>> GetIssueTypesReportAsync(string query = "")
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/reports/issue-types{query}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<IssueTypeReportDto>>()) ?? new();
    }

    // Reference Data
    public async Task<List<SystemDto>> GetSystemsAsync()
    {
        await SetAuthHeader();
        var response = await _http.GetAsync("api/reference/systems");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<SystemDto>>()) ?? new();
    }

    public async Task<List<IssueTypeDto>> GetIssueTypesAsync()
    {
        await SetAuthHeader();
        var response = await _http.GetAsync("api/reference/issue-types");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<IssueTypeDto>>()) ?? new();
    }

    public async Task<List<PriorityDto>> GetPrioritiesAsync()
    {
        await SetAuthHeader();
        var response = await _http.GetAsync("api/reference/priorities");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<PriorityDto>>()) ?? new();
    }
}

// Helper types for client
public record LoginResult(UserDto User, bool RequiresMfa, string Token);
public record PaginatedList<T>(List<T> Items, int Page, int TotalCount);
