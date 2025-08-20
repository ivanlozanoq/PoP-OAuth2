# OAuth2 Proof of Concept (PoC)

Una implementación simple y limpia de autenticación OAuth2 en C# usando arquitectura por capas, principios SOLID y prácticas de Clean Code.

## ¿Qué demuestra este PoC?

Este proyecto implementa el **protocolo OAuth2** siguiendo mejores prácticas de desarrollo:

- **🏗️ Clean Architecture**: 4 capas bien separadas (Domain, Application, Infrastructure, Presentation)
- **🔧 Principios SOLID**: Cada clase tiene una responsabilidad específica
- **🔐 OAuth2 Standard**: Implementación correcta del flujo de autorización
- **💉 Dependency Injection**: Inversión de dependencias en todas las capas
- **🚀 ASP.NET Core**: API REST moderna con documentación Swagger

## Arquitectura

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Presentation  │───▶│   Application    │───▶│     Domain      │
│  (Controllers)  │    │   (Services)     │    │ (Entities, IFs) │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                ▲                        ▲
                                │                        │
                       ┌─────────────────┐               │
                       │ Infrastructure  │───────────────┘
                       │  (Providers)    │
                       └─────────────────┘
```

## Ejecución Rápida

### 1. Clonar y ejecutar
```bash
cd microservicios-seguridad
dotnet restore
dotnet build
cd src/OAuth2PoC.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

### 2. Probar la API
```bash
curl -X POST http://localhost:5000/api/auth/login \
-H "Content-Type: application/json" \
-d '{"provider": "GitHub", "codeChallenge": null}'
```

### 3. Resultado esperado
```json
{
  "authorizationUrl": "https://github.com/login/oauth/authorize?client_id=...",
  "state": "ae0b2358-9980-4c44-b3da-49942a97e898"
}
```

## Endpoints Principales


| Endpoint | Method | Descripción |
|----------|--------|-------------|
| `/api/auth/login` | POST | Inicia flujo OAuth2 |
| `/api/auth/callback` | GET | Maneja respuesta de GitHub |
| `/api/auth/refresh` | POST | Refresca tokens |

## Principios SOLID Demostrados

- **S** - Cada servicio tiene una responsabilidad específica
- **O** - Fácil agregar nuevos proveedores OAuth2 sin modificar código existente  
- **L** - Todas las implementaciones son intercambiables
- **I** - Interfaces específicas y enfocadas
- **D** - Dependencias por abstracción, no por implementación concreta

---

# 🚀 Cómo Ejecutar desde Cero

## 1. Instalar .NET 8.0
```bash
# macOS (Homebrew)
brew install dotnet

# Windows: Descargar desde https://dotnet.microsoft.com/download/dotnet/8.0
# Linux: sudo apt-get install -y dotnet-sdk-8.0

# Verificar
dotnet --version
```

## 2. Ejecutar el Proyecto
```bash
# Clonar y navegar
cd microservicios-seguridad
dotnet restore
dotnet build

# Ejecutar
cd src/OAuth2PoC.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

## 3. Probar con cURL/Postman
```bash
curl -X POST http://localhost:5000/api/auth/login \
-H "Content-Type: application/json" \
-d '{"provider": "GitHub", "codeChallenge": null}'
```

**Respuesta esperada:**
```json
{
  "authorizationUrl": "https://github.com/login/oauth/authorize?client_id=...",
  "state": "ae0b2358-9980-4c44-b3da-49942a97e898"
}
```

**Proyecto creado para el Diplomado en Microservicios y Seguridad** 🎓
