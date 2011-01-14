﻿using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace CCNet.Common.Test.Services2
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] servicesToRun = new ServiceBase[] 
			{
				new WindowsService2A(),
				new WindowsService2B()
			};
			ServiceBase.Run(servicesToRun);
		}
	}

	/// <summary>
	/// Installer of two services in one binary assembly.
	/// </summary>
	[RunInstaller(true)]
	public class WindowsService2Installer : Installer
	{
		public WindowsService2Installer()
		{
			ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
			Installers.Add(processInstaller);

			processInstaller.Account = ServiceAccount.LocalService;

			ServiceInstaller installerA = new ServiceInstaller();
			installerA.ServiceName = "WindowsService2a";
			installerA.DisplayName = "[Test] Windows Service #2-A";
			Installers.Add(installerA);


			ServiceInstaller installerB = new ServiceInstaller();
			installerB.ServiceName = "WindowsService2b";
			installerB.DisplayName = "[Test] Windows Service #2-B";
			Installers.Add(installerB);
		}
	}

	/// <summary>
	/// Test service A.
	/// </summary>
	public class WindowsService2A : ServiceBase
	{
		private readonly IContainer components;

		public WindowsService2A()
		{
			ServiceName = "WindowsService2a";
			components = new Container();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	/// <summary>
	/// Test service B.
	/// </summary>
	public class WindowsService2B : ServiceBase
	{
		private readonly IContainer components;

		public WindowsService2B()
		{
			ServiceName = "WindowsService2b";
			components = new Container();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
