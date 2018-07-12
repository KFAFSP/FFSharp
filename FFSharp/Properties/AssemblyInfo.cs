using System.Reflection;
using System.Runtime.CompilerServices;

#region Assembly description
[assembly: AssemblyTitle("FFSharp")]
[assembly: AssemblyDescription("Managed .NET wrapper for FFmpeg")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FFSharp")]
[assembly: AssemblyCopyright("Copyright (c) Karl F. A. Friebel 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#endregion

#region Friend assemblies
// The test assembly is a friend.
[assembly: InternalsVisibleTo("FFSharp.Tests")]
#endregion

#region Version
[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#endregion