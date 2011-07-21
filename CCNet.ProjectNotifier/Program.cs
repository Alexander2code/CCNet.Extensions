﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CCNet.Common;
using CCNet.ProjectNotifier.Properties;
using QuickGraph;

namespace CCNet.ProjectNotifier
{
	/// <summary>
	/// Notifies other projects about successful build.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Main program.
		/// </summary>
		public static int Main(string[] args)
		{
			/*xxxargs = new[]
			{
				@"ProjectName=AggregatorService",
				@"RootPath=\\rufrt-vxbuild\e$\CCNET",
				@"ReferencesFolderName=References",
			};*/

			if (args == null || args.Length == 0)
			{
				DisplayUsage();
				return 0;
			}

			try
			{
				Arguments.Default = ArgumentProperties.Parse(args);
				PerformNotifications();
			}
			catch (Exception e)
			{
				return ErrorHandler.Runtime(e);
			}

			return 0;
		}

		/// <summary>
		/// Displays usage text.
		/// </summary>
		private static void DisplayUsage()
		{
			Console.WriteLine();
			Console.WriteLine(Resources.UsageInfo);
			Console.WriteLine();
		}

		#region Performing notifications

		/// <summary>
		/// Performs all notifications needed.
		/// </summary>
		private static void PerformNotifications()
		{
			var graph = new AdjacencyGraph<string, Edge<string>>();

			foreach (string projectFolder in Directory.GetDirectories(Arguments.RootPath))
			{
				string projectName = Path.GetFileName(projectFolder);
				string referencesDirectory = Path.Combine(projectFolder, Arguments.ReferencesFolderName);

				graph.AddVerticesAndEdgeRange(
					ReferenceMark.GetCurrent(ReferenceType.Internal, referencesDirectory)
					.Select(referenceName => new Edge<string>(referenceName, projectName)));

				graph.AddVerticesAndEdgeRange(
					ReferenceMark.GetCurrent(ReferenceType.External, referencesDirectory)
					.Select(referenceName => new Edge<string>(referenceName, projectName)));
			}

			GraphHelper.RemoveExplicitEdges(graph);

			List<string> projectsToNotify = new List<string>();
			IEnumerable<Edge<string>> outEdges;
			if (graph.TryGetOutEdges(Arguments.ProjectName, out outEdges))
			{
				projectsToNotify.AddRange(outEdges.Select(edge => edge.Target));
			}

			string fileName = ReferenceMark.GetReferenceMarkName(Arguments.ProjectName);
			foreach (string file in FindReferenceMarks(fileName))
			{
				string path = Path.GetDirectoryName(file);
				path = Path.GetDirectoryName(path);
				path = Path.GetDirectoryName(path);
				string projectName = Path.GetFileName(path);

				Console.WriteLine(
					Resources.LogReferencedBy,
					projectName);

				if (projectsToNotify.Contains(projectName))
				{
					ReferenceMark.MarkUpdatedFile(file);

					Console.WriteLine(
						Resources.LogTriggeredBuild,
						projectName);
				}
			}
		}

		/// <summary>
		/// Finds all reference mark files.
		/// </summary>
		private static List<string> FindReferenceMarks(string fileName)
		{
			return Directory.GetDirectories(Arguments.RootPath)
				.Select(dir => Path.Combine(dir, Arguments.ReferencesFolderName))
				.Where(Directory.Exists)
				.SelectMany(dir => Directory.GetFiles(dir, fileName, SearchOption.AllDirectories))
				.ToList();
		}

		#endregion
	}
}
