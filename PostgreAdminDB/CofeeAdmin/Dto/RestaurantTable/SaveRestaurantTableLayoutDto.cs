namespace CofeeAdmin.Dto.RestaurantTable
{
    public class SaveRestaurantTableLayoutDto
    {
        public int TableNumber { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Rotation { get; set; }
        public string Shape { get; set; } = "round";
        public int Capacity { get; set; }
    }
}
