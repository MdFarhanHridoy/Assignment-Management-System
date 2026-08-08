using AssignmentManagement.Application.Common.DTOs.Submissions;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class SubmitRequestValidator : AbstractValidator<SubmitRequest>
{
    public SubmitRequestValidator()
    {
        RuleFor(x => x.AnswerText).NotEmpty().WithMessage("Answer text is required.");
    }
}
