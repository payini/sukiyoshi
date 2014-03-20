using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightcoveSDK
{
	#region Public Enums

	public enum AccountType { Video, Audio }

	public enum ActionType { READ, WRITE };

	public enum Alignment { TOP_LEFT, BOTTOM_LEFT, TOP_RIGHT, BOTTOM_RIGHT }
	
	public enum BCEncodeType { MP4, FLV, UNDEFINED };

	public enum BCObjectType { videos, playlists };

	public enum BCSortByType { PUBLISH_DATE, CREATION_DATE, MODIFIED_DATE, PLAYS_TOTAL, PLAYS_TRAILING_WEEK };

	public enum BCSortOrderType { ASC, DESC };

	public enum BCVideoEconomics { FREE, AD_SUPPORTED };

	public enum ControllerType { AKAMAI_STREAMING, AKAMAI_SECURE_STREAMING, AKAMAI_STREAMING_LIVE, AKAMAI_LIVE, AKAMAI_HD, AKAMAI_HD_LIVE, AKAMAI_HD2_LIVE, LIMELIGHT_LIVE, LIMELIGHT_MEDIAVAULT, LIVE_STREAMING, TELEFONICA_STREAMING, TELEFONICA_REMOTE, TELEFONICA_PD, DEFAULT }
	
	public enum CuePointType { AD = 0, CODE = 1, CHAPTER = 2 };

	public enum ImageTypeEnum { THUMBNAIL, VIDEO_STILL, SYNDICATION_STILL, BACKGROUND, LOGO, LOGO_OVERLAY };

	public enum ItemCollection { total_count, items, page_number, page_size };

	public enum ItemStateEnum { ACTIVE, INACTIVE, PENDING, DELETED };

	public enum JSONType { Create, Update }

	public enum LogoOverlayAlignmentEnum { TOP_RIGHT, TOP_LEFT, BOTTOM_RIGHT, BOTTOM_LEFT }

	public enum MediaDeliveryTypeEnum { HTTP, HTTP_IOS, DEFAULT }
	
	public enum PlaylistTypeEnum { EXPLICIT, OLDEST_TO_NEWEST, NEWEST_TO_OLDEST, ALPHABETICAL, PLAYS_TOTAL, PLAYS_TRAILING_WEEK };

	public enum PlaylistFields {	ID, REFERENCEID, NAME, SHORTDESCRIPTION, VIDEOIDS, VIDEOS, THUMBNAILURL, FILTERTAGS, PLAYLISTTYPE, ACCOUNTID }

	public enum PlayerPlaylistType { None, ComboBox, Tabbed, VideoList }

	public enum UploadStatusEnum { UPLOADING, PROCESSING, COMPLETE, ERROR, UNDEFINED };
	
	public enum VideoFields
	{ 
		ID, NAME, SHORTDESCRIPTION, LONGDESCRIPTION, CREATIONDATE, PUBLISHEDDATE, 
		LASTMODIFIEDDATE, STARTDATE, ENDDATE, CAPTIONING, LINKURL, LINKTEXT, TAGS, VIDEOSTILLURL, 
		THUMBNAILURL, REFERENCEID, LENGTH, ECONOMICS, ITEMSTATE, PLAYSTOTAL, PLAYSTRAILINGWEEK, VERSION,
		CUEPOINTS, SUBMISSIONINFO, CUSTOMFIELDS, RELEASEDATE, FLVURL, HLSURL, IOSRENDITIONS, RENDITIONS, WVMRENDITIONS, GEOFILTERED, 
		GEORESTRICTED, GEOFILTEREXCLUDE, EXCLUDELISTEDCOUNTRIES, GEOFILTEREDCOUNTRIES, 
		ALLOWEDCOUNTRIES, ACCOUNTID, FLVFULLLENGTH, VIDEOFULLLENGTH 
	}

	public enum VideoCodecEnum { UNDEFINED, NONE, SORENSON, ON2, H264 };
		
	public enum VideoTypeEnum { FLV_PREVIEW, FLV_FULL, FLV_BUMPER, DIGITAL_MASTER };
		
    public enum WMode { Window, Transparent, Opaque }
		
	#endregion Public Enums
}
