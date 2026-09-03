# Spec de Seguridad — APIs / Backend
**Frente:** API · **Base:** OWASP API Security Top 10 2023 · **Núcleo:** secure-core.md (R1–R7)
**Formato:** EARS + criterios de aceptación · **Trazabilidad:** OWASP + Núcleo + Prueba (WSTG 4.2)
**Consumido por:** agentes de IA de generación y revisión de código

> Cada requisito cita las 3 patas de trazabilidad: categoría OWASP API · regla del núcleo · prueba WSTG concreta.

---

## REQ-API-01 — Object Level Authorization (BOLA)
**OWASP: API1:2023 · Núcleo: R3 · Prueba: WSTG-ATHZ-04 (IDOR) · Severidad: CRÍTICA**

WHEN un endpoint recibe un identificador de recurso (path, query o body),
THE SYSTEM SHALL verificar que el sujeto autenticado es propietario o está autorizado sobre ese
recurso ANTES de leerlo o mutarlo.

Criterios de aceptación:
- [ ] La verificación de ownership ocurre server-side; el cliente no la decide.
- [ ] 403 si el recurso existe pero no pertenece al sujeto; 404 si no existe.
- [ ] El identificador expuesto es UUID v4, no entero secuencial.
- [ ] Prueba WSTG-ATHZ-04 falla si se elimina la verificación de ownership.

---

## REQ-API-02 — Authentication
**OWASP: API2:2023 · Núcleo: R6 · Prueba: WSTG-ATHN-04 / WSTG-SESS-01 · Severidad: CRÍTICA**

WHEN un endpoint protegido recibe una petición,
THE SYSTEM SHALL validar firma, expiración, issuer y audience del token antes de procesarla.

Criterios de aceptación:
- [ ] JWT validado (firma+exp+iss+aud) en cada request; token inválido → 401.
- [ ] Token de acceso con vida ≤ 1 hora.
- [ ] El payload del token NO contiene PII (documento, nombre).
- [ ] El flujo de login incluye nonce/timestamp anti-replay validado server-side.
- [ ] Respuestas de login/registro/recuperación genéricas que NO permiten enumerar usuarios (anti-enumeración).

---

## REQ-API-03 — Object Property Level Authorization
**OWASP: API3:2023 · Núcleo: R7 · Prueba: WSTG-ATHZ-03 (mass assignment) · Severidad: ALTA**

WHEN un endpoint devuelve o recibe un objeto,
THE SYSTEM SHALL exponer y aceptar únicamente las propiedades autorizadas para el rol del sujeto.

Criterios de aceptación:
- [ ] La respuesta usa un DTO con whitelist de campos; nunca serializa la entidad de BD completa.
- [ ] El binding del body usa DTO con whitelist (anti mass-assignment); rol/idCentro/montos no son asignables desde el cliente.
- [ ] No se exponen propiedades internas (claves, IPs, rutas, configuración).

---

## REQ-API-04 — Unrestricted Resource Consumption
**OWASP: API4:2023 · Núcleo: A05 · Prueba: WSTG-ATHN-03 (rate limiting) · Severidad: ALTA**

WHILE el servicio atiende peticiones,
THE SYSTEM SHALL aplicar límites de consumo de recursos por cliente.

Criterios de aceptación:
- [ ] Rate limiting por IP/usuario en endpoints sensibles y de autenticación.
- [ ] Paginación obligatoria en colecciones; límite máximo de page size.
- [ ] Límite de tamaño de payload y de profundidad/complejidad de query (GraphQL).
- [ ] Timeouts en operaciones costosas.

---

## REQ-API-05 — Function Level Authorization
**OWASP: API5:2023 · Núcleo: R3 · Prueba: WSTG-ATHZ-02 (privilege escalation) · Severidad: ALTA**

WHEN un sujeto invoca una función administrativa o privilegiada,
THE SYSTEM SHALL verificar el rol/permiso requerido server-side antes de ejecutarla.

Criterios de aceptación:
- [ ] El control de rol está centralizado (policy/middleware), no duplicado por controlador.
- [ ] Endpoints administrativos rechazan (403) a roles no autorizados, no solo se ocultan en UI.
- [ ] Deny-by-default: si no hay regla de autorización definida, se deniega.

---

## REQ-API-06 — Sensitive Business Flows
**OWASP: API6:2023 · Núcleo: R4 · Prueba: WSTG-BUSL-01 (business logic) · Severidad: MEDIA**

WHERE un flujo de negocio es sensible (registro, generación de procesos, pago),
THE SYSTEM SHALL protegerlo contra automatización y abuso.

Criterios de aceptación:
- [ ] Mecanismo anti-automatización (CAPTCHA, device fingerprint o detección de anomalías).
- [ ] El flujo no puede ejecutarse fuera de orden ni saltándose validaciones server-side.

---

## REQ-API-07 — Server Side Request Forgery
**OWASP: API7:2023 · Núcleo: R5 · Prueba: WSTG-INPV-19 (SSRF) · Severidad: ALTA**

IF un endpoint recibe una URL o referencia de recurso remoto,
THEN THE SYSTEM SHALL validarla contra una whitelist antes de realizar cualquier petición saliente.

Criterios de aceptación:
- [ ] Whitelist de dominios/esquemas permitidos (solo https).
- [ ] Rechaza IPs privadas (RFC 1918), loopback y metadata endpoints (169.254.169.254).
- [ ] No sigue redirects hacia destinos fuera de la whitelist.

---

## REQ-API-08 — Security Misconfiguration
**OWASP: API8:2023 · Núcleo: A05 · Prueba: WSTG-CONF-07 (HSTS) / WSTG-CONF-06 (métodos HTTP) · Severidad: MEDIA**

THE SYSTEM SHALL responder con una configuración de seguridad endurecida por defecto.

Criterios de aceptación:
- [ ] Headers: HSTS, X-Content-Type-Options, X-Frame-Options, CSP, Referrer-Policy.
- [ ] CORS con orígenes explícitos en whitelist; nunca `*` junto con credenciales.
- [ ] Métodos HTTP innecesarios deshabilitados (TRACE/TRACK).
- [ ] Errores al cliente sin stack traces, nombres de tabla, rutas ni versiones internas.
- [ ] Headers de divulgación (Server, X-Powered-By) eliminados.

---

## REQ-API-09 — Improper Inventory Management
**OWASP: API9:2023 · Núcleo: A05 · Prueba: WSTG-CONF-01 (infra) / WSTG-CONF-04 (apps antiguas) · Severidad: MEDIA**

THE SYSTEM SHALL mantener un inventario controlado de endpoints y entornos.

Criterios de aceptación:
- [ ] APIs versionadas (X-Api-Version); versiones deprecadas documentadas y con fecha de retiro.
- [ ] Entornos no productivos (pre/QA) NO expuestos a internet con datos reales.
- [ ] Endpoints de debug/test no accesibles en producción.

---

## REQ-API-10 — Unsafe Consumption of APIs
**OWASP: API10:2023 · Núcleo: R5 · Prueba: WSTG-INPV-01 (input validation) · Severidad: MEDIA**

WHEN el servicio consume datos de una API de tercero,
THE SYSTEM SHALL validarlos y sanitizarlos antes de procesarlos o persistirlos.

Criterios de aceptación:
- [ ] Los datos de terceros se validan con el mismo rigor que el input de usuario.
- [ ] Se valida firma/origen de webhooks y callbacks antes de actuar sobre ellos.
- [ ] Timeouts y manejo de error explícito en integraciones (no fail-open).

---

## Cobertura OWASP API Security Top 10 2023
API1→REQ-API-01 · API2→REQ-API-02 · API3→REQ-API-03 · API4→REQ-API-04 · API5→REQ-API-05 ·
API6→REQ-API-06 · API7→REQ-API-07 · API8→REQ-API-08 · API9→REQ-API-09 · API10→REQ-API-10.
