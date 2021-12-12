﻿using CommunityToolkit.WinUI.UI.Controls;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dependencies
{
	public sealed partial class DependencyModuleList : DataGrid
	{
		public RelayCommand DoFindModuleInTreeCommand
		{
			get { return (RelayCommand)GetValue(DoFindModuleInTreeCommandProperty); }
			set { SetValue(DoFindModuleInTreeCommandProperty, value); }
		}

		public RelayCommand ConfigureSearchOrderCommand
		{
			get { return (RelayCommand)GetValue(ConfigureSearchOrderCommandProperty); }
			set { SetValue(ConfigureSearchOrderCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for DoFindModuleInTreeCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DoFindModuleInTreeCommandProperty =
			DependencyProperty.Register("DoFindModuleInTreeCommand", typeof(RelayCommand), typeof(DependencyModuleList), new PropertyMetadata(null));

		public static readonly DependencyProperty ConfigureSearchOrderCommandProperty =
			DependencyProperty.Register("ConfigureSearchOrderCommand", typeof(RelayCommand), typeof(DependencyModuleList), new PropertyMetadata(null));

		
		public DependencyModuleList()
		{
			this.InitializeComponent();
		}

		public void AddModule(DisplayModuleInfo NewModule)
		{
			// TODO : Find a way to properly bind commands instead of using this hack
			NewModule.DoFindModuleInTreeCommand = DoFindModuleInTreeCommand;
			NewModule.ConfigureSearchOrderCommand = ConfigureSearchOrderCommand;


			Items.Add(NewModule);
		}

		public ObservableCollection<DisplayModuleInfo> Items = new ObservableCollection<DisplayModuleInfo>();
	}
}