using AssignmentManagement.Application.Common.DTOs.Users;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => x.Email is not null);
        RuleFor(x => x.Role).IsInEnum().When(x => x.Role is not null);
    }
}
