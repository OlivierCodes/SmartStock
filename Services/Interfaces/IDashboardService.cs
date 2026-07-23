using System.Threading.Tasks;
using SmartStock.Models.DTOs;

namespace SmartStock.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
