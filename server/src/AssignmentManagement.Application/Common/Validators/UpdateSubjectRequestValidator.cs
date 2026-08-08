using AssignmentManagement.Application.Common.DTOs.Subjects;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).When(x => x.Name is not null);
        RuleFor(x => x.ClassId).NotEmpty().When(x => x.ClassId.HasValue);
    }
}
