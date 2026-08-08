using AssignmentManagement.Application.Common.DTOs.Submissions;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class ReviewSubmissionRequestValidator : AbstractValidator<ReviewSubmissionRequest>
{
    public ReviewSubmissionRequestValidator()
    {
        RuleFor(x => x.Marks).GreaterThanOrEqualTo(0).WithMessage("Marks must be >= 0.");
        RuleFor(x => x.Feedback).MaximumLength(2000).When(x => x.Feedback is not null);
    }
}
