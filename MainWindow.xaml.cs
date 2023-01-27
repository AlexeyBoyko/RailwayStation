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

namespace RailwayStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, SolidColorBrush> colorBrushes = new Dictionary<string, SolidColorBrush>()
        {
            ["Зелёный"] = Brushes.LightGreen,            
            ["Синий"] = Brushes.LightBlue,
            ["Красный"] = Brushes.LightCoral
        };       

        private string selectedColor;
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
            InitializeComponent();

            // увеличиваем масштаб изначальной схемы (задаём множитель для координат)
            Point.scaleFactor = scaleFactor;
                        
            var park = new Park(scaleFactor);
            DrawParkLabelOutline(park, fillVariants.Keys.ElementAt(0));
            
            // клонируем парк со смещением
            var park2 = park.CreateCopyWithShift(x: 90, y: 90);
            DrawParkLabelOutline(park2, fillVariants.Keys.ElementAt(1));
        }
        // Рисование элементов парка, подпись и подготовка внешнего контура для последующей заливки
        private void DrawParkLabelOutline(Park park, string parkKey)
        {
            // вычисление и сохранение внешнего контура
            Polygon parkField = park.InitPolygon();
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
        private void Color_Selected(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (selectedItem == (comboBox.Items.GetItemAt(0) as ComboBoxItem).Content.ToString())
            {
                selectedColor = null;
            }
            else
            {
                selectedColor = selectedItem;
            }
            if (comboFillVariant != null)
            {
                FillVariant_Selected(comboFillVariant);
            }
        }

        private void FillVariant_Selected(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;                                    
            FillVariant_Selected(comboBox);
        }
        private void FillVariant_Selected(ComboBox comboBox)
        {            
            var selectedItem = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (selectedColor == null || selectedItem == (comboBox.Items.GetItemAt(0) as ComboBoxItem).Content.ToString())
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
                fillVariants[selectedItem].Fill = colorBrushes[selectedColor];
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
