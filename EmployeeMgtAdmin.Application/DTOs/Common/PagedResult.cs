namespace EmployeeMgtAdmin.Application.DTOs.Common;

public class PagedResult
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);  
}