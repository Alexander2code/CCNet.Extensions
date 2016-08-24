﻿using System;

namespace CCNet.Build.Reconfigure
{
	public class ConsoleProjectConfiguration2 :
		IReferencesDirectory,
		ITfsControl,
		IPackagesDirectory
	{
		public string Area { get; set; }
		public string Name { get; set; }
		public string Branch { get; set; }
		public string Description { get; set; }
		public string OwnerEmail { get; set; }
		public TimeSpan CheckEvery { get; set; }

		public string TfsPath { get; set; }
		public string CustomIssues { get; set; }

		public string Server => "Application";
	}
}
