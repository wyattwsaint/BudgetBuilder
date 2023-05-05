using System;

namespace BudgetBuilder
{
    public record TransactionLineItem(
        DateOnly Date,
        string Description,
        decimal Amount,
        string? Category,
        int Count);
}
