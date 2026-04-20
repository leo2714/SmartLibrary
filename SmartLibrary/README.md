# 智慧图书馆建设管理系统

## 项目概述
支持智慧图书馆建设，管理图书资源和智能服务设施，提升用户体验。

## 技术栈
- **后端**: .NET 8 + ASP.NET Core Web API
- **前端**: 原生 HTML5 + CSS3 + JavaScript (ES6+)
- **数据库**: MySQL 8.0
- **ORM**: Entity Framework Core + Pomelo.EntityFrameworkCore.MySql
- **部署**: Docker & Docker Compose

## 项目结构
```
SmartLibrary/
├── Controllers/          # API 控制器
│   ├── AuthController.cs
│   ├── LibraryController.cs
│   └── DeviceController.cs
├── Data/                 # 数据层
│   └── LibraryDbContext.cs
├── DTOs/                 # 数据传输对象
│   └── DTOs.cs
├── Middlewares/          # 中间件
│   └── GlobalExceptionMiddleware.cs
├── Models/               # 数据模型
│   └── Models.cs
├── Services/             # 业务服务
│   ├── AuthService.cs
│   └── LibraryServices.cs
├── wwwroot/              # 静态资源
│   ├── css/
│   │   └── style.css
│   ├── js/
│   │   └── app.js
│   └── index.html
├── appsettings.json      # 配置文件
├── Program.cs            # 程序入口
├── SmartLibrary.csproj   # 项目文件
├── Dockerfile            # Docker 构建文件
├── docker-compose.yml    # Docker Compose 配置
└── teacher.json          # 管理员账号配置
```

## 快速启动

### 方式一：使用 Docker Compose（推荐）
```bash
cd SmartLibrary
docker-compose up -d --build
```
访问地址：http://localhost:800

### 方式二：直接运行
1. 确保 MySQL 数据库已创建并配置连接字符串
2. 安装依赖：`dotnet restore`
3. 运行应用：`dotnet run`
4. 访问地址：http://localhost:5000

## 默认账号
| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | 123456 | 管理员 |
| teacher1 | 123456 | 教师 |
| student1 | 123456 | 学生 |

## API 接口

### 认证模块
- `POST /api/auth/login` - 用户登录
- `POST /api/auth/register` - 用户注册
- `GET /api/auth/users` - 获取用户列表
- `GET /api/auth/users/{id}` - 获取用户详情
- `PUT /api/auth/users/{id}` - 更新用户
- `DELETE /api/auth/users/{id}` - 删除用户

### 图书模块
- `GET /api/books` - 获取图书列表
- `GET /api/books/{id}` - 获取图书详情
- `POST /api/books` - 添加图书
- `PUT /api/books/{id}` - 更新图书
- `DELETE /api/books/{id}` - 删除图书

### 借阅模块
- `GET /api/borrowrecords` - 获取借阅记录
- `GET /api/borrowrecords/{id}` - 获取借阅详情
- `POST /api/borrowrecords/borrow` - 借书
- `POST /api/borrowrecords/{id}/return` - 还书
- `DELETE /api/borrowrecords/{id}` - 删除记录

### 设备模块
- `GET /api/devices` - 获取设备列表
- `GET /api/devices/{id}` - 获取设备详情
- `POST /api/devices` - 添加设备
- `PUT /api/devices/{id}` - 更新设备
- `DELETE /api/devices/{id}` - 删除设备

### 预约模块
- `GET /api/reservations` - 获取预约列表
- `GET /api/reservations/{id}` - 获取预约详情
- `POST /api/reservations` - 创建预约
- `PUT /api/reservations/{id}/status` - 更新预约状态
- `DELETE /api/reservations/{id}` - 删除预约

## API 响应格式
```json
{
  "code": 200,
  "message": "操作成功",
  "data": {}
}
```

## 错误码
| 错误码 | 说明 |
|--------|------|
| 200 | 成功 |
| 400 | 请求错误 |
| 401 | 未授权 |
| 403 | 禁止访问 |
| 404 | 资源不存在 |
| 409 | 冲突 |
| 500 | 服务器内部错误 |

## 功能特性
- ✅ 用户认证与权限管理（JWT）
- ✅ 图书 CRUD 管理
- ✅ 借阅/归还管理
- ✅ 智能设备管理
- ✅ 图书预约管理
- ✅ 响应式界面设计
- ✅ 加载状态与错误提示
- ✅ 数据库自动初始化与种子数据
- ✅ Docker 容器化部署
