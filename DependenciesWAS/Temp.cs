using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependencies
{
    public class ModuleCacheKey
    {
        public ModuleCacheKey(string _Name, string _Filepath, ModuleFlag _Flags = ModuleFlag.NoFlag)
        {
            Name = _Name;
            Filepath = _Filepath;
            Flags = _Flags;
        }

        public ModuleCacheKey(ImportContext import)
        {
            Name = import.ModuleName;
            Filepath = import.PeFilePath;
            Flags = import.Flags;
        }

        // mandatory since ModuleCacheKey is used as a dictionnary key
        public override int GetHashCode()
        {
            int hashcode = Name.GetHashCode() ^ Flags.GetHashCode();

            if (Filepath != null)
            {
                hashcode ^= Filepath.GetHashCode();
            }

            return hashcode;
        }

        public string Name;
        public string Filepath;
        public ModuleFlag Flags;
    }



    public class ModulesCache : Dictionary<ModuleCacheKey, DisplayModuleInfo>
    {

    }

    /// <summary>
}


namespace Dependencies.Properties.Settings
{
    class Default
	{
        static public bool FullPath = false;
        static public bool Undecorate = false;
        static public bool BinaryCacheOptionValue = true;
        static public EventHandler<PropertyChangedEventArgs> PropertyChanged;
    }

}
