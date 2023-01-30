using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace RailwayStation.Tests
{
    public class ParkOutlineTests
    {        
        [StaFact]
        public void OutlineSize()
        {
            var park = new Park(1, "Парк");
            List<int> parkOutline = park.FindOuterLoop();
            // если считать что размер парка - это количество точек составляющих парк
            int parkSize = park.ParkSize;
            // то внешний контур парка не может быть больше по размеру чем сам парк
            Assert.True(parkOutline.Count <= parkSize);
        }
        [StaFact]
        public void OutlinePoints()
        {
            var park = new Park(1, "Парк");
            List<int> parkOutline = park.FindOuterLoop();
            // если считать что размер парка - это количество точек составляющих парк
            int parkSize = park.ParkSize;
            // индексы вершины составляющие внешний контур должны быть в пределах размеров парка
            Assert.All(parkOutline, item => Assert.True(item < parkSize));
        }
    }
}
