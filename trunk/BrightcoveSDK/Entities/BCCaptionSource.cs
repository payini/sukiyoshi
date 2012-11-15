using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BrightcoveSDK.Media
{
   /// <summary>
   /// RJE 10-16-2012
   /// Support for Closed Captioning
   /// http://docs.brightcove.com/en/media/reference.html#CaptionSource
   /// </summary>
	[DataContract]
   public class BCCaptionSource : BCObject
   {
      /// <summary>
      /// yes	 A Boolean indicating whether a CaptionSource is usable.
      /// </summary>
		[DataMember]
      public bool complete { get; set; }

      /// <summary>
      /// no	 The name of the caption source, which will be displayed in the Media module.
      /// </summary>
		[DataMember]
      public string displayName { get; set; }

      /// <summary>
      /// yes	 A number that uniquely identifies this CaptionSource object, assigned by Video Cloud when this object is created.
      /// </summary>
		[DataMember]
      public long id { get; set; }

      /// <summary>
      /// yes	 A Boolean indicating whether or not this CaptionSource is hosted on a remote server, as opposed to hosted by Brightcove.
      /// </summary>
		[DataMember]
      public bool isRemote { get; set; }

      /// <summary>
      /// no	 The complete path to the file.
      /// </summary>
		[DataMember]
      public string url { get; set; }

      public string ToJSON(JSONType type)
      {
         switch (type)
         {
            case JSONType.Create:
               break;

            default:
            case JSONType.Update:
               throw new System.ApplicationException("Unsupported JSONType for BCCaptionSource");
         }

         JSON.Builder builder = new JSON.Builder();

         // only consider writing: displayName and url

         builder.Append("{");
         builder.AppendField("displayName", displayName);
         if (!string.IsNullOrEmpty(url))
            builder.Append(",").AppendField("url", url);
         builder.Append("}");

         return builder.ToString();
      }
   }

   /// <summary>
   /// Returned from an add_caption call
   /// http://docs.brightcove.com/en/media/reference.html#Captioning
   /// </summary>
	[DataContract]
   public class BCCaptionSources : BCObject
   {
      /// <summary>
      /// Long	 yes	 A number that uniquely identifies this Captioning object, assigned by VideoCloud when this object is created.
      /// </summary>
      [DataMember]
      public long id { get; set; }

      /// <summary>
      /// Set	 no	 A set of sources which provide caption. Only one CaptionSource is supported at this time.
      /// </summary>
      [DataMember]
      public BCCaptionSource[] captionSources { get; set; }
   }
}