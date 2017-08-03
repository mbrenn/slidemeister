using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace SlideMeisterLib.Model
{
    /// <summary>
    /// Defines the possible units for the values
    /// </summary>
    public enum Units
    {
        /// <summary>
        /// Given as relative (0.0 to 1.0)
        /// </summary>
        Percentage,

        /// <summary>
        /// Given as absolute pixels
        /// </summary>
        Pixel
    }

    /// <summary>
    /// Defines the value including 
    /// </summary>
    public struct DoubleWithUnit
    {
        /// <summary>
        /// Defines the unit of the value
        /// </summary>
        public Units Unit;

        /// <summary>
        /// Defines the value itself
        /// </summary>
        public double Value;

        public override string ToString()
        {
            switch (Unit)
            {
                case Units.Percentage:
                    return $"{Value}%";
                case Units.Pixel:
                    return $"{Value}px";
                default:
                    return Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public DoubleWithUnit(double value, Units unit)
        {
            Unit = unit;
            Value = value;
        }

        public static DoubleWithUnit ToPercentage(double value)
        {
            return new DoubleWithUnit(value, Units.Percentage);
        }

        public static DoubleWithUnit ToPixel(double value)
        {
            return new DoubleWithUnit(value, Units.Pixel);
        }
    }

    public class Rectangle
    {
        public DoubleWithUnit X { get; set; }

        public DoubleWithUnit Y { get; set; }

        public DoubleWithUnit Width { get; set; }

        public DoubleWithUnit Height { get; set; }

        public override string ToString()
        {
            return $"{X}, {Y} ({Width}, {Height})";
        }

        public Rectangle()
        {
                
        }

        public Rectangle(DoubleWithUnit x, DoubleWithUnit y, DoubleWithUnit width, DoubleWithUnit height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(double x, double y, double width, double height)
        {
            X = new DoubleWithUnit(x, Units.Percentage);
            Y = new DoubleWithUnit(y, Units.Percentage);
            Width = new DoubleWithUnit(width, Units.Percentage);
            Height = new DoubleWithUnit(height, Units.Percentage);
        }
    }
}