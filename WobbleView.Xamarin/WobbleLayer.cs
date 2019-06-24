using CoreAnimation;
using CoreGraphics;

namespace WobbleView.Xamarin
{
    internal class WobbleLayer : CAShapeLayer
    {
        public IWobbleView Parent { get; set; }

        public override CGPoint Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                Parent.PositionChanged();
            }
        }
    }
}
