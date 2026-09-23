namespace WorkForceManagerAPI.Models.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];

    public static ApiResponse<T> Ok(T data, string message = "OK") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
}

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

    public static PagedResponse<T> Create(IEnumerable<T> data, int total, int page, int pageSize) =>
        new() { Success = true, Message = "OK", Data = data, TotalRecords = total, Page = page, PageSize = pageSize };
}
