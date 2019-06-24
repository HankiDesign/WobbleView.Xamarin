using System;
using CoreGraphics;

namespace WobbleView.Xamarin
{
    public static class Utils
    {
        public static CGPoint Substract (this CGPoint first, CGPoint second)
        {
            return new CGPoint(first.X-second.X, first.Y-second.Y);
        }
    }
}
