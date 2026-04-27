namespace Backend.Services;

public interface IValidationService
{
    string? ValidateUserRequest(string? name, string? email, string? password = null);
    string? ValidateSignUpRequest(string? firstName, string? lastName, string? email, string? password, string? phoneNumber);
    string? ValidateEventRequest(string? name, string? status, string? description, string? imageUrl = null, DateTime? eventDate = null);
}

public class ValidationService : IValidationService
{
    public string? ValidateUserRequest(string? name, string? email, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (name.Trim().Length > 100)
        {
            return "Name cannot exceed 100 characters.";
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (email.Trim().Length > 150)
        {
            return "Email cannot exceed 150 characters.";
        }

        if (password is not null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return "Password is required.";
            }

            if (password.Length < 6)
            {
                return "Password must be at least 6 characters.";
            }
        }

        return null;
    }

    public string? ValidateSignUpRequest(string? firstName, string? lastName, string? email, string? password, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return "First name is required.";
        }

        if (firstName.Trim().Length > 100)
        {
            return "First name cannot exceed 100 characters.";
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return "Last name is required.";
        }

        if (lastName.Trim().Length > 100)
        {
            return "Last name cannot exceed 100 characters.";
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (email.Trim().Length > 150)
        {
            return "Email cannot exceed 150 characters.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password is required.";
        }

        if (password.Length < 6)
        {
            return "Password must be at least 6 characters.";
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return "Phone number is required.";
        }

        if (phoneNumber.Trim().Length > 20)
        {
            return "Phone number cannot exceed 20 characters.";
        }

        return null;
    }

    public string? ValidateEventRequest(string? name, string? status, string? description, string? imageUrl = null, DateTime? eventDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Event name is required.";
        }

        if (name.Trim().Length > 150)
        {
            return "Event name cannot exceed 150 characters.";
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return "Status is required.";
        }

        if (status.Trim().Length > 100)
        {
            return "Status cannot exceed 100 characters.";
        }

        if (description is not null && description.Trim().Length > 1000)
        {
            return "Description cannot exceed 1000 characters.";
        }

        if (eventDate.HasValue && eventDate.Value.ToUniversalTime() <= DateTime.UtcNow)
        {
            return "Event date must be in the future.";
        }

        return null;
    }
}

