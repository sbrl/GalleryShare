using System;
using System.Drawing;

namespace GalleryShare
{
	public static class Extensions
	{
		public static PointF Multiply(this PointF a, PointF b)
		{
			return new PointF(
				a.X * b.X,
				a.Y * b.Y
			);
		}

		public static PointF Add(this PointF a, PointF b)
		{
			return new PointF(
				a.X + b.X,
				a.Y + b.Y
			);
		}

		public static Point ToIntPoint(this PointF source)
		{
			return new Point((int)source.X, (int)source.Y);
		}
	}
}

