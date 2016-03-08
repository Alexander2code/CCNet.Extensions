﻿using System;
using System.Xml.Linq;
using CCNet.Build.Common;
using CCNet.Build.Confluence;

namespace CCNet.Build.Reconfigure
{
	public partial class ProjectPage
	{
		public static XElement BuildStatus(ProjectStatus status)
		{
			PageDocument.StatusColor color;

			switch (status)
			{
				case ProjectStatus.Active:
					color = PageDocument.StatusColor.Green;
					break;

				case ProjectStatus.Temporary:
					color = PageDocument.StatusColor.Yellow;
					break;

				case ProjectStatus.Legacy:
					color = PageDocument.StatusColor.Red;
					break;

				default:
					color = PageDocument.StatusColor.Grey;
					break;
			}

			return PageDocument.BuildStatus(status.ToString(), color, false);
		}

		public static XElement BuildOwner(string userUid)
		{
			if (String.IsNullOrEmpty(userUid))
				return PageDocument.BuildStatus("none", PageDocument.StatusColor.Grey, false);

			return PageDocument.BuildUserLink(userUid);
		}

		public static string DisplayFramework(TargetFramework framework)
		{
			switch (framework)
			{
				case TargetFramework.Net20:
					return "2.0";

				case TargetFramework.Net35:
					return "3.5";

				case TargetFramework.Net40:
					return "4.0";

				case TargetFramework.Net45:
					return "4.5";

				default:
					throw new InvalidOperationException(
						String.Format("Unknown target framework '{0}'.", framework));
			}
		}

		public static XElement BuildFramework(TargetFramework framework)
		{
			var text = DisplayFramework(framework);
			return PageDocument.BuildStatus(text, PageDocument.StatusColor.Blue, true);
		}

		public static XElement BuildDocumentation(DocumentationType documentation)
		{
			PageDocument.StatusColor color;

			switch (documentation)
			{
				case DocumentationType.Full:
					color = PageDocument.StatusColor.Green;
					break;

				case DocumentationType.Partial:
					color = PageDocument.StatusColor.Yellow;
					break;

				default:
					color = PageDocument.StatusColor.Grey;
					break;
			}

			return PageDocument.BuildStatus(documentation.ToString(), color, true);
		}

		public static object[] BuildExplain(string anchor, XElement element)
		{
			return new object[]
			{
				element,
				"$nbsp$",
				new XElement(
					"sup",
					PageDocument.BuildPageLink("Projects FAQ", "explain?", anchor))
			};
		}

		public static XElement BuildNotAvailable()
		{
			return new XElement("p", new XElement("i", "not available yet"));
		}
	}
}
