using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Web;
using umbraco.cms.businesslogic.datatype.controls;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core;
using umbraco.cms.businesslogic.property;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for editMedia.
	/// </summary>
	public partial class editMedia : BasePages.UmbracoEnsuredPage
	{
        private readonly uicontrols.Pane _mediaPropertiesPane = new uicontrols.Pane();
        private readonly LiteralControl _updateDateLiteral = new LiteralControl();
        private readonly LiteralControl _mediaFileLinksLiteral = new LiteralControl();

	    public editMedia()
	    {
	        CurrentApp = BusinessLogic.DefaultApps.media.ToString();
	    }

		protected uicontrols.TabView TabView1;
		protected System.Web.UI.WebControls.TextBox documentName;
		private cms.businesslogic.media.Media _media;
		controls.ContentControl tmp;

		//protected System.Web.UI.WebControls.Literal SyncPath;

        protected override void OnInit(EventArgs e)
        {
            

            base.OnInit(e);

            _media = new cms.businesslogic.media.Media(int.Parse(Request.QueryString["id"]));

            // Save media on first load
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @nodeId",
                                       SqlHelper.CreateParameter("@nodeId", _media.Id)) > 0;
            if (!exists)
                _media.XmlGenerate(new XmlDocument());


            tmp = new controls.ContentControl(_media, controls.ContentControl.publishModes.NoPublish, "TabView1");
            tmp.Width = Unit.Pixel(666);
            tmp.Height = Unit.Pixel(666);
            plc.Controls.Add(tmp);

            tmp.Save += new System.EventHandler(Save);

            this._updateDateLiteral.ID = "updateDate";
            this._updateDateLiteral.Text = _media.VersionDate.ToShortDateString() + " " + _media.VersionDate.ToShortTimeString();

            this._mediaFileLinksLiteral.ID = "mediaFileLinks";
            _mediaPropertiesPane.addProperty(ui.Text("content", "updateDate", base.getUser()), this._updateDateLiteral);

            this.UpdateMediaFileLinksLiteral();
            _mediaPropertiesPane.addProperty(ui.Text("content", "mediaLinks"), this._mediaFileLinksLiteral);

            // add the property pane to the page rendering
            tmp.tpProp.Controls.AddAt(1, _mediaPropertiesPane);                       
        }

		protected void Page_Load(object sender, System.EventArgs e)
		{
			//if (!IsPostBack) 
			//{
			//    SyncPath.Text = _media.Path;
			//    newName.Text = _media.Text.Replace("'", "\\'");
			//}
			if (!IsPostBack)
			{
				ClientTools.SyncTree(_media.Path, 
					//if 'saved' is force reload
					Request.GetItemAs<bool>("saved"));
			}			
		}

		protected void Save(object sender, System.EventArgs e) 
		{
            // error handling test
            if (!Page.IsValid)
            {
                foreach (uicontrols.TabPage tp in tmp.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorHeader");
                    tp.CloseCaption = ui.Text("close");
                }
            }
            else if (Page.IsPostBack)
            {
                // hide validation summaries
                foreach (uicontrols.TabPage tp in tmp.GetPanels())
                {
                    tp.ErrorControl.Visible = false;
                }
            }

            //The value of the properties has been set on IData through IDataEditor in the ContentControl
            //so we need to 'retrieve' that value and set it on the property of the new IContent object.
            //NOTE This is a workaround for the legacy approach to saving values through the DataType instead of the Property 
            //- (The DataType shouldn't be responsible for saving the value - especically directly to the db).
            foreach (var item in tmp.DataTypes)
            {
                _media.getProperty(item.Key).Value = item.Value.Data.Value;
            }

            _media.Save();

            this._updateDateLiteral.Text = _media.VersionDate.ToShortDateString() + " " + _media.VersionDate.ToShortTimeString();
            this.UpdateMediaFileLinksLiteral();

			_media.XmlGenerate(new XmlDocument());

			this.RedirectToSelfIfValid(
				new
					{
						saved = true,
					},
				() => ClientTools.SyncTree(_media.Path, true));
		}

        private void UpdateMediaFileLinksLiteral()
        {
            var uploadField = new Factory().GetNewObject(new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));

            // always clear, incase the upload file was removed
            this._mediaFileLinksLiteral.Text = string.Empty;

            try
            {
                var uploadProperties = _media.GenericProperties
                    .Where(p => p.PropertyType.DataTypeDefinition.DataType.Id == uploadField.Id
                                && p.Value.ToString() != ""
                                && File.Exists(IOHelper.MapPath(p.Value.ToString())));

                var properties = uploadProperties as List<Property> ?? uploadProperties.ToList();

                if (properties.Any())
                {
                    this._mediaFileLinksLiteral.Text += "<table>";

                    foreach (var property in properties)
                    {
                        this._mediaFileLinksLiteral.Text += string.Format("<tr><td>{0}&nbsp;</td><td><a href=\"{1}\" target=\"_blank\">{1}</a></td></tr>", property.PropertyType.Name, property.Value);
                    }
                    
                    this._mediaFileLinksLiteral.Text += "</table>";
                }
            }
            catch
            {
                //the data type definition may not exist anymore at this point because another thread may
                //have deleted it.
            }
        }
	}
}
