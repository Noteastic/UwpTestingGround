using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


namespace Presentation
{
    public partial class NoteasticPageModel : ObservableObject
    {
        public static int id = 0;
        public NoteasticPageModel(Size size, float dpiScale)
        {
            Size = size;
            DpiScale = dpiScale;

            var idText = CanvasGeometry.CreateText(new CanvasTextLayout(CanvasDevice.GetSharedDevice(), (id++).ToString(), new CanvasTextFormat()
            {
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Center,
                FontSize = 80,
            }, (float)Size.Width, (float)Size.Height));
            DynamicElements.Add(idText);
        }

        public void AddRandomElement()
        {
            var random = new Random();
            var x = random.Next(50, (int)(Size.Width - 50));
            var y = random.Next(50, (int)(Size.Height - 50));
            var width = random.Next(25, 100);
            var height = random.Next(25, 100);
            var rotation = random.NextDouble() * 360;

            var rect = CanvasGeometry.CreateRectangle(CanvasDevice.GetSharedDevice(), new Rect(x, y, width, height));
            rect = rect.Transform(Matrix3x2.CreateRotation((float)rotation, new Vector2(x - width / 2, y - height / 2)));

            DynamicElements.Add(rect);
        }

        [ObservableProperty]
        private List<CanvasGeometry> _dynamicElements = new();

        [ObservableProperty]
        private Size _size;

        [ObservableProperty]
        private float _dpiScale;
    }
}
