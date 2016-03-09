﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CCNet.Build.Common;

namespace CCNet.Build.SetupProject
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				Execute.DisplayUsage("Makes required changes to a project file and source code.", typeof(Args));
				return 0;
			}

			try
			{
				Args.Current = new ArgumentProperties(args);
				Execute.DisplayCurrent(typeof(Args));

				SetupProject();
			}
			catch (Exception e)
			{
				return Execute.RuntimeError(e);
			}

			return 0;
		}

		private static void SetupProject()
		{
			RenderLinks();
			UpdateAssemblyInfo();
		}

		private static void RenderLinks()
		{
			List<dynamic> links;

			switch (Args.ProjectType)
			{
				case ProjectType.Library:
					links = BuildLinksLibrary();
					break;

				case ProjectType.Website:
					links = BuildLinksWebsite();
					break;

				default:
					throw new InvalidOperationException(
						String.Format("Unknown project type '{0}'.", Args.ProjectType));
			}

			foreach (var link in links)
			{
				Console.WriteLine("[LINK] {0} | {1}", link.Url, link.Image);
			}
		}

		private static List<dynamic> BuildLinksLibrary()
		{
			return new List<dynamic>
			{
				new
				{
					Url = String.Format("{0}/packages/{1}/", Config.NuGetUrl, Args.ProjectName),
					Image = String.Format("{0}/Content/Logos/nugetlogo.png", Config.NuGetUrl)
				},
				new
				{
					Url = String.Format("https://owl.cbsi.com/confluence/display/CCSSEDRU/{0}+library", Args.ProjectName),
					Image = "https://owl.cbsi.com/images/confluence_logo_landing.png"
				}
			};
		}

		private static List<dynamic> BuildLinksWebsite()
		{
			return new List<dynamic>
			{
				new
				{
					Url = String.Format("https://owl.cbsi.com/confluence/display/CCSSEDRU/{0}+web+site", Args.ProjectName),
					Image = "https://owl.cbsi.com/images/confluence_logo_landing.png"
				}
			};
		}

		private static void UpdateAssemblyInfo()
		{
			Console.WriteLine("Updating assembly information... ");

			var version = new Version(Args.CurrentVersion).Normalize();
			var text = File.ReadAllText(Paths.AssemblyInfoFile);

			text = new Regex(@"^\[assembly: AssemblyVersion\(""[0-9\.?]+""\)]", RegexOptions.Multiline)
				.Replace(text, String.Format("[assembly: AssemblyVersion(\"{0}\")]", version));

			text = new Regex(@"^\[assembly: AssemblyFileVersion\(""[0-9\.?]+""\)]", RegexOptions.Multiline)
				.Replace(text, String.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version));

			File.WriteAllText(Paths.AssemblyInfoFile, text, Encoding.UTF8);
			Console.WriteLine("OK");
		}
	}
}
