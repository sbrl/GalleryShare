using System;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Net.Mime;

namespace GalleryShare
{
	public class Utilities
	{
		private Utilities()
		{
		}
		
		/// <summary>
		/// Call this method to allow a given task to complete in the background.
		/// Errors will be handled correctly.
		/// Useful in fire-and-forget scenarios, like a TCP server for example.
		/// From http://stackoverflow.com/a/22864616/1460422
		/// </summary>
		/// <param name="task">The task to forget about.</param>
		/// <param name="acceptableExceptions">Acceptable exceptions. Exceptions specified here won't cause a crash.</param>
		public static async void ForgetTask(Task task, params Type[] acceptableExceptions)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				// TODO: consider whether derived types are also acceptable.
				if (!acceptableExceptions.Contains(ex.GetType()))
					throw;
			}
		}

		public static async Task<byte[]> GetEmbeddedResourceContent(string resourceName)
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			Stream stream = asm.GetManifestResourceStream(resourceName);
			MemoryStream ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			byte[] embeddedContent = ms.ToArray();
			ms.Dispose();
			stream.Dispose();
			return embeddedContent;
		}
	}
}

