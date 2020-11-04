using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using LaubPlusCo.Foundation.Helix.TemplateRepository.Data;
using Newtonsoft.Json;

namespace LaubPlusCo.Foundation.Helix.TemplateRepository.Services
{
  public class HelixTemplatesBranchService
  {
    private HelixTemplatesBranch[] _parsedBranches;

    protected string BranchesJson;
    public bool IsReady;

    public HelixTemplatesBranchService(string apiUrl, string repositoryRelativeUrl)
    {
      if (!apiUrl.EndsWith("/"))
        apiUrl = $"{apiUrl}/";
      if (repositoryRelativeUrl.StartsWith("/"))
        repositoryRelativeUrl = repositoryRelativeUrl.Substring(1);
      RepositoryUrl = $"{apiUrl}{repositoryRelativeUrl}";
      if (!RepositoryUrl.EndsWith("/"))
        RepositoryUrl = $"{RepositoryUrl}/";
      IsReady = TryFetchJson($"{RepositoryUrl}branches", out BranchesJson);
    }

    protected string RepositoryUrl { get; set; }

    public HelixTemplatesBranch[] Branches
    {
      get
      {
        if (string.IsNullOrEmpty(BranchesJson)) return new HelixTemplatesBranch[0];
        return _parsedBranches ??
               (_parsedBranches = JsonConvert.DeserializeObject<HelixTemplatesBranch[]>(BranchesJson));
      }
    }

    public void Refresh()
    {
      TryFetchJson($"{RepositoryUrl}branches", out BranchesJson);
    }

    protected bool TryFetchJson(string url, out string json)
    {
      json = string.Empty;
      try
      {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Accept = "application/json";
        request.UserAgent = "LaubPlusCo.Foundation.TemplateRepository";
        request.Method = WebRequestMethods.Http.Get;
        var response = (HttpWebResponse)request.GetResponse();
        using (var responseStream = response.GetResponseStream())
        {
          if (responseStream == null) return false;

          var encoding = string.IsNullOrEmpty(response.CharacterSet)
            ? Encoding.UTF8
            : Encoding.GetEncoding(response.CharacterSet);

          var reader = new StreamReader(responseStream, encoding);
          json = reader.ReadToEnd();
        }
        return true;
      }
      catch (Exception e)
      {
        Trace.WriteLine(e.Message, "Error");
        return false;
      }
    }

  }
}