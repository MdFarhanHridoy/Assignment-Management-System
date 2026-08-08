using AssignmentManagement.Application.Common.DTOs.Assignments;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.DeadlineUtc).GreaterThan(DateTime.UtcNow).WithMessage("Deadline must be a future date.");
        RuleFor(x => x.MaxMarks).GreaterThan(0);
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}
