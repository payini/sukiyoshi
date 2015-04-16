#This is a sample of the web.config setup that will work for this sdk

# Introduction #

To be able to support multiple accounts per website I've added support for a custom config section. A sample of the config setup is below and a sample of how to call the config and make calls are also below.


# Details #

**Web.Config**

```
<?xml version="1.0"?>

<configuration>
    <configSections>
		<section name="brightcove" type="BrightcoveSDK.BrightcoveConfig, BrightcoveSDK"></section>
    </configSections>
	<brightcove>
		<accounts>
			<add name="Display Name" type="Video" publisherId="0000000000">
				<ReadToken value="0000000000000000000000000000000000000000"></ReadToken>
				<ReadURL value="http://api.brightcove.com/services/library"></ReadURL>
				<WriteToken value="000000000000000000000000000000000000000"></WriteToken>
				<WriteURL value="http://api.brightcove.com/services/post"></WriteURL>
			</add>
			<add name="Display Name" type="Video" publisherId="0000000000">
				<ReadToken value="0000000000000000000000000000000000000000"></ReadToken>
				<ReadURL value="http://api.brightcove.com/services/library"></ReadURL>
				<WriteToken value="000000000000000000000000000000000000000"></WriteToken>
				<WriteURL value="http://api.brightcove.com/services/post"></WriteURL>
			</add>
		</accounts>
	</brightcove>
    <appSettings/>
    <connectionStrings/>
	<sitecore database="SqlServer">
		<customHandlers>
			<handler trigger="~BrightcoveVideo" handler="BrightcoveVideo.ashx" />
		</customHandlers>
		<pipelines>
			<renderField>
				<processor type="BrightcoveSDK.SitecoreUtil.Pipelines.GetRTEFieldValue,BrightcoveSDK.SitecoreUtil" />
			</renderField>
		</pipelines>
	</sitecore>
	<system.webServer>
		<handlers>
			<add verb="*" path="BrightcoveVideo.ashx" type="BrightcoveSDK.SitecoreUtil.Handlers.BrightcoveVideoHandler, BrightcoveSDK.SitecoreUtil"  name="BrightcoveVideoHandler"/>
		</handlers>	
	</system.webServer>
    <system.web>
      <pages>
        <controls>
			<add tagPrefix="bc" namespace="BrightcoveSDK.UI" assembly="BrightcoveSDK" />
			<add tagPrefix="bc" namespace="BrightcoveSDK.SitecoreUtil.UI" assembly="BrightcoveSDK.SitecoreUtil"/>
        </controls>
      </pages>
      <httpHandlers>
          <add verb="*" path="BrightcoveVideo.ashx" type="BrightcoveSDK.SitecoreUtil.Handlers.BrightcoveVideoHandler, BrightcoveSDK.SitecoreUtil" />
      </httpHandlers>
    </system.web>
</configuration>

```

**Classes To Access Config**

```
using System.Configuration;
using BrightcoveSDK;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.UI;

BrightcoveConfig config = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
BCAPI bc = new BCAPI(config.Accounts[0].PublisherID);
bc.FindAllPlaylists();
```