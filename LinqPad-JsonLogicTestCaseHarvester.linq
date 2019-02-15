<Query Kind="Statements">
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationCore.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\UIAutomationTypes.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\System.Windows.Input.Manipulations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\UIAutomationProvider.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <NuGetReference>HtmlAgilityPack.Net45</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

// This process is bound to break, so might require amending!
// 1. Navigate to http://jsonlogic.com/operations.html
// 2. After page fully loads, save it as Html (single page). I used Firefox for this
// 3. Point the "file" variable below to the results of step 2
// 4. Execute the script and save clipboard contents to "jsonlogic.com_tests.json" file
var file = @"CHANGEME.html";

var doc = new HtmlAgilityPack.HtmlDocument();
doc.Load(file);
var testRows = doc.DocumentNode.SelectNodes("/html/body/div/div/section/div/div[contains(@class, 'row')]");
var testStrings = testRows.Select(row => new [] 
{ 
	HttpUtility.HtmlDecode(row.SelectSingleNode("div[contains(@class, 'logic')]/textarea").InnerText),  
	HttpUtility.HtmlDecode(row.SelectSingleNode("div[contains(@class, 'data')]/textarea").InnerText), 
	HttpUtility.HtmlDecode(row.SelectSingleNode("div[contains(@class, 'result')]/textarea").InnerText) 
});
var testJson = new JArray(testStrings.Select(scenario => new JArray(scenario.Select(txt => JToken.Parse(txt)).ToArray())).ToArray());

System.Windows.Forms.Clipboard.Clear();
System.Windows.Forms.Clipboard.SetText(testJson.ToString().Dump());
