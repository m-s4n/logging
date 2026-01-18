using Microsoft.EntityFrameworkCore;
using Otomasyon.DataAccess.Contexts;
using Otomasyon.Domain.Exceptions;
using Otomasyon.Service.Dtos.AuthRole;

namespace Otomasyon.Service.Services.AuthRole;

public class AuthRoleService(AppDbContext dbContext):IAuthRoleService
{
    public async Task<CreateAuthRoleResponse> CreateAuthRoleAsync(CreateAuthRoleRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new RequiredFieldException(fieldName: nameof(request.Name));
        var name = request.Name.Trim().ToUpperInvariant();
        var exists = await dbContext.AuthRoles.AnyAsync(x => x.Name == name, ct);
        if(exists)
            throw new AlreadyExistsException(fieldName:nameof(request.Name), value: name);

        var role = new Domain.Entities.AuthRole()
        {
            Id = Guid.NewGuid(),
            Name = name,
        };
        
        await dbContext.AuthRoles.AddAsync(role);
        await dbContext.SaveChangesAsync(ct);
        return new CreateAuthRoleResponse
        {
            Id = role.Id,
            Name = role.Name
        };
    }

    public async Task<List<AuthRoleDto>> GetAllAuthRolesAsync(CancellationToken ct = default)
    {
        return await dbContext.AuthRoles.AsNoTracking().Select(r => new AuthRoleDto
        {
            Id = r.Id,
            Name = r.Name,
        }).ToListAsync(ct);
    }

    public async Task<UpdateAuthRoleResponse> UpdateAuthRoleAsync(UpdateAuthRoleRequest request, CancellationToken ct = default)
    {
        if (request.Id == Guid.Empty)
            throw new RequiredFieldException(fieldName: nameof(request.Id));
        
        if(string.IsNullOrWhiteSpace(request.Name))
            throw new RequiredFieldException(fieldName: nameof(request.Name));
        
        var newName = request.Name.Trim().ToUpperInvariant();

        var role = await dbContext
            .AuthRoles
            .FindAsync(request.Id, ct);

        if (role is null)
            throw new EntityNotFoundException<Domain.Entities.AuthRole>(entityId:request.Id);
        
        var exists = await dbContext
            .AuthRoles
            .AnyAsync(x => x.Name == newName && x.Id != request.Id, ct);
        if (exists)
            throw new AlreadyExistsException(fieldName:nameof(request.Name), value: newName);

        role.Name = newName;
        
        await dbContext.SaveChangesAsync(ct);

        return new UpdateAuthRoleResponse
        {
            Id = role.Id,
            Name = role.Name
        };
    }

    public async Task DeleteAuthRoleAsync(Guid id, CancellationToken ct = default)
    {
        if(id == Guid.Empty)
            throw new RequiredFieldException(fieldName: nameof(id));

        var role = await dbContext
            .AuthRoles
            .FindAsync(id, ct);
        if(role is null)
            throw new EntityNotFoundException<Domain.Entities.AuthUser>(entityId:id);
        
        var isUsed = await dbContext
            .AuthUserRoles
            .AnyAsync(x => x.RoleId == id, ct);

        if (isUsed)
            throw new EntityInUseException<Domain.Entities.AuthRole>(entityId: id);
        
        dbContext.AuthRoles.Remove(role);
        await dbContext.SaveChangesAsync(ct);
    }
}