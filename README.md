# 🛒 Sistema de Gestión Minimarket (POS Web Local)

![Estado](https://img.shields.io/badge/Estado-En_Desarrollo-orange?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET_Core-8.0-purple?style=for-the-badge&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-Express-red?style=for-the-badge&logo=microsoft-sql-server)
![Bootstrap](https://img.shields.io/badge/Frontend-Bootstrap_5-blue?style=for-the-badge&logo=bootstrap)
![License](https://img.shields.io/badge/Licencia-MIT-green?style=for-the-badge)

Sistema integral para la gestión de inventario, proveedores y ventas de un Minimarket. Desarrollado como una **Aplicación Web Local (Intranet)** utilizando tecnología ASP.NET Core MVC y SQL Server.

El sistema se enfoca en resolver problemas críticos como el control de vencimientos (FIFO), gestión de lotes y rapidez en el punto de venta.

---

## 🚀 Módulos y Funcionalidades

### ✅ Funcionalidades Activas
* **📦 Gestión de Categorías:** Clasificación organizada de productos (Abarrotes, Bebidas, etc.).
* **🏷️ Maestro de Productos:** Registro completo con código de barras, precios y unidades de medida (KG/UNIDAD).
* **🚛 Gestión de Proveedores:** Directorio de empresas proveedoras con validación de RUC y datos de contacto.
* **🛡️ Validaciones:** Sistema robusto que impide el ingreso de datos incompletos o erróneos.

### 🚧 Próximas Implementaciones
* **Entrada de Mercadería (Lotes):** Control de stock real y fechas de vencimiento.
* **Punto de Venta (POS):** Interfaz de caja rápida.
* **Dashboard:** Alertas de stock bajo y productos por vencer.

---

## 🛠️ Tecnologías del Proyecto

* **Backend:** C# / ASP.NET Core MVC (Entity Framework Core).
* **Base de Datos:** SQL Server (Relacional).
* **Frontend:** Razor Views, HTML5, CSS3, Bootstrap 5.
* **IDE Recomendado:** Visual Studio 2022.

---

## 💾 Guía de Instalación (Paso a Paso)

Si deseas probar este proyecto en tu máquina local, sigue estas instrucciones detalladas.

### 1. Clonar el Repositorio
Abre tu terminal (Git Bash o CMD) y ejecuta:

```bash
git clone [https://github.com/AbnerGA7/minimarket-app.git](https://github.com/AbnerGA7/minimarket-app.git)
2. Configurar la Base de Datos 🗄️
El proyecto incluye el script necesario para crear la base de datos automáticamente.

Ve a la carpeta Database/ dentro de este repositorio.

Abre el archivo Script_Minimarket.sql.

Copia todo el contenido.

Abre SQL Server Management Studio (SSMS).

Crea una Nueva Consulta (New Query), pega el código y presiona Ejecutar (F5).

Esto creará la base de datos minimarket-app y todas sus tablas.

3. Conectar la Aplicación
Para que el sistema se conecte a TU base de datos, debes configurar el servidor.

Abre el proyecto en Visual Studio.

Busca el archivo appsettings.json.

Modifica la cadena de conexión (ConnectionStrings) según tu servidor:

JSON

"ConnectionStrings": {
  // Si usas SQL Express:
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=minimarket-app;Trusted_Connection=True;TrustServerCertificate=True;"
  
  // Si usas SQL LocalDB (Visual Studio por defecto):
  // "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=minimarket-app;Trusted_Connection=True;TrustServerCertificate=True;"
}
4. Ejecutar el Proyecto ▶️
En Visual Studio, presiona F5 o el botón verde de Play.

El navegador se abrirá automáticamente en https://localhost:TU_PUERTO.

¡Listo! Ya puedes navegar por los módulos de Categorías, Productos y Proveedores.

📝 Comandos Útiles para Desarrolladores
Si realizas cambios en la Base de Datos y necesitas actualizar el código C# (Modelos), usa este comando en la consola del Administrador de Paquetes:

PowerShell

dotnet ef dbcontext scaffold "Server=.\SQLEXPRESS;Database=minimarket-app;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models -c MinimarketContext --force
👤 Autor
Desarrollado por Abner Gonzales. Estudiante de Ingeniería de Software.


### Tu siguiente paso 🚀
Una vez que hayas guardado el archivo SQL en la carpeta y actualizado el README, ejecuta estos comandos en tu terminal para subir todo a GitHub:

```bash
git add .
git commit -m "Agregado README profesional y Script de Base de Datos"
git push origin main