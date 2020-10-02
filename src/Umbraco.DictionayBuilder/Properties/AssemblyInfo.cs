using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Umbraco.DictionaryBuilder.Properties;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Umbraco.DictionaryBuilder")]
[assembly: AssemblyDescription("An Umbraco-CMS strongly typed dictionary item builder")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("GenericShape")]
[assembly: AssemblyProduct("Umbraco.DictionaryBuilder")]
[assembly: AssemblyCopyright("Copyright © m-fredsgaard 2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("df6ba6f4-20ab-47b0-bfaa-b37603f98cf9")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.Version)]
[assembly: InternalsVisibleTo("Umbraco.DictionaryBuilder.Tests")]
[assembly: InternalsVisibleTo("Umbraco.DictionaryBuilder.VueI18N")]

namespace Umbraco.DictionaryBuilder.Properties
{
    internal static class AssemblyInfo
    {
        public const string Version = "1.0.12";
    }
}