== CODE BEHIND == //also adjust the namespace to match your project

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using BrightcoveSDK;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.Containers;

namespace testsite.BC {
    public partial class VideoWriteMethods : System.Web.UI.Page {

        public BCAPI bc;

        protected void Page_Load(object sender, EventArgs e) {

            BrightcoveConfig config = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
            bc = new BCAPI(config.Accounts[0].PublisherID);

        }

        protected void CreateVideo() {
            //the uvVideo is a front end control 
            if (IsPostBack) {
                BCVideo newVid = new BCVideo();
                newVid.name = uvVideo.FileName;
                newVid.longDescription = "long description";
                newVid.shortDescription = "short desc";
                newVid.tags.Add("created");
                newVid.tags.Add("now vid");
                newVid.economics = BCVideoEconomics.AD_SUPPORTED;

                RPCResponse<long> rpcr = bc.CreateVideo(newVid, uvVideo.FileName, uvVideo.FileBytes);

                if (rpcr.error.message != null) {
                    Response.Write(rpcr.error.code);
                    Response.Write(rpcr.error.message);
                } else {
                    Response.Write(rpcr.result);
                }
            }
        }

        protected void UpdateVideo() {
            BCVideo sender = bc.FindVideoById(31270419001);
            sender.name = "now changed";
            RPCResponse<BCVideo> returned = bc.UpdateVideo(sender);
            ltlResult.Text = returned.result.name;
        }

        protected void DeleteVideo() {
            //delete by id
            RPCResponse rpcr = bc.DeleteVideo(40899784001);

            if (rpcr.error.message != null) {
                Response.Write(rpcr.error.message);
            } else {
                Response.Write("worked");
            }
            //delete by ref id
            RPCResponse rpcr2 = bc.DeleteVideo("myrefid");

            if (rpcr2.error.message != null) {
                Response.Write(rpcr2.error.message);
                Response.Write(rpcr2.error.code);
            } else {
                Response.Write("worked");
            }
        }

        protected void GetUploadStatus() {
            RPCResponse<UploadStatusEnum> rpcr = bc.GetUploadStatus(42029561001);
            if (rpcr.error.message != null) {
                Response.Write(rpcr.error.message);
            } else {
                Response.Write(rpcr.result.ToString());
            }
        }

        protected void ShareVideo() {
            RPCResponse<BCCollection<long>> vidIds = bc.ShareVideo(40899777001, true, 8385542001);
            foreach (long id in vidIds.result) {
                Response.Write(id.ToString());
            }
        }

        protected void AddImage() {

            //the uvVideo is a front end control 
            if (IsPostBack) {
                BCImage newImg = new BCImage();
                newImg.displayName = uvVideo.FileName;
                newImg.referenceId = "refid";
                newImg.type = ImageTypeEnum.VIDEO_STILL;
                RPCResponse<BCImage> rpcr = bc.AddImage(newImg, uvVideo.FileName, uvVideo.FileBytes, 42029561001);

                if (rpcr.error.message != null) {
                    Response.Write(rpcr.error.code);
                    Response.Write(rpcr.error.message);
                } else {
                    Response.Write(rpcr.result.displayName);
                }
            }
        }
    }
}

```