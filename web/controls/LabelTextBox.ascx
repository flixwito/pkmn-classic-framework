﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LabelTextBox.ascx.cs" Inherits="PkmnFoundations.GTS.controls.LabelTextBox" %>
<%@ Register TagPrefix="pf" Namespace="PkmnFoundations.Web" Assembly="PkmnFoundations.Web" %>

<pf:RequireCss Key="form" CssUrl="~/css/form.css" After="main" runat="server" />
<pf:RequireScript Key="jquery" ScriptUrl="~/scripts/jquery-1.11.1.min.js" runat="server" />
<pf:RequireScript Key="form" ScriptUrl="~/scripts/form.js" runat="server" />

<div class="pfLabelTextBox">
<asp:Label ID="theLabel" AssociatedControlID="theTextBox" runat="server" />
<asp:TextBox ID="theTextBox" runat="server" />
</div>
