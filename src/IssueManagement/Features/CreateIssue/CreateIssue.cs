using CaseExtensions;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IssueManagement.Features.CreateIssue;

public record CreateIssue(
  Guid OriginatorId,
  string Title,
  string Description
);

public class CreateIssueForm : FragmentModel
{
  public CreateIssueForm(
  ) : base(GetName(nameof(CreateIssueForm)))
  {
  }

  public Guid OriginatorId { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
}

public class IssueModel : FragmentModel
{
  public string Title { get; set; }

  public IssueModel(
    Issue issue
  ) : base(GetName(nameof(IssueModel)))
  {
    Title = issue.Title;
  }
}

public class FragmentModel
{
  public FragmentModel(
    string fragmentId
  )
  {
    FragmentId = fragmentId;
  }

  public static string GetName(
    string typeName
  )
  {
    return typeName.ToKebabCase();
  }

  public string FragmentId { get; }
}

public record IssueCreated(
  Guid OriginatorId,
  string Title,
  string Description
);

public class Issue
{
  public Guid Id { get; set; }
  public Guid OriginatorId { get; set; }
  public string Description { get; set; }
  public string Title { get; set; }

  public void Apply(
    IssueCreated created
  )
  {
    var (originatorId, title, description) = created;
    OriginatorId = originatorId;
    Title = title;
    Description = description;
  }
}

public class CreateIssueController : Controller
{
  [HttpGet, Route("/issues")]
  public IActionResult CreateIssueForm()
  {
    return View(
      "~/Features/CreateIssue/CreateIssue.cshtml",
      new CreateIssueForm()
    );
  }

  [HttpPost, Route("/issues")]
  public async Task<IActionResult> CreateIssue(
    [FromForm] CreateIssueForm form,
    [FromServices] IMessageBus bus,
    [FromServices] IDocumentSession session
  )
  {
    await bus.InvokeAsync(
      new CreateIssue(
        form.OriginatorId,
        form.Title,
        form.Description
      )
    );
    var issue = session
      .Query<Issue>()
      .FirstOrDefault(i => i.OriginatorId == form.OriginatorId);
    Console.WriteLine($"issue: ${issue}");
    return View(
      "~/Features/CreateIssue/CreateIssue.cshtml",
      new IssueModel(issue)
    );
  }
}

public class IssueCreatedHandler
{
  public void Handle(
    IssueCreated @event
  )
  {
    Console.WriteLine($"yay: {@event.Title}");
  }
}

public class CreateIssueHandler
{
  private readonly IDocumentSession session;

  public CreateIssueHandler(
    IDocumentSession session
  )
  {
    this.session = session;
  }

  public async Task<IssueCreated> Handle(
    CreateIssue command
  )
  {
    var (originatorId, title, description) = command;
    var created = new IssueCreated(
      originatorId,
      title,
      description
    );

    session.Events.StartStream(
      Guid.NewGuid(),
      created
    );
    await session.SaveChangesAsync();

    return created;
  }
}
