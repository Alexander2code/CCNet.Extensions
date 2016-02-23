﻿using System;
using System.Xml.Linq;

namespace CCNet.Build.Common
{
	/// <summary>
	/// Represents element from project XML document.
	/// </summary>
	public abstract class ProjectElement
	{
		/// <summary>
		/// Standard namespace for a project XML document.
		/// </summary>
		protected readonly XNamespace m_ns;

		/// <summary>
		/// Curent XML element.
		/// </summary>
		protected readonly XElement m_element;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		protected ProjectElement(XElement element)
		{
			m_ns = "http://schemas.microsoft.com/developer/msbuild/2003";

			if (element == null)
				throw new ArgumentNullException("element");

			m_element = element;

			Reload();
		}

		/// <summary>
		/// Reloads all the inner properties.
		/// </summary>
		protected abstract void Reload();
	}
}
