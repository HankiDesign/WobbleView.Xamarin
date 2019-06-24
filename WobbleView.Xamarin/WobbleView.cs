using System;
using System.ComponentModel;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace WobbleView.Xamarin
{
    [Register("WobbleView"), DesignTimeVisible(true)]
    public class WobbleView : UIView, IWobbleView, IUIDynamicAnimatorDelegate
    {
        private UIView[] vertexViews = new UIView[4];

        // views considered as midpoints of rectangle's edges
        private UIView[] midpointViews = new UIView[4];

        // views considered as centers for rectangle's edges
        private UIView[] centerViews = new UIView[4];


        private UIDynamicAnimator animator;
        private CADisplayLink displayLink;
        private CAShapeLayer maskLayer = new CAShapeLayer();

        // midpoints' attachment behaviours to vertices views
        private VertexAttachmentBehaviour[] verticesAttachments;


        // midpoints' attachment behaviours to center view
        private VertexAttachmentBehaviour[] centersAttachments;

        /*
          The frequency of oscillation for the wobble behavior.
          */
        [Export(nameof(Frequency)), Browsable(true)]
        public float Frequency { get; set; } = 3;

        /*
        The amount of damping to apply to the wobble behavior.
        */
        [Export(nameof(Damping)), Browsable(true)]
        public float Damping { get; set; } = 0.3F;

        /*
        A bitmask value that identifies the edges that you want to wobble.
        You can use this parameter to wobble only a subSet of the edges of the rectangle.
        */
        public ViewEdge Edges { get; set; } = ViewEdge.Right();

        public WobbleView(NSCoder aDecoder) : base(aDecoder)
        {
            SetUp();
        }

        public WobbleView(CGRect Frame) : base(Frame)
        {
            SetUp();
        }

        private void SetUp()
        {
            Layer.MasksToBounds = false;
            Layer.AddSublayer(maskLayer);
            (Layer as WobbleLayer).Parent = this;

            SetUpVertices();
            SetUpMidpoints();
            SetUpCenters();
            SetUpBehaviours();
            SetUpDisplayLink();
        }

        public void ReSet()
        {

            SetUpMidpoints();
            SetUpCenters();
            SetUpBehaviours();

            if (vertexViews[0].Layer.PresentationLayer != null)
            {

                var bezierPath = new UIBezierPath();
                bezierPath.MoveTo(vertexViews[0].Layer.PresentationLayer.Frame.Location.Substract(Layer.PresentationLayer.Frame.Location));
                bezierPath.AddLineTo(vertexViews[1].Layer.PresentationLayer.Frame.Location.Substract(Layer.PresentationLayer.Frame.Location));
                bezierPath.AddLineTo(vertexViews[2].Layer.PresentationLayer.Frame.Location.Substract(Layer.PresentationLayer.Frame.Location));
                bezierPath.AddLineTo(vertexViews[3].Layer.PresentationLayer.Frame.Location.Substract(Layer.PresentationLayer.Frame.Location));
                bezierPath.ClosePath();

                maskLayer.Path = bezierPath.CGPath;
                (Layer as CAShapeLayer).Path = bezierPath.CGPath;
                Layer.Mask = maskLayer;
            }
        }

        private void SetUpVertices()
        {

            vertexViews = new UIView[4];

            var verticesLocations = new CGPoint[] {
            new CGPoint( Frame.Location.X, Frame.Location.Y),
                new CGPoint(Frame.Location.X + Frame.Width, Frame.Location.Y),
                new CGPoint(Frame.Location.X + Frame.Width, Frame.Location.Y + Frame.Height),
                new CGPoint(Frame.Location.X, Frame.Location.Y + Frame.Height)
            };

            CreateAdditionalViews(ref vertexViews, verticesLocations);
        }

        private void SetUpMidpoints()
        {

            midpointViews = new UIView[4];

            var midpointsLocations = new CGPoint[] {
                new CGPoint( Frame.Location.X + Frame.Width / 2, Frame.Location.Y),
                new CGPoint(Frame.Location.X + Frame.Width, Frame.Location.Y + Frame.Height / 2),
                new CGPoint(Frame.Location.X + Frame.Width / 2, Frame.Location.Y + Frame.Height),
                new CGPoint(Frame.Location.X, Frame.Location.Y + Frame.Height / 2)
            };

            CreateAdditionalViews(ref midpointViews, midpointsLocations);
        }

        private void SetUpCenters()
        {

            centerViews = new UIView[4];

            var radius = Math.Min(Frame.Size.Width / 2, Frame.Size.Height / 2);

            var centersLocations = new CGPoint[] {
                new CGPoint( Frame.Location.X + Frame.Width / 2, Frame.Location.Y + radius),
                new CGPoint(Frame.Location.X + Frame.Width - radius, Frame.Location.Y + Frame.Height / 2),
                new CGPoint(Frame.Location.X + Frame.Width / 2, Frame.Location.Y + Frame.Height - radius),
                new CGPoint(Frame.Location.X + radius, Frame.Location.Y + Frame.Height / 2)
            };

            CreateAdditionalViews(ref centerViews, centersLocations);
        }

        private void SetUpBehaviours()
        {

            animator = new UIDynamicAnimator(this);
            animator.Delegate = this;
            verticesAttachments = new VertexAttachmentBehaviour[3];
            centersAttachments = new VertexAttachmentBehaviour[3];

            for (int i = 0; i < midpointViews.Length; i++)
            {
                var formerVertexIndex = i;
                var latterVertexIndex = (i + 1) % vertexViews.Length;

                CreateAttachmentBehaviour(ref verticesAttachments, midpointViews[i], formerVertexIndex);
                CreateAttachmentBehaviour(ref verticesAttachments, midpointViews[i], latterVertexIndex);
                CreateAttachmentBehaviour(ref centersAttachments, midpointViews[i], formerVertexIndex);
            }
        }

        private void SetUpDisplayLink()
        {
            displayLink = CADisplayLink.Create(DisplayLinkUpdate);
            displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
            displayLink.Paused = true;
        }

        // MARK: CADisplayLink selector
        private void DisplayLinkUpdate()
        {
            foreach (var behaviour in centersAttachments)
            {
                behaviour.AnchorPoint = centerViews[behaviour.VertexIndex].Layer.PresentationLayer.Frame.Location;
            }

            foreach (var behaviour in verticesAttachments)
            {
                behaviour.AnchorPoint = vertexViews[behaviour.VertexIndex].Layer.PresentationLayer.Frame.Location;
            }

            var bezierPath = new UIBezierPath();
            bezierPath.MoveTo(vertexViews[0].Layer.PresentationLayer.Frame.Location.Substract(Layer.PresentationLayer.Frame.Location));
            AddEdge(ref bezierPath, 0, 1, ViewEdge.Top());
            AddEdge(ref bezierPath, 1, 2, ViewEdge.Right());
            AddEdge(ref bezierPath, 2, 3, ViewEdge.Bottom());
            AddEdge(ref bezierPath, 3, 0, ViewEdge.Left());
            bezierPath.ClosePath();

            maskLayer.Path = bezierPath.CGPath;
            (Layer as CAShapeLayer).Path = bezierPath.CGPath;
            Layer.Mask = maskLayer;
        }

        public override UIColor BackgroundColor
        {
            get => base.BackgroundColor; set
            {
                base.BackgroundColor = value;
                (Layer as CAShapeLayer).FillColor = base.BackgroundColor.CGColor;
            }
        }

        private void CreateAdditionalViews(ref UIView[] views, CGPoint[] Locations)
        {
            foreach (var Location in Locations)
            {
                var view = new UIView(new CGRect(Location, new CGSize(1, 1)))
                {
                    BackgroundColor = UIColor.Clear
                };
                AddSubview(view);

                views.Append(view);
            }
        }

        private void CreateAttachmentBehaviour(ref VertexAttachmentBehaviour[] behaviours, UIView view, int vertexIndex)
        {

            var attachmentBehaviour = new VertexAttachmentBehaviour(view, vertexViews[vertexIndex].Frame.Location);
            attachmentBehaviour.Damping = Damping;
            attachmentBehaviour.Frequency = Frequency;
            attachmentBehaviour.VertexIndex = vertexIndex;
            animator.AddBehavior(attachmentBehaviour);

            behaviours.Append(attachmentBehaviour);
        }

        private void AddEdge(ref UIBezierPath bezierPath, int formerVertex, int latterVertex, ViewEdge curved)
        {

            if (curved.BoolValue)
            {
                var controlPoint = vertexViews[formerVertex].Layer.PresentationLayer.Frame.Location
                            .Substract(midpointViews[formerVertex].Layer.PresentationLayer.Frame.Location)
                            .Substract(vertexViews[latterVertex].Layer.PresentationLayer.Frame.Location)
                            .Substract(Layer.PresentationLayer.Frame.Location);

                bezierPath.AddQuadCurveToPoint(vertexViews[latterVertex].Layer.PresentationLayer.Frame.Location.Substract(Layer.PresentationLayer.Frame.Location),
                    controlPoint);

                return;
            }

            bezierPath.AddLineTo(vertexViews[latterVertex].Layer.PresentationLayer.Frame.Location.Substract(Layer.PresentationLayer.Frame.Location));
        }

        void IWobbleView.PositionChanged()
        {
            displayLink.Paused = false;

            var verticesOrigins = new CGPoint[] {
                new CGPoint(Frame.Location.X, Frame.Location.Y),
                new CGPoint(Frame.Location.X + Frame.Width, Frame.Location.Y),
                new CGPoint(Frame.Location.X + Frame.Width, Frame.Location.Y + Frame.Height),
                new CGPoint(Frame.Location.X, Frame.Location.Y + Frame.Height)
            };


            for (int i = 0; i < vertexViews.Length; i++)
            {
                vertexViews[i].Frame = new CGRect(verticesOrigins[i], vertexViews[i].Frame.Size);
            }

            var radius = Math.Min(Frame.Size.Width / 2, Frame.Size.Height / 2);


            var centersOrigins = new CGPoint[] {
            new CGPoint(Frame.Location.X + Frame.Width / 2, Frame.Location.Y + radius),
            new CGPoint(Frame.Location.X + Frame.Width - radius, Frame.Location.Y + Frame.Height / 2),
            new CGPoint(Frame.Location.X + Frame.Width / 2, Frame.Location.Y + Frame.Height - radius),
            new CGPoint(Frame.Location.X + radius, Frame.Location.Y + Frame.Height / 2)
        };


            for (int i = 0; i < centerViews.Length; i++)
            {
                centerViews[i].Frame = new CGRect(centersOrigins[i], centerViews[i].Frame.Size);
            }
        }

        public void WillResume(UIDynamicAnimator animator) { }

        public void DidPause(UIDynamicAnimator animator)
        {
            displayLink.Paused = true;
        }
    }
}