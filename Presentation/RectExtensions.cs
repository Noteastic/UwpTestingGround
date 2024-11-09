using Windows.Foundation;

public static class RectExtensions {
    public static bool OverlapsWith(this in Rect r1, in Rect r2, int margin = 0) {
        return r1.Left < r2.Right + margin && r1.Right > r2.Left - margin && r1.Top < r2.Bottom + margin && r1.Bottom > r2.Top - margin;
    }
    public static bool Contains(this in Rect r1, in Rect r2) {
        return r1.Contains(new Point(r2.X, r2.Y)) && r1.Contains(new Point(r2.X +r2.Width, r2.Y + r2.Height));
    }
}