﻿namespace SuperSafeBank.Domain;

public static class TransactionTypes
{
    public const string Transfer = "Transfer";
    public static readonly string[] TransferStates = ["Pending", "Withdrawn", "Deposited"];

    public const string Deposit = "Deposit";
    public static readonly string[] DepositStates = ["Pending", "Deposited"];

    public const string Withdraw = "Withdraw";
    public static readonly string[] WithdrawStates = ["Pending", "Withdrawn"];
}