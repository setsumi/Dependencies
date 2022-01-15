using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dependencies
{
	public class SearchFolderItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string Folder
		{
			get { return folder; }
			set { folder = value; OnPropertyChanged(); }
		}

		private string folder;
	}

	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SearchFolder : Page, INotifyPropertyChanged
	{
		public SearchFolder(DependencyWindow SelectedItem)
		{
			_SelectedItem = SelectedItem;
			_working_directory = SelectedItem.RootFolder;
			_CustomSearchFolders = new ObservableCollection<SearchFolderItem>();
			foreach (var item in SelectedItem.CustomSearchFolders)
			{
				_CustomSearchFolders.Add(
					new SearchFolderItem()
					{
						Folder = item
					}
				);
			}

			_CustomSearchFolders.Add(
					new SearchFolderItem()
					{
						Folder = "Test folder"
					}
				);
			_CustomSearchFolders.Add(
					new SearchFolderItem()
					{
						Folder = "Test folder 2"
					}
				);
			this.InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ObservableCollection<SearchFolderItem> SearchFolders => _CustomSearchFolders;
	
		public string WorkingDirectory
		{
			get { return _working_directory; }
			set { _working_directory = value; OnPropertyChanged(); }
		}

		public bool IsEmptyTextVisible(int count) => count == 0;
		private DependencyWindow _SelectedItem;
		private string _working_directory;
		private ObservableCollection<SearchFolderItem> _CustomSearchFolders;
	}
}
