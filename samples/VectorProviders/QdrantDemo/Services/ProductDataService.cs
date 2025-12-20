using QdrantDemo.Models;

namespace QdrantDemo.Services;

/// <summary>
/// Service providing sample product data for the demo.
/// </summary>
public static class ProductDataService
{
    /// <summary>
    /// Gets a collection of sample products for demonstration.
    /// </summary>
    public static List<Product> GetSampleProducts() =>
    [
        // Electronics
        new() { Id = 1, Name = "MacBook Pro 16\"", Description = "Powerful laptop with M3 Pro chip, perfect for developers and creative professionals", Category = "Electronics", Price = 2499.00m },
        new() { Id = 2, Name = "iPhone 15 Pro", Description = "Latest smartphone with titanium design, A17 Pro chip, and pro camera system", Category = "Electronics", Price = 999.00m },
        new() { Id = 3, Name = "Sony WH-1000XM5", Description = "Premium noise-cancelling wireless headphones with exceptional sound quality", Category = "Electronics", Price = 349.99m },
        new() { Id = 4, Name = "Samsung 4K OLED TV", Description = "65-inch OLED display with vivid colors and deep blacks for immersive viewing", Category = "Electronics", Price = 1799.00m },
        new() { Id = 5, Name = "iPad Air", Description = "Versatile tablet with M1 chip, perfect for work and entertainment on the go", Category = "Electronics", Price = 599.00m },

        // Home & Kitchen
        new() { Id = 6, Name = "Dyson V15 Vacuum", Description = "Cordless vacuum with laser dust detection and powerful suction for deep cleaning", Category = "Home & Kitchen", Price = 749.99m },
        new() { Id = 7, Name = "Instant Pot Duo", Description = "Multi-cooker pressure cooker that makes cooking fast, easy, and delicious meals", Category = "Home & Kitchen", Price = 89.99m },
        new() { Id = 8, Name = "KitchenAid Stand Mixer", Description = "Professional-grade mixer for baking, with 10 speeds and tilt-head design", Category = "Home & Kitchen", Price = 379.99m },
        new() { Id = 9, Name = "Nespresso Vertuo", Description = "Single-serve coffee maker with centrifusion technology for barista-quality espresso", Category = "Home & Kitchen", Price = 199.00m },
        new() { Id = 10, Name = "Roomba j7+", Description = "Self-emptying robot vacuum with obstacle avoidance and smart mapping", Category = "Home & Kitchen", Price = 799.00m },

        // Clothing & Fashion
        new() { Id = 11, Name = "Patagonia Nano Puff Jacket", Description = "Lightweight, warm, and packable insulated jacket for outdoor adventures", Category = "Clothing", Price = 229.00m },
        new() { Id = 12, Name = "Nike Air Max 90", Description = "Classic sneakers with visible Air cushioning and retro style", Category = "Clothing", Price = 130.00m },
        new() { Id = 13, Name = "Levi's 501 Original Jeans", Description = "Iconic straight-leg jeans with button fly, the original blue jean since 1873", Category = "Clothing", Price = 69.50m },
        new() { Id = 14, Name = "Ray-Ban Aviator Sunglasses", Description = "Timeless aviator design with polarized lenses for UV protection", Category = "Clothing", Price = 163.00m },
        new() { Id = 15, Name = "Cashmere Sweater", Description = "Luxuriously soft pure cashmere pullover for elegant everyday comfort", Category = "Clothing", Price = 198.00m },

        // Sports & Outdoors
        new() { Id = 16, Name = "Peloton Bike+", Description = "High-tech indoor cycling bike with rotating screen and auto-follow resistance", Category = "Sports", Price = 2495.00m },
        new() { Id = 17, Name = "YETI Tundra 65 Cooler", Description = "Rotomolded cooler with superior ice retention for camping and tailgating", Category = "Sports", Price = 375.00m },
        new() { Id = 18, Name = "Garmin Fenix 7X", Description = "Ultimate multisport GPS watch with solar charging and advanced training features", Category = "Sports", Price = 899.99m },
        new() { Id = 19, Name = "REI Co-op Tent", Description = "Lightweight backpacking tent with excellent weather protection and easy setup", Category = "Sports", Price = 349.00m },
        new() { Id = 20, Name = "Hydroflask Water Bottle", Description = "Insulated stainless steel bottle that keeps drinks cold for 24 hours", Category = "Sports", Price = 44.95m },

        // Books & Media
        new() { Id = 21, Name = "Kindle Paperwhite", Description = "Waterproof e-reader with adjustable warm light and weeks of battery life", Category = "Books", Price = 139.99m },
        new() { Id = 22, Name = "Clean Code by Robert Martin", Description = "A handbook of agile software craftsmanship for writing readable, maintainable code", Category = "Books", Price = 39.99m },
        new() { Id = 23, Name = "The Pragmatic Programmer", Description = "Classic book on software development, covering everything from career development to architecture", Category = "Books", Price = 49.99m },
        new() { Id = 24, Name = "Designing Data-Intensive Applications", Description = "Deep dive into distributed systems and database internals for modern applications", Category = "Books", Price = 44.99m },
        new() { Id = 25, Name = "Atomic Habits by James Clear", Description = "Practical guide to building good habits and breaking bad ones", Category = "Books", Price = 18.99m },

        // Beauty & Personal Care
        new() { Id = 26, Name = "Olaplex Hair Treatment", Description = "Bond-building hair treatment that repairs and strengthens damaged hair", Category = "Beauty", Price = 30.00m },
        new() { Id = 27, Name = "La Mer Moisturizer", Description = "Luxury skincare cream with legendary healing powers for radiant skin", Category = "Beauty", Price = 195.00m },
        new() { Id = 28, Name = "Philips Sonicare Toothbrush", Description = "Electric toothbrush with pressure sensor and multiple cleaning modes", Category = "Beauty", Price = 199.99m },
        new() { Id = 29, Name = "Theragun Elite", Description = "Professional-grade percussive therapy device for deep muscle treatment", Category = "Beauty", Price = 399.00m },
        new() { Id = 30, Name = "Dyson Airwrap", Description = "Multi-styler with Coanda technology for curls, waves, and smooth blowouts", Category = "Beauty", Price = 599.99m }
    ];
}
