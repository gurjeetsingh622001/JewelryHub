using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Utilities;
using JewelryHub.Application.Features.Catalog.Categories.Common;
using JewelryHub.Domain.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Description,
    string? IconUrl,
    Guid? ParentCategoryId,
    int DisplayOrder) : IRequest<CategoryDto>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentCategoryId is not null)
        {
            var parentExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.ParentCategoryId, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundException(nameof(Category), request.ParentCategoryId);
            }
        }

        var slug = SlugGenerator.FromName(request.Name);
        if (await _unitOfWork.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            throw new BusinessRuleException("A category with an equivalent name already exists.", nameof(request.Name));
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            IconUrl = request.IconUrl,
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Slug, category.Description, category.IconUrl,
            category.ParentCategoryId, category.DisplayOrder, category.IsActive);
    }
}

