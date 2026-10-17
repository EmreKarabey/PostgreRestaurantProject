using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Category.Queries.GetActiveList
{
    public class GetActiveCategoryListDto
    {
        public string CategoryName { get; set; }
        public string ImageURL { get; set; }
        public bool Status { get; set; }
    }
}
