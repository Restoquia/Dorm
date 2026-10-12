using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Admin
{
    public class FinancialsModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ILogger<FinancialsModel> _logger;

        public FinancialsModel(DormApiClient apiClient, ILogger<FinancialsModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public PaymentSummaryDto? Summary { get; set; }
        public List<PaymentResponseDto> Payments { get; set; } = new();
        public List<LeaseResponseDto> ActiveLeases { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? PaymentMethodFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty]
        public PaymentCreateDto NewPayment { get; set; } = new()
        {
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Cash",
            Status = "Completed"
        };

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Summary = await _apiClient.GetPaymentSummaryAsync();
                Payments = await _apiClient.GetPaymentsAsync(paymentMethod: PaymentMethodFilter, status: StatusFilter);
                ActiveLeases = await _apiClient.GetActiveLeasesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load financial records.");
            }
        }

        public async Task<IActionResult> OnPostRecordPaymentAsync()
        {
            if (NewPayment.LeaseId <= 0 || NewPayment.AmountPaid <= 0)
            {
                StatusMessage = "Please select a valid lease and specify a payment amount greater than zero.";
                StatusMessageType = "error";
                return RedirectToPage();
            }

            try
            {
                await _apiClient.PayRentAsync(NewPayment);
                StatusMessage = $"Payment of ₱{NewPayment.AmountPaid:N2} recorded successfully!";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Failed to record payment: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }
    }
}
