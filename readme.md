BlueTools - Tridion 2011 GUI Extension
------------------------
- Tutorial with detailed instructions at http://www.curlette.com/?p=401
- Tridion BluePrint-aware tools.  Currently only BlueCopy tool is implemented.
- Built using Tridion 2011 SP1
- Need to add the Tridion DLL Reference to the ServiceStack project and compile.  The Tridion DLL Tridion.ContentManager.CoreService.Client is located in the Tridion\bin\client folder.
- Should create a new website in IIS for the ServiceStack project
- To enable, edit  the Tridion GUI System.Config and add the GUI Extension reference
- Change the web.config and put your web server details in there
