using RailwayStation;
namespace RailwayStation.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var park = new Park(1, "Парк");
            List<int> parkOutline = park.FindOuterLoop();
            // если считать что размер парка - это количество точек составляющих парк
            int parkSize = park.ParkSize;
            // то внешний контур парка не может быть больше по размеру чем сам парк
            Assert.True(parkOutline.Count <= parkSize);
            // индексы вершины составляющие внешний контур должны быть в пределах размеров парка
            Assert.All(parkOutline, item => Assert.True(item < parkSize));
        }
    }
}