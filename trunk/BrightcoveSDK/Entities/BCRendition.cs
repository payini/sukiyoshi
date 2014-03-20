using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using BrightcoveSDK.JSON;

namespace BrightcoveSDK.Media
{
	
	/// <summary>
	/// The Rendition object represents one of the dynamic delivery 
	/// renditions of a video. A Video should have not more than 10 Renditions.
	/// </summary>
	[DataContract]
	public class BCRendition : BCObject, IComparable<BCRendition>
	{
		/// <summary>
		/// The name of the rendition dynamically created when you upload a video. You can view this name in the Video Files tab in Video Cloud.
		/// </summary>
		[DataMember]
		public string displayName { get; set; }

		/// <summary>
		/// If true, this rendition is audio-only and has no video content. Audio-only renditions can be used for mobile streaming over low-bandwidth connections. It is recommended that videos in iOS applications should include a 64 kbps audio-only rendition.
		/// </summary>
		[DataMember]
		public bool audioOnly { get; set; }

		/// <summary>
		/// The URL of the rendition file.
		/// </summary> 
		[DataMember]
		public string url { get; set; }

		/// <summary>
		/// The referenceId of the rendition file.
		/// </summary> 
		[DataMember]
		public string referenceId { get; set; }

		/// <summary>
		/// The rendition's encoding rate, in bits per second.
		/// </summary> 
		[DataMember]
		public int encodingRate { get; set; }
		
		/// <summary>
		/// The rendition's display height, in pixels.
		/// </summary> 
		[DataMember(Name = "frameHeight")]
		private string fHeight { get; set; }

		public int frameHeight {
			get {
				if (!String.IsNullOrEmpty(fHeight)) {
					return int.Parse(fHeight);
				} else {
					return 0;
				}
			}
			set {
				fHeight = value.ToString();
			}
		}

		/// <summary>
		/// The rendition's display width, in pixels.
		/// </summary> 
		[DataMember(Name = "frameWidth")]
		private string fWidth { get; set; }

		public int frameWidth {
			get {
				if (!String.IsNullOrEmpty(fWidth)) {
					return int.Parse(fWidth);
				} else {
					return 0;
				}
			}
			set {
				fWidth = value.ToString();
			}
		}

		/// <summary>
		/// The rendition ID.
		/// </summary>
		[DataMember]
		public long id { get; set; }

		/// <summary>
		/// The file size of the rendition, in bytes.
		/// </summary> 
		[DataMember]
		public long size { get; set; }
		
		/// <summary>
		/// Required, for remote assets. The complete path to the file hosted on 
		/// the remote server. If the file is served using progressive download, 
		/// then you must include the file name and extension for the file. You can also 
		/// use a URL that re-directs to a URL that includes the file name and extension. 
		/// If the file is served using Flash streaming, use the remoteStreamName 
		/// attribute to provide the stream name.
		/// </summary>
		[DataMember]
		public string remoteUrl { get; set; }

		/// <summary>
		/// required for streaming remote assets only. A stream name for Flash 
		/// streaming appended to the value of the remoteUrl property.
		/// </summary> 
		[DataMember]
		public string remoteStreamName { get; set; }

		/// <summary>
		/// The date/time that the video was uploaded to Video Cloud, in Epoch milliseconds form.
		/// </summary>
		[DataMember]
		public long uploadTimestampMillis { get; set; }

		/// <summary>
		/// The format of the wrapper that provides metadata and describes how the video and audio are stored in the file. Valid values is M2TS. See Supported Video Codecs and Containers for more information.
		/// </summary>
		[DataMember]
		public string videoContainer { get; set; }

		/// <summary>
		/// Required. The length of the remote video asset in milliseconds.
		/// </summary> 
		[DataMember]
		public long videoDuration { get; set; }

		[DataMember(Name = "videoCodec")]
		private string codecType { get; set; }

		/// <summary>
		/// Required. Valid values are SORENSON, ON2, and H264.
		/// </summary> 
		public VideoCodecEnum videoCodec {
			get {
				return (string.IsNullOrEmpty(codecType)) ? VideoCodecEnum.UNDEFINED : (VideoCodecEnum)Enum.Parse(typeof(VideoCodecEnum), codecType, true);
			}
			set {
				codecType = value.ToString();
			}
		}
			
		[DataMember]
		private string controllerType { get; set; }

		/// <summary>
		/// Required. Valid values are AKAMAI_STREAMING, AKAMAI_SECURE_STREAMING, AKAMAI_LIVE, AKAMAI_HD, AKAMAI_HD_LIVE, LIMELIGHT_LIVE, LIMELIGHT_MEDIAVAULT.
		/// </summary> 
		public ControllerType ControllerType {
			get {
				return (string.IsNullOrEmpty(controllerType)) ? ControllerType.UNDEFINED : (ControllerType)Enum.Parse(typeof(ControllerType), controllerType, true);
			}
			set {
				controllerType = value.ToString();
			}
		}
			
		#region IComparable Comparators

		public int CompareTo(BCRendition other) {
			return id.CompareTo(other.id);
		}

		#endregion
	}
	public static class BCRenditionExtensions
	{

		#region Extension Methods

		public static string ToJSON(this List<BCRendition> renditions) {

			StringBuilder jsonCP = new StringBuilder();

			foreach (BCRendition r in renditions) {
				if (jsonCP.Length > 0) {
					jsonCP.Append(",");
				}
				jsonCP.Append(r.ToJSON());
			}

			return "[" + jsonCP.ToString() + "]";
		}

		public static string ToJSON(this BCRendition rendition) {

			//--Build Rendition in JSON -------------------------------------//

			Builder jsonR = new Builder(",", "{", "}");
			
			//referenceId
			if (!string.IsNullOrEmpty(rendition.referenceId)) 
				jsonR.AppendField("referenceId", rendition.referenceId);

			//remoteUrl
			if (!string.IsNullOrEmpty(rendition.remoteUrl)) 
				jsonR.AppendField("remoteUrl", rendition.remoteUrl);

			//encodingRate
			jsonR.AppendField("encodingRate", rendition.encodingRate.ToString());
			
			//remoteStreamName
			if (!string.IsNullOrEmpty(rendition.remoteStreamName)) 
				jsonR.AppendField("remoteStreamName", rendition.remoteStreamName);
			
			//size
			if (!string.IsNullOrEmpty(rendition.size.ToString())) 
				jsonR.AppendObject("size", rendition.size.ToString());
			
			//videoDuration
			if (!string.IsNullOrEmpty(rendition.videoDuration.ToString())) 
				jsonR.AppendObject("videoDuration", rendition.videoDuration.ToString());
			
			//videoCodec
			if (!rendition.videoCodec.Equals(VideoCodecEnum.NONE)) 
				jsonR.AppendObject("videoCodec", rendition.videoCodec.ToString());
			
			//controllerType
			if (!rendition.ControllerType.Equals(ControllerType.UNDEFINED))
				jsonR.AppendObject("controllerType", rendition.ControllerType.ToString());
			
			return jsonR.ToString();
		}

		#endregion Extension Methods
	}
}
