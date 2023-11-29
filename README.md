# Word Processor (a.k.a) Document Editor Web services

--
Deployment instructions:

The Render service pulls the image created from this repo and uses it for the word-processor-server. To build a new image and deploy:

1. `cd ASP.NET Core`
2. Run `docker build . -t ghcr.io/requestoptix/rox-word-processor --platform=linux/amd64` to build the image.
3. Run `docker push ghcr.io/requestoptix/rox-word-processor:latest` to push the image to the GitHub container registry.

Note that you must have a personal access token that has permissions to read and write packages.
--

Word Processor (a.k.a) Document Editor requires server side interaction for below listed operations
* Import Word Document
* Paste with formatting
* Restrict Editing
* Spell Check

Web API can be written in

**ASP.NET MVC:**
*	[Syncfusion.EJ2.WordEditor.AspNet.Mvc5](https://www.nuget.org/packages/Syncfusion.EJ2.WordEditor.AspNet.Mvc5)
*	[Syncfusion.EJ2.WordEditor.AspNet.Mvc4](https://www.nuget.org/packages/Syncfusion.EJ2.WordEditor.AspNet.Mvc4)

Please refer [ASP.NET MVC Web API sample](https://github.com/SyncfusionExamples/EJ2-DocumentEditor-WebServices/tree/master/ASP.NET%20MVC).

**ASP.NET Core:**
*   [Syncfusion.EJ2.WordEditor.AspNet.Core](https://www.nuget.org/packages/Syncfusion.EJ2.WordEditor.AspNet.Core)

Please [ASP.NET MVC Web API sample](https://github.com/SyncfusionExamples/EJ2-DocumentEditor-WebServices/tree/master/ASP.NET%20Core).

**Java:**
*   syncfusion-ej2-wordprocessor-18.1.0.29.jar file available in build installed locaion.

Syntax:

>**(installed location)**/Syncfusion/Essential Studio/JavaScript - EJ2/18.1.0.29/JarFiles/syncfusion-ej2-wordprocessor-18.1.0.29.jar

>C:/Program Files (x86)/Syncfusion/Essential Studio/JavaScript - EJ2/18.1.0.29/JarFiles/syncfusion-ej2-wordprocessor-18.1.0.29.jar)

Please refer [Java Web API sample](https://github.com/SyncfusionExamples/EJ2-DocumentEditor-WebServices/tree/master/Java).