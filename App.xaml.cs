using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace RailwayStation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {     
        // метод для получения произвольного простого пути в графе
        // используется для создания целостной обводки соединительных путей (при рисовании обводки отдельными линиями на поворотах возникают прорехи)
        public static List<int> FindSimplePath(int[,] edges, int currentV, bool startPoint, 
                                                HashSet<int> alreadyVisited, List<int> currentPath)
        {        
            alreadyVisited.Add(currentV);
            currentPath.Add(currentV);
            // в общем случае строится два пути в разные стороны, если начальная точка не является конечной (т.е. не "лист" графа)
            List<int> oneSidePath = new List<int>(), otherSidePath = new List<int>();
            
            for (int i = 0; i < edges.GetLength(1); i++)
            {
                if (i!=currentV && edges[currentV, i] == 1 && 
                // для начальной точки (первого уровня рекурсии) допустимо построение пути до уже посещённой вершины,
                // т.к. к одной точке могут примыкать несколько разных путей
                // однако на следующем уровне рекурсии обход остановится в этой точке
                (startPoint || !alreadyVisited.Contains(i))) 
                {
                    // для начальной точки пробуем два направления обхода
                    if (startPoint)
                    {
                        // сначала строим путь в одну сторону
                        if(oneSidePath.Count==0)
                        {
                            oneSidePath = FindSimplePath(edges, i, false, alreadyVisited, currentPath);
                        }
                        // если первый путь уже построен, но есть ещё смежные точки, тогда продолжаем строить в другую сторону
                        else 
                        {
                            currentPath = new List<int>();
                            otherSidePath = FindSimplePath(edges, i, false, alreadyVisited, currentPath);
                        }                    
                    }
                    // для последующих вершин направление обхода м.б. только одно, 
                    // поэтому продолжаем рекурсивный обход в глубину
                    else
                    {
                        return FindSimplePath(edges, i, false, alreadyVisited, currentPath);
                    }
                }
            }
            // возврат параметра из рекурсии
            if (startPoint == false) 
            {
                return currentPath;
            }
            // на первом уровне (для начальной точки) при необходимости соединяем найденные разнонаправленные пути            
            else
            {                
                if (otherSidePath.Count!=0)
                {
                    // поворот начального пути в обратном направлении
                    oneSidePath.Reverse();
                    // наращиваем развернутый начальный путь вершинами из дополнительного
                    foreach(var point in otherSidePath)
                    {
                        oneSidePath.Add(point);
                    }
                }
                return oneSidePath;
            }
        }   
        public static Point[] InitPoints()
        {
            return new Point[] { new(0, 10), new(10, 10), new(170, 10), new(180, 20), new(200, 20), new(210, 40), new(220, 40),
                new(230, 50), new(240, 70), new(220, 70), new(210, 80), new(200, 90), new(140, 90), new(130, 80), new(80, 80), new(70, 70),
                new(60, 60), new(50, 50), new(40, 40), new(30, 30), new(20, 20), new(170, 30), new(180, 40), new(170, 60), new(180, 70)
                ,new(250, 70), new(100, 100)
            };            
        }
        public static Line[] InitLines(Point[] points)
        {
            // инициализируем линии станционного парка
            return new Line[] {

#region перегоны между станциями (магистральные линии)
            new(points[0], points[1]),
            new(points[8], points[25]),
            new(points[14], points[26]),
#endregion

            // станционные линии:

#region путевые линии (сгруппированы по путям, группа из нескольких линий означает что путь состоит из поворотных сегментов)

            new(points[1], points[2]),
            new(points[2], points[3]),

            new(points[20], points[3]),

            new(points[19], points[21]),
            new(points[21], points[22]),

            new(points[18], points[22]),

            new(points[17], points[7]),

            new(points[16], points[23]),
            new(points[23], points[24]),

            new(points[15], points[24]),

            new(points[13], points[10]),

            new(points[10], points[11]),
            new(points[11], points[12]),
            new(points[12], points[13]),
#endregion

#region горловина станции (дерево соединительных линий, со стрелочными переводами)
            new(points[3], points[4], true),
            new(points[4], points[5], true),
            new(points[5], points[6], true),
            new(points[22], points[5], true),
            new(points[6], points[7], true),
            new(points[7], points[8], true),
            new(points[8], points[9], true),
            new(points[9], points[10], true),
            new(points[24], points[9], true),
#endregion

#region стрелочная улица
            new(points[14], points[15], true),
            new(points[15], points[16], true),
            new(points[16], points[17], true),
            new(points[17], points[18], true),
            new(points[18], points[19], true),
            new(points[19], points[20], true),
            new(points[20], points[1], true),
            #endregion
            
            // соединительная линия отдельная от стрелочной улицы
            new(points[13], points[14], true),
        };
        }
    }    
    public class ColorItem
    {
        //private string name;
        public string Name { set; get; }
        public SolidColorBrush brush;        
    }
    public class Park
    {
        private Polygon outline;
        public Polygon Outline
        {
            get { return outline; }
        }
        private double scaleFactor;        
        private readonly Point[] points;
        private readonly Line[] lines;        
        public Park(double scaleFactor) : this(App.InitPoints(), scaleFactor)
        {
        }
        private Park(Point[] points, double scaleFactor)
        {
            this.scaleFactor = scaleFactor;
            this.points = points;            
            lines = App.InitLines(points);                                    
            InitOutline();
        }
        
        public Park CreateCopyWithShift(int x, int y)
        {
            Point[] shiftedPoints = points.Select(p => new Point((int)(p.X/scaleFactor) + x, (int)(p.Y/scaleFactor) + y)).ToArray();            
            return new Park(shiftedPoints, scaleFactor);
        }
        // центр описывающего прямоугольника
        public (double x, double y) GetRectCenter()
        {
            var result = (x: -1.0, y: -1.0);
            double maxX = points.Select(p => p.X).Max();
            double minX = points.Select(p => p.X).Min();
            double maxY = points.Select(p => p.Y).Max();
            double minY = points.Select(p => p.Y).Min();
            result.x = (minX + maxX)/2;
            result.y = (minY + maxY)/2;
            return result;
        }
        public List<UIElement> Draw()
        {
            List<UIElement> uIElements = new List<UIElement>();
            // обозначаем точки-развилки красными кружками
            // специального выделения для простых (поворотных) точек не предусмотрено            
            var circles = points.Where(p => p.IsSwitch).Select(p => new Ellipse().GetCircle(p.X, p.Y, Brushes.Red, scaleFactor));
            foreach(var circle in circles)
            {
                uIElements.Add(circle);
            }
            
            // проводим все линии
            var winLines = lines.Select(l => 
                    new System.Windows.Shapes.Line().InitLine(l.A.X, l.A.Y, l.B.X, l.B.Y, Brushes.Black, scaleFactor));            
            foreach (var line in winLines)
            {                
                uIElements.Add(line);
            }            
            
            // Обводим серым цветом соединительные/ходовые линии
            int thickness = (int)(5 * scaleFactor);
            var passageWays = lines.Where(l => l.IsPassageWay); 
            var passPoints = passageWays.SelectMany(l=>l.points).Distinct();
            // вспомогательная матрица смежности
            var adjacencyMatrix = new int[points.Length, points.Length];
            // заполняем матрицу только для соединительных/ходовых линий
            foreach(Line line in passageWays)
            {
                int pointA = Array.IndexOf(points, line.A);
                int pointB = Array.IndexOf(points, line.B);
                adjacencyMatrix[pointA, pointB] = 1;
                adjacencyMatrix[pointB, pointA] = 1;
            }
            var alreadyVisited = new HashSet<int>();
            foreach (var passPoint in passPoints)
            {
                int pointIndex = Array.IndexOf(points, passPoint);
                if (!alreadyVisited.Contains(pointIndex))
                {
                    List<int> path = App.FindSimplePath(adjacencyMatrix, pointIndex, startPoint: true, alreadyVisited, new List<int>());
                
                    Polyline polyline = new Polyline();
                    PointCollection pointCollection = new PointCollection(path.Count); 
                    foreach(var pointInd in path)
                    {
                        var point = points[pointInd];
                        pointCollection.Add(new System.Windows.Point(point.X, point.Y));
                    }
                    polyline.Points = pointCollection;
                    polyline.Stroke = Brushes.Gray;
                    polyline.StrokeThickness = thickness;
                    polyline.Opacity = 0.5;
                    uIElements.Add(polyline);
                }
            }
            return uIElements;  
        }
        // обход по внешнему периметру графа по часовой стрелке посредством выбора всегда самого левого "соседа"
        private List<int> FindOuterLoop()
        {   
            List<int> outerLoop = new List<int>();
            // берём произвольный "лист" графа (точка на перегоне, не относящаяся к станции)        
            Point a = points.Where(n => n.Neighbors.Count == 1).First();
            // находим входную стрелку
            Point b = a.GetSingleNeighbor(); // точка b принадлежит к станции и является внешней, т.к. смежна с точкой на перегоне
            Point begin = b;                 // запоминаем начальную точку внешнего контура станции            
            do
            {
                outerLoop.Add(Array.IndexOf(points, b));
                Point p = b.GetLeftmostNeighbor(a);
                a = b;
                b = p;
            }
            while (b != begin); 
            return outerLoop;        
        }
        // Инициализация внешнего контура парка для последующей заливки выбранным цветом
        private void InitOutline() 
        {
            List<int> outerLoop = FindOuterLoop();
            outline = new Polygon();            
            var winPoints = outerLoop.Select(p => new System.Windows.Point(points[p].X, points[p].Y));
            var outerLoopPointCollection = new PointCollection(winPoints);                        
            outline.Points = outerLoopPointCollection;                        
        }
    }
    
    public class Point
    {
        public static double scaleFactor = 1;

        private int x, y;        
        public int X
        {
            get { return x; }
        }
        public int Y
        {
            get { return y; }
        }

        private bool isSwitch = false;
        /// <summary>
        /// является ли точка развилкой (входной стрелкой) между путями или группами путей
        /// по умолчанию считаем что это внутренняя точка пути (схематично изображающая поворот/изгиб путевой линии)
        /// </summary>
        public bool IsSwitch
        {
            get { return isSwitch; }
        }
        private List<Point> neighbors = new List<Point>();
        public List<Point> Neighbors
        {
            get { return neighbors; }
        }
        public void AddNeighbor(Point p)
        {
            neighbors.Add(p);
            if (neighbors.Count == 3)
            {
                isSwitch = true;
            }
        }
        /// <summary>
        /// Получение соседа для терминальных точек (у которых он только один)
        /// </summary>
        /// <returns></returns>
        public Point GetSingleNeighbor()
        {
            return neighbors.Single();
        }
        /// <summary>
        /// Возврат наиболее левого соседа текущей точки при движении по направлению из точки previousNeighbor
        /// </summary>
        /// <param name="previousNeigbor"></param>
        /// <returns></returns>
        public Point GetLeftmostNeighbor(Point previousNeigbor)
        {
            if (isSwitch)
            {
                var forkNeighbors = neighbors.Except(new List<Point> { previousNeigbor });
                // отсеиваем "листья" графа (магистральные линии), чтобы не выйти за пределы станции
                var stationPoints = forkNeighbors.Where(n => n.neighbors.Count > 1);
                // выбираем самый левый из соседних узлов
                return stationPoints.MinBy(p => GetPointLineSide(previousNeigbor, this, p));
            }
            // если не развилка, то следующий сосед по определению только один 
            else
            {
                return neighbors.Except(new List<Point> { previousNeigbor }).Single();
            }
        }
        public Point(int x, int y)
        {
            this.x = (int)(x * scaleFactor);
            this.y = (int)(y * scaleFactor);
        }
        /// <summary>
        /// если понадобится выводить в консоль для отладки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return x + ";" + y;
        }

        /// <summary>
        /// Согласно теореме о расположении точек относительно прямой 
        /// уравнение прямой с подставленными в него координатами некоторой точки даст: 
        /// - положительное значение в случае нахождения точки в одной полуплоскости 
        /// - отрицательное если точка лежит в другой полуплоскости
        /// - и 0 если эта точка принадлежит прямой
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <returns>если при прохождении от A к P в точке B поворачиваем налево, то значение будет отрицательное</returns>
        private static float GetPointLineSide(Point a, Point b, Point p)
        {
            // фактически это скалярное произведение вектора AP на нормальный вектор прямой, проходящей через точки BP 
            return (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);
        }
    }
    public class Line
    {
        private Point a, b;
        public Point A
        {
            get {return a;}
        }
        public Point B
        {
            get {return b;}
        }

        public Point[] points;
        
        private bool isPassageWay = false;
        /// <summary>
        /// Признак для выделения линий серой обводкой:
        /// является ли линия проходной (горловина станции) или это один из путей
        /// </summary>
        public bool IsPassageWay
        {
            get { return isPassageWay; }
        }
        public Line(Point a, Point b)
        {
            this.a = a;
            this.b = b;
            a.AddNeighbor(b);
            b.AddNeighbor(a);
            points = new Point[2] { a, b };
        }
        public Line(Point a, Point b, bool isPassageWay) : this(a, b)
        {
            this.isPassageWay = isPassageWay;
        }
    }
    public static class LineExtension
    {
        public static System.Windows.Shapes.Line InitLine(this System.Windows.Shapes.Line line,
            double X1, double Y1, double X2, double Y2, 
            SolidColorBrush brush, double scaleFactor = 1.0, double opacity = 1.0, double thickness = 0.5)
        {
            line.X1 = X1;
            line.Y1 = Y1;
            line.X2 = X2;
            line.Y2 = Y2;
            line.Stroke = brush;
            line.StrokeThickness = thickness * scaleFactor;
            line.Opacity = opacity;
            return line;
        }
    }

    public static class EllipseExtension
    {
        public static Ellipse GetCircle(this Ellipse ellipse, int X, int Y, SolidColorBrush brush, double scaleFactor = 1.0, int radius = 2)
        {
            int scaledRadius = (int)( radius * scaleFactor);
            ellipse.Width = 2 * scaledRadius;
            ellipse.Height = 2 * scaledRadius;
            ellipse.Fill = brush;
            // находим координаты левой верхней границы окружности (точка на 135 градусов)
            double left = X - scaledRadius, top = Y - scaledRadius;
            ellipse.Margin = new Thickness(left, top, 0, 0);
            return ellipse;
        }
    }
}
// метод для соединения двух смежных линий
// может быть использован для построения путей при наличии матрицы инцидентности графа
// для этого необходимо в каждой вершине хранить информацию о линиях через неё проходящих
/*public static List<Point> ConnectPairOfLines(Line line1, Line line2)
    {
        Point[] points1 = line1.points, points2 = line2.points;
        Point commonPoint = points1.Intersect(points2).Single();
            var connectedPoints = new List<Point>(3);
            connectedPoints.Add(points1[(Array.IndexOf(points1, commonPoint) + 1) % 2]);
            connectedPoints.Add(commonPoint);
            connectedPoints.Add(points2[(Array.IndexOf(points2, commonPoint) + 1) % 2]);
            return connectedPoints;
    }*/