using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("FFSharp Tests")]
[assembly: AssemblyDescription("NUnit3 test assembly for the FFSharp project.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FFSharp")]
[assembly: AssemblyCopyright("Copyright © Karl F. A. Friebel 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("b4d037a5-059b-42a9-b2b9-1abab15c955a")]

[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Test assembly shall have internal access.
[assembly: InternalsVisibleTo("Tests")]