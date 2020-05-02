<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WCF.aspx.cs" Inherits="LegacyWebApp.WCF" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Windows Communication Foundation (WCF)</h1>
        <p class="lead">Use the buttons below to test the different options.</p>
    </div>
    <div>
        <asp:Button runat="server" class="btn btn-default" ID="btnAnonymous" Text="Call anonymously" OnClick="btnAnonymous_Click" />
        <asp:Button runat="server" class="btn btn-default" ID="btnManagedIdentity" Text="Call with managed identity" OnClick="btnManagedIdentity_Click" />
        <asp:Button runat="server" class="btn btn-default" ID="btnClientSecret" Text="Call with client secret" OnClick="btnClientSecret_Click" />
        <asp:Button runat="server" class="btn btn-default" ID="btnOnBehalfOf" Text="Call on-behalf-of (OBO)" OnClick="btnOnBehalfOf_Click" />
    </div>
    <div>
        <asp:Label runat="server" ID="lblResults" />
    </div>

</asp:Content>