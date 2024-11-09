using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public ObservableCollection<NoteasticPageModel> Pages = new()
        {
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1),
            new NoteasticPageModel(new Size(500, 500), 1),
            new NoteasticPageModel(new Size(1000, 500), 1),
            new NoteasticPageModel(new Size(500, 1000), 1),
            new NoteasticPageModel(new Size(1000, 1000), 1)
        };

        public MainPage()
        {
            this.InitializeComponent();

            Pages.CollectionChanged += Pages_CollectionChanged;
        }

        [RelayCommand]
        private void RemovePages()
        {
            Pages.Clear();

            (Window.Current.Content as Frame).Navigate(typeof(Page));

            GC.Collect();
        }

        partial void OnDpiScaleChanged(float value)
        {
            foreach (var page in Pages)
            {
                page.DpiScale = value;
            }
        }

        private void Pages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Calculate max width
            MaxWidth = Pages.Any() ? Pages.Max(p => p.Size.Width) : 1000;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            DpiScale = ScrollViewer.ZoomFactor;
        }

        [ObservableProperty]
        private float _dpiScale = 1;
        [ObservableProperty]
        private double _maxWidth = 0;


        private void ItemsRepeater_ElementPrepared(Microsoft.UI.Xaml.Controls.ItemsRepeater sender, Microsoft.UI.Xaml.Controls.ItemsRepeaterElementPreparedEventArgs args)
        {
            var page = args.Element.FindDescendantOrSelf<NoteasticPage>();
            page?.Prepare();
        }

        private void ItemsRepeater_ElementIndexChanged(Microsoft.UI.Xaml.Controls.ItemsRepeater sender, Microsoft.UI.Xaml.Controls.ItemsRepeaterElementIndexChangedEventArgs args)
        {

        }
        private void ItemsRepeater_ElementClearing(Microsoft.UI.Xaml.Controls.ItemsRepeater sender, Microsoft.UI.Xaml.Controls.ItemsRepeaterElementClearingEventArgs args)
        {

            var page = args.Element.FindDescendantOrSelf<NoteasticPage>();
            page?.Cleanup();
        }
    }
}
