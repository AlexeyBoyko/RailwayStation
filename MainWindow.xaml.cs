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
        public ObservableCollection<ColorItem> ColorItems { set; get; }
        public ColorItem SelectedColorItem { set; get; }        
        private Dictionary<string, Polygon> fillVariants = new Dictionary<string, Polygon>()
        {
            ["Парк 1"] = null,            
            ["Парк 2"] = null
        };           

        // константа используемая при первичной инициализации изображения 
        static readonly double scaleFactor = 2.0;
        // динамически изменяемый масштаб
        static double dynamicScaleFactor = 1.0;        
        public MainWindow()
        {   
            prepareBindColorsCombo();
            this.DataContext = this;                     
            InitializeComponent();            
            

            // увеличиваем масштаб изначальной схемы (задаём множитель для координат)
            Point.scaleFactor = scaleFactor;
                        
            var park = new Park(scaleFactor);
            DrawParkLabelOutline(park, fillVariants.Keys.ElementAt(0));
            
            // клонируем первый парк со смещением вправо вниз на 90
            var park2 = park.CreateCopyWithShift(x: 90, y: 90);
            DrawParkLabelOutline(park2, fillVariants.Keys.ElementAt(1));
        }
        private void prepareBindColorsCombo()
        {
             ColorItems = new ObservableCollection<ColorItem>();
             ColorItems.Add(new ColorItem{ Name = "Цвет" });
             ColorItems.Add(new ColorItem{ Name = "Зелёный", brush = Brushes.LightGreen });
             ColorItems.Add(new ColorItem{ Name = "Синий", brush = Brushes.LightBlue });
             ColorItems.Add(new ColorItem{ Name = "Красный", brush = Brushes.LightCoral });
        }
        // Рисование элементов парка, подпись и подготовка внешнего контура для последующей заливки
        private void DrawParkLabelOutline(Park park, string parkKey)
        {
            // вычисление и сохранение внешнего контура
            Polygon parkField = park.Outline;
            fillVariants[parkKey] = parkField;            
            canvas1.Children.Add(parkField);
            
            // инициализация и размещение по центру фигуры текстовой подписи
            TextBlock textBlock = new TextBlock();
            textBlock.Text = parkKey;
            textBlock.FontSize = 20;                                   
            var center = park.GetRectCenter();
            Canvas.SetLeft(textBlock, center.x);
            Canvas.SetTop(textBlock, center.y);
            canvas1.Children.Add(textBlock);

            // рисование элементов парка
            List<UIElement> uIElements = park.Draw();
            foreach(var uIElement in uIElements)
            {
                canvas1.Children.Add(uIElement);
            }
        }
        private void ColorSelectedHandler(object sender, RoutedEventArgs e)
        {            
            // заглушка до первичной инициализации
            if (comboFillVariant!=null)
            {
                FillCleanSelectedVariant(comboFillVariant);
            }             
        }

        private void FillVariantSelectedHandler(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;                                    
            FillCleanSelectedVariant(comboBox);
        }
        // заливка выбранного варианта и очистка невыбранных
        private void FillCleanSelectedVariant(ComboBox comboBox)
        {
            // заглушка до первичной инициализации
            if (comboBox==null)
            {
                return;
            }                        
            var selectedItem = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (SelectedColorItem.Name == "Цвет" || selectedItem == (comboBox.Items.GetItemAt(0) as ComboBoxItem).Content.ToString())
            {                
                foreach(var fillVariant in fillVariants)
                {
                    // для установки выбранного значения по умолчанию до инициализации вариантов
                    if (fillVariant.Value != null) 
                    {
                        fillVariant.Value.Fill = null;
                    }
                }
            }
            else
            {
                fillVariants[selectedItem].Fill = SelectedColorItem.brush;
                string otherKey = fillVariants.Keys.Where(k => k!=selectedItem).Single();
                fillVariants[otherKey].Fill = null;            
            }
        }

        // зумирование рисунка колесиком мышки
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {            
            if (scaleFactor*dynamicScaleFactor<=0.5 && e.Delta < 0)
            {
                return; // принудительная блокировка уменьшения масштаба до того как фигура исчезнет
            }

            // адаптивный шаг коэффициента трансформации обратно пропорционален первоначальному масштабу
            // это нужно для более плавного изменения изначально больших объектов
            dynamicScaleFactor += ((e.Delta > 0)?  1.0 : -1.0)/(2*scaleFactor);
            
            canvas1.RenderTransform = new ScaleTransform()
            {
                ScaleX = dynamicScaleFactor,
                ScaleY = dynamicScaleFactor
            };
        }       
    
    
    }    
}
