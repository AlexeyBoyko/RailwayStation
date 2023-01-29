using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace RailwayStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// белый цвет заливки по умолчанию позволяет зумировать выбранную область (парк) на произвольной точке внутри
        /// также это упрощает логику обработки выбранного цвета за счёт отсутствия "бесцветного" варианта. 
        /// </summary>
        private SolidColorBrush defaultColor = Brushes.White;
        public ObservableCollection<ColorItem> ColorItems { set; get; }
        private ColorItem selectedColorItem;
        public ColorItem SelectedColorItem
        {
            set
            {
                if (value != selectedColorItem)
                {
                    selectedColorItem = value;
                    // InitializeComponent инициализирует comboFillVariant позже чем comboColor
                    if (comboFillVariant != null)
                    {
                        FillCleanSelectedVariant();
                    }
                }
            }
        }
        public ObservableCollection<FillVariant> FillVariants { set; get; }
        private FillVariant selectedFillVariant;
        public FillVariant SelectedFillVariant
        {
            set
            {
                if (value != selectedFillVariant)
                {
                    selectedFillVariant = value;
                    FillCleanSelectedVariant();
                }
            }
        }
        /// <summary>
        /// константа используемая при первичной инициализации изображения 
        /// </summary>
        static readonly double scaleFactor = 2.0;
        /// <summary>
        /// динамически изменяемый колесиком мышки масштаб 
        /// </summary>
        static double dynamicScaleFactor = 1.0;
        public MainWindow()
        {
            // увеличиваем масштаб изначальной схемы (задаём множитель для координат)
            Point.scaleFactor = scaleFactor;
            // инициализируем структуру парков 
            var park1 = new Park(scaleFactor, "Парк 1");
            // клонируем первый парк со смещением вправо вниз на 90
            var park2 = park1.CreateCopyWithShift("Парк 2", x: 90, y: 90);
            // заполняем комбобокс вариантами заливки со ссылками на соответствующие контуры
            prepareBindFillVariantsCombo(park1, park2);

            // комбобокс для выбора цвета
            prepareBindColorsCombo();
            // устанавливаем класс содержащий контекст данных для заполнения комбобоксов
            this.DataContext = this;
            // создаётся окно приложения                     
            InitializeComponent();
            // рисуем парки и подписи к ним, а также контуры для последующей заливки
            DrawParkLabelOutline(park1);
            DrawParkLabelOutline(park2);
        }
        /// <summary>
        /// Заполнение модели данных используемых для соотв.комбобокса
        /// </summary>
        private void prepareBindColorsCombo()
        {
            ColorItems = new ObservableCollection<ColorItem>();
            ColorItems.Add(new ColorItem { Name = "Цвет", brush = defaultColor });
            ColorItems.Add(new ColorItem { Name = "Зелёный", brush = Brushes.LightGreen });
            ColorItems.Add(new ColorItem { Name = "Синий", brush = Brushes.LightBlue });
            ColorItems.Add(new ColorItem { Name = "Красный", brush = Brushes.LightCoral });
        }
        /// <summary>
        /// Заполнение модели данных используемых для соотв.комбобокса
        /// </summary>
        /// <param name="parks"></param>
        private void prepareBindFillVariantsCombo(params Park[] parks)
        {
            FillVariants = new ObservableCollection<FillVariant>();
            FillVariants.Add(new FillVariant { Name = "Вариант заливки" });
            foreach (Park park in parks)
            {
                FillVariants.Add(new FillVariant { Name = park.name, ParkField = park.Outline });
            }
        }
        /// <summary>
        /// Рисование элементов парка, подпись и размещение внешнего контура для последующей заливки
        /// </summary>
        /// <param name="park"></param>
        private void DrawParkLabelOutline(Park park)
        {
            // помещение внешнего контура на холст (невидимо, без прорисовки)
            Polygon parkField = park.Outline;
            canvas1.Children.Add(parkField);

            // инициализация и размещение по центру фигуры текстовой подписи
            TextBlock textBlock = new TextBlock();
            textBlock.Text = park.name;
            textBlock.FontSize = 20;
            var center = park.GetRectCenter();
            Canvas.SetLeft(textBlock, center.x);
            Canvas.SetTop(textBlock, center.y);
            canvas1.Children.Add(textBlock);

            // рисование элементов парка
            List<UIElement> uIElements = park.Draw();
            foreach (var uIElement in uIElements)
            {
                canvas1.Children.Add(uIElement);
            }
        }
        /// <summary>
        /// заливка выбранного варианта и "очистка" остальных 
        /// на деле отсутствие заливки есть не что иное как заливка белым цветом, который выбран по умолчанию
        /// </summary>        
        private void FillCleanSelectedVariant()
        {
            // устанавливаем цвет "выбранного варианта" (т.е. делаем фиктивный вызов если он не выбран)                                              
            selectedFillVariant.CheckSetFilling(selectedColorItem.brush);
            // очищаем (вернее, заливаем цветом по умолчанию) все остальные варианты
            var otherVariants = FillVariants.Where(fv => fv != selectedFillVariant);
            foreach (var fillVariant in otherVariants)
            {
                fillVariant.CheckSetFilling(defaultColor);
            }
        }
        /// <summary>
        /// зумирование рисунка колесиком мышки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (scaleFactor * dynamicScaleFactor <= 0.5 && e.Delta < 0)
            {
                return; // принудительная блокировка уменьшения масштаба до того как фигура исчезнет
            }
            // адаптивный шаг коэффициента трансформации обратно пропорционален первоначальному масштабу
            // это нужно для более плавного изменения изначально больших объектов
            dynamicScaleFactor += ((e.Delta > 0) ? 1.0 : -1.0) / (2 * scaleFactor);

            canvas1.RenderTransform = new ScaleTransform()
            {
                ScaleX = dynamicScaleFactor,
                ScaleY = dynamicScaleFactor
            };
        }
    }
}
