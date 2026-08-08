using AssignmentManagement.Application.Common.DTOs.Submissions;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class UpdateSubmissionRequestValidator : AbstractValidator<UpdateSubmissionRequest>
{
    public UpdateSubmissionRequestValidator()
    {
        RuleFor(x => x.AnswerText).NotEmpty().WithMessage("Answer text is required.");
    }
}
