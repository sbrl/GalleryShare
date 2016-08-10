using System;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using System.Collections.Generic;

namespace GalleryShare
{
	public static class ThumbnailGenerator
	{
		public static async Task SendThumbnailPng(string imagePath, Size thumbnailBounds, HttpListenerContext cycle)
		{
			bool dummy;
			Image imageToSend;
			if (Directory.Exists(imagePath))
				imageToSend = GenerateDirectoryThumbnail(imagePath, thumbnailBounds);
			else
				imageToSend = GenerateFileThumbnail(imagePath, thumbnailBounds, out dummy); // Has error image generation built in
			
			byte[] imageData = GetImageBytesPng(imageToSend);

			cycle.Response.ContentType = "image/png";
			cycle.Response.ContentLength64 = imageData.LongLength;

			await cycle.Response.OutputStream.WriteAsync(imageData, 0, imageData.Length);

			imageToSend.Dispose();
		}

		public static Image GenerateDirectoryThumbnail(string dirPath, Size thumbnailSize)
		{
			List<string> dirFiles = new List<string>(Directory.GetFiles(dirPath));
			dirFiles.Sort( (a, b) => File.GetLastWriteTime(a).CompareTo(File.GetLastWriteTime(b)) );

			Bitmap resultImage = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);
			using(Graphics context = Graphics.FromImage(resultImage))
			{
				bool isErrorImage;
				int filesSkipped = 0;
				for(int i = 0; i < 4 + filesSkipped; i++)
				{
					using (Image fileThumbnail = GenerateFileThumbnail(
						dirFiles[i],
						new Size(thumbnailSize.Width / 2, thumbnailSize.Height / 2),
						out isErrorImage
					))
					{
						// If an error image has been generated, then we don't really want to
						// include it in the preview
						if(isErrorImage)
						{
							filesSkipped++;
							continue;
						}
						PointF drawingPos = getDirectoryImageDrawPosition(i - filesSkipped).Multiply(new PointF(resultImage.Width, resultImage.Height));
						context.DrawImage(fileThumbnail, drawingPos.ToIntPoint());
					}
				}
			}

			return resultImage;
		}

		public static Image GenerateFileThumbnail(string imagePath, Size thumbnailBounds, out bool errorImage)
		{
			errorImage = false;
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
						return smallImage;
					}
				}
			}
			catch (Exception error)
			{
				errorImage = true;
				Console.WriteLine("[{0}] Error generating thumbnail {1}: {2}",
					DateTime.Now.ToShortTimeString(),
					imagePath,
					error.Message
				);

				using (Font drawingFont = new Font("Sans-serif", 12f, FontStyle.Regular, GraphicsUnit.Pixel))
				{
					return GenerateErrorImage(error.Message, drawingFont, Color.DarkGray, Color.Transparent);
				}
			}
		}

		public static Image GenerateErrorImage(string errorText, Font textFont, Color foregroundColour, Color backgroundColour)
		{
			SizeF textSize = SizeF.Empty;
			using(Image measuringImage = new Bitmap(1, 1))
			using(Graphics measuringContext = Graphics.FromImage(measuringImage))
			{
				textSize = measuringContext.MeasureString(errorText, textFont);
			}

			float questionMarkFontSize = 200;
			Image resultImage = new Bitmap((int)textSize.Width, (int)textSize.Height + (int)questionMarkFontSize + (int)textFont.Size * 2);
			using (Graphics resultContext = Graphics.FromImage(resultImage))
			using (SolidBrush drawingBrush = new SolidBrush(foregroundColour))
			using (Font largeFont = new Font(textFont.FontFamily, questionMarkFontSize + 10, FontStyle.Regular, GraphicsUnit.Pixel))
			{
				resultContext.Clear(backgroundColour);
				resultContext.DrawString(errorText, textFont, drawingBrush, 0f, resultImage.Height - textFont.Size * 2);
				SizeF questionMarkSize = resultContext.MeasureString("?", largeFont);
				resultContext.DrawString(
					"?", largeFont, drawingBrush,
					(textSize.Width / 2) - (questionMarkSize.Width / 2),
					-10f
				);
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

		private static PointF getDirectoryImageDrawPosition(int id)
		{
			switch(id)
			{
				case 0: return new PointF(0, 0);
				case 1: return new PointF(0.5f, 0);
				case 2: return new PointF(0.5f, 0.5f);
				case 3: return new PointF(0, 0.5f);
				default: throw new InvalidDataException($"Invalid id #{id}.");
			}
		}
	}
}

