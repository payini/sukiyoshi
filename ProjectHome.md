# Where to Start #

If you're looking to integrate a site with your Brightcove Account you will only need th e BrightcoveSDK.dll file. If you are looking to integrate Brightcove with your Sitecore installation you will need the package installer which contains all the files and directions you need. The [wiki](http://code.google.com/p/sukiyoshi/w/list) has examples that should help you get started.

# Download #

To download the latest files you can get them here:

## Sitecore 6.3 and earlier ##

[BrightcoveSDK.dll](https://sukiyoshi.googlecode.com/svn/tags/1.0/BrightcoveSDK/bin/Release/BrightcoveSDK.dll)

[Sitecore Package Install](https://sukiyoshi.googlecode.com/svn/tags/1.0/Website/data/packages/Brightcove3172011.zip)

## Sitecore 6.4+ ##

[BrightcoveSDK.dll](https://sukiyoshi.googlecode.com/svn/tags/1.2/BrightcoveSDK/bin/Release/BrightcoveSDK.dll)

[Sitecore Package Install](https://sukiyoshi.googlecode.com/svn/tags/1.2/Website/data/packages/Brightcove3172011.zip)

# Brightcove in Sitecore #

Also for more information about getting [Brightcove working in Sitecore](http://markstiles.net/Blog/2011/03/16/brightcove-in-sitecore.aspx) I've written an in depth article about how it works.

# About Sukiyoshi #

sukiyoshi is a .NET class library for the Brightcove 4 Media Application Programming Interface ( API ) implemented in C#.

This was derived from tanaris (thanks to Bob de Wit) but has changed sufficiently to require a new branch.

**This code base requires you to use .NET 3.5**

highlights:
  * updated the SearchVideos method in the Video Read section
  * added all the find by Unfiltered methods to the Video Read section
  * added the UnshareVideo in the Video Write section
  * added the AddLogoOverlay in the Video Write section
  * Updated the Enum conversions to more concise code
  * Better Custom Field Support
  * Reorganized the structure and namespace to improve consistency
  * I've added support for cue points (adding/removing) through video objects
  * Updated the namespaces
  * I'm using the .NET DataContractJsonSerializer to cast json objects to classes from query results.
  * read requests return a query result class that retrieves all the extra information, such as total count, page number and page size
  * when read calls require more than the limit of 100 videos there is a recursive call to retrieve beyond that limit.
  * there is a video comparer class to help merge search results from tags and text
  * bcvideo now implements IComparable to allow sorting by creation date to fix the problem of querying by creation\_date.
  * this version requires a new configuration section in the web.config to allow multiple accounts on the same site.
  * Custom Exception error class
  * all known enumeration types and some new for internal use
  * full read and write methods (initially tested but needs more thorough qa)
  * There is a small set of front end UI controls also
  * I've added a source library to integrate the package into Sitecore.
  * I've uploaded a package installer for Sitecore also

sukiyoshi is not officially supported by Brightcove but is initiated by Mark Stiles, a Principal Web Developer at Genzyme in Boston. Please post all support questions to the Google Code forum, not in the Brightcove Developer Community.