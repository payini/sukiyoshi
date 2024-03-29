﻿namespace BrightcoveSDK.SitecoreUtil.UI.HtmlControls
{
	using Sitecore;
	using Sitecore.Collections;
	using Sitecore.Data;
	using Sitecore.Data.Events;
	using Sitecore.Data.Items;
	using Sitecore.Data.Managers;
	using Sitecore.Data.Templates;
	using Sitecore.Diagnostics;
	using Sitecore.Globalization;
	using Sitecore.Resources;
	using Sitecore.Shell.Framework;
	using Sitecore.Shell.Framework.Commands;
	using Sitecore.Sites;
	using Sitecore.Text;
	using Sitecore.Web;
	using Sitecore.Web.UI.HtmlControls;
	using Sitecore.Web.UI.HtmlControls.Data;
	using Sitecore.Web.UI.Sheer;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using Sitecore.Web.UI.WebControls;
	using ItemCollection = Sitecore.Collections.ItemCollection;

	public class BCTreeviewEx : Sitecore.Web.UI.XmlControls.XmlControl
	{
        private Item _parentItem;
        private List<string> _selected;
        private readonly List<Item> _updatedItems = new List<Item>();
        private ID enabledItemsTemplateId;
        
        private void AddUpdatedItem(Item item, bool updateParent)
        {
            if (item != null)
            {
                if (updateParent)
                {
                    item = item.Parent;
                    if (item == null)
                    {
                        return;
                    }
                }
                foreach (Item item2 in this._updatedItems)
                {
                    if (item2.Axes.IsAncestorOf(item))
                    {
                        return;
                    }
                }
                for (int i = this._updatedItems.Count - 1; i >= 0; i--)
                {
                    Item item3 = this._updatedItems[i];
                    if (item.Axes.IsAncestorOf(item3))
                    {
                        this._updatedItems.Remove(item3);
                    }
                }
                this._updatedItems.Add(item);
            }
        }
        
        private void DataContext_OnChanged(object sender)
        {
            Sitecore.Web.UI.HtmlControls.DataContext dataContext = this.GetDataContext();
            if (dataContext != null)
            {
                this.UpdateFromDataContext(dataContext);
            }
        }
        
        protected void Drop(string data)
        {
            if ((data != null) && data.StartsWith("sitecore:"))
            {
                data = data.Substring(9);
                if (data.StartsWith("item:"))
                {
                    data = data.Substring(5);
                    int length = data.LastIndexOf(",");
                    if (length > 0)
                    {
                        string dragID = GetDragID(StringUtil.Left(data, length));
                        string str2 = GetDragID(StringUtil.Mid(data, length + 1));
                        Item target = Client.ContentDatabase.Items[str2];
                        Item item = Client.ContentDatabase.Items[dragID];
                        if ((target != null) && (item != null))
                        {
                            Items.DragTo(item, target, Sitecore.Context.ClientPage.ClientRequest.CtrlKey, !Sitecore.Context.ClientPage.ClientRequest.ShiftKey, !Sitecore.Context.ClientPage.ClientRequest.AltKey);
                        }
                        else if (item == null)
                        {
                            SheerResponse.Alert("The source item could not be found.\n\nIt may have been deleted by another user.", new string[0]);
                        }
                        else
                        {
                            SheerResponse.Alert("The target item could not be found.\n\nIt may have been deleted by another user.", new string[0]);
                        }
                    }
                }
            }
        }
        
        protected virtual void GetContextMenu()
        {
            this.GetContextMenu("below-right");
        }
        
        protected virtual void GetContextMenu(string where)
        {
            Assert.ArgumentNotNullOrEmpty(where, "where");
            IDataView dataView = this.GetDataView();
            if (dataView != null)
            {
                string source = Sitecore.Context.ClientPage.ClientRequest.Source;
                string control = Sitecore.Context.ClientPage.ClientRequest.Control;
                int num = source.LastIndexOf("_");
                Assert.IsTrue(num >= 0, "Invalid source ID");
                string id = ShortID.Decode(StringUtil.Mid(source, num + 1));
                Item item = dataView.GetItem(id);
                if (item != null)
                {
                    SheerResponse.DisableOutput();
                    Sitecore.Shell.Framework.ContextMenu menu = new Sitecore.Shell.Framework.ContextMenu();
                    CommandContext context = new CommandContext(item);
                    Sitecore.Web.UI.HtmlControls.Menu contextMenu = menu.Build(context);
                    contextMenu.AddDivider();
                    contextMenu.Add("__Refresh", "Refresh", "Applications/16x16/refresh.png", string.Empty, string.Concat(new object[] { "javascript:Sitecore.Treeview.refresh(\"", source, "\",\"", control, "\",\"", item.ID.ToShortID(), "\")" }), false, string.Empty, MenuItemType.Normal);
                    SheerResponse.EnableOutput();
                    SheerResponse.ShowContextMenu(control, where, contextMenu);
                }
            }
        }
        
        private Sitecore.Web.UI.HtmlControls.DataContext GetDataContext()
        {
            return (Sitecore.Context.ClientPage.FindSubControl(this.DataContext) as Sitecore.Web.UI.HtmlControls.DataContext);
        }
        
        private IDataView GetDataView()
        {
            string dataViewName = this.DataViewName;
            if (string.IsNullOrEmpty(dataViewName))
            {
                Sitecore.Web.UI.HtmlControls.DataContext dataContext = this.GetDataContext();
                if (dataContext != null)
                {
                    this.UpdateFromDataContext(dataContext);
                }
                dataViewName = this.DataViewName;
            }
            string parameters = this.Parameters;
            if (string.IsNullOrEmpty(dataViewName))
            {
                parameters = WebUtil.GetFormValue(this.ID + "_Parameters");
                UrlString str3 = new UrlString(parameters);
                dataViewName = str3["dv"];
            }
            return DataViewFactory.GetDataView(dataViewName, parameters);
        }
        
        private static string GetDragID(string id)
        {
            Assert.ArgumentNotNull(id, "id");
            int num = id.LastIndexOf("_");
            if (num >= 0)
            {
                id = StringUtil.Mid(id, num + 1);
            }
            if (ShortID.IsShortID(id))
            {
                id = ShortID.Decode(id);
            }
            return id;
        }
        
        private string GetFilter()
        {
            string filter = this.Filter;
            if (string.IsNullOrEmpty(filter))
            {
                UrlString str2 = new UrlString(WebUtil.GetFormValue(this.ID + "_Parameters"));
                filter = HttpUtility.UrlDecode(StringUtil.GetString(new string[] { str2["fi"] }));
            }
            return filter;
        }
        
        private string GetNodeID(string shortID)
        {
            Assert.ArgumentNotNullOrEmpty(shortID, "shortID");
            return (this.ID + "_" + shortID);
        }
        
        private string GetParameters()
        {
            UrlString str = new UrlString(this.Parameters);
            str["dv"] = this.DataViewName;
            str["fi"] = this.Filter;
            return str.ToString();
        }
        
        private List<string> GetSelectedIDs()
        {
            return new List<string>(WebUtil.GetFormValue(this.ID + "_Selected").Split(new char[] { ',' }));
        }
        
        public Item[] GetSelectedItems()
        {
            return this.GetSelectedItems(Language.Current, Sitecore.Data.Version.Latest);
        }
        
        public Item[] GetSelectedItems(Language language, Sitecore.Data.Version version)
        {
            List<Item> list = new List<Item>();
            IDataView dataView = this.GetDataView();
            if (dataView != null)
            {
                foreach (string str in this.GetSelectedIDs())
                {
                    if (!string.IsNullOrEmpty(str) && !(str == this.ID))
                    {
                        string id = ShortID.Decode(str);
                        Item item = dataView.GetItem(id, language, version);
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        
        public Item GetSelectionItem()
        {
            return this.GetSelectionItem(Language.Current, Sitecore.Data.Version.Latest);
        }
        
        public Item GetSelectionItem(Language language, Sitecore.Data.Version version)
        {
            Item[] selectedItems = this.GetSelectedItems(language, version);
            if (selectedItems.Length != 0)
            {
                return selectedItems[0];
            }
            return null;
        }
        
        private string GetStyle(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            if (item.TemplateID == TemplateIDs.TemplateField)
            {
                return string.Empty;
            }
            string style = item.Appearance.Style;
            if (string.IsNullOrEmpty(style) && (item.Appearance.Hidden || item.RuntimeSettings.IsVirtual))
            {
                style = "color:#666666";
            }
            if (string.IsNullOrEmpty(style))
            {
                ID enabledItemsTemplateId = this.EnabledItemsTemplateId;
                if (!Sitecore.Data.ID.IsNullOrEmpty(enabledItemsTemplateId) && (item.TemplateID != enabledItemsTemplateId))
                {
                    Template template = TemplateManager.GetTemplate(item);
                    if ((template != null) && !template.DescendsFrom(enabledItemsTemplateId))
                    {
                        style = "color:#666666";
                    }
                }
            }
            if (!string.IsNullOrEmpty(style))
            {
                style = " style=\"" + style + "\"";
            }
            return style;
        }
        
        protected virtual string GetTreeNodeName(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return Assert.ResultNotNull<string>(item.Appearance.DisplayName);
        }
        
        private void ItemMovedNotification(object sender, ItemMovedEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            this.AddUpdatedItem(args.Item, true);
            Item item = args.Item.Database.GetItem(args.OldParentID);
            if (item != null)
            {
                this.AddUpdatedItem(item, false);
            }
        }
        
        protected override void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnInit(e);
            this.Page.ClientScript.RegisterClientScriptInclude("TreeviewEx", "/sitecore/shell/Controls/TreeviewEx/TreeviewEx.js");
            SiteContext site = Sitecore.Context.Site;
            if (site != null)
            {
                site.Notifications.ItemCopied += (sender, args) => this.AddUpdatedItem(args.Copy, true);
                site.Notifications.ItemCreated += (sender, args) => this.AddUpdatedItem(args.Item, true);
                site.Notifications.ItemDeleted += (sender, args) => this.AddUpdatedItem(args.Item.Database.GetItem(args.ParentID), false);
                site.Notifications.ItemMoved += new ItemMovedDelegate(this.ItemMovedNotification);
                site.Notifications.ItemSaved += (sender, args) => this.AddUpdatedItem(args.Item, false);
                site.Notifications.ItemSortorderChanged += (sender, args) => this.AddUpdatedItem(args.Item, true);
                site.Notifications.ItemRenamed += (sender, args) => this.AddUpdatedItem(args.Item, false);
                site.Notifications.ItemChildrenChanged += (sender, args) => this.AddUpdatedItem(args.Item, false);
            }
        }
        
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            Sitecore.Web.UI.HtmlControls.DataContext dataContext = this.GetDataContext();
            if (dataContext != null)
            {
                dataContext.Changed += new Sitecore.Web.UI.HtmlControls.DataContext.DataContextChangedDelegate(this.DataContext_OnChanged);
                this.UpdateFromDataContext(dataContext);
                Item[] selectedItems = this.GetSelectedItems();
                if (selectedItems.Length > 0)
                {
                    dataContext.SetFolder(selectedItems[0].Uri);
                }
            }
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnPreRender(e);
            foreach (Item item in this._updatedItems)
            {
                this.Refresh(item);
            }
        }
        
        public void Refresh(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            string shortID = item.ID.ToShortID().ToString();
            string nodeID = this.GetNodeID(shortID);
            HtmlTextWriter output = new HtmlTextWriter(new StringWriter());
            this.RenderParent(output, item);
            SheerResponse.SetOuterHtml(nodeID, output.InnerWriter.ToString());
        }
        
        public void RefreshRoot()
        {
            HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
            this.RenderControl(writer);
            SheerResponse.SetOuterHtml(this.ID, writer.InnerWriter.ToString());
        }
        
        public void RefreshSelected()
        {
            foreach (Item item in this.GetSelectedItems())
            {
                this.Refresh(item);
            }
        }
        
        protected override void Render(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");
            Item parentItem = this.ParentItem;
            if (parentItem != null)
            {
                this.RenderChildren(output, parentItem);
            }
            else
            {
                Sitecore.Web.UI.HtmlControls.DataContext dataContext = this.GetDataContext();
                if (dataContext != null)
                {
                    IDataView dataView = dataContext.DataView;
                    if (dataView != null)
                    {
                        Item item2;
                        Item item3;
                        string filter = this.GetFilter();
                        dataContext.GetState(out item2, out item3);
                        this.Render(output, dataView, filter, item2, item3);
                    }
                }
            }
        }
        
        private void Render(HtmlTextWriter output, IDataView dataView, string filter, Item root, Item folder)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(dataView, "dataView");
            Assert.ArgumentNotNull(filter, "filter");
            Assert.ArgumentNotNull(root, "root");
            Assert.ArgumentNotNull(folder, "folder");
            output.Write("<div id=\"");
            output.Write(this.ID);
            output.Write("\" onclick=\"javascript:return Sitecore.Treeview.onTreeClick(this,event");
            if (!string.IsNullOrEmpty(this.Click))
            {
                output.Write(",'");
                output.Write(StringUtil.EscapeQuote(this.Click));
                output.Write("'");
            }
            output.Write(")\"");
            if (!string.IsNullOrEmpty(this.DblClick))
            {
                output.Write(" ondblclick=\"");
                output.Write(AjaxScriptManager.GetEventReference(this.DblClick));
                output.Write("\"");
            }
            if (!string.IsNullOrEmpty(this.ContextMenu))
            {
                output.Write(" oncontextmenu=\"");
                output.Write(AjaxScriptManager.GetEventReference(this.ContextMenu));
                output.Write("\"");
            }
            if (this.AllowDragging)
            {
                output.Write(" onmousedown=\"javascript:return Sitecore.Treeview.onTreeDrag(this,event)\" onmousemove=\"javascript:return Sitecore.Treeview.onTreeDrag(this,event)\" ondragstart=\"javascript:return Sitecore.Treeview.onTreeDrag(this,event)\" ondragover=\"javascript:return Sitecore.Treeview.onTreeDrop(this,event)\" ondrop=\"javascript:return Sitecore.Treeview.onTreeDrop(this,event)\"");
            }
            output.Write(">");
            output.Write("<input id=\"");
            output.Write(this.ID);
			string selectedVal = (this.SelectedIDs.Count > 0) ? StringUtil.Join(this.SelectedIDs, ",") : folder.ID.ToShortID().ToString();
			output.Write("_Selected\" type=\"hidden\" value=\"" + selectedVal + "\" />");
            output.Write("<input id=\"");
            output.Write(this.ID);
            output.Write("_Database\" type=\"hidden\" value=\"" + folder.Database.Name + "\" />");
            output.Write("<input id=\"");
            output.Write(this.ID);
            output.Write("_Parameters\" type=\"hidden\" value=\"" + this.GetParameters() + "\" />");
            if (!Sitecore.Data.ID.IsNullOrEmpty(this.EnabledItemsTemplateId))
            {
                output.Write("<input id=\"");
                output.Write(this.ID);
                output.Write("_templateID\" type=\"hidden\" value=\"" + this.EnabledItemsTemplateId + "\"/>");
            }
            if (this.ShowRoot)
            {
                this.RenderNode(output, dataView, filter, root, root, folder);
            }
            else
            {
                foreach (Item item in dataView.GetChildren(root, string.Empty, true, 0, 0, this.GetFilter()))
                {
                    this.RenderNode(output, dataView, filter, root, item, folder);
                }
            }
            output.Write("</div>");
        }
        
        private void RenderChildren(HtmlTextWriter output, Item parent)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(parent, "parent");
            IDataView dataView = this.GetDataView();
            if (dataView != null)
            {
                string filter = this.GetFilter();
                ItemCollection itemC = dataView.GetChildren(parent, string.Empty, true, 0, 0, filter);
                if (itemC != null)
                {
                    foreach (Item item in itemC)
                    {
                        this.RenderNodeBegin(output, dataView, filter, item, false, false);
                        RenderNodeEnd(output);
                    }
                }
            }
        }
        
        private void RenderNode(HtmlTextWriter output, IDataView dataView, string filter, Item root, Item parent, Item folder)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(dataView, "dataView");
            Assert.ArgumentNotNull(filter, "filter");
            Assert.ArgumentNotNull(root, "root");
            Assert.ArgumentNotNull(parent, "parent");
            Assert.ArgumentNotNull(folder, "folder");
            bool isExpanded = (parent.ID == root.ID) || (parent.Axes.IsAncestorOf(folder) && (parent.ID != folder.ID));
            this.RenderNodeBegin(output, dataView, filter, parent, parent.ID == folder.ID, isExpanded);
            if (isExpanded)
            {
                ItemCollection itemC = dataView.GetChildren(parent, string.Empty, true, 0, 0, this.GetFilter());
                if (itemC != null)
                {
                    foreach (Item item in itemC)
                    {
                        this.RenderNode(output, dataView, filter, root, item, folder);
                    }
                }
            }
            RenderNodeEnd(output);
        }
        
        private void RenderNodeBegin(HtmlTextWriter output, IDataView dataView, string filter, Item item, bool active, bool isExpanded)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(dataView, "dataView");
            Assert.ArgumentNotNull(filter, "filter");
            Assert.ArgumentNotNull(item, "item");
            string shortID = item.ID.ToShortID().ToString();
            string nodeID = this.GetNodeID(shortID);
            output.Write("<div id=\"");
            output.Write(nodeID);
            output.Write("\" class=\"scContentTreeNode\">");
            RenderTreeNodeGlyph(output, dataView, filter, item, shortID, isExpanded);
            string str3 = (active || this.SelectedIDs.Contains(shortID)) ? "scContentTreeNodeActive" : "scContentTreeNodeNormal";
            string style = this.GetStyle(item);
            output.Write("<a href=\"#\" class=\"" + str3 + "\"");
            if (!string.IsNullOrEmpty(item.Help.Text))
            {
                output.Write(" title=\"");
                output.Write(StringUtil.EscapeQuote(item.Help.Text));
                output.Write("\"");
            }
            output.Write(style);
            output.Write(">");
            RenderTreeNodeIcon(output, item);
            output.Write("<span>{0}</span>", this.GetTreeNodeName(item));
            output.Write("</a>");
        }
        
        private static void RenderNodeEnd(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");
            output.Write("</div>");
        }
        
        private void RenderParent(HtmlTextWriter output, Item parent)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(parent, "parent");
            IDataView dataView = this.GetDataView();
            if (dataView != null)
            {
                string filter = this.GetFilter();
                this.RenderNodeBegin(output, dataView, filter, parent, false, true);
                ItemCollection itemC = dataView.GetChildren(parent, string.Empty, true, 0, 0, filter);
                if (itemC != null)
                {
                    foreach (Item item in itemC)
                    {
                        this.RenderNodeBegin(output, dataView, filter, item, false, false);
                        RenderNodeEnd(output);
                    }
                }
                RenderNodeEnd(output);
            }
        }
        
        private static void RenderTreeNodeGlyph(HtmlTextWriter output, IDataView dataView, string filter, Item item, string id, bool isExpanded)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(dataView, "dataView");
            Assert.ArgumentNotNull(filter, "filter");
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNullOrEmpty(id, "id");
            ImageBuilder builder2 = new ImageBuilder {
                Class = "scContentTreeNodeGlyph"
            };
            ImageBuilder builder = builder2;
            if (dataView.HasChildren(item, filter))
            {
                if (isExpanded)
                {
                    builder.Src = "images/collapse15x15.gif";
                }
                else
                {
                    builder.Src = "images/expand15x15.gif";
                }
            }
            else
            {
                builder.Src = "images/noexpand15x15.gif";
            }
            output.Write(builder.ToString());
        }
        
        private static void RenderTreeNodeIcon(HtmlTextWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(item, "item");
            ImageBuilder builder2 = new ImageBuilder {
                Src = item.Appearance.Icon,
                Width = 0x10,
                Height = 0x10,
                Class = "scContentTreeNodeIcon"
            };
            ImageBuilder builder = builder2;
            if (!string.IsNullOrEmpty(item.Help.Text))
            {
                builder.Alt = item.Help.Text;
            }
            builder.Render(output);
        }
        
        private void SetSelectedIDs(List<string> ids)
        {
            Assert.ArgumentNotNull(ids, "ids");
            SheerResponse.SetAttribute(this.ID + "_Selected", "value", StringUtil.Join(ids, ","));
        }
        
        public void SetSelectedItem(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            Item selectionItem = this.GetSelectionItem();
            this.SelectedIDs.Clear();
            this.SelectedIDs.Add(item.ID.ToShortID().ToString());
            this.SetSelectedIDs(this.SelectedIDs);
            if (selectionItem != null)
            {
                this.Refresh(selectionItem);
            }
            this.Refresh(item);
        }
        
        private void UpdateFromDataContext(Sitecore.Web.UI.HtmlControls.DataContext dataContext)
        {
            Assert.ArgumentNotNull(dataContext, "dataContext");
            string parameters = dataContext.Parameters;
            string filter = dataContext.Filter;
            string dataViewName = dataContext.DataViewName;
            if (((parameters != this.Parameters) || (filter != this.Filter)) || (dataViewName != this.DataViewName))
            {
                this.Parameters = parameters;
                this.Filter = filter;
                this.DataViewName = dataViewName;
                SheerResponse.SetAttribute(this.ID + "_Parameters", "value", this.GetParameters());
            }
        }
        
        public bool AllowDragging
        {
            get
            {
                object obj2 = this.ViewState["AllowDragging"];
                return ((obj2 == null) || MainUtil.GetBool(obj2, true));
            }
            set
            {
                this.ViewState["AllowDragging"] = value;
            }
        }
        
        public string Click
        {
            get
            {
                return StringUtil.GetString(this.ViewState["Click"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["Click"] = value;
            }
        }
        
        public string ContextMenu
        {
            get
            {
                return StringUtil.GetString(this.ViewState["ContextMenu"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["ContextMenu"] = value;
            }
        }
        
        public string DataContext
        {
            get
            {
                return StringUtil.GetString(this.ViewState["DataContext"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["DataContext"] = value;
            }
        }
        
        private string DataViewName
        {
            get
            {
                return StringUtil.GetString(this.ViewState["DataViewName"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["DataViewName"] = value;
            }
        }
        
        public string DblClick
        {
            get
            {
                return StringUtil.GetString(this.ViewState["DblClick"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["DblClick"] = value;
            }
        }
        
        public ID EnabledItemsTemplateId
        {
            protected get
            {
                if (Sitecore.Data.ID.IsNullOrEmpty(this.enabledItemsTemplateId))
                {
                    string formValue = WebUtil.GetFormValue(this.ID + "_templateID");
                    if (string.IsNullOrEmpty(formValue))
                    {
                        return Sitecore.Data.ID.Null;
                    }
                    this.enabledItemsTemplateId = Sitecore.Data.ID.Parse(formValue, Sitecore.Data.ID.Null);
                }
                return this.enabledItemsTemplateId;
            }
            set
            {
                Assert.IsNotNull(value, "value");
                this.enabledItemsTemplateId = value;
            }
        }
        
        private string Filter
        {
            get
            {
                return StringUtil.GetString(this.ViewState["Filter"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["Filter"] = value;
            }
        }
        
        private string Parameters
        {
            get
            {
                return StringUtil.GetString(this.ViewState["Parameters"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["Parameters"] = value;
            }
        }
        
        public Item ParentItem
        {
            get
            {
                return this._parentItem;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this._parentItem = value;
            }
        }
        
        public List<string> SelectedIDs
        {
            get
            {
                if (this._selected == null)
                {
                    this._selected = this.GetSelectedIDs();
                }
                return this._selected;
            }
        }
        
        public bool ShowRoot
        {
            get
            {
                object obj2 = this.ViewState["ShowRoot"];
                return ((obj2 == null) || MainUtil.GetBool(obj2, true));
            }
            set
            {
                this.ViewState["ShowRoot"] = value;
            }
        }
    }
}
