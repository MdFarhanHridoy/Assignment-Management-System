using AssignmentManagement.Application.Common.DTOs.Enrollments;
using FluentValidation;

namespace AssignmentManagement.Application.Common.Validators;

public class CreateEnrollmentRequestValidator : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentRequestValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
    }
}
