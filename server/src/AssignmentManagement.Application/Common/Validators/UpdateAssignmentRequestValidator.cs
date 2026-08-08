using AssignmentManagement.Application.Common.DTOs.Assignments;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Description).NotEmpty().When(x => x.Description is not null);
        RuleFor(x => x.DeadlineUtc).GreaterThan(DateTime.UtcNow).When(x => x.DeadlineUtc.HasValue);
        RuleFor(x => x.MaxMarks).GreaterThan(0).When(x => x.MaxMarks.HasValue);
    }
}
