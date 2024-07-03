namespace SuperSafeBank.Domain;

public static class TransactionTypes
{
    public const string Transfer = "Transfer";
    public static readonly string[] TransferStates = ["Pending", "Withdrawn", "Deposited"];
}