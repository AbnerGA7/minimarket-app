# 🛒 **Sistema de Gestión para Minimarket (POS Web Local)**

<p align="center">
  <img src="https://img.shields.io/badge/Estado-En_Desarrollo-orange?style=for-the-badge">
  <img src="https://img.shields.io/badge/.NET_Core-8.0-purple?style=for-the-badge&logo=dotnet">
  <img src="https://img.shields.io/badge/SQL_Server-Express-red?style=for-the-badge&logo=microsoft-sql-server">
  <img src="https://img.shields.io/badge/Frontend-Bootstrap_5-blue?style=for-the-badge&logo=bootstrap">
  <img src="https://img.shields.io/badge/Licencia-MIT-green?style=for-the-badge">
</p>

Sistema integral para la gestión de inventario, proveedores y ventas de un minimarket.  
Desarrollado como **Aplicación Web Local (Intranet)** con **ASP.NET Core MVC**, **Entity Framework Core** y **SQL Server**.

Enfocado en resolver problemas reales como:
- Control de fechas de vencimiento (FIFO)
- Manejo de lotes
- Flujo rápido en el Punto de Venta (POS)

---

## 🚀 **Módulos y Funcionalidades**

### ✅ **Funcionalidades Activas**
✔️ **Gestión de Categorías:** Clasificación clara (Abarrotes, Bebidas, Limpieza, etc.)  
✔️ **Maestro de Productos:** Código de barras, precios, unidades (KG / UNIDAD)  
✔️ **Gestión de Proveedores:** Validación de RUC y datos completos  
✔️ **Validaciones de Datos:** Evita registros incompletos o inconsistentes

---

### 🚧 **Próximas Implementaciones**
🔄 **Entrada de Mercadería por Lotes:** Stock real + fechas de vencimiento  
💳 **Punto de Venta (POS):** Interfaz rápida y minimalista  
📊 **Dashboard Inteligente:** Alertas de stock bajo y productos por vencer  

---

## 🛠️ **Tecnologías Utilizadas**

| Capa | Tecnologías |
|------|-------------|
| **Backend** | C#, ASP.NET Core MVC, Entity Framework Core |
| **Base de Datos** | SQL Server Express / LocalDB |
| **Frontend** | Razor Views, HTML5, CSS3, Bootstrap 5 |
| **IDE** | Visual Studio 2022 |

---

## 💾 **Guía de Instalación (Paso a Paso)**

### **1️⃣ Clonar el Repositorio**
```bash
git clone https://github.com/AbnerGA7/minimarket-app.git
```
2️⃣ Crear la Base de Datos

Ve a la carpeta Database/.

Abre Script_Minimarket.sql.

Copia su contenido.

Abre SQL Server Management Studio (SSMS).

Crea una nueva consulta (New Query).

Pega el contenido y presiona F5.

Esto creará la base de datos minimarket-app con todas sus tablas.
3️⃣ Configurar la Cadena de Conexión

En appsettings.json:
```bash
"ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=minimarket-app;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
Si usas LocalDB
```bash
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=minimarket-app;Trusted_Connection=True;TrustServerCertificate=True;"
```
4️⃣ Ejecutar el Proyecto

En Visual Studio:

▶️ Presiona F5
El navegador abrirá:
```bash
https://localhost:PUERTO
```
🧰 Comandos Útiles 
Regenerar modelos desde la base de datos:
```bash
dotnet ef dbcontext scaffold "Server=.\SQLEXPRESS;Database=minimarket-app;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models -c MinimarketContext --force
```

👤 Autor
Abner Gonzales
Estudiante de Ingeniería de Software
Desarrollador de sistemas y plataformas web.
