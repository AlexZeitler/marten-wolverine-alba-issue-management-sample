using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Razor;

[assembly: AspMvcViewLocationFormat(@"~/Features/{1}/{0}.cshtml")]
[assembly: AspMvcPartialViewLocationFormat(@"~/Features/{1}/{0}.cshtml")]


namespace IssueManagement;

public class FeatureFolderLocationExpander : IViewLocationExpander
{
  public void PopulateValues(
    ViewLocationExpanderContext context
  )
  {
    // Don't need anything here, but required by the interface
  }

  public IEnumerable<string> ExpandViewLocations(
    ViewLocationExpanderContext context,
    IEnumerable<string> viewLocations
  )
  {
    return new[]
    {
      "/Views/Shared/{0}.cshtml",
      "/Features/{1}/{0}.cshtml"
    };
  }
}
