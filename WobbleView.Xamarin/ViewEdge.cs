namespace WobbleView.Xamarin
{
    public class ViewEdge
    {
        private readonly int value = 0;

        public ViewEdge(int value)
        {
            this.value = value;
        }

        public bool BoolValue
        {
            get { return value != 0; }
        }

        public int RawValue {
            get { return value; }
        }

        public static ViewEdge AllZeros() {
            return new ViewEdge(0);
        }

        public static ViewEdge None()
        {
            return new ViewEdge(0b0000);
        }

        public static ViewEdge Left()
        {
            return new ViewEdge(0b0001);
        }

        public static ViewEdge Top()
        {
            return new ViewEdge(0b0010);
        }

        public static ViewEdge Right()
        {
            return new ViewEdge(0b0100);
        }

        public static ViewEdge Bottom()
        {
            return new ViewEdge(0b1000);
        }

        public static ViewEdge All()
        {
            return new ViewEdge(0b1111);
        }
    }
}
