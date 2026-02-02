namespace ProductServiceLibrary;

public class ProductRepository
    {
        private static List<Product> _products = null!;
        private static List<Category> _categories = null!;
        private static int _nextProductId;
        private static readonly object _lock = new object();

        static ProductRepository()
        {
            InitializeData();
        }

        private static void InitializeData()
        {
            _categories = new List<Category>
            {
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items" },
                new Category { Id = 3, Name = "Books", Description = "Books and publications" },
                new Category { Id = 4, Name = "Home & Garden", Description = "Home improvement and garden supplies" },
                new Category { Id = 5, Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear" },
                new Category { Id = 6, Name = "Toys & Games", Description = "Toys, games, and puzzles" },
                new Category { Id = 7, Name = "Food & Beverage", Description = "Food items and beverages" }
            };

            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Wireless Bluetooth Headphones",
                    Description = "Premium noise-cancelling wireless headphones with 30-hour battery life",
                    Price = 149.99m,
                    Category = "Electronics",
                    SKU = "ELEC-WBH-001",
                    StockQuantity = 45,
                    ImageUrl = "https://example.com/images/headphones.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-6),
                    LastModifiedDate = DateTime.Now.AddDays(-10)
                },
                new Product
                {
                    Id = 2,
                    Name = "4K Smart TV 55 inch",
                    Description = "Ultra HD Smart TV with HDR support and built-in streaming apps",
                    Price = 599.99m,
                    Category = "Electronics",
                    SKU = "ELEC-TV-002",
                    StockQuantity = 12,
                    ImageUrl = "https://example.com/images/smart-tv.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-4),
                    LastModifiedDate = DateTime.Now.AddDays(-5)
                },
                new Product
                {
                    Id = 3,
                    Name = "Laptop Backpack",
                    Description = "Water-resistant backpack with padded laptop compartment up to 17 inches",
                    Price = 49.99m,
                    Category = "Electronics",
                    SKU = "ELEC-BAG-003",
                    StockQuantity = 78,
                    ImageUrl = "https://example.com/images/backpack.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-8),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 4,
                    Name = "Men's Cotton T-Shirt",
                    Description = "Comfortable 100% cotton t-shirt available in multiple colors",
                    Price = 19.99m,
                    Category = "Clothing",
                    SKU = "CLO-TSHIRT-004",
                    StockQuantity = 150,
                    ImageUrl = "https://example.com/images/tshirt.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-3),
                    LastModifiedDate = DateTime.Now.AddDays(-2)
                },
                new Product
                {
                    Id = 5,
                    Name = "Women's Running Shoes",
                    Description = "Lightweight running shoes with superior cushioning and breathability",
                    Price = 89.99m,
                    Category = "Clothing",
                    SKU = "CLO-SHOE-005",
                    StockQuantity = 63,
                    ImageUrl = "https://example.com/images/running-shoes.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-2),
                    LastModifiedDate = DateTime.Now.AddDays(-15)
                },
                new Product
                {
                    Id = 6,
                    Name = "Denim Jeans",
                    Description = "Classic fit denim jeans with stretch fabric for comfort",
                    Price = 59.99m,
                    Category = "Clothing",
                    SKU = "CLO-JEAN-006",
                    StockQuantity = 92,
                    ImageUrl = "https://example.com/images/jeans.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-5),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 7,
                    Name = "The Great Gatsby",
                    Description = "Classic American novel by F. Scott Fitzgerald - Paperback edition",
                    Price = 12.99m,
                    Category = "Books",
                    SKU = "BOOK-FICT-007",
                    StockQuantity = 200,
                    ImageUrl = "https://example.com/images/gatsby.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-12),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 8,
                    Name = "Clean Code",
                    Description = "A Handbook of Agile Software Craftsmanship by Robert C. Martin",
                    Price = 44.99m,
                    Category = "Books",
                    SKU = "BOOK-TECH-008",
                    StockQuantity = 35,
                    ImageUrl = "https://example.com/images/clean-code.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-9),
                    LastModifiedDate = DateTime.Now.AddDays(-30)
                },
                new Product
                {
                    Id = 9,
                    Name = "Cookbook: Quick & Easy Meals",
                    Description = "Collection of 200+ recipes for busy weeknights",
                    Price = 24.99m,
                    Category = "Books",
                    SKU = "BOOK-COOK-009",
                    StockQuantity = 48,
                    ImageUrl = "https://example.com/images/cookbook.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-7),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 10,
                    Name = "Robot Vacuum Cleaner",
                    Description = "Smart robot vacuum with mapping technology and app control",
                    Price = 299.99m,
                    Category = "Home & Garden",
                    SKU = "HOME-VAC-010",
                    StockQuantity = 18,
                    ImageUrl = "https://example.com/images/robot-vacuum.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-3),
                    LastModifiedDate = DateTime.Now.AddDays(-7)
                },
                new Product
                {
                    Id = 11,
                    Name = "Indoor Plant Set",
                    Description = "Set of 3 low-maintenance indoor plants with decorative pots",
                    Price = 39.99m,
                    Category = "Home & Garden",
                    SKU = "HOME-PLT-011",
                    StockQuantity = 55,
                    ImageUrl = "https://example.com/images/plants.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-4),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 12,
                    Name = "LED Desk Lamp",
                    Description = "Adjustable LED desk lamp with USB charging port and touch control",
                    Price = 34.99m,
                    Category = "Home & Garden",
                    SKU = "HOME-LAMP-012",
                    StockQuantity = 67,
                    ImageUrl = "https://example.com/images/desk-lamp.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-6),
                    LastModifiedDate = DateTime.Now.AddDays(-20)
                },
                new Product
                {
                    Id = 13,
                    Name = "Yoga Mat Premium",
                    Description = "Extra thick non-slip yoga mat with carrying strap",
                    Price = 29.99m,
                    Category = "Sports & Outdoors",
                    SKU = "SPORT-YGA-013",
                    StockQuantity = 88,
                    ImageUrl = "https://example.com/images/yoga-mat.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-5),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 14,
                    Name = "Camping Tent 4-Person",
                    Description = "Weather-resistant camping tent with easy setup system",
                    Price = 159.99m,
                    Category = "Sports & Outdoors",
                    SKU = "SPORT-TNT-014",
                    StockQuantity = 22,
                    ImageUrl = "https://example.com/images/tent.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-8),
                    LastModifiedDate = DateTime.Now.AddDays(-45)
                },
                new Product
                {
                    Id = 15,
                    Name = "Adjustable Dumbbells Set",
                    Description = "Space-saving adjustable dumbbell set, 5-52.5 lbs per dumbbell",
                    Price = 349.99m,
                    Category = "Sports & Outdoors",
                    SKU = "SPORT-DMB-015",
                    StockQuantity = 14,
                    ImageUrl = "https://example.com/images/dumbbells.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-2),
                    LastModifiedDate = DateTime.Now.AddDays(-3)
                },
                new Product
                {
                    Id = 16,
                    Name = "Building Blocks Set",
                    Description = "Creative building blocks set with 500+ pieces for ages 4+",
                    Price = 49.99m,
                    Category = "Toys & Games",
                    SKU = "TOY-BLK-016",
                    StockQuantity = 110,
                    ImageUrl = "https://example.com/images/building-blocks.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-7),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 17,
                    Name = "Board Game: Strategy Master",
                    Description = "Award-winning strategy board game for 2-4 players",
                    Price = 39.99m,
                    Category = "Toys & Games",
                    SKU = "TOY-BRD-017",
                    StockQuantity = 42,
                    ImageUrl = "https://example.com/images/board-game.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-10),
                    LastModifiedDate = DateTime.Now.AddDays(-60)
                },
                new Product
                {
                    Id = 18,
                    Name = "Remote Control Car",
                    Description = "High-speed RC car with rechargeable battery and 2.4GHz remote",
                    Price = 69.99m,
                    Category = "Toys & Games",
                    SKU = "TOY-RC-018",
                    StockQuantity = 33,
                    ImageUrl = "https://example.com/images/rc-car.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-4),
                    LastModifiedDate = DateTime.Now.AddDays(-12)
                },
                new Product
                {
                    Id = 19,
                    Name = "Organic Coffee Beans",
                    Description = "Premium organic Arabica coffee beans, medium roast - 1lb bag",
                    Price = 14.99m,
                    Category = "Food & Beverage",
                    SKU = "FOOD-COF-019",
                    StockQuantity = 125,
                    ImageUrl = "https://example.com/images/coffee.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-6),
                    LastModifiedDate = DateTime.Now.AddDays(-5)
                },
                new Product
                {
                    Id = 20,
                    Name = "Herbal Tea Collection",
                    Description = "Assorted herbal tea collection with 8 different flavors",
                    Price = 19.99m,
                    Category = "Food & Beverage",
                    SKU = "FOOD-TEA-020",
                    StockQuantity = 95,
                    ImageUrl = "https://example.com/images/tea-collection.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-3),
                    LastModifiedDate = null
                },
                new Product
                {
                    Id = 21,
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with adjustable DPI settings",
                    Price = 24.99m,
                    Category = "Electronics",
                    SKU = "ELEC-MOU-021",
                    StockQuantity = 142,
                    ImageUrl = "https://example.com/images/mouse.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-11),
                    LastModifiedDate = DateTime.Now.AddDays(-25)
                },
                new Product
                {
                    Id = 22,
                    Name = "USB-C Hub Adapter",
                    Description = "7-in-1 USB-C hub with HDMI, USB 3.0, SD card reader, and more",
                    Price = 39.99m,
                    Category = "Electronics",
                    SKU = "ELEC-HUB-022",
                    StockQuantity = 58,
                    ImageUrl = "https://example.com/images/usb-hub.jpg",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-5),
                    LastModifiedDate = null
                }
            };

            _nextProductId = _products.Max(p => p.Id) + 1;
        }

        public List<Product> GetAllProducts()
        {
            lock (_lock)
            {
                return _products.Where(p => p.IsActive).ToList();
            }
        }

        public Product? GetProductById(int productId)
        {
            lock (_lock)
            {
                return _products.FirstOrDefault(p => p.Id == productId && p.IsActive);
            }
        }

        public List<Product> GetProductsByCategory(string category)
        {
            lock (_lock)
            {
                return _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && p.IsActive).ToList();
            }
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return GetAllProducts();
            }

            lock (_lock)
            {
                return _products.Where(p => p.IsActive &&
                    (p.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     p.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     p.Category.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     p.SKU.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            }
        }

        public List<Category> GetCategories()
        {
            lock (_lock)
            {
                return _categories.ToList();
            }
        }

        public Product CreateProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            lock (_lock)
            {
                product.Id = _nextProductId++;
                product.CreatedDate = DateTime.Now;
                product.IsActive = true;
                _products.Add(product);
                return product;
            }
        }

        public bool UpdateProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            lock (_lock)
            {
                var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return false;
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Category = product.Category;
                existingProduct.SKU = product.SKU;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.IsActive = product.IsActive;
                existingProduct.LastModifiedDate = DateTime.Now;

                return true;
            }
        }

        public bool DeleteProduct(int productId)
        {
            lock (_lock)
            {
                var product = _products.FirstOrDefault(p => p.Id == productId);
                if (product == null)
                {
                    return false;
                }

                product.IsActive = false;
                product.LastModifiedDate = DateTime.Now;
                return true;
            }
        }

        public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            lock (_lock)
            {
                return _products.Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice).ToList();
            }
        }
    }
