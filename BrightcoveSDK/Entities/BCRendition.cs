﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

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
		[DataMember]
		public int frameHeight { get; set; }

		/// <summary>
		/// The rendition's display width, in pixels.
		/// </summary> 
		[DataMember]
		public int frameWidth { get; set; }

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
				if (codecType.Equals(VideoCodecEnum.H264.ToString())) {
					return VideoCodecEnum.H264;
				}
				else if (codecType.Equals(VideoCodecEnum.NONE.ToString())) {
					return VideoCodecEnum.NONE;
				}
				else if (codecType.Equals(VideoCodecEnum.ON2.ToString())) {
					return VideoCodecEnum.ON2;
				}
				else if (codecType.Equals(VideoCodecEnum.SORENSON.ToString())) {
					return VideoCodecEnum.SORENSON;
				}
				else {
					return VideoCodecEnum.UNDEFINED;
				}
			}
			set {
				codecType = value.ToString();
			}
		}
						
		#region IComparable Comparators

		public int CompareTo(BCRendition other) {
			return url.CompareTo(other.url);
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

			StringBuilder jsonPlaylist = new StringBuilder();

			StringBuilder jsonR = new StringBuilder();
			jsonR.Append("{");

			//referenceId
			if (!string.IsNullOrEmpty(rendition.referenceId)) {
				jsonR.Append("\"referenceId\": \"" + rendition.referenceId + "\"");
			}

			//remoteUrl
			if (!string.IsNullOrEmpty(rendition.remoteUrl)) {
				if (jsonR.Length > 0) jsonR.Append(",");
				jsonR.Append("\"remoteUrl\": \"" + rendition.remoteUrl + "\"");
			}

			//remoteStreamName
			if (!string.IsNullOrEmpty(rendition.remoteStreamName)) {
				if (jsonR.Length > 0) jsonR.Append(","); 
				jsonR.Append(",\"remoteStreamName\": \"" + rendition.remoteStreamName + "\"");
			}

			//size
			if (!string.IsNullOrEmpty(rendition.size.ToString())) {
				if (jsonR.Length > 0) jsonR.Append(","); 
				jsonR.Append(",\"size\": " + rendition.size.ToString());
			}

			//videoDuration
			if (!string.IsNullOrEmpty(rendition.videoDuration.ToString())) {
				if (jsonR.Length > 0) jsonR.Append(","); 
				jsonR.Append(",\"videoDuration\": " + rendition.videoDuration.ToString());
			}

			//type
			if (!string.IsNullOrEmpty(rendition.videoCodec.ToString())) {
				if (jsonR.Length > 0) jsonR.Append(","); 
				jsonR.Append(",\"videoCodec\": \"" + (int)Enum.Parse(typeof(VideoCodecEnum), rendition.videoCodec.ToString()) + "\"");
			}

			return "{" + jsonR.ToString() + "}";
		}

		#endregion Extension Methods
	}
}