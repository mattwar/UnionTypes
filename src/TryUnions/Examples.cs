#if false
// as adhoc type union
global using AdHocShape = UnionTypes.OneOf<
    UnionTypes.UntaggedShape.Nothing, 
    UnionTypes.UntaggedShape.Square, 
    UnionTypes.UntaggedShape.Circle, 
    UnionTypes.UntaggedShape.Rectangle, 
    UnionTypes.UntaggedShape.Triangle>;

// adhoc subset of shape
global using SquareOrCircle = UnionTypes.OneOf<
    UnionTypes.UntaggedShape.Square,
    UnionTypes.UntaggedShape.Circle>;

namespace UnionTypes
{
    // as sub-classes, without tags
    public record UntaggedShape
    {
        private UntaggedShape() { }

        public bool IsNothing => this is Nothing;
        public bool IsSquare => this is Square;
        public bool IsCircle => this is Circle;
        public bool IsRectangle => this is Rectangle;
        public bool IsTriangle => this is Triangle;

        public sealed record Nothing : UntaggedShape { public static readonly Nothing Instance = new Nothing(); }
        public sealed record Square(double Length) : UntaggedShape;
        public sealed record Circle(double Radius) : UntaggedShape;
        public sealed record Rectangle(double Width, double Height) : UntaggedShape;
        public sealed record Triangle(double LengthA, double LengthB, double LengthC) : UntaggedShape;
    }

    // as sub-classes with tags
    public record TaggedShape
    {
        private enum Tag { Nothing, Square, Circle, Rectangle, Triangle };

        private readonly Tag _tag;
        private TaggedShape(Tag tag) { _tag = tag; }

        public bool IsNothing => _tag == Tag.Nothing;
        public bool IsSquare => _tag == Tag.Square;
        public bool IsCircle => _tag == Tag.Circle;
        public bool IsRectangle => _tag == Tag.Rectangle;
        public bool IsTriangle => _tag == Tag.Triangle;

        public sealed record Nothing() : TaggedShape(Tag.Nothing) { public static readonly Nothing Instance = new Nothing(); }
        public sealed record Square(double Length) : TaggedShape(Tag.Square);
        public sealed record Circle(double Radius) : TaggedShape(Tag.Circle);
        public sealed record Rectangle(double Width, double Height) : TaggedShape(Tag.Rectangle);
        public sealed record Triangle(double LengthA, double LengthB, double LengthC) : TaggedShape(Tag.Triangle);
    }

    // as struct with separate fields for data
    public record struct StructShape1
    {
        private enum Tag { Nothing = 0, Square, Circle, Rectangle, Triangle };

        private readonly Tag _tag;

        // separate fields for each kind with data
        private readonly SquareData _square;
        private readonly CircleData _circle;
        private readonly RectangleData _rectangle;
        private readonly TriangleData _triangle;

        private StructShape1(Tag tag, SquareData square, CircleData circle, RectangleData rectangle, TriangleData triangle)
        { 
            _tag = tag;
            _square = square;
            _circle = circle;
            _rectangle = rectangle;
            _triangle = triangle;
        }

        public static readonly StructShape1 Nothing = 
            new StructShape1(Tag.Nothing, default, default, default, default);

        public static StructShape1 CreateSquare(double length) =>
            new StructShape1(Tag.Square, new SquareData(length), default, default, default);

        public static StructShape1 CreateCircle(double radius) =>
            new StructShape1(Tag.Circle, default, new CircleData(radius), default, default);

        public static StructShape1 CreateRectangle(double width, double height) =>
            new StructShape1(Tag.Rectangle, default, default, new RectangleData(width, height), default);

        public static StructShape1 CreateTriangle(double lengthA, double lengthB, double lengthC) =>
            new StructShape1(Tag.Triangle, default, default, default, new TriangleData(lengthA, lengthB, lengthC));

        public bool IsNothing => _tag == Tag.Nothing;
        public bool IsSquare => _tag == Tag.Square;
        public bool IsCircle => _tag == Tag.Circle;
        public bool IsRectangle => _tag == Tag.Rectangle;
        public bool IsTriangle => _tag == Tag.Triangle;

        public bool TryGetSquare(out double length)
        {
            if (IsSquare) { length = _square.Length; return true; }
            length = default; return false;
        }

        public bool TryGetCircle(out double radius)
        {
            if (IsCircle) { radius = _circle.Radius; return true; }
            radius = default; return false;
        }

        public bool TryGetRectangle(out double width, out double height)
        {
            if (IsRectangle) { width = _rectangle.Width; height = _rectangle.Height; return true; }
            width = default; height = default; return false;
        }

        public bool TryGetTriangle(out double lengthA, out double lengthB, out double lengthC)
        {
            if (IsTriangle) { lengthA = _triangle.LengthA; lengthB = _triangle.LengthB; lengthC = _triangle.LengthC; return true; }
            lengthA = default; lengthB = default; lengthC = default; return false;
        }

        private record struct SquareData(double Length);
        private record struct CircleData(double Radius);
        private record struct RectangleData(double Width, double Height);
        private record struct TriangleData(double LengthA, double LengthB, double LengthC);
    }

    // as struct with overlapping fields for data
    public record struct StructShape2
    {
        private enum Tag { Nothing = 0, Square, Circle, Rectangle, Triangle };

        private readonly Tag _tag;

        // separate fields for each kind with data
        private readonly double _value1;
        private readonly double _value2;
        private readonly double _value3;

        private StructShape2(Tag tag, double value1, double value2, double value3)
        {
            _tag = tag;
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
        }

        public static readonly StructShape2 Nothing =
            new StructShape2(Tag.Nothing, default, default, default);

        public static StructShape2 Square(double length) =>
            new StructShape2(Tag.Square, length, default, default);

        public static StructShape2 Circle(double radius) =>
            new StructShape2(Tag.Circle, radius, default, default);

        public static StructShape2 Rectangle(double width, double height) =>
            new StructShape2(Tag.Rectangle, width, height, default);

        public static StructShape2 Triangle(double lengthA, double lengthB, double lengthC) =>
            new StructShape2(Tag.Triangle, lengthA, lengthB, lengthC);

        public bool IsNothing => _tag == Tag.Nothing;
        public bool IsSquare => _tag == Tag.Square;
        public bool IsCircle => _tag == Tag.Circle;
        public bool IsRectangle => _tag == Tag.Rectangle;
        public bool IsTriangle => _tag == Tag.Triangle;

        public bool TryGetSquare(out double length)
        {
            if (IsSquare) { length = _value1; return true; }
            length = default; return false;
        }

        public bool TryGetCircle(out double radius)
        {
            if (IsCircle) { radius = _value1; return true; }
            radius = default; return false;
        }

        public bool TryGetRectangle(out double width, out double height)
        {
            if (IsRectangle) { width = _value1; height = _value2; return true; }
            width = default; height = default; return false;
        }

        public bool TryGetTriangle(out double lengthA, out double lengthB, out double lengthC)
        {
            if (IsTriangle) { lengthA = _value1; lengthB = _value2; lengthC = _value3; return true; }
            lengthA = default; lengthB = default; lengthC = default; return false;
        }
    }

    // as struct with overlapping fields for data
    public record struct StructShape3
    {
        private enum Tag { Nothing = 0, Square, Circle, Rectangle, Triangle };

        private readonly Tag _tag;

        // separate fields for each kind with data
        private readonly double _value1;
        private readonly double _value2;
        private readonly double _value3;

        private StructShape3(Tag tag, double value1, double value2, double value3)
        {
            _tag = tag;
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
        }

        public static StructShape3 CreateNothing() =>
            new StructShape3(Tag.Nothing, default, default, default);

        public static StructShape3 CreateSquare(double length) =>
            new StructShape3(Tag.Square, length, default, default);

        public static StructShape3 CreateCircle(double radius) =>
            new StructShape3(Tag.Circle, radius, default, default);

        public static StructShape3 CreateRectangle(double width, double height) =>
            new StructShape3(Tag.Rectangle, width, height, default);

        public static StructShape3 CreateTriangle(double lengthA, double lengthB, double lengthC) =>
            new StructShape3(Tag.Triangle, lengthA, lengthB, lengthC);

        public static StructShape3 Create(Nothing nothing) =>
            CreateNothing();

        public static StructShape3 Create(Square square) =>
            CreateSquare(square.Length);

        public static StructShape3 Create(Circle circle) =>
            CreateCircle(circle.Radius);

        public static StructShape3 Create(Rectangle rectangle) =>
            CreateRectangle(rectangle.Width, rectangle.Height);

        public bool IsNothing => _tag == Tag.Nothing;
        public bool IsSquare => _tag == Tag.Square;
        public bool IsCircle => _tag == Tag.Circle;
        public bool IsRectangle => _tag == Tag.Rectangle;
        public bool IsTriangle => _tag == Tag.Triangle;

        public record struct Nothing();
        public record struct Square(double Length);
        public record struct Circle(double Radius);
        public record struct Rectangle(double Width, double Height);
        public record struct Triangle(double LengthA, double LengthB, double LengthC);

        public Nothing ToNothing() =>
            IsNothing ? new Nothing() : throw new InvalidCastException();

        public Square ToSquare() =>
            IsSquare ? new Square(_value1) : throw new InvalidCastException();

        public Circle ToCircle() =>
            IsCircle ? new Circle(_value1) : throw new InvalidCastException();

        public Rectangle ToRectangle() =>
            IsRectangle ? new Rectangle(_value1, _value2) : throw new InvalidCastException();

        public Triangle ToTriangle() =>
            IsTriangle ? new Triangle(_value1, _value2, _value3) : throw new InvalidCastException();

        public bool TryGetSquare(out Square square)
        {
            if (IsSquare) { square = ToSquare(); return true; }
            square = default; 
            return false;
        }

        public bool TryGetCircle(out Circle circle)
        {
            if (IsCircle) { circle = ToCircle(); return true; };
            circle = default; 
            return false;
        }

        public bool TryGetRectangle(out Rectangle rectangle)
        {
            if (IsRectangle) { rectangle = ToRectangle(); return true; }
            rectangle = default;
            return false;
        }

        public bool TryGetTriangle(out Triangle triangle)
        {
            if (IsTriangle) { triangle = ToTriangle(); return true; }
            triangle = default;
            return false;
        }
    }
}
#endif