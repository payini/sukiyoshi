using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightcoveSDK.JSON;
using BrightcoveSDK.Media;
using BrightcoveSDK.HTTP;
using BrightcoveSDK.Containers;

namespace BrightcoveSDK
{
	public partial class BCAPI
	{
		#region Video Write

		#region Create Video

		//favors no processing renditions
		public RPCResponse<long> CreateVideo(BCVideo video) {
			return CreateVideo(video, "");
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename) {
			return CreateVideo(video, filename, null);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing) {
			return CreateVideo(video, filename, file, H264NoProcessing, false);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing, bool preserve_source_rendition) {
			return CreateVideo(video, filename, file, H264NoProcessing, preserve_source_rendition, -1);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing, bool preserve_source_rendition, long maxsize) {
			return CreateVideo(video, filename, file, H264NoProcessing, preserve_source_rendition, maxsize, null);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing, bool preserve_source_rendition, long maxsize, string file_checksum) {
			return CreateVideo(video, filename, file, BCEncodeType.UNDEFINED, false, H264NoProcessing, preserve_source_rendition, maxsize, file_checksum, null);
		}

		//favors multiple renditions
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file) {
			return CreateVideo(video, filename, file, BCEncodeType.UNDEFINED);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to) {
			return CreateVideo(video, filename, file, encode_to, false);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, false);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, bool preserve_source_rendition) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, preserve_source_rendition, -1);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, bool preserve_source_rendition, long maxsize) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, preserve_source_rendition, maxsize, null);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, bool preserve_source_rendition, long maxsize, string file_checksum) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, false, preserve_source_rendition, maxsize, file_checksum, null);
		}

      /// <summary>
      /// The above calls difficult to track and prone to caller error.
      /// This initializes all the defaults and let's the caller pick which properties to override.
      /// </summary>
      public class BCCreateVideoOptions
      {
         public string filename = "";
         public byte[] file = null;
         public BCEncodeType encode_to = BCEncodeType.UNDEFINED;
         public bool create_multiple_renditions = false;
         public bool preserve_source_rendition = false;
         public bool H264NoProcessing = false;
         public long maxsize = -1;
         public string file_checksum = null;

         /// <summary>
         /// path to local file to upload (which will be streamed)
         /// </summary>
         public string fileFullPath = null;
      }

		public RPCResponse<long> CreateVideo(BCVideo video, BCCreateVideoOptions options) {
			return CreateVideo(video, 
            options.filename, options.file, options.encode_to, options.create_multiple_renditions, options.H264NoProcessing, 
            options.preserve_source_rendition, options.maxsize, options.file_checksum, options.fileFullPath);
		}

		/// <summary>
		/// Upload a file to your Brightcove account
		/// </summary>
		/// <param name="video">
		/// The metadata for the video you'd like to create. This takes the form of a 
		/// JSON object of name/value pairs, each of which corresponds to a settable 
		/// property of the Video object.
		/// </param>
		/// <param name="filename">
		/// The name of the file that's being uploaded. You don't need to specify this in 
		/// the JSON if it is specified in the file part of the POST. 
		/// </param>
		/// <param name="file">
		/// A byte array of the video file you're uploading. This takes the 
		/// form of a file part, in a multipart/form-data HTTP request. This input stream and 
		/// the filename and maxSide parameters are automatically inferred from that file part.
		/// </param>
		/// <param name="encode_to">
		/// If the file requires transcoding, use this parameter to specify the target encoding. Valid 
		/// values are MP4 or FLV, representing the H264 and VP6 codecs respectively. Note that transcoding 
		/// of FLV files to another codec is not currently supported. This parameter is optional and defaults to FLV.
		/// </param>
		/// <param name="create_multiple_renditions">
		/// If the file is a supported transcodeable type, this optional flag can be used to control the 
		/// number of transcoded renditions. If true (default), multiple renditions at varying encoding 
		/// rates and dimensions are created. Setting this to false will cause a single transcoded VP6 
		/// rendition to be created at the standard encoding rate and dimensions. 
		/// </param>
		/// <param name="H264NoProcessing">
		/// If the video file is H.264 encoded and if create_multiple_ renditions=true, then multiple 
		/// VP6 renditions are created and in addition the H.264 source is retained as an additional rendition. 
		/// </param>
		/// <param name="preserve_source_rendition">
		/// Use this option to prevent H.264 source files from being transcoded. This parameter cannot be 
		/// used in combination with create_multiple_renditions. It is optional and defaults to false.
		/// </param>
		/// <param name="maxsize">
		/// The maximum size that the file will be. This is used as a limiter to know when 
		/// something has gone wrong with the upload. The maxSize is same as the file you uploaded. 
		/// You don't need to specify this in the JSON if it is specified in the file part of the POST.  
		/// </param>
		/// <param name="file_checksum">
		/// An optional MD5 hash of the file. The checksum can be used to verify that the file checked 
		/// into your Brightcove Media Library is the same as the file you uploaded. 
		/// </param>
		/// <returns>
		/// The id of the video that's been created. if null or error returns -1
		/// </returns>
		private RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, 
         bool H264NoProcessing, bool preserve_source_rendition, long maxsize, string file_checksum, string fileFullPath) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "create_video";
			rpc.parameters = "\"video\": " + video.ToCreateJSON() + ", \"token\": \"" + Account.WriteToken.Value + "\"";
			if (maxsize > -1) {
				rpc.parameters += ", \"maxsize\": " + maxsize.ToString();
			}
			if (file_checksum != null) {
				rpc.parameters += ", \"file_checksum\": \"" + file_checksum + "\"";
			}
			if (!encode_to.Equals(BCEncodeType.UNDEFINED)) {
				rpc.parameters += ", \"encode_to\": " + encode_to.ToString();
			}
			rpc.parameters += ", \"create_multiple_renditions\": " + create_multiple_renditions.ToString().ToLower();
			rpc.parameters += ", \"H264NoProcessing\": " + H264NoProcessing.ToString().ToLower();
			rpc.parameters += ", \"preserve_source_rendition\": " + preserve_source_rendition.ToString().ToLower();
			postParams.Add("json", rpc.ToJSON());

			//add the file to the post
			if(!string.IsNullOrEmpty(filename)){
				rpc.parameters += ", \"filename\": \"" + filename + "\"";
			}
			if (!string.IsNullOrEmpty(filename) && file != null) {
				postParams.Add("file", new FileParameter(file, filename));
			}
         else if (!string.IsNullOrEmpty(fileFullPath)) {
            postParams.Add("file", new UploadFileParameter(fileFullPath));
         }

			//Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = string.IsNullOrEmpty(fileFullPath) ?
            BCAPIRequest.ExecuteWrite(postParams, this.Account) :
            BCAPIRequest.ExecuteWriteNew(postParams, this.Account);

			RPCResponse<long> rpcr2 = new RPCResponse<long>();
			rpcr2.error = rpcr.error;
			rpcr2.id = rpcr.id;
			if (!string.IsNullOrEmpty(rpcr.result)) {
				rpcr2.result = long.Parse(rpcr.result);
			} else {
				rpcr2.result = -1;
			}

			return rpcr2;
		}

		#endregion Create Video

      #region Add Captioning

      public class BCAddCaptioningOptions
      {
         /// <summary>
         /// (optional)	The name of the file that is being uploaded. 
         /// You don't need to specify this in the JSON if it is specified in the file part of the POST.
         /// </summary>
         public string filename;

         /// <summary>
         /// (optional) The maximum size that the file will be. This is used as a limiter to know when something has gone wrong with the upload. 
         /// The maxsize is same as the size of the file you uploaded. You don't need to specify this in the JSON if it is specified in the file part of the POST.
         /// </summary>
         public long maxsize;

         /// <summary>
         /// (optional) An input stream associated with the video file you're uploading. 
         /// This takes the form of a file part, in a multipart/form-data HTTP request. 
         /// This input stream and the filename and maxsize parameters are automatically inferred from that file part.
         /// </summary>
         public string file;

         /// <summary>
         /// (optional)	An optional MD5 hash of the file. 
         /// The checksum can be used to verify that the file checked into your Video Cloud Media Library is the same as the file you uploaded. 
         /// Checksums should use lowercase letters, not uppercase letters.
         /// </summary>
         public string file_checksum;

         /// <summary>
         /// The ID of the video to delete Captioning from. You must specify either video_id or video_reference_id.
         /// </summary>
         public long video_id;

         /// <summary>
         /// The publisher-generated reference ID of the video to delete the Captioning from. You must specify either video_id or video_reference_id.
         /// </summary>
         public string video_reference_id;
      }


      /// <summary>
      /// Adds a Captioning to an existing video
      /// http://docs.brightcove.com/en/media/reference.html#Video_Write
      /// The captions can either be uploaded to BC or be referred to by an external url
      /// BrightCove supports the DFXP and SMPTE-TT formats
      /// http://support.brightcove.com/en/docs/using-captions-smart-player-api
      /// </summary>
      /// <param name="caption_source">Metadata about the captions</param>
      /// <param name="options">Parameters (options) including the ID of the video that MUST be set</param>
      /// <param name="captions">a buffer containing DFXP or SMPTE-TT caption data</param>
      /// <returns>the captionSources object</returns>
      public RPCResponse<BCCaptionSources> AddCaptioning(BCCaptionSource caption_source, BCAddCaptioningOptions options, byte[] captions)
      {
			Dictionary<string, object> postParams = new Dictionary<string, object>();

         Builder builder = new Builder()
            .AppendObject("caption_source", caption_source.ToJSON(JSONType.Create))
            .Append(",").AppendField("token", Account.WriteToken.Value);

         if (!string.IsNullOrEmpty(options.filename))
            builder.Append(",").AppendField("filename", options.filename);

         if (options.maxsize > 0)
            builder.Append(",").AppendField("maxsize", options.maxsize);

         if (!string.IsNullOrEmpty(options.file))
            throw new System.ArgumentException("The file property not supported.  Pass the captions in as a byte array.");

         if (!string.IsNullOrEmpty(options.file_checksum))
            builder.Append(",").AppendField("file_checksum", options.file_checksum);

         // Either a video_id or video_reference_id is required
         if (options.video_id > 0)
            builder.Append(",").AppendField("video_id", options.video_id);
         else if (!string.IsNullOrEmpty(options.video_reference_id))
            builder.Append(",").AppendField("video_reference_id", options.video_reference_id);
         else
            throw new System.ArgumentException("A video_id or video_reference_id is required for add_captioning");

			RPCRequest rpc = new RPCRequest();
			rpc.method = "add_captioning";
         rpc.parameters = builder.ToString();
			postParams.Add("json", rpc.ToJSON());

         postParams.Add("file", new UploadBufferParameter(captions, "captions.dfxp"));

			// Get the JSon reader returned from the APIRequest
			// RPCResponse<BCCaptionSources> rpcr = BCAPIRequest.ExecuteWriteFile<BCCaptionSources>(postParams, this.Account, @"C:\dev\svn\BrightCove\TestFormApp\sample_captions.dfxp");
			RPCResponse<BCCaptionSources> rpcr = BCAPIRequest.ExecuteWriteNew<BCCaptionSources>(postParams, this.Account);

         return rpcr;
      }

      #endregion Add Captioning

      #region Delete Captioning

      public RPCResponse DeleteCaptioning(long video_id) {
         return DeleteCaptioning(video_id, null);
      }
      public RPCResponse DeleteCaptioning(string video_reference_id) {
         return DeleteCaptioning(-1, video_reference_id);
      }

      private RPCResponse DeleteCaptioning(long video_id, string video_reference_id)
      {
			Dictionary<string, object> postParams = new Dictionary<string, object>();

         Builder builder = new Builder()
            .AppendField("token", Account.WriteToken.Value);

         // Either a video_id or video_reference_id is required
         if (video_id > 0)
            builder.Append(",").AppendField("video_id", video_id);
         else if (!string.IsNullOrEmpty(video_reference_id))
            builder.Append(",").AppendField("video_reference_id", video_reference_id);
         else
            throw new System.ArgumentException("A video_id or video_reference_id is required for delete_captioning");

			RPCRequest rpc = new RPCRequest();
			rpc.method = "delete_captioning";
         rpc.parameters = builder.ToString();
			postParams.Add("json", rpc.ToJSON());

			// Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = BCAPIRequest.ExecuteWrite(postParams, this.Account);

         return rpcr;
      }

      #endregion Delete Captioning

      #region Update Video

      /// <summary>
		/// Updates the video you specify
		/// </summary>
		/// <param name="video">
		/// The metadata for the video you'd like to update. This takes the form of a JSON object of name/value pairs, each of which corresponds to a settable property of the Video object. 
		/// </param>
		/// <returns></returns>
		public RPCResponse<BCVideo> UpdateVideo(BCVideo video) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "update_video";
			rpc.parameters = "\"video\": " + video.ToJSON() + " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCVideo> rpcr = BCAPIRequest.ExecuteWrite<BCVideo>(postParams, Account);

			return rpcr;
		}

		#endregion Update Video

		#region Delete Video

		//delete by video id
		public RPCResponse DeleteVideo(long video_id) {
			return DeleteVideo(video_id, true);
		}

		public RPCResponse DeleteVideo(long video_id, bool cascade) {
			return DeleteVideo(video_id, cascade, true);
		}

		public RPCResponse DeleteVideo(long video_id, bool cascade, bool delete_shares) {
			return DeleteVideo(video_id, null, cascade, delete_shares);
		}

		//delete by reference id
		public RPCResponse DeleteVideo(string reference_id) {
			return DeleteVideo(reference_id, true);
		}

		public RPCResponse DeleteVideo(string reference_id, bool cascade) {
			return DeleteVideo(reference_id, cascade, true);
		}

		public RPCResponse DeleteVideo(string reference_id, bool cascade, bool delete_shares) {
			return DeleteVideo(-1, reference_id, cascade, delete_shares);
		}

		/// <summary>
		/// Deletes a video.
		/// </summary>
		/// <param name="video_id">
		/// The id of the video you'd like to delete
		/// </param>
		/// <param name="reference_id">
		/// The publisher-assigned reference id of the video you'd like to delete.
		/// </param>
		/// <param name="cascade">
		/// Set this to true if you want to delete this video even if it is part of a 
		/// manual playlist or assigned to a player. The video will be removed from 
		/// all playlists and players in which it appears, then deleted. 
		/// defaults to true
		/// </param>
		/// <param name="delete_shares">
		/// Set this to true if you want also to delete shared copies of this video. 
		/// Note that this will delete all shared copies from sharee accounts, regardless 
		/// of whether or not those accounts are currently using the video in playlists or players.
		/// defaults to true
		/// </param>
		private RPCResponse DeleteVideo(long video_id, string reference_id, bool cascade, bool delete_shares) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "delete_video";
			if (video_id > -1) {
				rpc.parameters = "\"video_id\": " + video_id.ToString();
			} else if (reference_id != null) {
				rpc.parameters = "\"reference_id\": \"" + reference_id.ToString() + "\"";
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			rpc.parameters += ", \"cascade\": " + cascade.ToString().ToLower();
			rpc.parameters += ", \"delete_shares\": " + delete_shares.ToString().ToLower();
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = BCAPIRequest.ExecuteWrite(postParams, Account);

			return rpcr;
		}

		#endregion Delete Video

		#region Get Upload Status

		public RPCResponse<UploadStatusEnum> GetUploadStatus(string reference_id) {
			return GetUploadStatus(-1, reference_id);
		}

		public RPCResponse<UploadStatusEnum> GetUploadStatus(long video_id) {
			return GetUploadStatus(video_id, null);
		}

		/// <summary>
		/// Call this function in an HTTP POST request to determine the status of an upload.
		/// </summary>
		/// <param name="video_id">
		/// The id of the video whose status you'd like to get.
		/// </param>
		/// <param name="reference_id">
		/// The publisher-assigned reference id of the video whose status you'd like to get.
		/// </param>
		/// <returns>
		/// an UploadStatusEnum that specifies the current state of the upload.
		/// </returns>
		private RPCResponse<UploadStatusEnum> GetUploadStatus(long video_id, string reference_id) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "get_upload_status";
			if (video_id > -1) {
				rpc.parameters = "\"video_id\": " + video_id.ToString();
			} else if (reference_id != null) {
				rpc.parameters = "\"reference_id\": " + video_id.ToString();
			}
			rpc.parameters += " ,\"token\": \"" + Account.WriteToken.Value + "\"";

			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = BCAPIRequest.ExecuteWrite(postParams, Account);
			RPCResponse<UploadStatusEnum> rpcr2 = new RPCResponse<UploadStatusEnum>();
			rpcr2.error = rpcr.error;
			rpcr2.id = rpcr.id;

			switch (rpcr.result) {
				case "COMPLETE":
					rpcr2.result = UploadStatusEnum.COMPLETE;
					break;
				case "ERROR":
					rpcr2.result = UploadStatusEnum.ERROR;
					break;
				case "PROCESSING":
					rpcr2.result = UploadStatusEnum.PROCESSING;
					break;
				case "UPLOADING":
					rpcr2.result = UploadStatusEnum.UPLOADING;
					break;
				default:
					rpcr2.result = UploadStatusEnum.UNDEFINED;
					break;
			}
			return rpcr2;
		}

		#endregion Get Upload Status

		#region Share Video

		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, long sharee_account_id) {
			return ShareVideo(video_id, false, sharee_account_id);
		}

		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, bool auto_accept, long sharee_account_id) {

			List<long> sharee_account_ids = new List<long>();
			sharee_account_ids.Add(sharee_account_id);

			return ShareVideo(video_id, false, sharee_account_ids);
		}

		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, List<long> sharee_account_ids) {
			return ShareVideo(video_id, false, sharee_account_ids);
		}

		/// <summary>
		/// Shares the specified video with a list of sharee accounts
		/// </summary>
		/// <param name="video_id">
		/// The id of the video whose status you'd like to get.
		/// </param>
		/// <param name="auto_accept">
		/// If the target account has the option enabled, setting this flag to true will bypass 
		/// the approval process, causing the shared video to automatically appear in the target 
		/// account's library. If the target account does not have the option enabled, or this 
		/// flag is unspecified or false, then the shared video will be queued up to be approved 
		/// by the target account before appearing in their library.	
		/// defaults to false
		/// </param>
		/// <param name="sharee_account_ids">
		/// List of Account IDs to share video with.
		/// </param>
		/// <returns></returns>
		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, bool auto_accept, List<long> sharee_account_ids) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "share_video";
			rpc.parameters = "\"video_id\": " + video_id;
			rpc.parameters += ", \"auto_accept\": " + auto_accept.ToString().ToLower();
			rpc.parameters += ", \"sharee_account_ids\": [";
			for (int i = 0; i < sharee_account_ids.Count; i++) {
				if (i > 0) {
					rpc.parameters += ", ";
				}
				rpc.parameters += "\"" + sharee_account_ids[i].ToString() + "\"";
			}
			rpc.parameters += "]";
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCCollection<long>> rpcr = BCAPIRequest.ExecuteWrite<BCCollection<long>>(postParams, Account);

			return rpcr;
		}

		#endregion Share Video

		#region Unshare Video

		public RPCResponse<BCCollection<long>> UnshareVideo(long video_id, long sharee_account_id) {
			
			List<long> sharee_account_ids = new List<long>();
			sharee_account_ids.Add(sharee_account_id);

			return UnshareVideo(video_id, sharee_account_ids);
		}

		/// <summary>
		/// Deletes the specified previously shared video from a list of sharee accounts. If a shared version of the specified video does not exist in a sharee account, no action is taken. For more information, see Media Sharing with the Media API.
		/// </summary>
		/// <param name="video_id">The ID for video that will be shared. The video be owned by the account making the call.</param>
		/// <param name="sharee_account_ids">List of Account IDs from which to stop sharing the video.</param>
		/// <returns>Array of sharee account IDs for accounts previously containing shared videos specifically removed by this method.</returns>
		public RPCResponse<BCCollection<long>> UnshareVideo(long video_id, List<long> sharee_account_ids) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "unshare_video";
			rpc.parameters = "\"video_id\": " + video_id;
			rpc.parameters += ", \"sharee_account_ids\": [";
			for (int i = 0; i < sharee_account_ids.Count; i++) {
				if (i > 0) {
					rpc.parameters += ", ";
				}
				rpc.parameters += "\"" + sharee_account_ids[i].ToString() + "\"";
			}
			rpc.parameters += "]";
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCCollection<long>> rpcr = BCAPIRequest.ExecuteWrite<BCCollection<long>>(postParams, Account);

			return rpcr;
		}

		#endregion Unshare Video

		#region Add Image

		//using video id
		public RPCResponse<BCImage> AddImage(BCImage image, long video_id) {
			return AddImage(image, "", video_id);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, long video_id) {
			return AddImage(image, filename, null, video_id);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id) {
			return AddImage(image, filename, file, video_id, true);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, bool resize) {
			return AddImage(image, filename, file, video_id, resize, -1);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, bool resize, long maxsize) {
			return AddImage(image, filename, file, video_id, resize, maxsize, null);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, bool resize, long maxsize, string file_checksum) {
			return AddImage(image, filename, file, video_id, null, resize, maxsize, file_checksum);
		}

		//using ref id
		public RPCResponse<BCImage> AddImage(BCImage image, string video_reference_id) {
			return AddImage(image, "", video_reference_id);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, string video_reference_id) {
			return AddImage(image, filename, null, video_reference_id);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id) {
			return AddImage(image, filename, file, video_reference_id, true);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id, bool resize) {
			return AddImage(image, filename, file, video_reference_id, resize, -1);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id, bool resize, long maxsize) {
			return AddImage(image, filename, file, video_reference_id, resize, maxsize, null);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id, bool resize, long maxsize, string file_checksum) {
			return AddImage(image, filename, file, -1, video_reference_id, resize, maxsize, file_checksum);
		}

		/// <summary>
		/// Add a new thumbnail or video still image to a video, or assign an existing image to another video.
		/// </summary>
		/// <param name="image">
		/// The metadata for the image you'd like to create (or update). This takes the form of a 
		/// JSON object of name/value pairs, each of which corresponds to a property of the Image object. 
		/// </param>
		/// <param name="filename">
		/// The name of the file that's being uploaded. You don't need to specify this in the JSON 
		/// if it is specified in the file part of the POST. 
		/// </param>
		/// <param name="maxsize">
		/// The maximum size that the file will be. This is used as a limiter to know when something 
		/// has gone wrong with the upload. The maxSize is same as the file you uploaded. You don't 
		/// need to specify this in the JSON if it is specified in the file part of the POST.
		/// </param>
		/// <param name="file">
		/// An input stream associated with the image file you're uploading. This takes the form of a 
		/// file part, in a multipart/form-data HTTP request. This input stream and the filename and 
		/// maxSize parameters are automatically inferred from that file part. 
		/// </param>
		/// <param name="file_checksum">
		/// An optional MD5 hash of the file. The checksum can be used to verify that the file checked 
		/// into your Brightcove Media Library is the same as the file you uploaded. 
		/// </param>
		/// <param name="video_id">
		/// The ID of the video you'd like to assign an image to.
		/// </param>
		/// <param name="video_reference_id">
		/// The publisher-assigned reference ID of the video you'd like to assign an image to.
		/// </param>
		/// <param name="resize">
		/// Set this to false if you don't want your image to be automatically resized to the default 
		/// size for its type. By default images will be resized. 
		/// </param>
		/// <returns></returns>
		private RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, string video_reference_id, bool resize, long maxsize, string file_checksum) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "add_image";
			rpc.parameters = "\"image\": " + image.ToJSON();
			rpc.parameters += ", \"filename\": \"" + filename + "\"";
			if (video_id > -1) {
				rpc.parameters += ",\"video_id\": \"" + video_id.ToString() + "\"";
			} else if (video_reference_id != null) {
				rpc.parameters += ",\"video_reference_id\": \"" + video_reference_id + "\"";
			}
			if (maxsize > -1) {
				rpc.parameters += ", \"maxsize\": \"" + maxsize.ToString() + "\"";
			}

			if (file_checksum != null) {
				rpc.parameters += ", \"file_checksum\": \"" + file_checksum + "\"";
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//add the file to the post
			if (file != null && !string.IsNullOrEmpty(filename)) {
				postParams.Add("file", new FileParameter(file, filename));
			}

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCImage> rpcr = BCAPIRequest.ExecuteWrite<BCImage>(postParams, Account);

			return rpcr;
		}

		#endregion Add Image

		#region Add Logo Overlay

		//using video id
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, long video_id) {
			return AddLogoOverlay(logo_overlay, null, video_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, byte[] file, long video_id) {
			return AddLogoOverlay(logo_overlay, file, null, video_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, byte[] file, string file_checksum, long video_id) {
			return AddLogoOverlay(logo_overlay, -1, file, file_checksum, video_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, long maxsize, byte[] file, string file_checksum, long video_id) {
			return AddLogoOverlay(logo_overlay, "", maxsize, file, file_checksum, video_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, string filename, long maxsize, byte[] file, string file_checksum, long video_id) {
			return AddLogoOverlay(logo_overlay, filename, maxsize, file, file_checksum, video_id, null);
		}
		
		//using ref id
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, string video_reference_id) {
			return AddLogoOverlay(logo_overlay, null, video_reference_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, byte[] file, string video_reference_id) {
			return AddLogoOverlay(logo_overlay, file, "", video_reference_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, byte[] file, string file_checksum, string video_reference_id) {
			return AddLogoOverlay(logo_overlay, -1, file, file_checksum, video_reference_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, long maxsize, byte[] file, string file_checksum, string video_reference_id) {
			return AddLogoOverlay(logo_overlay, "", maxsize, file, file_checksum, video_reference_id);
		}
		public RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, string filename, long maxsize, byte[] file, string file_checksum, string video_reference_id) {
			return AddLogoOverlay(logo_overlay, filename, maxsize, file, file_checksum, -1, video_reference_id);
		}

		/// <summary>
		/// Add a logo overlay image to a video. For code examples, see Adding Logo Overlays to Videos with the Media API: http://support.brightcove.com/en/docs/adding-logo-overlays-videos-media-api
		/// </summary>
		/// <param name="logo_overlay">The metadata for the logo overlay you want to create (or update). This takes the form of a JSON object of name/value pairs, each of which corresponds to a property of the LogoOverlay object.</param>
		/// <param name="file_name">The name of the file that's being uploaded. You don't need to specify this in the JSON if it is specified in the file part of the POST.</param>
		/// <param name="maxsize">The maximum size that the file will be. This is used as a limiter to know when something has gone wrong with the upload. The maxsize is same as the file you uploaded. You don't need to specify this in the JSON if it is specified in the file part of the POST.</param>
		/// <param name="file">An input stream associated with the image file you're uploading. This takes the form of a file part, in a multipart/form-data HTTP request. This input stream and the filename and maxsize parameters are automatically inferred from that file part.</param>
		/// <param name="file_checksum">An optional MD5 hash of the file. The checksum can be used to verify that the file checked into your Video Cloud Media Library is the same as the file you uploaded.</param>
		/// <param name="video_id">The ID of the video you want to assign a logo overlay to. You must specify either a video_id or a video_reference_id.</param>
		/// <param name="video_reference_id">The publisher-assigned reference ID of the video you want to assign a logo overlay to. You must specify either a video_id or a video_reference_id.</param>
		/// <returns>LogoOverlay</returns>
		private RPCResponse<LogoOverlay> AddLogoOverlay(LogoOverlay logo_overlay, string filename, long maxsize, byte[] file, string file_checksum, long video_id, string video_reference_id) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "add_logo_overlay";
			rpc.parameters += ", \"filename\": \"" + filename + "\"";
			if (video_id > -1) {
				rpc.parameters += ",\"video_id\": \"" + video_id.ToString() + "\"";
			} else if (video_reference_id != null) {
				rpc.parameters += ",\"video_reference_id\": \"" + video_reference_id + "\"";
			}
			if (maxsize > -1) {
				rpc.parameters += ", \"maxsize\": \"" + maxsize.ToString() + "\"";
			}

			if (file_checksum != null) {
				rpc.parameters += ", \"file_checksum\": \"" + file_checksum + "\"";
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//add the file to the post
			if (file != null && !string.IsNullOrEmpty(filename)) {
				postParams.Add("file", new FileParameter(file, filename));
			}
			
			//Get the JSon reader returned from the APIRequest
			RPCResponse<LogoOverlay> rpcr = BCAPIRequest.ExecuteWrite<LogoOverlay>(postParams, Account);

			return rpcr;
		}
		#endregion Add Logo Overlay

		#region Remove Logo Overlay

		public RPCResponse<BCVideo> RemoveLogoOverlay(string video_reference_id) {
			return RemoveLogoOverlay(-1, video_reference_id);
		}
		public RPCResponse<BCVideo> RemoveLogoOverlay(long videoId) {
			return RemoveLogoOverlay(videoId, null);
		}
		/// <summary>
		/// Removes a logo overlay previously assigned to a video.
		/// </summary>
		/// <param name="videoId">The ID of the video to remove the logo overlay from. You must specify either a video_id or a video_reference_id</param>
		/// <param name="reference_id">The publisher-assigned reference ID of the video to remove the logo overlay from. You must specify either a video_id or a video_reference_id.</param>
		/// <returns>void</returns>
		private RPCResponse<BCVideo> RemoveLogoOverlay(long video_id, string video_reference_id) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "remove_logo_overlay";
			if (video_id > -1) {
				rpc.parameters += ",\"video_id\": \"" + video_id.ToString() + "\"";
			} else if (video_reference_id != null) {
				rpc.parameters += ",\"video_reference_id\": \"" + video_reference_id + "\"";
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCVideo> rpcr = BCAPIRequest.ExecuteWrite<BCVideo>(postParams, Account);

			return rpcr;
		}
		#endregion Remove Logo Overlay

		#endregion Video Write
	}
}
