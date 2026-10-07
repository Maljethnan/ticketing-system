using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://localhost:5001/api";

    public ApiClient(HttpClient http) => _http = http;

    public async Task<LoginResult> LoginAsync(string username, string password, string? tfaCode = null)
    {
        var payload = new { Username = username, Password = password, TfaCode = tfaCode };
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/auth/login", payload);
        return await response.Content.ReadFromJsonAsync<LoginResult>() ?? new LoginResult();
    }

    public async Task<List<TicketDto>> GetTicketsAsync(FilterRequest filter)
    {
        var query = $"?page={filter.Page}&pageSize={filter.PageSize}";
        if (!string.IsNullOrEmpty(filter.Search)) query += $"&search={filter.Search}";
        if (filter.StatusId.HasValue) query += $"&statusId={filter.StatusId.Value}";
        if (filter.PriorityId.HasValue) query += $"&priorityId={filter.PriorityId.Value}";
        if (filter.SystemId.HasValue) query += $"&systemId={filter.SystemId.Value}";
        if (filter.FromDate.HasValue) query += $"&fromDate={filter.FromDate.Value:yyyy-MM-dd}";
        if (filter.ToDate.HasValue) query += $"&toDate={filter.ToDate.Value:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<List<TicketDto>>($"{BaseUrl}/tickets{query}") ?? new List<TicketDto>();
    }

    public async Task<TicketDto?> GetTicketAsync(int id)
    {
        return await _http.GetFromJsonAsync<TicketDto>($"{BaseUrl}/tickets/{id}");
    }

    public async Task<int> CreateTicketAsync(CreateTicketDto dto)
    {
        var result = await _http.PostAsJsonAsync($"{BaseUrl}/tickets", dto);
        return result.IsSuccessStatusCode ? 1 : 0;
    }

    public async Task UpdateStatusAsync(int ticketId, int statusId, string? note = null)
    {
        var req = new StatusUpdateRequest(statusId, note);
        await _http.PutAsJsonAsync($"{BaseUrl}/tickets/{ticketId}/status", req);
    }

    public async Task ApproveClosureAsync(int ticketId)
    {
        var req = new CloseRequest();
        await _http.PostAsJsonAsync($"{BaseUrl}/tickets/{ticketId}/close", req);
    }

    public async Task ReopenTicketAsync(int ticketId, string reason)
    {
        var req = new ReopenRequest(reason);
        await _http.PostAsJsonAsync($"{BaseUrl}/tickets/{ticketId}/reopen", req);
    }

    public async Task<List<CommentDto>> GetCommentsAsync(int ticketId)
    {
        return await _http.GetFromJsonAsync<List<CommentDto>>($"{BaseUrl}/comments/ticket/{ticketId}") ?? new List<CommentDto>();
    }

    public async Task AddCommentAsync(int ticketId, string text)
    {
        await _http.PostAsJsonAsync($"{BaseUrl}/comments", new { TicketId = ticketId, Text = text });
    }

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
        return await _http.GetFromJsonAsync<DashboardStats>($"{BaseUrl}/reports/dashboard");
    }

    public async Task<List<StatusDistributionDto>> GetStatusDistributionAsync(string query)
    {
        return await _http.GetFromJsonAsync<List<StatusDistributionDto>>($"{BaseUrl}/reports/status-distribution{query}") ?? new List<StatusDistributionDto>();
    }

    public async Task<List<PriorityDistributionDto>> GetPriorityDistributionAsync(string query)
    {
        return await _http.GetFromJsonAsync<List<PriorityDistributionDto>>($"{BaseUrl}/reports/priority-distribution{query}") ?? new List<PriorityDistributionDto>();
    }

    public async Task<List<SystemDto>> GetSystemsAsync()
    {
        return await _http.GetFromJsonAsync<List<SystemDto>>($"{BaseUrl}/systems") ?? new List<SystemDto>();
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<CategoryDto>>($"{BaseUrl}/categories") ?? new List<CategoryDto>();
    }

    public async Task<List<IssueTypeDto>> GetIssueTypesAsync()
    {
        return await _http.GetFromJsonAsync<List<IssueTypeDto>>($"{BaseUrl}/issue-types") ?? new List<IssueTypeDto>();
    }

    public async Task<List<PriorityDto>> GetPrioritiesAsync()
    {
        return await _http.GetFromJsonAsync<List<PriorityDto>>($"{BaseUrl}/priorities") ?? new List<PriorityDto>();
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public bool RequiresMfa { get; set; }
    public UserDto? User { get; set; }
}
