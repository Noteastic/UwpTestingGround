using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Presentation
{
    [ObservableObject]
    public sealed partial class NoteasticPage : UserControl
    {
        public NoteasticPage()
        {
            this.InitializeComponent();
        }

        public async void Prepare()
        {
            //CanvasVirtualControl.Prepare();

            //await CanvasVirtualControl.InvalidateAsync();
        }
        public void Cleanup()
        {
            //CanvasVirtualControl.Cleanup();
        }

        async partial void OnPageModelChanged(NoteasticPageModel value)
        {
            CanvasVirtualControl.Clear();
            await CanvasVirtualControl.InvalidateAsync();
        }

        [RelayCommand]
        private void Render(DrawingOptions drawingOptions)
        {
            var ds = drawingOptions.DrawingSession;
            var ct = drawingOptions.CancellationToken;

            Thread.Sleep(1000);

            // Get the total height of the CanvasVirtualControl
            double totalHeight = PageModel.Size.Height;
            float stripeHeight = 20; // Height of each stripe

            // List of colors to cycle through
            var colors = new List<Color> { Colors.Blue, Colors.Red, Colors.Green, Colors.Yellow, Colors.Purple };

            for (float y = 0; y < totalHeight; y += stripeHeight)
            {
                // Use the % operator to cycle through the list of colors
                var stripeColor = colors[(int)(y / stripeHeight) % colors.Count];
                ds.FillRectangle(new Rect(0, y, PageModel.Size.Width, stripeHeight), new CanvasSolidColorBrush(ds, stripeColor) { Opacity = 0.6f });
            }

            foreach (var dynamicElement in PageModel.DynamicElements)
            {
                ds.DrawGeometry(dynamicElement, Colors.Black, 5);
                ds.FillGeometry(dynamicElement, Colors.White);
            }
        }

        [RelayCommand]
        private async Task GenerateNewPageAsync()
        {
            var sw = Stopwatch.StartNew();
            await CanvasVirtualControl.InvalidateAsync();
            Debug.WriteLine($"GenerateNewPageAsync: {sw.ElapsedMilliseconds}ms");
        }

        [RelayCommand]
        private async Task AddRandomElement()
        {
            PageModel.AddRandomElement();

            await CanvasVirtualControl.InvalidateAsync();
        }

        [ObservableProperty]
        private NoteasticPageModel _pageModel;
    }
}
