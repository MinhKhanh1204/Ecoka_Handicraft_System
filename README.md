# Ecoka Handicraft System

Ecoka Handicraft System là một ứng dụng thương mại điện tử bán hàng thủ công mỹ nghệ, được xây dựng theo kiến trúc **microservices** với ASP.NET Core.

---

## Mục lục

- [Tổng quan kiến trúc](#tổng-quan-kiến-trúc)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Mô hình dữ liệu](#mô-hình-dữ-liệu)
- [API Endpoints](#api-endpoints)
- [Cấu hình & Khởi chạy](#cấu-hình--khởi-chạy)
- [Bảo mật](#bảo-mật)

---

## Tổng quan kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                      MVCApplication                         │
│                  (ASP.NET Core MVC – Frontend)              │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP (HttpClient)
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   API Gateway (Ocelot)                      │
│                     Port: 5000                              │
└──────┬──────────────────┬────────────────────┬─────────────┘
       │                  │                    │
       ▼                  ▼                    ▼
┌─────────────┐  ┌─────────────────┐  ┌──────────────────┐
│ AccountAPI  │  │   ProductAPI    │  │    OrderAPI      │
│  Port 7018  │  │   Port 7109     │  │   Port 7289      │
└──────┬──────┘  └───────┬─────────┘  └───────┬──────────┘
       │                 │                     │
       └─────────────────┴─────────────────────┘
                         │
                         ▼
             ┌───────────────────────┐
             │  SQL Server Database  │
             │  HandicraftShop_PRN232│
             └───────────────────────┘
```

Mọi request từ **MVCApplication** đều đi qua **API Gateway**, nơi định tuyến đến đúng microservice. JWT được xác thực tại tầng Gateway.

---

## Công nghệ sử dụng

| Thành phần         | Công nghệ                        |
|--------------------|----------------------------------|
| Framework          | ASP.NET Core 8                   |
| ORM                | Entity Framework Core 8          |
| Database           | SQL Server                       |
| API Gateway        | Ocelot                           |
| Xác thực           | JWT Bearer (HS256)               |
| Mã hóa mật khẩu   | BCrypt.Net                       |
| Upload ảnh         | Cloudinary                       |
| Object Mapping     | AutoMapper (OrderAPI)            |
| Frontend           | ASP.NET Core MVC + Bootstrap 5   |

---

## Cấu trúc dự án

```
Ecoka_Handicraft_System/
├── APIGateway/             # Ocelot API Gateway
│   ├── Program.cs
│   ├── appsettings.json    # JWT config
│   └── gatewaysettings.json# Ocelot routing rules
│
├── AccountAPI/             # Microservice xác thực & tài khoản
│   ├── Controllers/        # AuthController (login, register)
│   ├── Services/           # AuthService, CloudinaryService
│   ├── Repositories/       # AccountRepository
│   ├── Models/             # Account, Customer, Role, UserRole
│   ├── DTOs/               # LoginRequestDto, LoginResponseDto, RegisterCustomerRequestDto
│   ├── Helpers/            # JwtTokenHelper, CloudinarySettings
│   └── Middlewares/        # GlobalExceptionMiddleware
│
├── ProductAPI/             # Microservice quản lý sản phẩm & danh mục
│   ├── Controllers/        # ProductController (public)
│   ├── Admin/
│   │   ├── Controllers/    # ProductAdminController, CategoryController
│   │   ├── Services/       # ProductAdminService, CategoryService
│   │   └── Repositories/   # ProductAdminRepository, CategoryRepository
│   ├── Services/           # ProductService
│   ├── Repositories/       # ProductRepository
│   ├── Models/             # Product, Category, ProductImage
│   └── Middlewares/        # GlobalExceptionMiddleware
│
├── OrderAPI/               # Microservice quản lý đơn hàng
│   ├── Controllers/        # OrdersController
│   ├── Services/           # OrderService (AutoMapper)
│   ├── Repositories/       # OrderRepository, OrderItemRepository
│   ├── Models/             # Order, OrderItem
│   ├── DTOs/               # OrderReadDto, OrderCreateDto, OrderUpdateDto
│   └── Profiles/           # OrderProfile (AutoMapper)
│
└── MVCApplication/         # Frontend MVC
    ├── Controllers/        # HomeController, OrdersController
    ├── Areas/Admin/        # DashboardController (Admin panel)
    ├── Services/           # OrderService (gọi qua Gateway)
    ├── Models/             # Order, OrderItem DTOs
    └── Views/              # Razor views + Admin views
```

---

## Mô hình dữ liệu

### AccountAPI – Database: `HandicraftShop_PRN232`

```
ACCOUNTS (AccountID PK, Username, Email, Password, Avatar, CreatedAt, Status)
    └──< USER_ROLE (AccountID FK, RoleID FK, Status)
    └── CUSTOMERS (CustomerID = AccountID, FullName, DateOfBirth, Gender, Phone, Address, Status)

ROLES (RoleID PK, RoleName, Description)
    └──< USER_ROLE (AccountID FK, RoleID FK, Status)
```

### ProductAPI – Database: `HandicraftShop_PRN232`

```
CATEGORIES (CategoryID PK, CategoryName, Description, Status)
    └──< PRODUCTS (ProductID PK, CategoryID FK, ProductName, Description, Material,
                   Price, Discount, StockQuantity, CreatedAt, Status)
            └──< PRODUCT_IMAGES (ImageID PK, ProductID FK, ImageURL, IsMain)
```

### OrderAPI – Database: `HandicraftShop_PRN232`

```
ORDERS (OrderID PK, CustomerID, StaffID, VoucherID, OrderDate, TotalAmount,
        PaymentMethod, PaymentStatus, ShippingStatus, ShippingAddress, Note, UpdatedAt)
    └──< ORDER_ITEMS (OrderItemID PK, OrderID FK, ProductID, Quantity, UnitPrice, Discount)
```

**Trạng thái đơn hàng:**
- `PaymentStatus`: `Pending` | `Paid` | `Refunded` | `Failed`
- `ShippingStatus`: `Pending` | `Approved` | `Cancelled` | `Returned`

---

## API Endpoints

### API Gateway (Port 5000)

| Upstream (Client gọi)             | Downstream (Internal)                     | Phương thức         |
|-----------------------------------|-------------------------------------------|---------------------|
| `/auth/{everything}`              | AccountAPI `/api/auth/{everything}`       | GET/POST/PUT/DELETE |
| `/products/{everything}`          | ProductAPI `/api/products/{everything}`   | GET/POST/PUT/DELETE |
| `/customer/orders/{everything}`   | OrderAPI `/api/customer/orders/{everything}` | GET/POST/PUT/DELETE |
| `/categories/{everything}`        | ProductAPI `/api/admin/categories/{everything}` | GET/POST/PUT/DELETE |

### AccountAPI (Port 7018)

| Method | Endpoint                     | Mô tả                             |
|--------|------------------------------|-----------------------------------|
| POST   | `/api/auth/login`            | Đăng nhập, trả về JWT token       |
| POST   | `/api/auth/register-customer`| Đăng ký tài khoản khách hàng mới  |

**Request đăng nhập:**
```json
{ "username": "string", "password": "string" }
```

**Response đăng nhập:**
```json
{ "success": true, "data": { "token": "eyJ..." }, "message": null }
```

### ProductAPI (Port 7109)

#### Public API

| Method | Endpoint           | Mô tả                                |
|--------|--------------------|--------------------------------------|
| GET    | `/api/products`    | Lấy tất cả sản phẩm đang `Active`   |

#### Admin API

| Method | Endpoint                       | Mô tả                        |
|--------|--------------------------------|------------------------------|
| GET    | `/api/admin/products`          | Lấy tất cả sản phẩm          |
| GET    | `/api/admin/products/search`   | Tìm kiếm theo từ khóa        |
| GET    | `/api/admin/products/{id}`     | Lấy sản phẩm theo ID         |
| POST   | `/api/admin/products`          | Tạo sản phẩm mới             |
| PUT    | `/api/admin/products/{id}`     | Cập nhật sản phẩm            |
| DELETE | `/api/admin/products/{id}`     | Xóa sản phẩm                 |
| GET    | `/api/admin/categories`        | Lấy tất cả danh mục          |
| GET    | `/api/admin/categories/search` | Tìm kiếm danh mục            |
| GET    | `/api/admin/categories/{id}`   | Lấy danh mục theo ID         |
| POST   | `/api/admin/categories`        | Tạo danh mục mới             |
| PUT    | `/api/admin/categories/{id}`   | Cập nhật danh mục            |
| DELETE | `/api/admin/categories/{id}`   | Xóa danh mục                 |

### OrderAPI (Port 7289)

| Method | Endpoint                              | Mô tả                              |
|--------|---------------------------------------|------------------------------------|
| GET    | `/api/customer/orders`                | Lấy danh sách đơn hàng             |
| GET    | `/api/customer/orders/search`         | Tìm kiếm đơn hàng                  |
| GET    | `/api/customer/orders/{orderId}`      | Xem chi tiết đơn hàng              |
| POST   | `/api/customer/orders`                | Tạo đơn hàng mới                   |
| PUT    | `/api/customer/orders/{orderId}/cancel` | Hủy đơn hàng                     |
| GET    | `/api/customer/orders/has-purchased`  | Kiểm tra đã mua sản phẩm chưa      |

---

## Cấu hình & Khởi chạy

### Yêu cầu hệ thống

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local hoặc remote)
- Tài khoản [Cloudinary](https://cloudinary.com) (để upload ảnh)

### Bước 1 – Cấu hình

Tạo file `appsettings.json` cho từng service dựa trên file mẫu `appsettings.example.json`.

**AccountAPI** – cần cấu hình connection string, JWT, và Cloudinary:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=HandicraftShop_PRN232;..."
  },
  "Jwt": {
    "Key": "<your-secret-key-min-32-chars>",
    "Issuer": "EcokaIssuer",
    "Audience": "EcokaAudience"
  },
  "CloudinarySettings": {
    "CloudName": "<your-cloud-name>",
    "ApiKey": "<your-api-key>",
    "ApiSecret": "<your-api-secret>"
  }
}
```

**ProductAPI / OrderAPI** – cần cấu hình connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=HandicraftShop_PRN232;..."
  }
}
```

**APIGateway** – cần cấu hình JWT (cùng key với AccountAPI):
```json
{
  "Jwt": {
    "Key": "<your-secret-key-min-32-chars>",
    "Issuer": "EcokaIssuer",
    "Audience": "EcokaAudience"
  }
}
```

**MVCApplication** – cần cấu hình URL của Gateway:
```json
{
  "ApiGateway": {
    "ApiBaseUrl": "https://localhost:5000/"
  }
}
```

### Bước 2 – Tạo database

Dùng SQL Server Management Studio hoặc script để tạo database `HandicraftShop_PRN232` với các bảng theo mô hình dữ liệu ở trên.

### Bước 3 – Khởi chạy

Khởi chạy theo thứ tự sau (mỗi service trong một terminal riêng):

```bash
# 1. AccountAPI (port 7018)
cd AccountAPI && dotnet run

# 2. ProductAPI (port 7109)
cd ProductAPI && dotnet run

# 3. OrderAPI (port 7289)
cd OrderAPI && dotnet run

# 4. API Gateway (port 5000) – sau khi các service đã lên
cd APIGateway && dotnet run

# 5. MVC Frontend
cd MVCApplication && dotnet run
```

Hoặc dùng Visual Studio / Rider để chạy Multiple Startup Projects.

---

## Bảo mật

- **JWT**: Token hết hạn sau **2 giờ**. Khóa JWT phải giống nhau giữa `AccountAPI` và `APIGateway`.
- **BCrypt**: Mật khẩu được băm bằng BCrypt trước khi lưu vào database.
- **Credentials**: **Không** commit file `appsettings.json` chứa thông tin nhạy cảm (connection string, JWT key, Cloudinary secret) lên môi trường public. Sử dụng biến môi trường hoặc **User Secrets** (`dotnet user-secrets`) trong môi trường development.

```bash
# Ví dụ dùng dotnet user-secrets cho AccountAPI
cd AccountAPI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;"
dotnet user-secrets set "Jwt:Key" "your-secret-key"
dotnet user-secrets set "CloudinarySettings:ApiSecret" "your-cloudinary-secret"
```