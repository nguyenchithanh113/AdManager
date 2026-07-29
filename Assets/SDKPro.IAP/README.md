# SDKPro IAP

`SDKPro.IAP` is the single purchase engine used by both old-game migrations and
new architectures. It wraps Unity IAP 5's `StoreController` flow without
initializing Unity Gaming Services or owning project rewards.

## Compatibility

- The package dependency is Unity IAP `5.2.1`, which supports Unity 2021.3 and
  ships Google Play Billing Library 8.3.
- Unity 2022.3 and newer projects should pin `com.unity.purchasing` to `5.4.2`
  in their root manifest to use Google Play Billing Library 9.0. The SDKPro API
  is compatible with both Unity IAP lines.

## Setup

1. Create an `IapCatalog` asset.
2. Give every product a stable game-facing key, its store product ID, product
   type, and optional Google/Apple overrides.
3. Add `IapManager` to the project's persistent SDK object and assign the
   catalog.
4. Register fulfillment handlers before initialization.
5. Initialize once, then purchase by the stable product key.

```csharp
IapManager.Instance.RegisterFulfillmentHandler(
    "no_ads",
    (purchase, token) =>
    {
        DisableAdsPermanently();
        return Task.FromResult(IapFulfillmentResult.Granted());
    });

await IapManager.Instance.InitializeAsync(token);
IapManager.Instance.Buy(
    "no_ads",
    () => Debug.Log("Purchase fulfilled"),
    error => Debug.LogError(error),
    "shop");
```

The fulfillment callback is the authoritative place to validate and grant the
purchase. The service records the transaction before confirming it, so a
pending transaction redelivered after a restart is not granted twice.

The default `PlayerPrefsIapFulfillmentStore` is a local crash/retry safeguard,
not anti-fraud protection. Games with valuable inventory should validate on a
backend and provide a server-backed `IIapFulfillmentStore`.

## Catalog price policy

Store metadata is always authoritative for localized display price, currency,
and analytics revenue. `Editor fallback price` is only a presentation fallback
when the fake store or product fetch does not return metadata. It must not be
treated as the amount charged to the player.

## Ownership and restore

Only confirmed non-consumables and subscriptions are reported as owned.
Pending orders are never treated as entitlements. Apple restore is exposed
through `RestoreAsync`; Google and other stores also refresh ownership during
the initial `FetchPurchases` call.

## Unity Gaming Services

The package deliberately does not call `UnityServices.InitializeAsync`.
Projects using Analytics, Authentication, Cloud Code, or a remote catalog own
their UGS initialization order.
