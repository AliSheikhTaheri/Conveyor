<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UpgradeSchema.ascx.cs" Inherits="TheFarm.Umbraco.EmbeddedContent.umbraco.EmbeddedContent.UpgradeSchema" %>

<div>
<h3>Thank you for installing The FARM's EmbeddedContent data type!</h3>
<p>
    We hope you will enjoy using our data type and that it will enrich your Umbraco experience even more. 
</p>
<p>    
    Please vote for the <a href="http://our.umbraco.org/projects/backoffice-extensions/embedded-content" target="_blank">project</a> if you like it. 
</p>
<p>
    You can leave your feedback <a href="http://our.umbraco.org/projects/backoffice-extensions/embedded-content/general" target="_blank">here</a>, we'd love to hear how you've been using EmbeddedContent, what additional features you would like to see or if you just want to chat about it. :)<br />
    Should you unfortunately encounter an issue when using the data type please let us know <a href="http://our.umbraco.org/projects/backoffice-extensions/embedded-content/bugs" target="_blank">here</a> so we can fix it!
</p>
<p>&nbsp;</p>
<asp:Panel runat="server" ID="start" Visible="false">
<p>
    The following will update your current EmbeddedContent instances to reflect the latest changes. <b>Please make a db backup beforehand!</b>
</p>
<asp:Button runat="server" ID="go" Text="Update EmbeddedContent Schema" onclick="go_Click" />
 </asp:Panel>
<asp:Panel runat="server" ID="end" Visible="false">
<p>
Upgrade processed finished, <asp:Literal runat="server" ID="number" /> prevalue entries updated. You're ready to go now, enjoy!
</p>
</asp:Panel>
</div>