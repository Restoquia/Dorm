using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Tenant
{
    public class PaymentsModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ISessionService _session;
        private readonly ILogger<PaymentsModel> _logger;

        public PaymentsModel(DormApiClient apiClient, ISessionService session, ILogger<PaymentsModel> logger)
        {
            _apiClient = apiClient;
            _session = session;
            _logger = logger;
        }

        public List<LeaseResponseDto> ActiveLeases { get; set; } = new();
        public LeaseBalanceDto? StatementOfAccount { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? LeaseId { get; set; }

        [BindProperty]
        public PaymentCreateDto PaymentInput { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = _session.GetCurrentUser();
            if (currentUser == null) return RedirectToPage("/Account/Login");

            try
            {
                var leases = await _apiClient.GetLeasesAsync(userId: currentUser.UserId);
                ActiveLeases = leases.Where(l => l.Status == "Active").ToList();

                if (!LeaseId.HasValue && ActiveLeases.Count > 0)
                {
                    LeaseId = ActiveLeases.First().LeaseId;
                }

                if (LeaseId.HasValue)
                {
                    StatementOfAccount = await _apiClient.GetLeaseStatementOfAccountAsync(LeaseId.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payments for user {UserId}", currentUser.UserId);
                StatusMessage = "Could not load payment data. API server may be offline.";
                StatusMessageType = "warning";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = _session.GetCurrentUser();
            if (currentUser == null) return RedirectToPage("/Account/Login");

            try
            {
                var receipt = await _apiClient.PayRentAsync(PaymentInput);
                StatusMessage = $"Payment of ₱{PaymentInput.AmountPaid:N2} via {PaymentInput.PaymentMethod} was recorded successfully! Receipt #{receipt?.PaymentId}";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Payment error: {ex.Message}";
                StatusMessageType = "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing error.");
                StatusMessage = "An unexpected error occurred while processing payment.";
                StatusMessageType = "error";
            }

            return RedirectToPage(new { leaseId = PaymentInput.LeaseId });
        }
    }
}
