using FluentValidation;

namespace CRMProAI.Application.Common.Validation;

public record TestValidationRequest(
    string Name,
    string Email,
    decimal EstimatedValue);

public class TestValidationRequestValidator : AbstractValidator<TestValidationRequest>
{
    public TestValidationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0).WithMessage("Estimated value must be greater than zero.");
    }
}
