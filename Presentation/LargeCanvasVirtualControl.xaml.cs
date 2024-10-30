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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using Windows.UI.Core;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

public record RenderOptions(CanvasDrawingSession DrawingSession, Rect Region);

namespace Presentation {

    [ObservableObject]
    public sealed partial class LargeCanvasVirtualControl : UserControl {
        public LargeCanvasVirtualControl() {
            this.InitializeComponent();
        }

        private void CanvasVirtualControl_CreateResources(CanvasVirtualControl sender, CanvasCreateResourcesEventArgs args) {
            if (args.Reason == CanvasCreateResourcesReason.DpiChanged) {
                _renderSynchronous = true;
            }
        }

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
                _rects = OptimizeRects(_rects);
                try {
                    if (_renderSynchronous) {
                        while (_rects.TryPeek(out Rect region)) {
                            using (var ds = sender.CreateDrawingSession(region)) {
                                RenderCommand?.Execute(new RenderOptions(ds, region));
                            }
                            _rects.Pop();
                        }
                    }
                    await Task.Run(async () => {
                        while (_rects.TryPeek(out Rect region)) {
                            if (ct.IsCancellationRequested)
                                return;

                            CanvasDrawingSession ds = null;
                            try {
                                ds = sender.CreateDrawingSession(region);
                                RenderCommand?.Execute(new RenderOptions(ds, region));
                            } finally {
                                if (ds is not null) {
                                    sender.SuspendDrawingSession(ds);
                                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                                        ds.Dispose();
                                    });
                                }
                            }

                            _rects.Pop();
                        }
                    }, ct);
                    if (ct.IsCancellationRequested)
                        return;

                    _renderSynchronous = false;
                } finally {
                    _renderSemaphore.Release();
                }
            } catch (OperationCanceledException) {
                return;
            } catch (ArgumentException) {
                return; // Triggered on CreateDrawingSession 'randomly' on hectic movements
            }
        }

        [ObservableProperty]
        private float _dpiScale = 1;
        [ObservableProperty]
        private IRelayCommand<RenderOptions> _renderCommand;


        private Stack<Rect> OptimizeRects(Stack<Rect> rects) {
            // Union all overlapping rects
            var optimizedRects = new Stack<Rect>();
            while (rects.Count > 0) {
                var rect = rects.Pop();
                while (rects.Count > 0) {
                    var nextRect = rects.Pop();
                    if (rect.OverlapsWith(nextRect, 20)) {
                        rect = RectHelper.Union(rect, nextRect);
                    } else {
                        rects.Push(nextRect);
                        break;
                    }
                }
                optimizedRects.Push(rect);
            }
            return optimizedRects;
        }
        async partial void OnDpiScaleChanged(float value) {
            await _renderSemaphore.WaitAsync();
            try {
                CanvasVirtualControl.DpiScale = DpiScale;
            } finally {
                _renderSemaphore.Release();
            }
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private SemaphoreSlim _renderSemaphore = new SemaphoreSlim(1);
        private Stack<Rect> _rects = new Stack<Rect>();
        private bool _renderSynchronous = false;
    }
}
