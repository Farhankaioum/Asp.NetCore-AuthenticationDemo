using System.Collections.Generic;

namespace IdentityExample.Data
{
    public interface IShoppingCart<T> where T : class
    {
        void AddItem(params T[] items);
        void ClearCart();
        void RemoveItem(T item);
    }

    public abstract class ShoppingCart<T> : IShoppingCart<T> where T : class
    {
        public List<T> CardValue = new List<T>();

        public void AddItem(params T[] items)
        {
            if (items.Length == 1)
                CardValue.Add(items[0]);
            else
            {
                foreach (var item in items)
                    CardValue.Add(item);
            }

        }

        public void RemoveItem(T item)
        {
            CardValue.Remove(item);
        }

        public void ClearCart()
        {
            CardValue.Clear();
        }
    }
}
