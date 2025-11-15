using FluentValidation;
using UserManagement.DTOs;

namespace UserManagement.Validation
{
    public class UserCreateDtoValidator : AbstractValidator <UserCreateDto>
    {
        public  UserCreateDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).Matches("^[A-Za-zÀ-ž' -]+$");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).Matches("^[A-Za-zÀ-ž' -]+$");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }
}
