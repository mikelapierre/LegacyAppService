<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LegacyWebApp.Default" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Legacy Web App</h1>
        <p class="lead">Use the menu options to test the different technology stacks (ASMX, WCF, Web API).</p>
        <!--<p><a href="http://www.asp.net" class="btn btn-primary btn-lg">Learn more &raquo;</a></p>-->
    </div>
    <div>
        <asp:Label runat="server" ID="lblHeadersAndClaims" />
    </div>

</asp:Content>