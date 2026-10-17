using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.RestaurantTable.Queries.GetActiveTableList
{
    public class GetActiveTableListDto
    {
        public int TableNumber { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Rotation { get; set; }
        public string Shape { get; set; } 
        public int Capacity { get; set; }
    }
}
