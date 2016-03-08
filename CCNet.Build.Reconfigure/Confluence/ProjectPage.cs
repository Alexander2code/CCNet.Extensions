﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CCNet.Build.Common;
using CCNet.Build.Confluence;
using CCNet.Build.Tfs;

namespace CCNet.Build.Reconfigure
{
	public class ProjectPage<T> : IPageBuilder
		where T : ProjectProperties, new()
	{
		public string ProjectName { get; set; }
		public T Properties { get; set; }

		public XElement Stats { get; set; }
		public XElement History { get; set; }
		public XElement About { get; set; }

		public ProjectPage(string projectName, PageDocument document)
		{
			if (String.IsNullOrEmpty(projectName))
				throw new ArgumentNullException("projectName");

			if (document == null)
				throw new ArgumentNullException("document");

			ProjectName = projectName;

			var page = document.Root;
			var map = ExtractProperties(page);

			Properties = new T();
			Properties.ParseTable(map);

			Stats = ExtractStats(page);
			History = ExtractHistory(page);
			About = ExtractAbout(page);
		}

		public virtual string ProjectFile
		{
			get
			{
				var fileName = String.Format("{0}.csproj", ProjectName);
				return String.Format("{0}/{1}", Properties.TfsPath, fileName);
			}
		}

		private static Dictionary<string, string> ExtractProperties(XElement page)
		{
			var table = page.XElement("table");
			if (table == null)
				throw new InvalidOperationException("Cannot locate properties table.");

			var map = new Dictionary<string, string>();
			foreach (var tr in table.XElements("tbody/tr"))
			{
				var columns = tr.Elements().ToList();
				if (columns.Count < 2)
					throw new InvalidOperationException("Properties table should contain at least 2 columns.");

				var th = columns[0];
				var td = columns[1];

				var key = th.XValue();
				var value = td.XValue();

				var code = td.XElement("code");
				if (code != null)
					value = code.XValue();

				var status = td.XElement("ac:structured-macro[@ac:name='status']/ac:parameter[@ac:name='title']");
				if (status != null)
					value = status.XValue();

				var user = td.XElements("ac:link/ri:user").Select(e => e.XAttribute("ri:userkey").Value).FirstOrDefault();
				if (user != null)
					value = user;

				map[key] = value;
			}

			if (map.Count == 0)
				throw new InvalidOperationException("Cannot locate any rows in properties.");

			return map;
		}

		private static List<XElement> ExtractSections(XElement page)
		{
			return page.XElements("ac:structured-macro[@ac:name='section']").ToList();
		}

		private static List<XElement> ExtractColumns(XElement section)
		{
			return section.XElements("ac:rich-text-body/ac:structured-macro[@ac:name='column']").ToList();
		}

		private static XElement ExtractFromDetails(XElement page, int index)
		{
			var sections = ExtractSections(page);
			if (sections.Count != 2)
				return null;

			var details = sections[0];
			var columns = ExtractColumns(details);
			if (columns.Count != 3)
				return null;

			var stats = columns[index].XElements("ac:rich-text-body").FirstOrDefault();
			if (stats == null)
				return null;

			return stats;
		}

		private static XElement ExtractStats(XElement page)
		{
			return ExtractFromDetails(page, 0);
		}

		private static XElement ExtractHistory(XElement page)
		{
			return ExtractFromDetails(page, 2);
		}

		private static XElement ExtractAbout(XElement page)
		{
			var sections = ExtractSections(page);
			if (sections.Count != 2)
				return null;

			var about = sections[1].XElements("ac:rich-text-body").FirstOrDefault();
			if (about == null)
				return null;

			return about;
		}

		private XElement BuildLinks()
		{
			var title = new XElement("p", new XElement("code", new XElement("u", "ʟɪɴᴋs")));

			return PageDocument.BuildBody(
				title,
				new XElement(
					"p",
					new XElement(
						"a",
						new XAttribute("href", String.Format("{0}/packages/{1}/", Config.NuGetUrl, ProjectName)),
						PageDocument.BuildImage(String.Format("{0}/favicon.ico", Config.NuGetUrl)),
						"$nbsp$NuGet package")),
				new XElement(
					"p",
					new XElement(
						"a",
						new XAttribute("href", String.Format("{0}/server/{1}/project/{2}/ViewProjectReport.aspx", Config.CCNetUrl, Properties.Type, ProjectName)),
						PageDocument.BuildImage(String.Format("{0}/favicon.ico", Config.CCNetUrl)),
						String.Empty,
						"$nbsp$Build project")));
		}

		private XElement BuildStats()
		{
			var title = new XElement("p", new XElement("code", new XElement("u", "sᴛᴀᴛs")));

			if (Stats == null || !Stats.Elements().Any())
			{
				return PageDocument.BuildBody(
					title,
					new XElement("p", new XElement("i", "not available yet")));
			}

			Stats.Elements().First().Remove();
			Stats.AddFirst(title);

			return Stats;
		}

		private XElement BuildHistory()
		{
			var title = new XElement("p", new XElement("code", new XElement("u", "ʜɪsᴛᴏʀʏ")));

			if (History == null || !History.Elements().Any())
			{
				return PageDocument.BuildBody(
					title,
					new XElement("p", new XElement("i", "not available yet")));
			}

			History.Elements().First().Remove();
			History.AddFirst(title);

			return History;
		}

		private XElement BuildDetails()
		{
			return PageDocument.BuildSection(
				PageDocument.BuildBody(
					PageDocument.BuildColumn("300px", BuildStats()),
					PageDocument.BuildColumn("200px", BuildLinks()),
					PageDocument.BuildColumn(null, BuildHistory())));
		}

		private XElement BuildAbout()
		{
			if (About == null)
			{
				About = PageDocument.BuildBody(new XElement("p", "..."));
			}

			return PageDocument.BuildSection(About);
		}

		private XElement BuildMore()
		{
			return PageDocument.BuildInfo(
				PageDocument.BuildBody(
					new XElement(
						"p",
						"Знаешь об этом компоненте что-то еще? Пожалуйста напиши! (в произвольной форме, после заголовка About)",
						new XElement("br"),
						"Например цели создания, какие функции выполняет, может быть какие-то неочевидные особенности или решения по дизайну или структуре классов, и$nbsp$т.$nbsp$п.")));
		}

		public PageDocument BuildPage()
		{
			var page = new PageDocument();

			//var toc = new XElement("p", PageDocument.BuildToc());
			var properties = Properties.Build();
			var details = BuildDetails();
			var h2 = new XElement("h2", "About");
			var about = BuildAbout();
			var h4 = new XElement("h4", "Please contribute...");
			var more = BuildMore();

			//page.Root.Add(toc);
			page.Root.Add(properties);
			page.Root.Add(details);
			page.Root.Add(h2);
			page.Root.Add(about);
			page.Root.Add(h4);
			page.Root.Add(more);

			return page;
		}

		public void CheckPage(string areaName, TfsClient client)
		{
			var path = Properties.TfsPath;

			if (!path.Contains(String.Format("/{0}/", areaName)))
				throw new InvalidOperationException(
					String.Format("TFS path '{0}' seems not conforming with area name '{1}'.", path, areaName));

			if (!path.EndsWith(String.Format("/{0}", ProjectName)))
				throw new InvalidOperationException(
					String.Format("TFS path '{0}' seems not conforming with project name '{1}'.", path, ProjectName));

			var xml = client.ReadFile(ProjectFile);
			var project = new ProjectDocument();
			project.Load(xml);

			Properties.ProjectUid = project.GetProjectGuid();
		}
	}
}
