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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

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


    /// <summary>
    /// Dependency tree building behaviour.
    /// A full recursive dependency tree can be memory intensive, therefore the
    /// choice is left to the user to override the default behaviour.
    /// </summary>
    public class BinaryCacheOption : IValueConverter
    {
        public enum BinaryCacheOptionValue
        {
            [Description("No (faster, but locks dll until Dependencies is closed)")]
            No = 0,

            [Description("Yes (prevents file locking issues)")]
            Yes = 1
        }

        public static BinaryCacheOptionValue GetGlobalBehaviour()
        {
            return (BinaryCacheOptionValue)(new BinaryCacheOption()).Convert(
                Dependencies.Properties.Settings.Default.BinaryCacheOptionValue,
                null,// targetType
                null,// parameter
                null // System.Globalization.CultureInfo
            );
        }

        #region BinaryCacheOption.IValueConverter_contract
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            bool StrOption = (bool)value;

            switch (StrOption)
            {
                default:
                case true:
                    return BinaryCacheOptionValue.Yes;
                case false:
                    return BinaryCacheOptionValue.No;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            BinaryCacheOptionValue Behaviour = (BinaryCacheOptionValue)(int)value;

            switch (Behaviour)
            {
                default:
                case BinaryCacheOptionValue.Yes:
                    return true;
                case BinaryCacheOptionValue.No:
                    return false;
            }
        }
        #endregion BinaryCacheOption.IValueConverter_contract
    }

    /// <summary>
    /// User context of every dependency tree node.
    /// </summary>
    public struct DependencyNodeContext
    {
        public DependencyNodeContext(DependencyNodeContext other)
        {
            ModuleInfo = other.ModuleInfo;
            IsDummy = other.IsDummy;
        }

        /// <summary>
        /// We use a WeakReference to point towars a DisplayInfoModule
        /// in order to reduce memory allocations.
        /// </summary>
        public WeakReference ModuleInfo;

        /// <summary>
        /// Depending on the dependency tree behaviour, we may have to
        /// set up "dummy" nodes in order for the parent to display the ">" button.
        /// Those dummy are usually destroyed when their parents is expandend and imports resolved.
        /// </summary>
        public bool IsDummy;
    }

    /// <summary>
    /// Deprendency Tree custom node. It's DataContext is a DependencyNodeContext struct
    /// </summary>
    public class ModuleTreeViewItem : TreeViewNode, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DependencyNodeContext DataContext;

        private object _Header;



        public ModuleTreeViewItem()
        {
            _importsVerified = false;
            _Parent = null;
            Dependencies.Properties.Settings.Default.PropertyChanged += this.ModuleTreeViewItem_PropertyChanged;
        }

        public ModuleTreeViewItem(ModuleTreeViewItem Parent)
        {
            _importsVerified = false;
            _Parent = Parent;
            Dependencies.Properties.Settings.Default.PropertyChanged += this.ModuleTreeViewItem_PropertyChanged;
        }

        public ModuleTreeViewItem(ModuleTreeViewItem Other, ModuleTreeViewItem Parent)
        {
            _importsVerified = false;
            _Parent = Parent;
            this.DataContext = new DependencyNodeContext((DependencyNodeContext)Other.DataContext);
            Dependencies.Properties.Settings.Default.PropertyChanged += this.ModuleTreeViewItem_PropertyChanged;
        }

        #region PropertyEventHandlers 
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ModuleTreeViewItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FullPath")
            {
                this.Header = (object)GetTreeNodeHeaderName(Dependencies.Properties.Settings.Default.FullPath);
            }
        }
#endregion PropertyEventHandlers

#region Getters

        public string GetTreeNodeHeaderName(bool FullPath)
        {
            return (((DependencyNodeContext)this.DataContext).ModuleInfo.Target as DisplayModuleInfo).ModuleName;
        }

        public object Header
		{
            get
			{
                return _Header;
			}
            set
			{
                _Header = value;
                OnPropertyChanged(nameof(Header));
			}
		}

        public string ModuleFilePath
        {
            get
            {
                return (((DependencyNodeContext)this.DataContext).ModuleInfo.Target as DisplayModuleInfo).Filepath;
            }
        }

        public ModuleTreeViewItem ParentModule
        {
            get
            {
                return _Parent;
            }
        }


        public ModuleFlag Flags
        {
            get
            {
                return ModuleInfo.Flags;
            }
        }

        private bool _has_error;

        public bool HasErrors
        {
            get
            {
                if (!_importsVerified)
                {
                    _has_error = VerifyModuleImports();
                    _importsVerified = true;

                    // Update tooltip only once some basic checks are done
#if TODO

                    this.ToolTip = ModuleInfo.Status;
#endif
                }

                // propagate error for parent
                if (_has_error)
                {
                    ModuleTreeViewItem ParentModule = this.ParentModule;
                    if (ParentModule != null)
                    {
                        ParentModule.HasChildErrors = true;
                    }
                }

                return _has_error;
            }

            set
            {
                if (value == _has_error) return;
                _has_error = value;
                OnPropertyChanged("HasErrors");
            }
        }


        public string Tooltip
        {
            get
            {
                return ModuleInfo.Status;
            }
        }

        public bool HasChildErrors
        {
            get
            {
                return _has_child_errors;
            }
            set
            {
                if (value)
                {
                    ModuleInfo.Flags |= ModuleFlag.ChildrenError;
                }
                else
                {
                    ModuleInfo.Flags &= ~ModuleFlag.ChildrenError;
                }
#if TODO

                ToolTip = ModuleInfo.Status;
#endif
                _has_child_errors = true;
                OnPropertyChanged("HasChildErrors");

                // propagate error for parent
                ModuleTreeViewItem ParentModule = this.ParentModule;
                if (ParentModule != null)
                {
                    ParentModule.HasChildErrors = true;
                }
            }
        }

        public DisplayModuleInfo ModuleInfo
        {
            get
            {
                return (((DependencyNodeContext)this.DataContext).ModuleInfo.Target as DisplayModuleInfo);
            }
        }


        private bool VerifyModuleImports()
        {

            // current module has issues
            if ((Flags & (ModuleFlag.NotFound | ModuleFlag.MissingImports | ModuleFlag.ChildrenError)) != 0)
            {
                return true;
            }

            // no parent : it's probably the root item
            ModuleTreeViewItem ParentModule = this.ParentModule;
            if (ParentModule == null)
            {
                return false;
            }

            // Check we have any imports issues
            foreach (PeImportDll DllImport in ParentModule.ModuleInfo.Imports)
            {
                if (DllImport.Name != ModuleInfo._Name)
                    continue;



                List<Tuple<PeImport, bool>> resolvedImports = BinaryCache.LookupImports(DllImport, ModuleInfo.Filepath);
                if (resolvedImports.Count == 0)
                {
                    return true;
                }

                foreach (var Import in resolvedImports)
                {
                    if (!Import.Item2)
                    {
                        return true;
                    }
                }
            }



            return false;
        }



#endregion Getters


#region Commands 
        public RelayCommand OpenPeviewerCommand
        {
            get
            {
                if (_OpenPeviewerCommand == null)
                {
                    _OpenPeviewerCommand = new RelayCommand((param) => this.OpenPeviewer((object)param));
                }

                return _OpenPeviewerCommand;
            }
        }

        public bool OpenPeviewer(object Context)
        {
#if TODO

            string programPath = Dependencies.Properties.Settings.Default.PeViewerPath;
            Process PeviewerProcess = new Process();

            if (Context == null)
            {
                return false;
            }

            if (!File.Exists(programPath))
            {
                System.Windows.MessageBox.Show(String.Format("{0:s} file could not be found !", programPath));
                return false;
            }

            string Filepath = ModuleFilePath;
            if (Filepath == null)
            {
                return false;
            }

            PeviewerProcess.StartInfo.FileName = String.Format("\"{0:s}\"", programPath);
            PeviewerProcess.StartInfo.Arguments = String.Format("\"{0:s}\"", Filepath);
            return PeviewerProcess.Start();
#endif
            return false;
        }

        public RelayCommand OpenNewAppCommand
        {
            get
            {
                #if TODO

                if (_OpenNewAppCommand == null)
                {
                    _OpenNewAppCommand = new RelayCommand((param) =>
                    {
                        string Filepath = ModuleFilePath;
                        if (Filepath == null)
                        {
                            return;
                        }

                        Process OtherDependenciesProcess = new Process();
                        OtherDependenciesProcess.StartInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
                        OtherDependenciesProcess.StartInfo.Arguments = String.Format("\"{0:s}\"", Filepath);
                        OtherDependenciesProcess.Start();
                    });
                }
#endif
                return _OpenNewAppCommand;
            }
        }

#endregion // Commands 

        private RelayCommand _OpenPeviewerCommand;
        private RelayCommand _OpenNewAppCommand;
        private ModuleTreeViewItem _Parent;
        private bool _importsVerified;
        private bool _has_child_errors;


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

		public DependencyWindow(String Filename, List<string> CustomSearchFolders = null)
        {
			this.InitializeComponent();

            if (CustomSearchFolders != null)
            {
                this.CustomSearchFolders = CustomSearchFolders;
            }
            else
            {
                this.CustomSearchFolders = new List<string>();
            }

            this.Filename = Filename;
            this.WorkingDirectory = Path.GetDirectoryName(this.Filename);
            InitializeView();
        }

        public void InitializeView()
        {
            if (!NativeFile.Exists(this.Filename))
            {
#if TODO
                MessageBox.Show(
                    String.Format("{0:s} is not present on the disk", this.Filename),
                    "Invalid PE",
                    MessageBoxButton.OK
                );
#endif
                return;
            }

            this.Pe = (Application.Current as App).LoadBinary(this.Filename);
            if (this.Pe == null || !this.Pe.LoadSuccessful)
            {
#if TODO

                MessageBox.Show(
                    String.Format("{0:s} is not a valid PE-COFF file", this.Filename),
                    "Invalid PE",
                    MessageBoxButton.OK
                );
#endif
                return;
            }

            this.SymPrv = new PhSymbolProvider();
            this.RootFolder = Path.GetDirectoryName(this.Filename);
            this.SxsEntriesCache = SxsManifest.GetSxsEntries(this.Pe);
            this.ProcessedModulesCache = new ModulesCache();
            this.ApiSetmapCache = Phlib.GetApiSetSchema();
            this._SelectedModule = null;
            this._DisplayWarning = false;

            // TODO : Find a way to properly bind commands instead of using this hack
#if TODO
            this.ModulesList.Items.Clear();
            this.ModulesList.DoFindModuleInTreeCommand = DoFindModuleInTree;
            this.ModulesList.ConfigureSearchOrderCommand = ConfigureSearchOrderCommand;
#endif
            var RootFilename = Path.GetFileName(this.Filename);
            var RootModule = new DisplayModuleInfo(RootFilename, this.Pe, ModuleSearchStrategy.ROOT);
            this.ProcessedModulesCache.Add(new ModuleCacheKey(RootFilename, this.Filename), RootModule);

            ModuleTreeViewItem treeNode = new ModuleTreeViewItem();
            DependencyNodeContext childTreeInfoContext = new DependencyNodeContext()
            {
                ModuleInfo = new WeakReference(RootModule),
                IsDummy = false
            };

            treeNode.DataContext = childTreeInfoContext;
            treeNode.Header = treeNode.GetTreeNodeHeaderName(Dependencies.Properties.Settings.Default.FullPath);
            //treeNode.IsExpanded = true;

            /*TreeViewNode node = new TreeViewNode();
            node.IsExpanded = true;
            node.Content = treeNode;*/
            this.DllTreeView.RootNodes.Clear();
            this.DllTreeView.RootNodes.Add(treeNode);

            /*
            // Recursively construct tree of dll imports
            ConstructDependencyTree(treeNode, this.Pe);*/
        }
    }
}
