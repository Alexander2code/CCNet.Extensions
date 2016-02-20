﻿using System;
using System.Text;
using System.Xml;
using CCNet.Build.Common;

namespace CCNet.Build.Reconfigure
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				Execute.DisplayUsage("Generates .config files for all CCNet build servers.", typeof(Args));
				return 0;
			}

			try
			{
				Args.Current = new ArgumentProperties(args);
				Execute.DisplayCurrent(typeof(Args));

				Reconfigure();
			}
			catch (Exception e)
			{
				return Execute.RuntimeError(e);
			}

			return 0;
		}

		private static void Reconfigure()
		{
			BuildLibraryConfig();
		}

		private static XmlWriter WriteConfig(string filePath)
		{
			return new XmlTextWriter(filePath, Encoding.UTF8)
			{
				Formatting = Formatting.Indented,
				IndentChar = '\t',
				Indentation = 1
			};
		}

		private static void BuildLibraryConfig()
		{
			Console.WriteLine("Generate library config...");
			Console.WriteLine("Output file: {0}", Paths.LibraryConfig);

			using (var writer = WriteConfig(Paths.LibraryConfig))
			{
				writer.Begin();

				writer.Comment("SERVER NAME");
				writer.CbTag("define", "serverName", "Library");

				writer.Comment("IMPORT GLOBAL");
				writer.CbTag("include", "href", "Global.config");

				WriteLibraryProject(
					writer,
					new LibraryProjectConfiguration
					{
						Name = "V3.Storage",
						Description = "Client library and value templates for V3 storage",
						Category = "ContentCast",
						TfsPath = "$/Main/ContentCast/V3/V3.Storage",
						Framework = TargetFramework.Net45
					});

				WriteLibraryProject(
					writer,
					new LibraryProjectConfiguration
					{
						Branch = "Test",
						Name = "V3.Storage",
						Description = "Client library and value templates for V3 storage",
						Category = "ContentCast",
						TfsPath = "$/Main/ContentCast/V3/V3.Storage",
						Framework = TargetFramework.Net45
					});

				WriteLibraryProject(
					writer,
					new LibraryProjectConfiguration
					{
						Name = "CC.Showcase",
						Description = "Client library for Showcase DB",
						Category = "ContentCast",
						TfsPath = "$/Main/ContentCast/Showcase/CC.Showcase",
						Framework = TargetFramework.Net45
					});

				WriteLibraryProject(
					writer,
					new LibraryProjectConfiguration
					{
						Name = "Lean.ResourceLocators",
						Description = "Some library from Sergey",
						Category = "Sandbox",
						TfsPath = "$/Sandbox/skolemasov/Lean/Lean.ResourceLocators",
						Framework = TargetFramework.Net45
					});

				WriteLibraryProject(
					writer,
					new LibraryProjectConfiguration
					{
						Name = "Lean.Serialization",
						Description = "Some library from Sergey",
						Category = "Sandbox",
						TfsPath = "$/Sandbox/skolemasov/Lean/Lean.Serialization",
						Framework = TargetFramework.Net45
					});

				writer.End();
			}
		}

		private static void WriteLibraryProject(XmlWriter writer, LibraryProjectConfiguration project)
		{
			writer.Comment(String.Format("PROJECT: {0}", project.UniqueName));

			using (writer.OpenTag("project"))
			{
				writer.WriteElementString("name", project.UniqueName);
				writer.WriteElementString("description", project.Description);
				writer.WriteElementString("queue", project.Category);
				writer.WriteElementString("category", project.Category);

				writer.WriteElementString("workingDirectory", project.WorkingDirectory);
				writer.WriteElementString("artifactDirectory", project.WorkingDirectory);
				using (writer.OpenTag("state"))
				{
					writer.WriteAttributeString("type", "state");
					writer.WriteAttributeString("directory", project.WorkingDirectory);
				}

				writer.WriteElementString("webURL", project.WebUrl);

				using (writer.OpenTag("sourcecontrol"))
				{
					writer.WriteAttributeString("type", "multi");
					using (writer.OpenTag("sourceControls"))
					{
						using (writer.OpenTag("filesystem"))
						{
							writer.WriteElementString("repositoryRoot", project.WorkingDirectoryReferences);
							writer.WriteElementString("autoGetSource", "false");
							writer.WriteElementString("ignoreMissingRoot", "true");
						}

						using (writer.OpenTag("vsts"))
						{
							writer.WriteElementString("executable", "$(tfsExecutable)");
							writer.WriteElementString("server", "$(tfsUrl)");
							writer.WriteElementString("project", project.TfsPath);
							writer.WriteElementString("workingDirectory", project.WorkingDirectorySource);
							writer.WriteElementString("applyLabel", "false");
							writer.WriteElementString("autoGetSource", "true");
							writer.WriteElementString("cleanCopy", "true");
							writer.WriteElementString("deleteWorkspace", "true");
						}
					}
				}

				writer.Tag("labeller", "type", "shortDateLabeller");

				using (writer.OpenTag("triggers"))
				{
					writer.Tag("intervalTrigger", "name", "source or references", "seconds", "30", "buildCondition", "IfModificationExists", "initialSeconds", "5");
				}

				using (writer.OpenTag("prebuild"))
				{
					CleanupLibraryProject(writer, project);
				}

				using (writer.OpenTag("tasks"))
				{
					using (writer.OpenTag("exec"))
					{
						writer.WriteElementString("executable", "$(ccnetBuildSetupPackages)");
						writer.WriteElementString(
							"buildArgs",
							String.Format(
								@"
					""ProjectName={0}""
					""ProjectPath={1}""
					""PackagesPath={2}""
					""NuGetExecutable=$(nugetExecutable)""
					""NuGetUrl={3}""
				",
								project.Name,
								project.WorkingDirectorySource,
								project.WorkingDirectoryPackages,
								project.NugetRestoreUrl));

						writer.WriteElementString("description", "Setup packages");
					}

					using (writer.OpenTag("msbuild"))
					{
						writer.WriteElementString("executable", project.MsbuildExecutable);
						writer.WriteElementString("targets", "Build");
						writer.WriteElementString("workingDirectory", project.WorkingDirectorySource);
						writer.WriteElementString("buildArgs", String.Format(@"/noconsolelogger /p:Configuration=Release;OutDir={0}\", project.WorkingDirectoryRelease));
						writer.WriteElementString("description", "Build library");
					}

					using (writer.OpenTag("exec"))
					{
						writer.WriteElementString("executable", "$(ccnetBuildGenerateNuspec)");
						writer.WriteElementString(
							"buildArgs",
							String.Format(
								@"
					""PackageType=Library""
					""ProjectName={0}""
					""ProjectDescription={1}""
					""CompanyName=CNET Content Solutions""
					""CurrentVersion=$[$CCNetLabel]""
					""TargetFramework={2}""
					""OutputDirectory={3}""
					""IncludeXmlDocumentation={4}""
				",
								project.Name,
								project.Description,
								project.Framework,
								project.WorkingDirectoryNuget,
								project.IncludeXmlDocumentation));

						writer.WriteElementString("description", "Generate nuspec file");
					}

					using (writer.OpenTag("exec"))
					{
						writer.WriteElementString("executable", "$(nugetExecutable)");
						writer.WriteElementString(
							"buildArgs",
							String.Format(
								@"pack ""{0}\{1}.nuspec"" -OutputDirectory ""{0}"" -NonInteractive -Verbosity Detailed",
								project.WorkingDirectoryNuget,
								project.Name));

						writer.WriteElementString("description", "Build package");
					}

					using (writer.OpenTag("exec"))
					{
						writer.WriteElementString("executable", "$(nugetExecutable)");
						writer.WriteElementString(
							"buildArgs",
							String.Format(
								@"push ""{0}\{1}.$[$CCNetLabel].nupkg"" -Source ""{2}"" -NonInteractive -Verbosity Detailed",
								project.WorkingDirectoryNuget,
								project.Name,
								project.NugetPushUrl));

						writer.WriteElementString("description", "Publish package");
					}
				}

				using (writer.OpenTag("publishers"))
				{
					writer.Tag("modificationHistory", "onlyLogWhenChangesFound", "true");
					writer.Tag("xmllogger");
					writer.Tag("statistics");
					writer.Tag("artifactcleanup", "cleanUpMethod", "KeepLastXBuilds", "cleanUpValue", "100");
					writer.Tag("artifactcleanup", "cleanUpMethod", "KeepMaximumXHistoryDataEntries", "cleanUpValue", "100");

					CleanupLibraryProject(writer, project);
				}
			}
		}

		private static void CleanupLibraryProject(XmlWriter writer, LibraryProjectConfiguration project)
		{
			writer.CbTag("DeleteDirectory", "path", project.WorkingDirectorySource);
			writer.CbTag("DeleteDirectory", "path", project.WorkingDirectoryRelease);
			writer.CbTag("DeleteDirectory", "path", project.WorkingDirectoryPackages);
			writer.CbTag("DeleteDirectory", "path", project.WorkingDirectoryNuget);
		}
	}
}
