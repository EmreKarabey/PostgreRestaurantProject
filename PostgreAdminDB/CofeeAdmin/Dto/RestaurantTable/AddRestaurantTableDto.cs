namespace CofeeAdmin.Dto.RestaurantTable
{
    public class AddRestaurantTableDto
    {
        public int TableNumber { get; set; }
        public double PositionX { get; set; } = 0.25;
        public double PositionY { get; set; } = 0.40;
        public double Rotation { get; set; }
        public string Shape { get; set; } = "round";
        public int Capacity { get; set; }
    }
}
