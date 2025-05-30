namespace SimpleStore.Models
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public PaginationInfo PaginationInfo { get; set; }
        public string SearchQuery { get; set; }
        public string Category { get; set; }
        public string SortOrder { get; set; }
    }

    public class PaginationInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }

    public class ProductSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public string SortOrder { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}