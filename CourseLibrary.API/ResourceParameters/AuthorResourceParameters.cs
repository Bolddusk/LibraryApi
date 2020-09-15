using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.ResourceParameters
{
    public class AuthorResourceParameters
    {
        const int maxPageSize = 20;
        public string mainCategory { get; set; }
        public string searchQuery { get; set; }
        public int PageNumber { get; set; } = 1;
        public int _pageSize = 10;
        public int PageSize {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
