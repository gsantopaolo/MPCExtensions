using MPCExtensions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MPCExtensions.Controls
{
    [TemplatePart(Name = PART_ROOT_NAME, Type = typeof(Canvas))]
    public class ScatterView : ItemsControl
    {
        private const string PART_ROOT_NAME = "PART_ROOT";

        private Canvas _canvas;

        public ScatterView()
        {
            this.DefaultStyleKey = typeof(ScatterView);
            //this.PointerPressed += ScatterView_PointerPressed;

        }


        public bool AllowItemsRotation
        {
            get { return (bool)GetValue(AllowItemsRotationProperty); }
            set { SetValue(AllowItemsRotationProperty, value); }
        }
        public static readonly DependencyProperty AllowItemsRotationProperty =
           DependencyProperty.Register(nameof(AllowItemsRotation), typeof(bool), typeof(ScatterView), new PropertyMetadata(true));

        protected override void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle)
        {
            base.OnItemContainerStyleChanged(oldItemContainerStyle, newItemContainerStyle);
        }

        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);
        }

        protected override void OnItemContainerStyleSelectorChanged(StyleSelector oldItemContainerStyleSelector, StyleSelector newItemContainerStyleSelector)
        {
            base.OnItemContainerStyleSelectorChanged(oldItemContainerStyleSelector, newItemContainerStyleSelector);
        }

        protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
        {
            base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
        }

        protected override void OnItemTemplateSelectorChanged(DataTemplateSelector oldItemTemplateSelector, DataTemplateSelector newItemTemplateSelector)
        {
            base.OnItemTemplateSelectorChanged(oldItemTemplateSelector, newItemTemplateSelector);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (element is FrameworkElement)
                ((FrameworkElement)element).Loaded += Element_Loaded;
        }

        private void Element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;


            if (element != null && _canvas != null)
            {
                var manManager = new ManipulationManager(element, _canvas);

                manManager.OnFilterManipulation = ManipulationFilter.Clamp;
                manManager.Configure(true, AllowItemsRotation, true, true);

                element.Loaded -= Element_Loaded;
            }
        }

        protected override void OnApplyTemplate()
        {

            _canvas = GetTemplateChild(PART_ROOT_NAME) as Canvas;
            InitEvents();
        }

        private void InitEvents()
        {
            if (_canvas != null)
                _canvas.SizeChanged += canvas_SizeChanged;
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update clipping geometry to its new size
            // That way contained elements will continue stay at the same relative position after the resize
            if (_canvas != null)
            {
                _canvas.Clip = new Windows.UI.Xaml.Media.RectangleGeometry
                {
                    Rect = new Windows.Foundation.Rect(0, 0, _canvas.ActualWidth, _canvas.ActualHeight)
                };
            }
        }
    }
}
