using FluentValidation;
using UserManagement.DTOs;

namespace UserManagement.Validation
{
    public class RequestCreateDtoValidator : AbstractValidator<RequestCreateDto>
    {
        private static readonly HashSet<string> AllowedPriorities =
            new(StringComparer.OrdinalIgnoreCase) { "Low", "Normal", "High" };

        public RequestCreateDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Priority)
                .Must(p => string.IsNullOrWhiteSpace(p) || AllowedPriorities.Contains(p!))
                .WithMessage("The priority must be one of the following values: Low, Normal or High.");
            RuleFor(x => x.DueDate).Must(d => !d.HasValue || d.Value.Date > DateTime.UtcNow.Date)
                .WithMessage("DueDate must be in the future.");
           
        }
    }
    
}
