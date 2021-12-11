using Dependencies;
using Dependencies.ClrPh;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dependencies
{

	/// <summary>
	/// ImportContext : Describe an import module parsed from a PE.
	/// Only used during the dependency tree building phase
	/// </summary>
	public struct ImportContext
	{
		// Import "identifier" 
		public string ModuleName;

		// Return how the module was found (NOT_FOUND otherwise)
		public ModuleSearchStrategy ModuleLocation;

		// If found, set the filepath and parsed PE, otherwise it's null
		public string PeFilePath;
		public PE PeProperties;

		// Some imports are from api sets
		public bool IsApiSet;
		public string ApiSetModuleName;

		// module flag attributes
		public ModuleFlag Flags;
	}


	public sealed partial class DependencyWindow : TabViewItem
	{
		PE Pe;
		public string RootFolder;
		public string WorkingDirectory;
		string Filename;
		PhSymbolProvider SymPrv;
		SxsEntries SxsEntriesCache;
		ApiSetSchema ApiSetmapCache;
		ModulesCache ProcessedModulesCache;
		DisplayModuleInfo _SelectedModule;
		bool _DisplayWarning;

		public List<string> CustomSearchFolders;

		public DependencyWindow()
		{
			this.InitializeComponent();
		}
	}
}
