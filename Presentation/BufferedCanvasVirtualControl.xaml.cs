using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public record DrawingOptions(CanvasDrawingSession DrawingSession, CancellationToken CancellationToken = default);

    [ObservableObject]
    public sealed partial class BufferedCanvasVirtualControl : UserControl, ICanvasResourceCreator
    {
        public CanvasDevice Device { get; } = CanvasDevice.GetSharedDevice();

        public BufferedCanvasVirtualControl()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            Setup();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Teardown();
        }
        partial void OnIsRenderedChanged(bool value)
        {
            Opacity = IsRendered ? 1 : 0.0001; // Not completely invisible to not prevent rendering of virtual surface
        }

        private SemaphoreSlim _dpiChangedSemaphore = new(1);
        async partial void OnDpiScaleChanged(float value)
        {
            ThrowIfNotSetup();

            var entered = _dpiChangedSemaphore.Wait(0);
            if (!entered)
                return;

            try
            {
                await _renderSemaphore.WaitAsync();
                if (_canvasVirtualControl is null)
                    return;

                try
                {
                    _canvasVirtualControl.DpiScale = DpiScale;
                } finally
                {
                    _renderSemaphore.Release();
                }
            } finally {
                _dpiChangedSemaphore.Release();
            }
        }

        private CancellationTokenSource _invalidatedCts = new();
        public async Task<bool> InvalidateAsync()
        {
            ThrowIfNotSetup();

            var ct = ReissueRenderingToken();

            await _renderSemaphore.WaitAsync();
            if (ct.IsCancellationRequested)
                return false;

            CanvasCommandList newSurface = await Task.Run(async () =>
            {

            });

            try
            {
                newSurface = await Task.Run(async () =>
                {
                    if (ct.IsCancellationRequested)
                        return null;

                    await _renderSemaphore.WaitAsync();
                    if (ct.IsCancellationRequested)
                        return null;

                    try
                    {
                        var newSurface = new CanvasCommandList(Device);

                        using (var ds = newSurface.CreateDrawingSession())
                        {
                            if (AsyncRenderCommand is not null)
                            {
                                await AsyncRenderCommand.ExecuteAsync(new DrawingOptions(ds, ct));
                            } else
                            {
                                RenderCommand?.Execute(new DrawingOptions(ds));
                            }
                        }

                        return newSurface;

                        try
                        {
                        } catch (OperationCanceledException)
                        {
                            newSurface?.Dispose();
                            throw;
                        }

                    } finally
                    {
                        _renderSemaphore.Release();
                    }
                });

                if (newSurface is null || ct.IsCancellationRequested)
                    return false;
            } catch (OperationCanceledException) {
                newSurface?.Dispose();
                            return false;
            }

            _image?.Dispose();
            _image = newSurface;

            if (_canvasVirtualControl is not null && _canvasVirtualControl.ReadyToDraw)
                _canvasVirtualControl.Invalidate();

            return true;
        }
        public void Clear()
        {
            ReissueRenderingToken(); // Cancel rendering

            IsRendered = false;
            _image?.Dispose();
            _image = null;

            if (_canvasVirtualControl is not null && _canvasVirtualControl.ReadyToDraw)
                _canvasVirtualControl.Invalidate();
        }

        private void RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            ThrowIfNotSetup();

            if (_image is null)
                return;

            // Check if render lock is acquired and exit if it is
            bool entered = _renderSemaphore.Wait(0);
            if (!entered)
                return;

            try
            {
                foreach (var region in args.InvalidatedRegions)
                {
                    using (var ds = sender.CreateDrawingSession(region))
                    {
                        ds.DrawImage(_image);
                    }
                }
            } finally {
                _renderSemaphore.Release();
            }

            IsRendered = true;
        }

        private bool _isSetup = false;
        private void Setup()
        {
            if (_isSetup)
                return;

            Content = _canvasVirtualControl = new CanvasVirtualControl()
            {
                CustomDevice = Device,
                ClearColor = Colors.Transparent,
                DpiScale = DpiScale
            };

            _canvasVirtualControl.CreateResources += CreateResources;
            _canvasVirtualControl.RegionsInvalidated += RegionsInvalidated;

            _isSetup = true;
        }

        private void Teardown()
        {
            if (!_isSetup)
                return;
            _isSetup = false;

            _canvasVirtualControl.RegionsInvalidated -= RegionsInvalidated;

            _canvasVirtualControl.RemoveFromVisualTree();
            _image?.Dispose();

            _canvasVirtualControl = null;
            _image = null;

            Content = null;
        }

        private void CreateResources(CanvasVirtualControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (args.Reason == CanvasCreateResourcesReason.FirstTime)
            {
                args.TrackAsyncAction(InvalidateAsync().AsAsyncAction());
            }
        }
        private void ThrowIfNotSetup()
        {
            if (!_isSetup)
                throw new InvalidOperationException("Control is not setup.");
        }
        private CancellationToken ReissueRenderingToken()
        {
            _invalidatedCts.Cancel();
            _invalidatedCts.Dispose();
            return (_invalidatedCts = new CancellationTokenSource()).Token;
        }

        [ObservableProperty]
        private IRelayCommand<DrawingOptions> _renderCommand;
        [ObservableProperty]
        private IAsyncRelayCommand<DrawingOptions> _asyncRenderCommand;
        [ObservableProperty]
        private float _dpiScale = 1;
        [ObservableProperty]
        private bool _isRendered = false;

        private SemaphoreSlim _renderSemaphore = new(1);
        private CanvasVirtualControl _canvasVirtualControl;
        private CanvasCommandList _image;
    }
}
