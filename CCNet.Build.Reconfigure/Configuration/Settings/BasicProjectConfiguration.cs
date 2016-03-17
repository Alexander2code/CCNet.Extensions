﻿using System;
using System.Collections.Generic;
using System.Linq;
using CCNet.Build.Common;

namespace CCNet.Build.Reconfigure
{
	public abstract class BasicProjectConfiguration : ProjectConfiguration
	{
		public TargetFramework Framework { get; set; }
		public DocumentationType Documentation { get; set; }
		public string RootNamespace { get; set; }
		public string CustomVersions { get; set; }

		public string MsbuildExecutable
		{
			get
			{
				//return @"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe";
				return @"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe";
				switch (Framework)
				{
					case TargetFramework.Net20:
					case TargetFramework.Net35:
					case TargetFramework.Net40:
					case TargetFramework.Net45:
						return @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe";

					default:
						throw new InvalidOperationException(
							String.Format("Unknown target framework '{0}'.", Framework));
				}
			}
		}

		public string NugetRestoreUrl
		{
			get
			{
				if (String.IsNullOrEmpty(Branch))
					return "$(nugetUrl)/api/v2";

				return String.Format("$(nugetUrl)/private/{0}/api/v2", Branch);
			}
		}

		public bool IncludeXmlDocumentation
		{
			get { return Documentation != DocumentationType.None; }
		}

		public string CheckIssues
		{
			get { return String.Join("|", GetIssuesToCheck().Where(code => code != null)); }
		}

		private string ProjectTargetFrameworkIssue
		{
			get
			{
				switch (Framework)
				{
					case TargetFramework.Net20:
						return "P10"; // ProjectTargetFramework20

					case TargetFramework.Net35:
						return "P11"; // ProjectTargetFramework35

					case TargetFramework.Net40:
						return "P12"; // ProjectTargetFramework40

					case TargetFramework.Net45:
						return "P13"; // ProjectTargetFramework45

					default:
						throw new InvalidOperationException(
							String.Format("Unknown target framework '{0}'.", Framework));
				}
			}
		}

		private string DocumentationIssue
		{
			get
			{
				switch (Documentation)
				{
					case DocumentationType.None:
						return "P15"; // ProjectDocumentationNone

					case DocumentationType.Partial:
						return "P16"; // ProjectDocumentationPartial

					case DocumentationType.Full:
						return "P17"; // ProjectDocumentationFull

					default:
						throw new InvalidOperationException(
							String.Format("Unknown documentation type '{0}'.", Documentation));
				}
			}
		}

		protected virtual List<string> GetIssuesToCheck()
		{
			var checks = new List<string>();

			// file structure
			checks.AddRange(new[]
			{
				"F01", // ProjectFolderShouldHaveProjectName
				"F02", // ProjectFileShouldExist
				"F03", // AssemblyInfoShouldExist
				"F04", // PrimarySolutionShouldExist
				"F05", // NugetConfigShouldExist
				"F06", // PackagesFolderShouldNotHavePackages
				"F07", // LocalFilesShouldMatchProjectFiles
				null
			});

			// file contents
			checks.AddRange(new[]
			{
				"C01", // CheckAssemblyInfo
				"C02", // CheckPrimarySolution
				"C03", // CheckNugetConfig
				null
			});

			// project properties
			checks.AddRange(new[]
			{
				"P01", // CheckProjectConfigurations
				"P02", // CheckProjectPlatforms
				"P03", // CheckProjectSourceControl
				"P07", // CheckProjectAssemblyName
				"P08", // CheckProjectRootNamespace
				"P09", // CheckProjectStartupObject
				"P14", // CheckProjectCompilation
				null
			});

			checks.Add("P04"); // ProjectOutputTypeLibrary

			checks.Add(ProjectTargetFrameworkIssue);
			checks.Add(DocumentationIssue);

			return checks;
		}
	}
}
