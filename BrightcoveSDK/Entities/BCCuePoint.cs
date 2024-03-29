﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using BrightcoveSDK.JSON;

namespace BrightcoveSDK.Media
{
	/// <summary>
	/// The CuePoint object is a marker set at a precise time point in the 
	/// duration of a video. You can use cue points to trigger mid-roll 
	/// ads or to separate chapters or scenes in a long-form video.
	/// </summary>
	[DataContract]
	public class BCCuePoint : BCObject, IComparable<BCCuePoint>
	{
        
        //[DataMember(Name = "id")]

        //[DataMember(Name = "videoId"]

        //[DataMember(Name = "typeEnum"]

		/// <summary>
		/// Required. A name for the cue point, so that you can refer to it.
		/// </summary> 
		[DataMember]
		public string name { get; set; }

		/// <summary>
		/// Required. The time of the cue point, measured in milliseconds from the beginning of the video.
		/// </summary> 
		[DataMember]
		public long time { get; set; }

		[DataMember(Name="forceStop")]
		private string fStop { get; set; }

		/// <summary>
		/// If true, the video stops playback at the cue point. This setting is valid only for AD type cue points.
		/// </summary> 
		public bool forceStop { 
			get {
				try {
					return bool.Parse(fStop);
				}
				catch(ArgumentNullException){
					return false;
				}
			}
			set {
				fStop = value.ToString();	
			}
		}

		[DataMember(Name = "type")]
		private int cueType { get; set; }

		/// <summary>
		/// Required. An integer code corresponding to the type of cue point. One of 0 (AD), 1 (CODE), 
		/// or 2 (CHAPTER). An AD cue point is used to trigger mid-roll ad requests. A CHAPTER cue 
		/// point indicates a chapter or scene break in the video. A CODE cue point causes an event 
		/// that you can listen for and respond to.
		/// </summary> 
		public CuePointType type {
			get {
				return (CuePointType)Enum.ToObject(typeof(CuePointType), cueType);
			}
			set {
				cueType = (int)value;
			}
		}
		
		/// <summary>
		/// A string that can be passed along with a CODE cue point.
		/// </summary> 
		[DataMember]
		public string metadata { get; set; }

		#region IComparable Comparators

		public int CompareTo(BCCuePoint other) {
			return name.CompareTo(other.name);
		}

		#endregion
	}

    public static class BCCuePointExtensions {

        #region Extension Methods

        public static string ToJSON(this List<BCCuePoint> cuePoints) {

			StringBuilder jsonCP = new StringBuilder();
            
            foreach(BCCuePoint cp in cuePoints){
                if (jsonCP.Length > 0)
                    jsonCP.Append(",");
                jsonCP.Append(cp.ToJSON());
            }

            return string.Format("[{0}]", jsonCP.ToString());
        }

        public static string ToJSON(this BCCuePoint cuePoint) {

			Builder jsonCP = new Builder(",", "{", "}");
			
			//name
            if (!string.IsNullOrEmpty(cuePoint.name))
                jsonCP.AppendField("name", cuePoint.name);
            
            //time
            if (!string.IsNullOrEmpty(cuePoint.time.ToString()))
                jsonCP.AppendObject("time", cuePoint.time);
            
			//forceStop
            jsonCP.AppendField("forceStop", cuePoint.forceStop.ToString().ToLower());
            
            //type
            if (!string.IsNullOrEmpty(cuePoint.type.ToString()))
                jsonCP.AppendObject("type", (int)Enum.Parse(typeof(CuePointType), cuePoint.type.ToString()));
            
            //metadata
            if (!string.IsNullOrEmpty(cuePoint.metadata))
                jsonCP.AppendField("metadata", cuePoint.metadata);
            
            return jsonCP.ToString();
        }

        #endregion
    }
}
