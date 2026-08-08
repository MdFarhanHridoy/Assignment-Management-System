using AssignmentManagement.Application.Common.DTOs.Subjects;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ClassId).NotEmpty();
    }
}
