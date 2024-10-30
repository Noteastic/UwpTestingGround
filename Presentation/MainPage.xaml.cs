using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Presentation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [ObservableObject]
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        [RelayCommand]
        private void Render(RenderOptions renderOptions) {
            var ds = renderOptions.DrawingSession;
            var region = renderOptions.Region;

            // Get the total height of the CanvasVirtualControl
            float totalHeight = 100000;
            float stripeHeight = 20; // Height of each stripe

            // List of colors to cycle through
            var colors = new List<Color> { Colors.Blue, Colors.Red, Colors.Green, Colors.Yellow, Colors.Purple };

            // Calculate the starting y position based on the region.Top
            float startY = (float)Math.Floor(region.Top / stripeHeight) * stripeHeight;

            for (float y = startY; y < totalHeight; y += stripeHeight) {
                // Exit if the current y position is not visible anymore
                if (y > region.Bottom) {
                    break;
                }

                // Use the % operator to cycle through the list of colors
                var stripeColor = colors[(int)(y / stripeHeight) % colors.Count];
                Thread.Sleep(10);
                ds.FillRectangle(new Rect(0, y, 500, stripeHeight), stripeColor);
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            CanvasVirtualControl.DpiScale = ScrollViewer.ZoomFactor;
        }
    }
}

public static class RectExtensions {
    public static bool OverlapsWith(this in Rect r1, in Rect r2, int margin = 0) {
        return r1.Left < r2.Right + margin && r1.Right > r2.Left - margin && r1.Top < r2.Bottom + margin && r1.Bottom > r2.Top - margin;
    }
    public static bool Contains(this in Rect r1, in Rect r2) {
        return r1.Contains(new Point(r2.X, r2.Y)) && r1.Contains(new Point(r2.X +r2.Width, r2.Y + r2.Height));
    }
}