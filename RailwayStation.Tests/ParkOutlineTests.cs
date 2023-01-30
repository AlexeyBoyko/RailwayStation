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
        public void OutlinePointsIndexes()
        {
            var park = new Park(1, "Парк");
            List<int> parkOutline = park.FindOuterLoop();
            // если считать что размер парка - это количество точек составляющих парк
            int parkSize = park.ParkSize;
            // индексы вершины составляющие внешний контур должны быть в пределах размеров парка
            Assert.All(parkOutline, item => Assert.True(item < parkSize));
        }
        [StaFact]
        public void OutlineSequence()
        {
            var park = new Park(1, "Парк");
            List<int> parkOutline = park.FindOuterLoop();
            List<int> expectedOutline = new List<int>();
            // для текущей схемы контур должен содержать последовательно вершины от 1 до 20
            for(int i=1; i<=20; i++)
            {
                expectedOutline.Add(i);
            }
            Assert.Equal(expectedOutline, parkOutline);
        }
    }
}
