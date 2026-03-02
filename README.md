# Ecoka Handicraft System

Hệ thống thương mại điện tử bán hàng thủ công mỹ nghệ, được xây dựng theo kiến trúc **Microservices** với **Ocelot API Gateway** và giao diện người dùng **ASP.NET Core MVC**.

---

## Mô hình kiến trúc tổng quan

```
┌─────────────────────────────────────────┐
│         MVCApplication (Frontend)        │
│         Razor MVC - Giao diện web        │
└──────────────────┬──────────────────────┘
                   │ HttpClient
                   ▼
┌─────────────────────────────────────────┐
│       API Gateway (Ocelot) :5000        │
│   Định tuyến + Xác thực JWT toàn cục    │
└────┬──────────────┬──────────┬──────────┘
     │              │          │
     ▼              ▼          ▼
┌──────────┐  ┌───────────┐  ┌──────────┐
│AccountAPI│  │ProductAPI │  │ OrderAPI │
│  :7018   │  │  :7109    │  │  :7289   │
│ Tài khoản│  │ Sản phẩm  │  │ Đơn hàng │
└────┬─────┘  └─────┬─────┘  └────┬─────┘
     │             │              │
     └─────────────┴──────────────┘
                   │
     ┌─────────────▼──────────────┐
     │  SQL Server Database       │
     │  HandicraftShop_PRN232     │
     └────────────────────────────┘
```

---

## Các thành phần của hệ thống

### 1. APIGateway (:5000)
Cổng vào duy nhất của hệ thống, sử dụng thư viện **Ocelot**.

**Chức năng:**
- Nhận toàn bộ request từ client (MVC hoặc bên ngoài)
- Xác thực JWT Bearer Token trước khi chuyển tiếp request
- Định tuyến (proxy) request đến đúng microservice

**Bảng định tuyến (`gatewaysettings.json`):**

| Upstream (Gateway) | Downstream (Microservice) | Phương thức |
|--------------------|--------------------------|-------------|
| `/auth/{everything}` | `AccountAPI:7018/api/auth/{everything}` | GET, POST, PUT, DELETE |
| `/products/{everything}` | `ProductAPI:7109/api/products/{everything}` | GET, POST, PUT, DELETE |
| `/customer/orders/{everything}` | `OrderAPI:7289/api/customer/orders/{everything}` | GET, POST, PUT, DELETE |
| `/categories/{everything}` | `ProductAPI:7109/api/admin/categories/{everything}` | GET, POST, PUT, DELETE |

---

### 2. AccountAPI (:7018)
Microservice quản lý tài khoản và xác thực.

**Công nghệ:**
- JWT Bearer Authentication (phát hành token)
- Cloudinary (lưu trữ ảnh đại diện)
- Entity Framework Core

**Các thực thể dữ liệu:**

| Bảng | Mô tả |
|------|-------|
| `ACCOUNTS` | Tài khoản đăng nhập: username, email, password, avatar, token phục hồi mật khẩu |
| `CUSTOMERS` | Thông tin khách hàng (1-1 với Account): họ tên, ngày sinh, giới tính, điện thoại, địa chỉ |
| `ROLES` | Vai trò người dùng (admin, staff, customer) |
| `USER_ROLES` | Bảng liên kết nhiều-nhiều giữa Account và Role |

**API Endpoints:**

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/auth/login` | Đăng nhập, trả về JWT token |
| POST | `/api/auth/register-customer` | Đăng ký tài khoản khách hàng (multipart/form-data) |

**Các lớp dịch vụ:**
- `IAuthService` → Đăng nhập, đăng ký khách hàng
- `ICloudinaryService` → Upload ảnh đại diện
- `IAccountRepository` → Thao tác CSDL

---

### 3. ProductAPI (:7109)
Microservice quản lý sản phẩm và danh mục.

**Các thực thể dữ liệu:**

| Bảng | Mô tả |
|------|-------|
| `PRODUCTS` | Sản phẩm: tên, mô tả, chất liệu, giá, chiết khấu, tồn kho, trạng thái |
| `CATEGORIES` | Danh mục sản phẩm: tên, mô tả |
| `PRODUCT_IMAGES` | Ảnh sản phẩm: đường dẫn, cờ ảnh chính |

**API Endpoints (khách hàng):**

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/products` | Lấy danh sách tất cả sản phẩm |

**API Endpoints (admin):**

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET/POST/PUT/DELETE | `/api/admin/categories/{...}` | Quản lý danh mục |
| GET/POST/PUT/DELETE | `/api/admin/products/{...}` | Quản lý sản phẩm (admin) |

**Các lớp dịch vụ:**
- `IProductService` → Xem danh sách sản phẩm (khách hàng)
- `IProductAdminService` → CRUD sản phẩm (admin)
- `ICategoryService` → Quản lý danh mục
- `IProductRepository`, `IProductAdminRepository`, `ICategoryRepository` → Thao tác CSDL

---

### 4. OrderAPI (:7289)
Microservice quản lý đơn hàng.

**Các thực thể dữ liệu:**

| Bảng | Mô tả |
|------|-------|
| `ORDERS` | Đơn hàng: mã khách, mã nhân viên, mã voucher, ngày đặt, tổng tiền, phương thức/trạng thái thanh toán, trạng thái vận chuyển, địa chỉ giao hàng |
| `ORDER_ITEMS` | Chi tiết đơn hàng: mã sản phẩm, số lượng, đơn giá, chiết khấu |

**Trạng thái đơn hàng:**

| Enum | Giá trị |
|------|---------|
| `PaymentStatus` | Pending, Paid, Refunded, Failed |
| `ShippingStatus` | Pending, Approved, Cancelled, Returned |

**API Endpoints:**

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/customer/orders/` | Lấy danh sách đơn hàng của khách |
| GET | `/api/customer/orders/search` | Tìm kiếm đơn hàng theo bộ lọc |
| GET | `/api/customer/orders/{orderId}` | Chi tiết đơn hàng |
| POST | `/api/customer/orders/` | Tạo đơn hàng mới |
| PUT | `/api/customer/orders/{orderId}/cancel` | Hủy đơn hàng |
| GET | `/api/customer/orders/has-purchased` | Kiểm tra lịch sử mua sản phẩm |

**Các lớp dịch vụ:**
- `IOrderService` → CRUD đơn hàng, tìm kiếm, hủy, kiểm tra lịch sử mua
- `IOrderRepository` → Thao tác CSDL đơn hàng
- `IOrderItemRepository` → Thao tác CSDL chi tiết đơn hàng
- AutoMapper Profiles → Chuyển đổi giữa Model và DTO

---

### 5. MVCApplication (Frontend)
Ứng dụng web giao diện người dùng, sử dụng ASP.NET Core MVC + Razor Views.

**Cấu trúc:**
- `Controllers/` → HomeController, OrdersController (gọi API Gateway)
- `Areas/Admin/Controllers/` → DashboardController (giao diện quản trị)
- `Views/` → Home, Orders, Shared (Razor templates)
- `Services/IOrderService` → Lớp dịch vụ gọi API Gateway qua HttpClient
- `wwwroot/` → CSS, JavaScript, hình ảnh tĩnh

**Luồng hoạt động:**
1. Người dùng gửi request đến MVC
2. Controller gọi `IOrderService` (hoặc service tương ứng)
3. Service gửi HTTP request đến API Gateway (:5000)
4. Gateway xác thực JWT và chuyển tiếp đến microservice phù hợp
5. Kết quả trả về, Controller render Razor View

---

## Luồng xác thực (Authentication Flow)

```
1. Client POST /auth/login  →  Gateway  →  AccountAPI
2. AccountAPI xác thực credentials, phát JWT token
3. Client lưu token, đính kèm vào header: Authorization: Bearer <token>
4. Mọi request tiếp theo qua Gateway đều được kiểm tra token
5. Microservice trích xuất claims từ token (ví dụ: User.FindFirst("sub"))
```

**Cấu hình JWT:**
- Issuer: `EcokaIssuer`
- Audience: `EcokaAudience`
- Algorithm: HMAC SHA-256 (SymmetricSecurityKey)

---

## Cơ sở dữ liệu

Tất cả microservice đều dùng chung một database SQL Server: **`HandicraftShop_PRN232`**

| Microservice | Phương thức kết nối |
|--------------|-------------------|
| AccountAPI | Windows Integrated Authentication |
| ProductAPI | SQL Authentication (sa / `<your-password>`) |
| OrderAPI | SQL Authentication (sa / `<your-password>`) |

> **Lưu ý:** Dù chia sẻ cùng database, mỗi microservice chỉ truy cập các bảng thuộc phạm vi của mình thông qua `DBContext` riêng biệt.

---

## Công nghệ sử dụng

| Lĩnh vực | Công nghệ |
|----------|-----------|
| Framework | ASP.NET Core (.NET 8) |
| ORM | Entity Framework Core |
| API Gateway | Ocelot |
| Xác thực | JWT Bearer Token |
| Lưu trữ ảnh | Cloudinary |
| Object Mapping | AutoMapper |
| HTTP Client | IHttpClientFactory |
| Frontend | ASP.NET Core MVC + Razor Views |
| Database | Microsoft SQL Server |

---

## Cách chạy hệ thống

1. **Tạo database** `HandicraftShop_PRN232` trên SQL Server và chạy các migration.
2. **Cập nhật connection string** trong `appsettings.json` của từng project cho phù hợp với môi trường local.
3. **Chạy đồng thời** tất cả các project theo thứ tự:
   - `AccountAPI` (:7018)
   - `ProductAPI` (:7109)
   - `OrderAPI` (:7289)
   - `APIGateway` (:5000)
   - `MVCApplication`
4. Truy cập ứng dụng qua địa chỉ của `MVCApplication`.

> Có thể dùng **Multiple Startup Projects** trong Visual Studio để chạy tất cả cùng lúc.