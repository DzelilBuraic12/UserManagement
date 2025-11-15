using FluentValidation;
using UserManagement.DTOs;
using static UserManagement.Domain.Entities.Request;

public class RequestUpdateDtoValidator : AbstractValidator<RequestUpdateDto>
{
    public RequestUpdateDtoValidator()
    {
        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title!).NotEmpty().MaximumLength(200);
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description!).NotEmpty();
        });

        When(x => x.Priority != null, () =>
        {
            RuleFor(x => x.Priority!)
                .Must(p => Enum.TryParse<RequestPriority>(p, true, out _))
                .WithMessage("Priority must be one of: Low, Normal, High.");
        });

        When(x => x.DueDate.HasValue, () =>
        {
            RuleFor(x => x.DueDate!.Value)
                .GreaterThan(DateTime.UtcNow.Date).WithMessage("DueDate must be in the future.");
        });
    }
}
