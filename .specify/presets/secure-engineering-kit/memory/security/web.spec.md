# Spec de Seguridad — Aplicaciones Web (navegador)
**Frente:** Web · **Base:** OWASP Top 10 2021/2025 RC1 + WSTG 4.2 · **Núcleo:** secure-core.md (R1–R7)
**Formato:** EARS + criterios de aceptación · **Trazabilidad:** OWASP + Núcleo + Prueba (WSTG 4.2)
**Consumido por:** agentes de IA de generación y revisión de código

> Cubre las 10 categorías del Top 10 web MÁS los controles de navegador que el Top 10 general no
> detalla (XSS/encoding, CSRF, CSP, manejo de token de sesión, open-redirect/CORS, SRI).

---

## REQ-WEB-01 — Broken Access Control
**OWASP: A01:2021/2025 · Núcleo: R3 · Prueba: WSTG-ATHZ-02/04 · Severidad: CRÍTICA**

WHEN el usuario solicita un recurso o acción,
THE SYSTEM SHALL aplicar la autorización server-side, nunca confiando en controles del cliente.

Criterios de aceptación:
- [ ] Rutas/acciones protegidas validan permiso en el servidor; ocultar en UI no es control.
- [ ] Deny-by-default; 403 ante acceso no autorizado.
- [ ] Identificadores de recurso no enumerables (UUID).

---

## REQ-WEB-02 — Cryptographic Failures
**OWASP: A02:2021 (A04:2025) · Núcleo: R2 · Prueba: WSTG-CRYP-01/04 · Severidad: ALTA**

WHEN la app web protege datos en tránsito o en reposo,
THE SYSTEM SHALL usar TLS 1.2+ y criptografía fuerte sin secretos en el cliente.

Criterios de aceptación:
- [ ] Todo el tráfico sobre TLS 1.2+; sin contenido mixto (mixed content).
- [ ] Contraseñas con bcrypt(≥12)/Argon2id/scrypt; sin MD5/SHA sin salt.
- [ ] Cero secretos en el bundle JS/HTML; IV aleatorio si se cifra server-side.

---

## REQ-WEB-03 — Injection (server-side)
**OWASP: A03:2021 (A05:2025) Injection · Núcleo: R5 · Prueba: WSTG-INPV-05 (SQLi) / WSTG-INPV-12 (command) · Severidad: CRÍTICA**

WHEN el backend web construye queries o comandos con datos de usuario,
THE SYSTEM SHALL usar exclusivamente queries parametrizadas / ORM con prepared statements.

Criterios de aceptación:
- [ ] Cero concatenación de input en SQL, comandos o templates.
- [ ] Validación whitelist de parámetros de búsqueda/filtro/orden.
- [ ] Pruebas WSTG-INPV-05 y WSTG-INPV-12 asociadas.

---

## REQ-WEB-04 — Cross-Site Scripting (XSS)
**OWASP: A03 Injection · Núcleo: R5 · Prueba: WSTG-CLNT-01 (DOM XSS) / WSTG-INPV-01-02 · Severidad: ALTA**

WHEN datos no confiables se renderizan en la página,
THE SYSTEM SHALL aplicar output encoding según el contexto (HTML, atributo, JS, URL, CSS).

Criterios de aceptación:
- [ ] No se usa innerHTML / dangerouslySetInnerHTML / v-html con datos no confiables.
- [ ] El HTML de usuario se sanitiza con librería robusta (DOMPurify), no con regex.
- [ ] CSP estricta: `default-src 'self'`, sin `'unsafe-inline'` ni `'unsafe-eval'`.

---

## REQ-WEB-05 — Insecure Design
**OWASP: A04:2021 (A06:2025) Insecure Design · Núcleo: R7 · Prueba: WSTG-BUSL-01..07 · Severidad: ALTA**

WHEN se diseña un flujo o endpoint web,
THE SYSTEM SHALL aplicar mínima exposición y validación de lógica de negocio server-side.

Criterios de aceptación:
- [ ] Devuelve solo los campos necesarios; nunca el objeto de BD completo.
- [ ] Config interna (IPs, URLs de servicios, claves) nunca se expone al cliente.
- [ ] Flujos multipaso validan orden y estado previo server-side.

---

## REQ-WEB-06 — Security Misconfiguration & Headers
**OWASP: A05:2021/2025 · Núcleo: A05 · Prueba: WSTG-CONF-07 (HSTS) / WSTG-CLNT-09 (clickjacking) · Severidad: MEDIA**

THE SYSTEM SHALL enviar el conjunto completo de headers de seguridad en cada respuesta.

Criterios de aceptación:
- [ ] CSP, HSTS, X-Content-Type-Options:nosniff, X-Frame-Options:DENY (o frame-ancestors),
      Referrer-Policy:no-referrer, Permissions-Policy.
- [ ] Clickjacking mitigado vía X-Frame-Options / CSP frame-ancestors.
- [ ] Errores sin stack traces ni información interna; Server/X-Powered-By eliminados.

---

## REQ-WEB-07 — Vulnerable Components & Supply Chain
**OWASP: A06:2021 (A03:2025 Supply Chain) · Núcleo: A06 · Prueba: WSTG-CONF-01 / SCA · Severidad: MEDIA**

THE SYSTEM SHALL controlar la integridad y versión de sus dependencias de front.

Criterios de aceptación:
- [ ] Subresource Integrity (SRI) en scripts/CSS servidos desde CDNs externos.
- [ ] Versiones pinneadas; SCA sobre dependencias npm sin HIGH/CRITICAL sin mitigar.

---

## REQ-WEB-08 — Authentication & Session/Token Handling
**OWASP: A07:2021/2025 Auth Failures · Núcleo: R6 · Prueba: WSTG-SESS-01..09 · Severidad: ALTA**

THE SYSTEM SHALL gestionar autenticación, sesiones y tokens sin exponerlos a código del cliente.

Criterios de aceptación:
- [ ] Tokens de sesión en cookie `HttpOnly; Secure; SameSite`; NO en localStorage.
- [ ] Sin datos sensibles en URL (query string), DOM ni historial del navegador.
- [ ] Invalidación de sesión server-side en logout; rate limiting en login.
- [ ] Token anti-CSRF (synchronizer o double-submit) en operaciones que cambian estado.
- [ ] Mensajes de login/recuperación genéricos que NO permiten enumerar usuarios (anti-enumeración).

---

## REQ-WEB-09 — Software & Data Integrity / Redirects & CORS
**OWASP: A08:2021/2025 Integrity Failures · Núcleo: R5 · Prueba: WSTG-CLNT-04 (open redirect) / WSTG-CLNT-07 (CORS) · Severidad: MEDIA**

IF la app redirige, expone CORS o consume datos/recursos externos,
THEN THE SYSTEM SHALL validar integridad y restringir destinos/orígenes a una whitelist.

Criterios de aceptación:
- [ ] Redirecciones validan el destino contra whitelist; sin open redirect por parámetro.
- [ ] CORS con orígenes explícitos; nunca `Access-Control-Allow-Origin: *` con credenciales.
- [ ] Datos/recursos externos verificados (firma/SRI) antes de usarse.

---

## REQ-WEB-10 — Logging & Mishandling Exceptional Conditions
**OWASP: A09 Logging (A10:2025 Mishandling Exceptional Conditions) · Núcleo: A05 · Prueba: WSTG-ERRH-01/02 · Severidad: MEDIA**

THE SYSTEM SHALL registrar eventos de seguridad y manejar errores sin fallar abierto.

Criterios de aceptación:
- [ ] Log estructurado de auth/autorización (sin contraseñas, tokens ni PII).
- [ ] Manejo explícito de excepciones; un error nunca concede acceso por defecto (no fail-open).
- [ ] Mensajes de error al cliente sin información interna.

---

## REQ-WEB-11 — SSRF (server-side de la capa web)
**OWASP: A10:2021 SSRF · Núcleo: R5 · Prueba: WSTG-INPV-19 (SSRF) · Severidad: ALTA**

IF el backend web recibe una URL o referencia remota desde el cliente,
THEN THE SYSTEM SHALL validarla contra una whitelist antes de cualquier petición saliente.

Criterios de aceptación:
- [ ] Whitelist de dominios/esquemas (solo https); rechaza IPs privadas, loopback y metadata.
- [ ] No sigue redirects hacia destinos fuera de la whitelist.

---

## REQ-WEB-12 — Unrestricted File Upload (data-driven)
**OWASP: A04:2021 Insecure Design (A05 Misconfig) · Núcleo: R5 · Prueba: WSTG-BUSL-09 (malicious file upload) · Severidad: ALTA**

WHEN la app recibe un archivo subido por el usuario,
THE SYSTEM SHALL validar tipo, extensión, tamaño y contenido antes de almacenarlo o servirlo.

Criterios de aceptación:
- [ ] Whitelist de extensiones/MIME permitidos; rechaza ejecutables y dobles extensiones.
- [ ] Límite de tamaño; nombre de archivo saneado (sin path traversal).
- [ ] Validación de contenido real (magic bytes), no solo la extensión declarada.
- [ ] Los archivos se almacenan fuera del webroot o sin permisos de ejecución.

> Patrón de alta frecuencia en el histórico de pentests (R10): "Unrestricted File Upload" recurrente.

---

## Cobertura OWASP Top 10 Web (2021 → 2025 RC1)
A01→REQ-WEB-01 · A02→REQ-WEB-02 · A03→REQ-WEB-03 y REQ-WEB-04 (XSS) · A04→REQ-WEB-05 y REQ-WEB-12 ·
A05→REQ-WEB-06 · A06→REQ-WEB-07 · A07→REQ-WEB-08 · A08→REQ-WEB-09 · A09→REQ-WEB-10 ·
A10(2021 SSRF)→REQ-WEB-11 · A10(2025 Mishandling)→REQ-WEB-10.
Controles de navegador: XSS→REQ-WEB-04 · CSRF→REQ-WEB-08 · CSP→REQ-WEB-04/06 ·
sesión/token→REQ-WEB-08 · open-redirect/CORS→REQ-WEB-09 · SRI→REQ-WEB-07 ·
clickjacking→REQ-WEB-06 · file upload→REQ-WEB-12 · anti-enumeración→REQ-WEB-08.
