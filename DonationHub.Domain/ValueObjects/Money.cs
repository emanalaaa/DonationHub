namespace DonationHub.Domain.ValueObjects;

using DonationHub.Domain.Exceptions;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    private Money() { }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Money amount must be greater than or equal to zero.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new DomainException("Currency must be a valid 3-letter ISO code.");
        }

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0m, currency);

    public Money Add(Money other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException($"Cannot add money with differing currencies '{Currency}' and '{other.Currency}'.");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}

