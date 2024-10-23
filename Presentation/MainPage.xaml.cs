using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
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
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private SemaphoreSlim _renderSemaphore = new SemaphoreSlim(1);
        private Stack<Rect> _rects = new Stack<Rect>();
        private async void CanvasVirtualControl_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args) {

            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            try {
                await _renderSemaphore.WaitAsync(ct);

                foreach (var region in args.InvalidatedRegions) {
                    _rects.Push(region);
                }
                try {
                    while (_rects.TryPeek(out Rect region)) {
                        if (_renderSynchronous) {
                            using (var ds = sender.CreateDrawingSession(region)) {
                                Render(ds, region, ct);
                            }
                        } else {
                            using (var ds = sender.CreateDrawingSession(region)) {
                                sender.SuspendDrawingSession(ds);
                                await Task.Run(() => {
                                    sender.ResumeDrawingSession(ds);
                                    Render(ds, region, ct);
                                    sender.SuspendDrawingSession(ds);
                                    return ds;
                                }, ct);
                            }
                        }
                        _rects.Pop();
                    }
                    _renderSynchronous = false;
                } finally {
                    _renderSemaphore.Release();
                }
            } catch (OperationCanceledException) {
                return;
            }
        }

        private void Render(CanvasDrawingSession ds, Rect region, CancellationToken ct = default) {
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
                if (ct.IsCancellationRequested)
                    return;
                ds.FillRectangle(new Rect(0, y, 500, stripeHeight), stripeColor);
            }
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            if (!e.IsIntermediate) {
                await _renderSemaphore.WaitAsync();
                try {
                    CanvasVirtualControl.DpiScale = ScrollViewer.ZoomFactor;
                } finally {
                    _renderSemaphore.Release();
                }
            }
        }

        private void CanvasVirtualControl_CreateResources(CanvasVirtualControl sender, CanvasCreateResourcesEventArgs args) {
            if (args.Reason == CanvasCreateResourcesReason.DpiChanged) {
                _renderSynchronous = true;
            }
        }

        private bool _renderSynchronous = false;
    }
}
