using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SDKPro.IAP
{
    [Serializable]
    public sealed class IapProductConfig
    {
        [SerializeField] private string m_Key;
        [SerializeField] private string m_DefaultStoreId;
        [SerializeField] private ProductType m_ProductType = ProductType.Consumable;
        [SerializeField] private string m_GooglePlayId;
        [SerializeField] private string m_AppleAppStoreId;
        [SerializeField] private string m_MacAppStoreId;
        [SerializeField]
        [Tooltip("UI fallback only. The localized store price is authoritative.")]
        private string m_EditorFallbackPrice;

        public string Key => m_Key;
        public string DefaultStoreId =>
            string.IsNullOrWhiteSpace(m_DefaultStoreId) ? m_Key : m_DefaultStoreId;
        public ProductType ProductType => m_ProductType;
        public string EditorFallbackPrice => m_EditorFallbackPrice;

        public string GetStoreId(string storeName)
        {
            if (storeName == GooglePlay.Name &&
                !string.IsNullOrWhiteSpace(m_GooglePlayId))
            {
                return m_GooglePlayId;
            }

            if (storeName == AppleAppStore.Name &&
                !string.IsNullOrWhiteSpace(m_AppleAppStoreId))
            {
                return m_AppleAppStoreId;
            }

            if (storeName == MacAppStore.Name &&
                !string.IsNullOrWhiteSpace(m_MacAppStoreId))
            {
                return m_MacAppStoreId;
            }

            return DefaultStoreId;
        }

        internal ProductDefinition ToProductDefinition(string storeName)
        {
            return new ProductDefinition(m_Key, GetStoreId(storeName), m_ProductType);
        }
    }

    [CreateAssetMenu(fileName = "IapCatalog", menuName = "SDKPro/IAP Catalog")]
    public sealed class IapCatalog : ScriptableObject
    {
        [SerializeField] private List<IapProductConfig> m_Products = new();

        public IReadOnlyList<IapProductConfig> Products => m_Products;

        public bool TryGet(string key, out IapProductConfig product)
        {
            product = null;
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            foreach (IapProductConfig candidate in m_Products)
            {
                if (candidate != null &&
                    string.Equals(candidate.Key, key, StringComparison.Ordinal))
                {
                    product = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryValidate(out string error)
        {
            error = null;
            if (m_Products == null || m_Products.Count == 0)
            {
                error = "The IAP catalog contains no products.";
                return false;
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < m_Products.Count; i++)
            {
                IapProductConfig product = m_Products[i];
                if (product == null)
                {
                    error = $"IAP catalog entry {i} is null.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(product.Key))
                {
                    error = $"IAP catalog entry {i} has no game-facing key.";
                    return false;
                }

                if (!keys.Add(product.Key))
                {
                    error = $"Duplicate IAP product key '{product.Key}'.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(product.DefaultStoreId))
                {
                    error = $"IAP product '{product.Key}' has no store product ID.";
                    return false;
                }
            }

            return true;
        }

        internal List<ProductDefinition> BuildDefinitions(string storeName)
        {
            var definitions = new List<ProductDefinition>(m_Products.Count);
            foreach (IapProductConfig product in m_Products)
            {
                definitions.Add(product.ToProductDefinition(storeName));
            }

            return definitions;
        }
    }
}
