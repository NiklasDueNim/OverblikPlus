using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using OverblikPlus.AuthHelpers;
using OverblikPlus.Models.Dtos.Budget;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class BudgetService : IBudgetService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authStateProvider;

    public BudgetService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
    }

    public async Task<List<BudgetDto>> GetAllBudgetsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<BudgetDto>>("api/budget");
            return response ?? new List<BudgetDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fejl ved hentning af budget: {ex.Message}");
            return new List<BudgetDto>();
        }
    }

    public async Task<BudgetDto?> GetBudgetByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BudgetDto>($"api/budget/{id}");
            return response;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fejl ved hentning af budget {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<BudgetDto?> CreateBudgetAsync(BudgetDto budget)
    {
        try
        {
            var user = _authStateProvider.User?.Id;
            if (user == null)
            {
                Console.Error.WriteLine("Bruger-ID er null.");
                return null;
            }

            budget.UserId = user;
            var response = await _httpClient.PostAsJsonAsync("api/budget", budget);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BudgetDto>();
                return result;
            }
            else
            {
                Console.Error.WriteLine($"Fejl ved oprettelse af budgetpost: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fejl ved oprettelse af budgetpost: {ex.Message}");
            return null;
        }
    }

    public async Task<BudgetDto?> UpdateBudgetAsync(Guid id, BudgetDto budget)
    {
        try
        {
            var updateDto = new
            {
                Date = budget.Date,
                Activity = budget.Activity,
                Voucher = budget.Voucher,
                MoneyIn = budget.MoneyIn,
                MoneyOut = budget.MoneyOut,
                Note = budget.Note
            };

            var response = await _httpClient.PutAsJsonAsync($"api/budget/{id}", updateDto);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BudgetDto>();
                return result;
            }
            else
            {
                Console.Error.WriteLine($"Fejl ved opdatering af budgetpost: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fejl ved opdatering af budgetpost: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteBudgetAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/budget/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fejl ved sletning af budgetpost: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> UploadVoucherAsync(Stream fileStream, string fileName)
    {
        try
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);

            var response = await _httpClient.PostAsync("api/budget/upload", content);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Upload response: {jsonString}");
                
                using var doc = System.Text.Json.JsonDocument.Parse(jsonString);
                if (doc.RootElement.TryGetProperty("url", out var urlProperty))
                {
                    var url = urlProperty.GetString();
                    Console.WriteLine($"Uploaded voucher URL: {url}");
                    return url;
                }
                else
                {
                    Console.Error.WriteLine("Response does not contain 'url' property");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"Fejl ved upload af bilag: {response.StatusCode}, {errorContent}");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fejl ved upload af bilag: {ex.Message}");
            Console.Error.WriteLine($"Exception details: {ex}");
            return null;
        }
    }
}
