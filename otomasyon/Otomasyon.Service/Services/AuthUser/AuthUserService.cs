using Microsoft.EntityFrameworkCore;
using Otomasyon.DataAccess.Contexts;
using Otomasyon.Service.Dtos.AuthUser;
using Otomasyon.Domain.Exceptions;
using Otomasyon.Service.Services.PasswordHasher;

namespace Otomasyon.Service.Services.AuthUser;

public class AuthUserService(AppDbContext dbContext, IPasswordHasher passwordHasher):IAuthUserService
{
    public async Task<CreateAuthUserResponse> CreateAuthUserAsync(CreateAuthUserRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Email))
            throw new RequiredFieldException(fieldName:nameof(request.Email));
        if(string.IsNullOrWhiteSpace(request.Username))
            throw new RequiredFieldException(fieldName:nameof(request.Username));
        if(string.IsNullOrWhiteSpace(request.Password))
            throw new RequiredFieldException(fieldName:nameof(request.Password));

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.Username.Trim();

        var emailExists = await dbContext
            .AuthUsers
            .AnyAsync(u => u.Email == email);
        
        if(emailExists)
            throw new AlreadyExistsException(fieldName:nameof(request.Email), value:email);
        
        
        
        var authUser = new Otomasyon.Domain.Entities.AuthUser()
        {
            Email = request.Email,
            UserName = request.Username,
            Id = Guid.NewGuid(),
            IsActive = true
        };
        
        authUser.SetPassword(password:request.Password, passwordHasher.Hash);

        await dbContext.AddAsync(authUser);
        await dbContext.SaveChangesAsync();
        return new CreateAuthUserResponse
        {
            Id = authUser.Id,
            Email = authUser.Email,
            Username = authUser.UserName,
        };
    }

    public async Task<List<AuthUserDto>> GetAllAuthUsersAsync()
    {
        return await dbContext
            .AuthUsers
            .AsNoTracking()
            .Select(u => new AuthUserDto
        {
            Id = u.Id,
            Email = u.Email,
            UserName = u.UserName,
        }).ToListAsync();
    }

    public async Task<UpdateAuthUserResponse> UpdateAuthUserAsync(UpdateAuthUserRequest request)
    {
        var authUser = await dbContext
            .AuthUsers
            .FirstOrDefaultAsync(u => u.Id == request.Id);

        if (authUser is null) throw new EntityNotFoundException<Domain.Entities.AuthUser>(entityId:request.Id);
        
        // email'i başkası kullanıyor mu
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await dbContext
            .AuthUsers
            .AnyAsync(u => u.Email == normalizedEmail && u.Id != request.Id);
        
        // email kullanımda
        if (emailExists)
            throw new AlreadyExistsException(fieldName: "Email", value: normalizedEmail);

        authUser.Email = request.Email.Trim().ToLowerInvariant();
        authUser.UserName = request.UserName.Trim();
        
        // değişiklik yoksa dön
        if (!dbContext.ChangeTracker.HasChanges())
            return new UpdateAuthUserResponse()
            {
                Id = authUser.Id,
                Email = authUser.Email,
                UserName = authUser.UserName,
            };
        
        await dbContext.SaveChangesAsync();

        return new UpdateAuthUserResponse
        {
            Id = authUser.Id,
            Email = authUser.Email,
            UserName = authUser.UserName,
        };

    }
    
    public async Task DeleteAuthUserAsync(Guid id)
    {
        var authUser = await dbContext
            .AuthUsers
            .FindAsync(id);
        if (authUser is null) 
            throw new EntityNotFoundException<Domain.Entities.AuthUser>(entityId:id);
        
        dbContext.AuthUsers.Remove(authUser);
        await dbContext.SaveChangesAsync();
    }
    
}