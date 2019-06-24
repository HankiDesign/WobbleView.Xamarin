using CoreGraphics;
using UIKit;

namespace WobbleView.Xamarin
{
    public class VertexAttachmentBehaviour : UIAttachmentBehavior
    {
        public VertexAttachmentBehaviour(IUIDynamicItem item, CGPoint anchorPoint) : base(item, anchorPoint)
        {
        }

        public int VertexIndex { get; set; }
    }
}
