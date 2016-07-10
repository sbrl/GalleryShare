using System;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GalleryShare
{
	public static class ThumbnailGenerator
	{
		public static void GenerateThumbnailPng(string imagePath, Size thumbnailBounds, Stream outputStream)
		{
			try {
				using (Bitmap rawImage = new Bitmap(imagePath)) {
					float scaleFactor = Math.Min(
						thumbnailBounds.Width / (float)rawImage.Width,
						thumbnailBounds.Height / (float)rawImage.Height
					);
					Size thumbnailSize = new Size(
						(int)(rawImage.Width * scaleFactor),
						(int)(rawImage.Height * scaleFactor)
					);
					
					using (Bitmap smallImage = new Bitmap(thumbnailSize.Width, thumbnailSize.Height))
					using (Graphics context = Graphics.FromImage(smallImage)) {
						context.CompositingMode = CompositingMode.SourceCopy;
						context.InterpolationMode = InterpolationMode.HighQualityBicubic;
						context.DrawImage(rawImage, new Rectangle(
							Point.Empty,
							thumbnailSize
						));
						
						smallImage.Save(outputStream, ImageFormat.Png);
					}
				}
			}
			catch (Exception error)
			{
				throw new NotImplementedException("Error images haven't been implemented yet.", error);
			}
		}
	}
}

