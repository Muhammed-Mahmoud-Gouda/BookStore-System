# 📚 BookStore

<div align="center">

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core%2010.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![C#](https://img.shields.io/badge/C%23-14.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

**A professional E-Commerce Book Store built with ASP.NET Core MVC following N-Tier Architecture and best practices.**

[Features](#-features) • [Architecture](#-architecture) • [Database](#-database-design) • [Getting Started](#-getting-started) • [Roadmap](#-roadmap)

</div>

---

## 📌 Overview

BookStore is a full-featured e-commerce platform specialized in technical and programming books. Built with **ASP.NET Core MVC 10.0**, it follows clean architecture principles to ensure scalability, maintainability, and separation of concerns.

---

## ✨ Features

### ✅ Phase 1 — Core (Current)
- 📂 **Category Management** — Full CRUD with image upload
- 📦 **Product Management** — Books with full specs (Author, ISBN, Publisher, Format...)
- 🖼️ **Multiple Image Upload** — Upload & manage multiple book cover images
- 👤 **Customer Management** — Full CRUD with multiple address support
- 🧾 **Order Management** — Create orders, manage status, auto stock update
- 🔍 **Search & Filter** — Filter by category, price, availability
- 📄 **Pagination** — Server-side pagination across all listing pages
- ✅ **Custom Validation** — File extension, file size, Egyptian phone, price range, year range

### 🔲 Upcoming Phases
- 🔐 **Authentication & Authorization** (ASP.NET Core Identity)
- 🛒 **Shopping Cart** (Session-based)
- ⭐ **Reviews & Ratings**
- 📊 **Admin Dashboard** with charts
- 🌐 **REST API** with JWT Authentication

---

## 🏗️ Architecture

The project follows **N-Tier Architecture** with strict separation of concerns:

```
BookStore.sln
│
├── 📦 BookStore.Entities          → Domain Models & Enums
│   ├── Models/
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── ProductImage.cs
│   │   ├── Customer.cs
│   │   ├── CustomerAddress.cs
│   │   ├── Order.cs
│   │   └── OrderItem.cs
│   └── Enums/
│       ├── OrderStatus.cs
│       └── BookFormat.cs
│
├── 🗄️ BookStore.DAL               → Data Access Layer
│   ├── Configurations/            → Fluent API Configurations
│   ├── Repositories/
│   │   ├── Interfaces/            → IGenericRepository, IProductRepository...
│   │   └── Implementations/       → GenericRepository, ProductRepository...
│   ├── ApplicationDbContext/      → AppDbContext
│   └── Extensions/                → DALServicesExtension
│
├── ⚙️ BookStore.BLL               → Business Logic Layer
│   ├── DTOs/                      → Create, Update, Result DTOs
│   ├── Services/
│   │   ├── Interfaces/            → ICategoryService, IProductService...
│   │   └── Implementations/       → CategoryService, ProductService...
│   ├── Validators/                → Custom Validation Attributes
│   ├── Helpers/                   → UploadHelper, PaginationHelper, SlugHelper
│   └── Extensions/                → ApplicationServicesExtension
│
└── 🌐 BookStore.Web               → Presentation Layer
    ├── Controllers/               → CategoryController, ProductController...
    ├── ViewModels/                → Create, Edit, Index ViewModels
    ├── Views/                     → Razor Views
    └── wwwroot/
        └── Files/
            ├── Products/          → Book cover images
            └── Categories/        → Category images
```

### Dependency Flow

```
Web  →  BLL  →  DAL  →  Entities
```

> Each layer only knows about the layer directly below it through **Interfaces**.

---

## 🗃️ Database Design

### Entities & Relationships

```
Categories ──< Products ──< ProductImages
Customers  ──< CustomerAddresses
Customers  ──< Orders ──< OrderItems >── Products
```

### Tables

| Table | Description |
|-------|-------------|
| `Categories` | Book categories with optional image |
| `Products` | Books with full specs (ISBN, Author, Publisher...) |
| `ProductImages` | Multiple images per book (IsMain, DisplayOrder) |
| `Customers` | Customer info with unique email |
| `CustomerAddresses` | Multiple addresses per customer (IsDefault) |
| `Orders` | Order with shipping snapshot (Street, City, PostalCode) |
| `OrderItems` | Junction table — Unique(OrderId, ProductId) |

### Key Design Decisions

- **Shipping Snapshot** — Order stores shipping address data directly to preserve historical accuracy even if the customer later modifies or deletes their address
- **UnitPrice Snapshot** — OrderItem stores the price at the time of purchase, not the current product price
- **Soft Fields** — `IsActive` on Category, Product, and Customer for logical deletion
- **Composite Unique Index** — `OrderItems(OrderId, ProductId)` to prevent duplicate items in the same order

---

## 🔧 Patterns & Concepts

| Pattern / Concept | Implementation |
|---|---|
| **N-Tier Architecture** | 4 separate projects (Entities, DAL, BLL, Web) |
| **Repository Pattern** | `IGenericRepository<T>` + specific repositories |
| **Unit of Work** | `IUnitOfWork` managing all repositories + transactions |
| **Dependency Injection** | Native ASP.NET Core DI with Extension Methods |
| **Custom Validation** | `ValidationAttribute` for file, phone, price, year |
| **Image Upload** | `IFormFile` → Server → Store path in DB |
| **DTOs** | Separate Create / Update / Result DTOs per entity |
| **ViewModels** | Separate ViewModels for each View with SelectLists |
| **Fluent API** | All DB configurations via `IEntityTypeConfiguration<T>` |
| **Explicit Transactions** | Used in Order creation and multi-step operations |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/your-username/BookStore.git
cd BookStore
```

**2. Configure the connection string**

Open `BookStore.Web/appsettings.json` and update:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookStoreDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "ImageSettings": {
    "ProductImagesPath": "Files/Products",
    "CategoryImagesPath": "Files/Categories",
    "MaxFileSizeInMB": 5,
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".webp" ]
  }
}
```

**3. Apply Migrations**

Open **Package Manager Console** and make sure:
- **Startup Project** → `BookStore.Web`
- **Default Project** → `BookStore.DAL`

```bash
Add-Migration InitialCreate
Update-Database
```

**4. Run the application**
```bash
cd BookStore.Web
dotnet run
```

Or press **F5** in Visual Studio.

---

## 📁 Project Structure

### BookStore.Entities
```
BookStore.Entities/
├── Models/
│   ├── Category.cs
│   ├── Product.cs
│   ├── ProductImage.cs
│   ├── Customer.cs
│   ├── CustomerAddress.cs
│   ├── Order.cs
│   └── OrderItem.cs
└── Enums/
    ├── OrderStatus.cs          → Pending, Processing, Shipped, Delivered, Cancelled
    └── BookFormat.cs           → Paperback, Hardcover, Digital
```

### BookStore.DAL
```
BookStore.DAL/
├── ApplicationDbContext/
│   └── AppDbContext.cs
├── Configurations/
│   ├── CategoryConfiguration.cs
│   ├── ProductConfiguration.cs
│   ├── ProductImageConfiguration.cs
│   ├── CustomerConfiguration.cs
│   ├── CustomerAddressConfiguration.cs
│   ├── OrderConfiguration.cs
│   └── OrderItemConfiguration.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── IProductImageRepository.cs
│   │   ├── ICustomerRepository.cs
│   │   ├── ICustomerAddressRepository.cs
│   │   ├── IOrderRepository.cs
│   │   ├── IOrderItemRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Implementations/
│       ├── GenericRepository.cs
│       ├── CategoryRepository.cs
│       ├── ProductRepository.cs
│       ├── ProductImageRepository.cs
│       ├── CustomerRepository.cs
│       ├── CustomerAddressRepository.cs
│       ├── OrderRepository.cs
│       ├── OrderItemRepository.cs
│       └── UnitOfWork.cs
├── Extensions/
│   └── DALServicesExtension.cs
└── Migrations/
```

### BookStore.BLL
```
BookStore.BLL/
├── DTOs/
│   ├── Category/
│   │   ├── CategoryCreateDto.cs
│   │   ├── CategoryUpdateDto.cs
│   │   └── CategoryResultDto.cs
│   ├── Product/
│   │   ├── ProductCreateDto.cs
│   │   ├── ProductUpdateDto.cs
│   │   ├── ProductResultDto.cs
│   │   └── ProductImageResultDto.cs
│   ├── Customer/
│   │   ├── CustomerCreateDto.cs
│   │   ├── CustomerUpdateDto.cs
│   │   ├── CustomerResultDto.cs
│   │   ├── CustomerAddressCreateDto.cs
│   │   ├── CustomerAddressUpdateDto.cs
│   │   └── CustomerAddressResultDto.cs
│   └── Order/
│       ├── OrderCreateDto.cs
│       ├── OrderUpdateDto.cs
│       ├── OrderResultDto.cs
│       ├── OrderItemCreateDto.cs
│       └── OrderItemResultDto.cs
├── Services/
│   ├── Interfaces/
│   │   ├── ICategoryService.cs
│   │   ├── IProductService.cs
│   │   ├── ICustomerService.cs
│   │   ├── ICustomerAddressService.cs
│   │   └── IOrderService.cs
│   └── Implementations/
│       ├── CategoryService.cs
│       ├── ProductService.cs
│       ├── CustomerService.cs
│       ├── CustomerAddressService.cs
│       └── OrderService.cs
├── Validators/
│   ├── AllowedExtensionsAttribute.cs
│   ├── MaxFileSizeAttribute.cs
│   ├── EgyptianPhoneAttribute.cs
│   ├── PriceRangeAttribute.cs
│   ├── NonNegativeStockAttribute.cs
│   ├── FutureDateAttribute.cs
│   └── YearRangeAttribute.cs
├── Helpers/
│   ├── UploadHelper.cs
│   ├── PaginationHelper.cs
│   └── SlugHelper.cs
└── Extensions/
    └── ApplicationServicesExtension.cs
```

### BookStore.Web
```
BookStore.Web/
├── Controllers/
│   ├── HomeController.cs
│   ├── CategoryController.cs
│   ├── ProductController.cs
│   ├── CustomerController.cs
│   └── OrderController.cs
├── ViewModels/
│   ├── Category/
│   │   ├── CategoryCreateViewModel.cs
│   │   ├── CategoryEditViewModel.cs
│   │   └── CategoryIndexViewModel.cs
│   ├── Product/
│   │   ├── ProductCreateViewModel.cs
│   │   ├── ProductEditViewModel.cs
│   │   └── ProductIndexViewModel.cs
│   ├── Customer/
│   │   ├── CustomerCreateViewModel.cs
│   │   ├── CustomerEditViewModel.cs
│   │   ├── CustomerIndexViewModel.cs
│   │   ├── CustomerAddressCreateViewModel.cs
│   │   └── CustomerAddressEditViewModel.cs
│   └── Order/
│       ├── OrderCreateViewModel.cs
│       ├── OrderEditViewModel.cs
│       ├── OrderIndexViewModel.cs
│       ├── OrderDetailsViewModel.cs
│       └── OrderItemViewModel.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _Navbar.cshtml
│   │   ├── _Footer.cshtml
│   │   ├── _Notifications.cshtml
│   │   ├── _Pagination.cshtml
│   │   └── Error.cshtml
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Category/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   ├── Product/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   ├── Customer/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Delete.cshtml
│   │   ├── Addresses.cshtml
│   │   ├── CreateAddress.cshtml
│   │   └── EditAddress.cshtml
│   └── Order/
│       ├── Index.cshtml
│       ├── Details.cshtml
│       ├── Create.cshtml
│       └── Edit.cshtml
├── wwwroot/
│   ├── Files/
│   │   ├── Products/
│   │   └── Categories/
│   ├── css/
│   │   ├── shared.css
│   │   └── home.css
│   └── js/
│       └── site.js
├── appsettings.json
└── Program.cs
```
---

## 🗺️ Roadmap

| Phase | Status | Features |
|-------|--------|----------|
| **Phase 1** | ✅ Done | N-Tier · Repository · CRUD · Image Upload · Custom Validation · DI |
| **Phase 2** | 🔲 Planned | ASP.NET Core Identity · Roles · Login & Register · `[Authorize]` |
| **Phase 3** | 🔲 Planned | Shopping Cart · Wishlist · Search · Caching |
| **Phase 4** | 🔲 Planned | CQRS · MediatR · AutoMapper · FluentValidation |
| **Phase 5** | 🔲 Planned | REST API · JWT · Swagger · DTOs |
| **Phase 6** | 🔲 Planned | Unit Tests · xUnit · Moq · CI/CD · Docker |

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request


---

# 👨‍💻 Author
<div>

**Muhammed Mahmoud**

ASP.NET Core Backend Developer passionate about building scalable web applications and clean architectures.
</div>

---

<div align="center">

**⭐ Support**

If you like this project, consider giving it a ⭐ on GitHub!


*Read · Code · Build*

</div>
