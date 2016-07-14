using System;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Net;

namespace GalleryShare
{
	public static class ThumbnailGenerator
	{
		public static async Task SendThumbnailPng(string imagePath, Size thumbnailBounds, HttpListenerContext cycle)
		{
			Image imageToSend;
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
					
					Bitmap smallImage = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);
					using (Graphics context = Graphics.FromImage(smallImage)) {
						context.CompositingMode = CompositingMode.SourceCopy;
						context.InterpolationMode = InterpolationMode.HighQualityBicubic;
						context.DrawImage(rawImage, new Rectangle(
							Point.Empty,
							thumbnailSize
						));
						
						//smallImage.Save(outputStream, ImageFormat.Png);
						imageToSend = smallImage;
					}
				}
			}
			catch (Exception error)
			{
				Console.WriteLine("[{0}] Error generating thumbnail {1}: {2}",
					DateTime.Now.ToShortTimeString(),
					imagePath,
					error.Message
				);

				using (Font drawingFont = new Font("Sans-serif", 18f, FontStyle.Regular, GraphicsUnit.Pixel))
				{
					imageToSend = GenerateErrorImage(error.Message, drawingFont, Color.DarkGray, Color.Transparent);
				}
			}

			byte[] imageData = GetImageBytesPng(imageToSend);

			cycle.Response.ContentType = "image/png";
			cycle.Response.ContentLength64 = imageData.LongLength;

			await cycle.Response.OutputStream.WriteAsync(imageData, 0, imageData.Length);

			imageToSend.Dispose();
		}

		public static Image GenerateErrorImage(string errorText, Font textFont, Color foregroundColour, Color backgroundColour)
		{
			SizeF textSize = SizeF.Empty;
			using(Image measuringImage = new Bitmap(1, 1))
			using(Graphics measuringContext = Graphics.FromImage(measuringImage))
			{
				textSize = measuringContext.MeasureString(errorText, textFont);
			}

			Image resultImage = new Bitmap((int)textSize.Width, (int)textSize.Height);
			using (Graphics resultContext = Graphics.FromImage(resultImage))
			using (SolidBrush drawingBrush = new SolidBrush(foregroundColour))
			{
				resultContext.Clear(backgroundColour);
				resultContext.DrawString(errorText, textFont, drawingBrush, 0f, 0f);
			}
			return resultImage;
		}

		public static byte[] GetImageBytesPng(Image sourceImage)
		{
			using (MemoryStream mStream = new MemoryStream())
			{
				sourceImage.Save(mStream, ImageFormat.Png);
				return mStream.ToArray();
			}
		}
	}
}

