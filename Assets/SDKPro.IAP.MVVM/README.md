# SDKPro IAP MVVM

This package is a thin, optional R3 view-model layer over `SDKPro.IAP`. It does
not contain a second purchase engine and does not depend on ASS MVVM or a
specific dependency-injection container.

Concrete game view models provide:

- The stable product key.
- The reward/entitlement fulfillment implementation.
- Any project analytics, placement tracking, presentation, and reward effects.

The base model exposes reactive price, availability, ownership, and busy state
plus a purchase-result stream. Store price metadata comes from the shared
service; the catalog fallback is used only when store metadata is unavailable.

VContainer projects can register concrete view models as singletons exactly as
they do today. Old games do not install this package and use `IapManager`
directly.

When installing from Git, add `com.sdkpro.iap`, `com.sdkpro.iap.mvvm`, and R3
as direct entries in the consuming project's manifest. Unity's Package Manager
does not resolve sibling Git packages from this repository by package name
alone.
