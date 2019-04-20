# revit-fabulous
A project template for creating Revit add-ins using F# and Fabulous.

*Allows real-time Revit and WPF code changes without restarting Revit!*

## Features
- The Revit project currently references the Revit 2019 API, but can easily be changed via NuGet.
- The .addin file is automatically copied to the Revit addins folder via a pre-build script.
- Revit 2019 "edit and continue" support: The pre-build script that copies the .addin file to the Revit addins folder also updates the .dll path to point to your debug folder to allow "edit and continue" of Revit code while debugging.
- Fabulous provides an MVU "Elmish" architecture (similar to React.js) that allows you to generate your UI using strongly typed functional programming techniques.  (It can also support using XAML templates).
- The Fabulous "Live Update" feature detects changes to the WPF project while debugging and updates your UI _within seconds_ -- **without having to manually recompile and restart Revit!**
- Fabulous eliminates the need to write view models, and eliminates the need for value converters!
- Fabulous gives your dialog windows the ability to function as their own SPA -- meaning you can easily create virtual dialogs from within your current dialog window.
- Two built-in examples: one that opens a UI without any required initializing parameters, and one to show how to functionally inject dependencies (data and behavior).

## Projects

### *{ProjectName}*.Domain
This is an F# .net standard library that will be used for modeling a domain that will be shared between the Revit and the WPF projects.  This project is agnostic of all other projects.

### *{ProjectName}*.Revit 
This is an F# project using Revit 2019 API (via nuget).  This project features an .addin file, and pre-build scripts to copy the file and redirect the .dll to your debug folder to support edit-and-continue of Revit code while debugging.
This project consumes the Domain project and the WPF project and defines Revit command classes that serve as application roots.

### *{ProjectName}*.WPF
This is an F# project using Fabulous.  It consumes the Domain project, but is agnostic of Revit API and the Revit project.  

## Notes

### Why not put everything in one project?
The WPF project is agnostic of Revit API and the Revit Addin project.  While you can certainly merge the projects together, I think this separation of concerns will emphasize creating a proper F# domain model that can be shared between the two projects.  It will also eliminate the temptation to reference the Revit OOP API directly in your Fabulous models (which might prevent you from taking advantage of the goodness of the F# type system and immutability).
There are also a few practical reasons:  The Fabulous "Live Update" feature sometimes chokes on certain constructs in code.  For example, I tried to use a "System.Lazy" to ensure that the "init()" function was called only once, but this caused LiveUpdate to choke.  So I think this is a good reason to keep the WPF project as simple as possible.  Also note that the Revit project supports Revit 2019 "edit and continue" via the project pre-build script, so Revit code changes can be handled separately.
