using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;
using ValidationException = SmartPrompt.Application.Common.Exceptions.ValidationException;

namespace SmartPrompt.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler(
    IApplicationDbContext context, 
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(request.Email), new[] { "A user with this email already exists." } }
            });
        }

        var entity = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.HashPassword(request.Password)
        };

        context.Users.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
