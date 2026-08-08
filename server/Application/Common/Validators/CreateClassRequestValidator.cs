using AssignmentManagement.Application.Common.DTOs.Classes;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
