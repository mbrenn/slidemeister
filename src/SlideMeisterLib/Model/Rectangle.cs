using System.Security.Cryptography.X509Certificates;

namespace SlideMeisterLib.Model
{
    public class Rectangle
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public override string ToString()
        {
            return $"{X}, {Y} ({Width}, {Height})";
        }
    }
}