using System;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace BrightcoveSDK.HTTP
{
	public class MultipartForm
	{
		private static readonly Encoding encoding = Encoding.UTF8;

      private static readonly int DEFAULT_STREAM_BUFFER_SIZE = 1024 * 1024; // Stream the file in 1 MB chunks
      private int BufferSize;

      private string userAgent;
      private string formDataBoundary;
      private string contentType;
      private string postUrl;
      private Dictionary<string, object> postParameters;

      /// <summary>
      /// Helper class to build streams for each chunk of the form
      /// </summary>
      private class FormDataChunk
      {
         public Stream Stream { get; set; }
         public long Length { get; set; }

         public FormDataChunk() { }

         public FormDataChunk(string s) : this(encoding.GetBytes(s)) { }

         public FormDataChunk(byte[] buffer)
         {
            Stream = new MemoryStream(buffer);
            Length = buffer.Length;
         }

         public FormDataChunk(FileInfo fi)
         {
            Stream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
            Length = fi.Length;
         }
      }

      List<FormDataChunk> FormData = null;

      private string FormDataPart(string content)
      {
         return string.Format("--{0}\r\nContent-Disposition: form-data; {1}", formDataBoundary, content);
      }

      public MultipartForm() : this(DEFAULT_STREAM_BUFFER_SIZE) { }

      public MultipartForm(int BufferSize)
      {
         this.BufferSize = BufferSize;
         ResetForm();
      }

      /// <summary>
      /// Clear out all the form data (from BuildForm call)
      /// </summary>
      public void ResetForm()
      {
         this.userAgent = null;
         this.postUrl = null ;
         this.postParameters = null;

			formDataBoundary = null;
			contentType = null;

         FormData = null;
      }

      /// <summary>
      /// Build the multipart form for submission
      /// </summary>
      public void BuildForm(string postUrl, string userAgent, Dictionary<string, object> postParameters)
      {
         ResetForm();  // just for good measure...

         this.userAgent = userAgent;
         this.postUrl = postUrl;
         this.postParameters = postParameters;

			formDataBoundary = "-----------------------------" + DateTime.Now.Ticks.ToString().Substring(0, 14);
			contentType = "multipart/form-data; boundary=" + formDataBoundary;

         FormData = new List<FormDataChunk>();

			foreach (var param in postParameters) 
         {
				if (param.Value is UploadFileParameter) 
            {
					UploadFileParameter fileToUpload = (UploadFileParameter)param.Value;
               FileInfo fi = new FileInfo(fileToUpload.FilePath);
               if (!fi.Exists)
                  throw new ArgumentException(string.Format("File to upload [{0}] does not exist", fileToUpload.FilePath));

               // do not include the full path in the "filename" set in the form
               string filename = fi.Name ?? param.Key;

					// Build the header part
					string header = FormDataPart(string.Format("name=\"{0}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n", param.Key, filename, fileToUpload.ContentType));

               // Add file header information
               FormData.Add(new FormDataChunk(header));

               // Add the file data itself
               FormData.Add(new FormDataChunk(fi));
				}
            else if (param.Value is UploadBufferParameter)
            {
               UploadBufferParameter uploadBuffer = (UploadBufferParameter)param.Value;

               // do not include the full path in the "filename" set in the form
               string filename = uploadBuffer.FileName ?? param.Key;

					// Build the header part
					string header = FormDataPart(string.Format("name=\"{0}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n", param.Key, filename, uploadBuffer.ContentType));

               // Add file header information
               FormData.Add(new FormDataChunk(header));

               // Add the file data itself
               FormData.Add(new FormDataChunk(uploadBuffer.Data));
            }
				else 
            {
               // Add just a simple key value pair
					string postData = FormDataPart(string.Format("name=\"{0}\"\r\n\r\n{1}\r\n", param.Key, param.Value));

               FormData.Add(new FormDataChunk(postData));
				}
			}

         // Add the footer
         FormData.Add(new FormDataChunk(string.Format("\r\n--{0}--\r\n", formDataBoundary)));
      }

      /// <summary>
      /// Now Post the form!
      /// Note: you must call BuildForm before posting
      /// </summary>
      public HttpWebResponse PostForm()
      {
         if (FormData == null)
            throw new Exception("BuildForm before posting");

         if (FormData.Count == 0)
            throw new Exception("FormData is empty");

			HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;
			if (request == null)
				throw new NullReferenceException("request is not a http request");

         long length = 0;
         foreach (FormDataChunk chunk in FormData)
            length += chunk.Length;

			// Set up the request properties
			request.Method = "POST";
			request.ContentType = contentType;
			request.UserAgent = userAgent;
			request.CookieContainer = new CookieContainer();
			request.ContentLength = length;  // We need to count how many bytes we're sending (before sending).

         // walk the Streams in the FormData chunks and send them to the request stream in chunks
         using (Stream requestStream = request.GetRequestStream())
         {
            foreach (FormDataChunk chunk in FormData)
            {
               chunk.Stream.Seek(0, SeekOrigin.Begin);
               
               using (BinaryReader br = new BinaryReader(chunk.Stream))
               {
                  for (byte[] bytes = br.ReadBytes(BufferSize); bytes.Length > 0; bytes = br.ReadBytes(BufferSize))
                  {
                     requestStream.Write(bytes, 0, bytes.Length);
                     requestStream.Flush(); // send it over the wire!
                  }
                  br.Close();
               }

               chunk.Stream.Close();
            }
            
            // one final flush and close
            requestStream.Flush();
				requestStream.Close();
         }

			return request.GetResponse() as HttpWebResponse;
      }

      /// <summary>
      /// Builds a string with the form information for troubleshootings
      /// This can be called after BuildForm()
      /// </summary>
      public override string ToString()
      {
         if (FormData == null)
            return "The form has not been built yet.";

         var sb = new StringBuilder();

         sb.AppendFormat("Buffer Size: {0}\r\n", BufferSize);
         sb.AppendFormat("Post URL: {0}\r\n", postUrl);
         sb.AppendFormat("User Agent: {0}\r\n", userAgent);
         
         sb.Append("Parameters: \r\n");
         foreach (string key in postParameters.Keys)
            sb.AppendFormat("   {0} ==> {1}\r\n", key, postParameters[key]);
         
         sb.Append("\r\nForm Data Parts\r\n");
         foreach (FormDataChunk chunk in FormData)
         {
            using (BinaryReader br = new BinaryReader(chunk.Stream))
            {
               byte[] bytes = br.ReadBytes(500);
               sb.Append(encoding.GetString(bytes));
               if (bytes.Length >= 500)
                  sb.Append("\r\n... (truncated) ...\r\n");
               br.Close();
            }
         }

         return sb.ToString();
      }
	}

   /// <summary>
   /// Represents a file on disk to be read in and included in the form
   /// </summary>
	public class UploadFileParameter
	{
		/// <summary>
		/// Fully qualified path of the file
		/// </summary>
		public string FilePath { get; set; }
		
		public string ContentType { get; set; }

		public UploadFileParameter(string filepath) : this(filepath, null) {}

		public UploadFileParameter(string filepath, string contenttype) {
			this.FilePath = filepath;
			this.ContentType = contenttype ?? "application/octet-stream";
		}
	}

	/// <summary>
	/// Represents a binary buffer of data to be included in the form
	/// </summary>
	public class UploadBufferParameter
	{
		/// <summary>
		/// Binary buffer to upload
		/// </summary>
		public byte[] Data { get; set; }

		/// <summary>
		/// Filename to place in the upload information (does not have to correspond to a real file)
		/// </summary>
		public string FileName { get; set; }
		
		public string ContentType { get; set; }

		public UploadBufferParameter(byte[] data, string filename) : this(data, filename, null) { }

		public UploadBufferParameter(byte[] data, string filename, string contenttype) {
			this.Data = data;
			this.FileName = filename;
			this.ContentType = contenttype ?? "application/octet-stream";
		}
	}
}