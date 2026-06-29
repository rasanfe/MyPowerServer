# 🛠️ Mi "PowerServer" — porque las malas prácticas a veces ¡molan!

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![PowerServer](https://img.shields.io/badge/Appeon%20PowerServer-5.1-FF6C2C?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)
![Blog](https://img.shields.io/badge/blog-rsrsystem-FF5722?style=flat-square&logo=blogger&logoColor=white)

> Un backend **.NET 10** estilo *PowerServer* hecho a mano, con DataWindows en el servidor — el ejemplo
> de mi charla en la **Appeon PowerBuilder Regional Conference Spain 2025**.

## 📋 ¿Qué es esto?

El proyecto que presenté en directo para enseñar cómo, "a lo bestia", se puede montar **tu propio
PowerServer**: una API ASP.NET Core que recibe la **sintaxis de un DataWindow** + un **JSON** con los
buffers, monta un `DataStore` en el servidor (con los SDKs de Appeon **DWNet/SnapObjects**) y ejecuta
el `Update` contra **SQL Server**. Más "malas prácticas que molan" que arquitectura de libro… pero
funciona y se entiende. 😉

> 🎤 **Appeon PowerBuilder Regional Conference in Spain 2025** — Madrid, 22 de abril de 2025.

## 🧩 Dependencias

| Paquete | Versión |
|---------|---------|
| [DWNet.Data](https://www.nuget.org/packages/DWNet.Data) · DWNet.Data.AspNetCore | `5.1.0` |
| [SnapObjects.Data](https://www.nuget.org/packages/SnapObjects.Data) · .AspNetCore · .SqlServer | `5.1.0` |
| [PowerScript.Bridge](https://www.nuget.org/packages/PowerScript.Bridge) | `3.1.0` |
| [Serilog.AspNetCore](https://www.nuget.org/packages/Serilog.AspNetCore) | `10.0.0` |
| [Swashbuckle.AspNetCore](https://www.nuget.org/packages/Swashbuckle.AspNetCore) | `10.2.3` |

> 🆕 **Migración a .NET 10:** los SDKs de Appeon (DWNet/SnapObjects **5.1.0**, PowerScript.Bridge
> **3.1.0**) ya soportan `net10.0`.

## 🛠️ Requisitos

- **.NET SDK 10.0** o superior
- **SQL Server 2022** accesible (cadena de conexión en `appsettings.json`)

## 🗄️ Base de datos

En la carpeta [`database/`](database/) tenéis una **copia de la base de datos de demo** lista para restaurar:

- **`PersonDemo03.rar`** → backup de **SQL Server 2022** (descomprimid el `.rar` para sacar el `.bak`).
- Restauradlo en vuestra instancia con el nombre **`PersonDemo03`** (es el catálogo que esperan la API y el cliente PowerBuilder).

```sql
RESTORE DATABASE PersonDemo03
  FROM DISK = 'C:\ruta\PersonDemo03.bak'
  WITH MOVE 'PersonDemo03'     TO 'C:\...\DATA\PersonDemo03.mdf',
       MOVE 'PersonDemo03_log' TO 'C:\...\DATA\PersonDemo03_log.ldf',
       REPLACE;
```

> Como es **SQL Server 2022**, restaurad sobre una instancia 2022 (o superior); versiones anteriores no abren el backup.

Luego ajustad la cadena de conexión en `appsettings.json` (apartado `ConnectionStrings` → **`PersonDemo03`**): poned vuestro `Data Source`, usuario y contraseña. Tenéis la plantilla en **`appsettings_example.json`**.

## 🚀 Ejecutar

```bat
dotnet run --project MyPowerServer
```

La API levanta con **Swagger** para probar los endpoints.

## 👤 Autor

**Ramón San Félix Ramón**
🔗 [LinkedIn](https://www.linkedin.com/in/rasanfe) · 🐙 [github.com/rasanfe](https://github.com/rasanfe)

---

📨 **Blog:** <https://rsrsystem.blogspot.com/>

> ¡Nos vemos en el próximo artículo! Y recuerda: en PowerBuilder, los límites solo están en nuestra imaginación. 🚀
