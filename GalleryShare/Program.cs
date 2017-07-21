using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace GalleryShare
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			string directory = Environment.CurrentDirectory;
			int port = 3333;
			List<string> extras = new List<string>();
			for(int i = 0; i < args.Length; i++)
			{
				if (!args[i].StartsWith("-"))
				{
					extras.Add(args[i]);
					continue;
				}

				string trimmedArg = args[i].Trim('-');
				switch (trimmedArg)
				{
					case "port":
					case "p":
						port = int.Parse(args[++i]);
						break;
					
					case "d":
					case "directory":
						directory = args[++i];
						break;

					case "h":
					case "help":
						Console.WriteLine("GalleryShare");
						Console.WriteLine("by Starbeamrainbowlabs, find out more at https://github.com/sbrl/GalleryShare");
						Console.WriteLine();
						Console.WriteLine("Usage: ");
						Console.WriteLine("    ./GalleryShare.exe [options]");
						Console.WriteLine();
						Console.WriteLine("Options: ");
						Console.WriteLine("    -h  --help                        Show this message");
						Console.WriteLine("    -d  --directory {directory_path}  Specify the directory to serve files from. Defaults tot he current directory.");
						Console.WriteLine("    -p  --port {port_number}          Specify the port to listen on. Default: 3333.");
						Console.WriteLine();
						return 0;
					
					default:
						Console.Error.WriteLine("Error: Unknown argument '{0}'.", args[i]);
						Console.Error.WriteLine("Try ./GalleryShare --help to learn how to use GalleryShare.");
						return 1;
				}
			}

			GalleryServer gserver = new GalleryServer(directory, port);
			try
			{
				gserver.StartSync();
			}
			catch(AggregateException agError)
			{
				agError.Handle((error) => {
					ExceptionDispatchInfo.Capture(error).Throw();
					throw error;
				});
			}
			catch
			{
				Console.Error.WriteLine("Something went very wrong O.o");
				throw;
			}

			return 255;
		}
	}
}
