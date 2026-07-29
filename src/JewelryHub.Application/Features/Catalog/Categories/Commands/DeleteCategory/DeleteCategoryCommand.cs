using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        var hasSubCategories = await _unitOfWork.Categories.AnyAsync(c => c.ParentCategoryId == request.Id, cancellationToken);
        if (hasSubCategories)
        {
            throw new BusinessRuleException("Cannot delete a category that still has sub-categories. Re-parent or delete them first.");
        }

        var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == request.Id, cancellationToken);
        if (hasProducts)
        {
            throw new BusinessRuleException("Cannot delete a category that still has products listed under it.");
        }

        // Remove() is intercepted and converted into a soft delete by
        // AuditableEntitySaveChangesInterceptor — no hard DELETE is issued.
        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
