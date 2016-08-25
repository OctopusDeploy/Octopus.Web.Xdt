[![Build status](https://ci.appveyor.com/api/projects/status/3cl8bmipyldw3xgx?svg=true)](https://ci.appveyor.com/project/OctopusDeploy/octopus-web-xdt)

# Octopus.Web.Xdt

A .NET Core port of `Microsoft.Web.Xdt`, based on the code from https://xdt.codeplex.com as of August 2016.

At this point, the original project does not take community contributions, so this is a separate project.

There are hopes that the upstream `Microsoft.Web.Xdt` package will be converted, and this package deprecated.

## Conversion notes

* A lot of the [corefx](https://github.com/dotnet/corefx) code was inlined, as `XmlTextReader` and `XmlTextWriter` and internal to corefx at the moment
* Some changes were made to the inlined corefx code to disable line `CRLF` normalisation
* File encoding support was removed in the port - behaviour is undefined at this point
* Tests were converted to NUnit, as MSTest is not supported
* Test code was modified to read test case files in a different way as file based resources work differently under .NET Core
