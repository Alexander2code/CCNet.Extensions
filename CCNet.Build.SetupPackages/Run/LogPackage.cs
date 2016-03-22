﻿using System;
using System.Text;
using CCNet.Build.Common;

namespace CCNet.Build.SetupPackages
{
	public class LogPackage
	{
		public string Name { get; set; }
		public bool IsLocal { get; set; }
		public Version SourceVersion { get; set; }
		public Version BuildVersion { get; set; }
		public bool ProjectReference { get; set; }
		public bool PinnedToCurrent { get; set; }
		public Version PinnedToSpecific { get; set; }

		public string Location
		{
			get
			{
				return IsLocal ? "Local" : "Remote";
			}
		}

		public string Comment
		{
			get
			{
				var sb = new StringBuilder(Location);

				if (PinnedToCurrent)
				{
					sb.Append(", pinned to its current version");
				}
				else
				{
					if (PinnedToSpecific != null)
					{
						sb.AppendFormat(", pinned to version {0}", PinnedToSpecific.Normalize());
					}
				}

				return sb.ToString();
			}
		}

		public void Report()
		{
			if (SourceVersion == null)
				throw new InvalidOperationException(
					String.Format("Source version is missing for package '{0}'.", Name));

			if (BuildVersion == null)
				throw new InvalidOperationException(
					String.Format("Build version is missing for package '{0}'.", Name));

			var source = SourceVersion.Normalize().ToString();
			var build = BuildVersion.Normalize().ToString();

			if (ProjectReference)
			{
				source = source + " (csproj)";
			}

			Console.WriteLine(
				"[PACKAGE] {0} | {1} | {2} | {3}",
				Name,
				source,
				build,
				Comment);
		}
	}
}
