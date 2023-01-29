using RailwayStation;
namespace RailwayStation.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var park = new Park(1, "����");
            List<int> parkOutline = park.FindOuterLoop();
            // ���� ������� ��� ������ ����� - ��� ���������� ����� ������������ ����
            int parkSize = park.ParkSize;
            // �� ������� ������ ����� �� ����� ���� ������ �� ������� ��� ��� ����
            Assert.True(parkOutline.Count <= parkSize);
            // ������� ������� ������������ ������� ������ ������ ���� � �������� �������� �����
            Assert.All(parkOutline, item => Assert.True(item < parkSize));
        }
    }
}