using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SDKPro.IAP
{
    public enum IapServiceState
    {
        Uninitialized,
        Initializing,
        Ready,
        ReadyWithoutEntitlements,
        Failed,
        Disposed
    }

    public enum IapPurchaseStatus
    {
        Succeeded,
        Failed,
        Cancelled,
        Deferred,
        Unavailable
    }

    public sealed class IapInitializationResult
    {
        public bool Succeeded { get; }
        public bool EntitlementsLoaded { get; }
        public bool ProductsComplete { get; }
        public IReadOnlyList<string> MissingProductKeys { get; }
        public string Error { get; }

        private IapInitializationResult(
            bool succeeded,
            bool entitlementsLoaded,
            bool productsComplete,
            IReadOnlyList<string> missingProductKeys,
            string error)
        {
            Succeeded = succeeded;
            EntitlementsLoaded = entitlementsLoaded;
            ProductsComplete = productsComplete;
            MissingProductKeys = missingProductKeys ?? Array.Empty<string>();
            Error = error;
        }

        public static IapInitializationResult Ready(
            bool entitlementsLoaded,
            IReadOnlyList<string> missingProductKeys = null)
        {
            IReadOnlyList<string> missing =
                missingProductKeys ?? Array.Empty<string>();
            return new IapInitializationResult(
                true,
                entitlementsLoaded,
                missing.Count == 0,
                missing,
                null);
        }

        public static IapInitializationResult Failed(string error)
        {
            return new IapInitializationResult(
                false,
                false,
                false,
                Array.Empty<string>(),
                error);
        }
    }

    public sealed class IapRestoreResult
    {
        public bool Succeeded { get; }
        public string Error { get; }

        public IapRestoreResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Error = error;
        }
    }

    public sealed class IapProductInfo
    {
        public string Key { get; }
        public string StoreSpecificId { get; }
        public ProductType ProductType { get; }
        public string LocalizedTitle { get; }
        public string LocalizedDescription { get; }
        public string LocalizedPriceString { get; }
        public decimal LocalizedPrice { get; }
        public string IsoCurrencyCode { get; }
        public bool AvailableToPurchase { get; }

        internal IapProductInfo(Product product)
        {
            Key = product.definition.id;
            StoreSpecificId = product.definition.storeSpecificId;
            ProductType = product.definition.type;
            LocalizedTitle = product.metadata.localizedTitle;
            LocalizedDescription = product.metadata.localizedDescription;
            LocalizedPriceString = product.metadata.localizedPriceString;
            LocalizedPrice = product.metadata.localizedPrice;
            IsoCurrencyCode = product.metadata.isoCurrencyCode;
            AvailableToPurchase = product.availableToPurchase;
        }
    }

    public sealed class IapFulfillmentContext
    {
        public string ProductKey { get; }
        public string StoreSpecificId { get; }
        public ProductType ProductType { get; }
        public string TransactionId { get; }
        public string Receipt { get; }
        public string Placement { get; }
        public IapProductInfo Product { get; }

        internal IapFulfillmentContext(
            Product product,
            string transactionId,
            string receipt,
            string placement)
        {
            ProductKey = product.definition.id;
            StoreSpecificId = product.definition.storeSpecificId;
            ProductType = product.definition.type;
            TransactionId = transactionId;
            Receipt = receipt;
            Placement = placement;
            Product = new IapProductInfo(product);
        }
    }

    public readonly struct IapFulfillmentResult
    {
        public bool Succeeded { get; }
        public string Error { get; }

        private IapFulfillmentResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Error = error;
        }

        public static IapFulfillmentResult Granted()
        {
            return new IapFulfillmentResult(true, null);
        }

        public static IapFulfillmentResult Retry(string error)
        {
            return new IapFulfillmentResult(false, error);
        }
    }

    public sealed class IapPurchaseResult
    {
        public IapPurchaseStatus Status { get; }
        public string ProductKey { get; }
        public string TransactionId { get; }
        public string Placement { get; }
        public string Error { get; }
        public bool WasAlreadyFulfilled { get; }
        public IapProductInfo Product { get; }
        public bool Succeeded => Status == IapPurchaseStatus.Succeeded;

        internal IapPurchaseResult(
            IapPurchaseStatus status,
            string productKey,
            string transactionId,
            string placement,
            string error,
            bool wasAlreadyFulfilled,
            IapProductInfo product)
        {
            Status = status;
            ProductKey = productKey;
            TransactionId = transactionId;
            Placement = placement;
            Error = error;
            WasAlreadyFulfilled = wasAlreadyFulfilled;
            Product = product;
        }

        public static IapPurchaseResult Unavailable(string key, string error)
        {
            return new IapPurchaseResult(
                IapPurchaseStatus.Unavailable,
                key,
                null,
                null,
                error,
                false,
                null);
        }
    }

    public interface IIapFulfillmentStore
    {
        bool IsFulfilled(string transactionKey);
        void MarkFulfilled(string transactionKey);
    }

    public sealed class PlayerPrefsIapFulfillmentStore : IIapFulfillmentStore
    {
        private const string Prefix = "sdkpro.iap.fulfilled.";

        public bool IsFulfilled(string transactionKey)
        {
            return PlayerPrefs.GetInt(Prefix + Hash(transactionKey), 0) == 1;
        }

        public void MarkFulfilled(string transactionKey)
        {
            PlayerPrefs.SetInt(Prefix + Hash(transactionKey), 1);
            PlayerPrefs.Save();
        }

        private static string Hash(string value)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte item in bytes)
            {
                builder.Append(item.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }

    internal static class IapTaskUtility
    {
        public static async Task<T> WithCancellation<T>(
            this Task<T> task,
            CancellationToken token)
        {
            if (!token.CanBeCanceled)
            {
                return await task;
            }

            var cancellation = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(() => cancellation.TrySetResult(true)))
            {
                if (task != await Task.WhenAny(task, cancellation.Task))
                {
                    throw new OperationCanceledException(token);
                }
            }

            return await task;
        }
    }
}
