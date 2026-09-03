---
name: 'Autenticación API'
description: 'Decoradores de Autorización.'
applyTo: 'src/**/Controllers/**/*.cs'
---
# Autenticación y Autorización
- Proteger controladores por defecto con `[Authorize]`.
- Endpoints de escritura: usar políticas específicas `[Authorize(Policy = "scope.write")]`.
- Para métodos públicos usar `[AllowAnonymous]`.
- JWT validación es manejada vía OpenIddict Resource Server (la API no emite tokens, solo valida).