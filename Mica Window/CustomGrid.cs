using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace CustomGrid
{
    public enum GridType : uint {
        Auto,
        Pixel,
        Star
    }
    public class CustomGrid : Panel
    {
        public CustomGrid()
        {
            Background = Brushes.Transparent;
        }
        public IEnumerable<UIElement> ChildrenAsElements => Enumerable.Cast<UIElement>(Children).Where(Element => Element.Visibility != Visibility.Collapsed);
        public static readonly DependencyProperty GridTypeProperty = DependencyProperty.Register(
            "GridType",
            typeof(GridType),
            typeof(CustomGrid),
            new PropertyMetadata(
                GridType.Star,
                new PropertyChangedCallback((obj, evargs) =>
                {
                    var Parent = (obj as FrameworkElement)?.Parent as CustomGrid;
                    if (Parent == null) return;
                }
            )
            )
        );
        public static void SetGridType(UIElement element, GridType value) => element.SetValue(GridTypeProperty, value);
        public static GridType GetGridType(UIElement element) => (GridType)element.GetValue(GridTypeProperty);
        public static readonly DependencyProperty GridValueProperty = DependencyProperty.Register(
            "GridValue",
            typeof(double),
            typeof(CustomGrid),
            new PropertyMetadata(
                1.0,
                new PropertyChangedCallback((obj, evargs) =>
                {
                    var Parent = (obj as FrameworkElement)?.Parent as CustomGrid;
                    if (Parent == null) return;
                }
            )
            )
        );
        public static void SetGridValue(UIElement element, double value) => element.SetValue(GridValueProperty, value);
        public static double GetGridValue(UIElement element) => (double)element.GetValue(GridValueProperty);
    }
    public class ElementOverlay : CustomGrid
    {
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var Element in ChildrenAsElements)
            {
                Element.Measure(finalSize);
                Element.Arrange(new Rect(0, 0, Element.DesiredSize.Width, Element.DesiredSize.Height));
            }

            return finalSize;
        }
    }
    public class ColumnGrid : CustomGrid
    {

        public static readonly DependencyProperty FillHeightProperty = DependencyProperty.Register(
            "FillHeight",
            typeof(bool),
            typeof(CustomGrid),
            new PropertyMetadata(
                true,
                new PropertyChangedCallback((obj, evargs) =>
                {
                    var Parent = (obj as FrameworkElement)?.Parent as CustomGrid;
                    if (Parent == null) return;
                }
            )
            )
        );
        public static void SetFillHeight(UIElement element, bool value) => element.SetValue(FillHeightProperty, value);
        public static bool GetFillHeight(UIElement element) => (bool)element.GetValue(FillHeightProperty);

        protected override Size MeasureOverride(Size availableSize)
        {
            IEnumerable<(UIElement Element, GridType GridType, double GridValue)> definition = ChildrenAsElements.Select(Element => (Element, GetGridType(Element), GetGridValue(Element)));
            
            double TotalPixel = definition.Where(x => x.GridType == GridType.Pixel).Select(x =>
            {
                x.Element.Measure(new Size(x.GridValue, availableSize.Height));
                return x.Element.DesiredSize.Width;
            }).Sum();

            double TotalAuto = 0;
            foreach (var x in definition.Where(x => x.GridType == GridType.Auto))
            {
                x.Element.Measure(new Size(availableSize.Width - TotalAuto - TotalPixel, availableSize.Height));
                TotalAuto += x.Element.DesiredSize.Width;
            }

            double TotalStar = definition.Sum(x => x.GridType == GridType.Star ? x.GridValue : 0);

            double Star = TotalStar == 0 ? 0 : (availableSize.Width - TotalPixel - TotalAuto) / TotalStar;

            if (Star < 0) Star = 0;

            foreach (var x in definition.Where(x => x.GridType == GridType.Star))
            {
                x.Element.Measure(new Size(x.GridValue * Star, availableSize.Height));
            }

            

            var Height = GetFillHeight(this) ? availableSize.Height : Math.Min(Children.Count == 0 ? 0 : ChildrenAsElements.Max(x => x.DesiredSize.Height), availableSize.Height);

            return new Size(Star > 0 ? availableSize.Width : TotalPixel + TotalAuto, Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            IEnumerable<(UIElement Element, GridType GridType, double GridValue)> definition = ChildrenAsElements.Select(Element => (Element, GetGridType(Element), GetGridValue(Element)));

            double TotalPixel = definition.Where(x => x.GridType == GridType.Pixel).Select(x =>
            {
                x.Element.Measure(new Size(x.GridValue, finalSize.Height));
                return x.Element.DesiredSize.Width;
            }).Sum();

            double TotalAuto = 0;
            foreach (var x in definition.Where(x => x.GridType == GridType.Auto))
            {
                x.Element.Measure(new Size(finalSize.Width - TotalAuto - TotalPixel, finalSize.Height));
                TotalAuto += x.Element.DesiredSize.Width;
            }

            double TotalStar = definition.Sum(x => x.GridType == GridType.Star ? x.GridValue : 0);
            
            double Star = TotalStar == 0 ? 0 : (finalSize.Width - TotalPixel - TotalAuto) / TotalStar;

            if (Star < 0) Star = 0;

            foreach (var x in definition.Where(x => x.GridType == GridType.Star))
            {
                x.Element.Measure(new Size(x.GridValue * Star, finalSize.Height));
            }

            IEnumerable<(UIElement Element, double Width)> ColumnWidth = definition.Select(x =>
                (
                    x.Element,
                    x.GridType switch
                    {
                        GridType.Auto => x.Element.DesiredSize.Width,
                        GridType.Pixel => x.GridValue,
                        GridType.Star => Star * x.GridValue,
                        _ => 0,
                    }
                )
            );

            double X = 0;
            foreach (var (Element, Width) in ColumnWidth)
            {
                Element.Arrange(new Rect(X, 0, Width, finalSize.Height));
                X += Width;
            }

            return finalSize;
        }
        public ColumnGrid AddChild(GridType GridType, double GridValue, UIElement Element)
        {
            SetGridType(Element, GridType);
            SetGridValue(Element, GridValue);
            Children.Add(Element);
            return this;
        }
        public ColumnGrid AddChild(GridType GridType, UIElement Element)
            => AddChild(GridType: GridType, GridValue: 1, Element: Element);
        public ColumnGrid AddChild(UIElement Element)
            => AddChild(GridType: GridType.Star, Element: Element);
    }
    public class RowGrid : CustomGrid
    {
        public static readonly DependencyProperty FillWidthProperty = DependencyProperty.Register(
            "FillWidth",
            typeof(bool),
            typeof(CustomGrid),
            new PropertyMetadata(
                true,
                new PropertyChangedCallback((obj, evargs) =>
                {
                    var Parent = (obj as FrameworkElement)?.Parent as CustomGrid;
                    if (Parent == null) return;
                }
            )
            )
        );
        public static void SetFillWidth(UIElement element, bool value) => element.SetValue(FillWidthProperty, value);
        public static bool GetFillWidth(UIElement element) => (bool)element.GetValue(FillWidthProperty);

        protected override Size MeasureOverride(Size availableSize)
        {
            IEnumerable<(UIElement Element, GridType GridType, double GridValue)> definition = ChildrenAsElements.Select(Element => (Element, GetGridType(Element), GetGridValue(Element)));

            double TotalPixel = definition.Where(x => x.GridType == GridType.Pixel).Select(x =>
            {
                x.Element.Measure(new Size(availableSize.Width, x.GridValue));
                return x.Element.DesiredSize.Height;
            }).Sum();

            double TotalAuto = 0;
            foreach (var x in definition.Where(x => x.GridType == GridType.Auto))
            {
                x.Element.Measure(new Size(availableSize.Width, availableSize.Height - TotalAuto - TotalPixel));
                TotalAuto += x.Element.DesiredSize.Height;
            }

            double TotalStar = definition.Sum(x => x.GridType == GridType.Star ? x.GridValue : 0);

            double Star = TotalStar == 0 ? 0 : (availableSize.Height - TotalPixel - TotalAuto) / TotalStar;

            if (Star < 0) Star = 0;

            foreach (var x in definition.Where(x => x.GridType == GridType.Star))
            {
                x.Element.Measure(new Size(availableSize.Width, x.GridValue * Star));
            }


            var Width = GetFillWidth(this) ? availableSize.Width : Math.Min(Children.Count == 0 ? 0 : ChildrenAsElements.Max(x => x.DesiredSize.Width), availableSize.Width);

            return new Size(Width, Star > 0 ? availableSize.Height : TotalPixel + TotalAuto);
        }

        protected override Size ArrangeOverride(Size finalSize)
            => Arranging(ChildrenAsElements, finalSize);

        protected virtual Size Arranging(IEnumerable<UIElement> Children, Size finalSize)
        {
            IEnumerable<(UIElement Element, GridType GridType, double GridValue)> definition = Children.Select(Element => (Element, GetGridType(Element), GetGridValue(Element)));

            double TotalPixel = definition.Where(x => x.GridType == GridType.Pixel).Select(x =>
            {
                x.Element.Measure(new Size(finalSize.Width, x.GridValue));
                return x.Element.DesiredSize.Height;
            }).Sum();

            double TotalAuto = 0;
            foreach (var x in definition.Where(x => x.GridType == GridType.Auto))
            {
                x.Element.Measure(new Size(finalSize.Width, finalSize.Height - TotalAuto - TotalPixel));
                TotalAuto += x.Element.DesiredSize.Height;
            }

            double TotalStar = definition.Sum(x => x.GridType == GridType.Star ? x.GridValue : 0);

            double Star = TotalStar == 0 ? 0 : (finalSize.Height - TotalPixel - TotalAuto) / TotalStar;

            if (Star < 0) Star = 0;

            foreach (var x in definition.Where(x => x.GridType == GridType.Star))
            {
                x.Element.Measure(new Size(finalSize.Width, x.GridValue * Star));
            }
            
            if (Star < 0) Star = 0;
            
            IEnumerable<(UIElement Element, double Width)> RowHeight = definition.Select(x =>
                (
                    x.Element,
                    x.GridType switch
                    {
                        GridType.Auto => x.Element.DesiredSize.Height,
                        GridType.Pixel => x.GridValue,
                        GridType.Star => Star * x.GridValue,
                        _ => 0,
                    }
                )
            );

            double Y = 0;
            foreach (var (Element, Height) in RowHeight)
            {
                Element.Arrange(new Rect(0, Y, finalSize.Width, Height));
                Y += Height;
            }

            return finalSize;
        }

        //public RowGrid AddChild(RowDefinition RowDefinition, UIElement Element)
        //{
        //    SetRowDefinition(Element, RowDefinition);
        //    Children.Add(Element);
        //    return this;
        //}
        //public RowGrid AddChild(GridLength GridLength, UIElement Element)
        //    => AddChild(new RowDefinition { Height = GridLength }, Element);
    }
    public class ReverseRowGrid : RowGrid
    {
        protected override Size Arranging(IEnumerable<UIElement> Children, Size finalSize)
            => base.Arranging(Children.Reverse(), finalSize);
    }
    public class ReverseStackPanel : Panel
    {
        public new ICollection<UIElement> Children => (ICollection<UIElement>)base.Children;
        public ReverseStackPanel()
        {

        }
        public ReverseStackPanel AddChild(UIElement Child)
        {
            Children.Add(Child);
            return this;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var Element in Children) Element.Measure(availableSize);

            double Height, Width;

            Height = Children.Sum(Element => Element.DesiredSize.Height);

            Width = Math.Min(Children.Count == 0 ? 0 : Children.Max(x => x.DesiredSize.Width), availableSize.Width);

            Width = Width == 0 ? Width + 1 : Width;
            Height = Height == 0 ? Height + 1 : Height;
            return new Size(Width, Height);
        }

        protected virtual Size Arranging(IEnumerable<UIElement> Children, Size finalSize)
        {
            double Y = 0;
            foreach (var Element in Children)
            {
                Element.Measure(finalSize);
                var Height = Element.DesiredSize.Height;
                Element.Arrange(new Rect(0, Y, finalSize.Width, Height));
                Y += Height;
            }

            return finalSize;
        }
    }
    public class CenterHorizontally : UserControl
    {
        public new UIElement Content => (UIElement)base.Content;
        public CenterHorizontally()
        {

        }
        protected override Size MeasureOverride(Size availableSize)
        {
            Content.Measure(availableSize);
            return new Size(availableSize.Width, Math.Min(availableSize.Height, Content.DesiredSize.Height));
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Content.Measure(finalSize);
            Content.Arrange(new Rect((Content.DesiredSize.Width - finalSize.Width) / 2, 0, Content.DesiredSize.Width, Content.DesiredSize.Height));
            return finalSize;
        }
        public CenterHorizontally SetChild(UIElement Element)
        {
            base.Content = Element;
            return this;
        }
    }
    public class CenterVertically : UserControl
    {
        public new UIElement Content => (UIElement)base.Content;
        public CenterVertically()
        {

        }
        protected override Size MeasureOverride(Size availableSize)
        {
            Content.Measure(availableSize);
            return new Size(Math.Min(availableSize.Width, Content.DesiredSize.Width), availableSize.Height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Content.Measure(finalSize);
            Content.Arrange(new Rect(0, (Content.DesiredSize.Height - finalSize.Height) / 2, Content.DesiredSize.Width, Content.DesiredSize.Height));
            return finalSize;
        }

        public CenterVertically SetChild(UIElement Element)
        {
            base.Content = Element;
            return this;
        }
    }
    public class CenterBoth : UserControl
    {
        public UIElement UIElementContent => (UIElement)Content;
        public CenterBoth()
        {

        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElementContent.Measure(finalSize);
            UIElementContent.Arrange(new Rect((finalSize.Width - UIElementContent.DesiredSize.Width) / 2, (finalSize.Height - UIElementContent.DesiredSize.Height) / 2, UIElementContent.DesiredSize.Width, UIElementContent.DesiredSize.Height));
            return finalSize;
        }
        public CenterBoth SetChild(UIElement Element)
        {
            base.Content = Element;
            return this;
        }
    }
    public class Overlay : Grid
    {

        public Overlay()
        {

        }

        public Overlay AddChild(UIElement Element)
        {
            Children.Add(Element);
            return this;
        }
    }
}